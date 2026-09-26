using System.Text.Json;
using AbotEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SingleRunner;

/// <summary>Thread-safe holder for the latest agent snapshot (single-writer, many-reader).</summary>
public static class DashboardState
{
    private static volatile AgentSnapshot current = new();
    public static StatusJournal Journal { get; } = new();
    public static AgentSnapshot Current
    {
        get => current;
        set
        {
            current = value;
            Journal.Observe(new[] { value });
        }
    }
}

/// <summary>
/// A minimal Kestrel web app that serves the live dashboard. Bound to 0.0.0.0 so it
/// is reachable from a phone or another PC on the same network at http://&lt;pc-ip&gt;:port.
/// </summary>
public static class Dashboard
{
    // camelCase so the JSON keys match what the page's JavaScript reads (s.stepIndex, s.armor, ...).
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static Task Start(int port)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();            // keep the console clean for the bot log
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        var app = builder.Build();

        app.MapGet("/", () => Results.Content(HtmlPage, "text/html"));
        object Payload()
        {
            var snapshot = DashboardState.Current;
            var payload = new
            {
                serverNowMs = Environment.TickCount64,
                snapshot,
                overall = StatusJournal.Overview(new[] { snapshot }),
                events = DashboardState.Journal.Recent(),
            };
            return payload;
        }
        app.MapGet("/state.json", () => Results.Content(JsonSerializer.Serialize(Payload(), JsonOpts), "application/json"));
        app.MapGet("/api/status", () => Results.Json(Payload(), JsonOpts));
        app.MapGet("/health", () =>
        {
            var snapshot = DashboardState.Current;
            var overview = StatusJournal.Overview(new[] { snapshot });
            var ok = overview.State != "Error";
            return Results.Json(new { ok, status = snapshot.StrategyStatus, snapshot.Pid }, JsonOpts,
                statusCode: ok ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
        });

        return app.RunAsync();
    }

    // Self-contained page: dark, mobile-friendly, polls /state.json once a second and
    // shows a health banner driven by parse success and staleness.
    private const string HtmlPage = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>A-Bot SingleRunner</title>
<style>
  :root { color-scheme: dark; }
  body { margin:0; font-family:system-ui,-apple-system,Segoe UI,Roboto,sans-serif;
         background:#0d1117; color:#c9d1d9; }
  header { padding:14px 18px; font-size:15px; font-weight:600; letter-spacing:.3px; }
  .banner { padding:16px 18px; font-size:20px; font-weight:700; text-align:center; }
  .ok   { background:#12401f; color:#7ee787; }
  .warn { background:#4d3b00; color:#f2cc60; }
  .bad  { background:#4d1414; color:#ff7b72; }
  .grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(140px,1fr));
          gap:10px; padding:14px 18px; }
  .card { background:#161b22; border:1px solid #21262d; border-radius:10px; padding:12px; }
  .card .k { font-size:11px; text-transform:uppercase; color:#8b949e; letter-spacing:.5px; }
  .card .v { font-size:22px; font-weight:700; margin-top:4px; }
  .card .v.small { font-size:15px; line-height:1.35; }
  .bars { padding:0 18px; }
  .bar { height:20px; border-radius:5px; background:#21262d; margin:6px 0; position:relative; overflow:hidden; }
  .bar > span { position:absolute; inset:0; width:0; transition:width .3s; }
  .bar > label { position:absolute; left:8px; top:2px; font-size:12px; font-weight:600; }
  .arm > span { background:#d29922; } .shd > span { background:#388bfd; } .str > span { background:#8957e5; }
  h2 { font-size:13px; text-transform:uppercase; color:#8b949e; padding:6px 18px 0; margin:14px 0 0; }
  ul { list-style:none; margin:6px 18px; padding:0; }
  li { background:#161b22; border-left:3px solid #388bfd; padding:8px 10px; margin:5px 0;
       border-radius:0 6px 6px 0; font-size:13px; word-break:break-word; }
  .err { color:#ff7b72; }
  .status li { border-left-color:#3fb950; }
  .history li { border-left-color:#8b949e; font-size:12px; }
  .history time { color:#8b949e; margin-right:6px; }
  .muted { color:#8b949e; font-size:12px; padding:6px 18px 18px; }
</style>
</head>
<body>
<header>A-Bot · SingleRunner <span id="mode" class="muted"></span></header>
<div id="banner" class="banner warn">connecting…</div>
<div class="bars">
  <div class="bar arm"><span id="armBar"></span><label id="armLbl">armor —</label></div>
  <div class="bar shd"><span id="shdBar"></span><label id="shdLbl">shield —</label></div>
  <div class="bar str"><span id="strBar"></span><label id="strLbl">struct —</label></div>
</div>
<div class="grid">
  <div class="card"><div class="k">System</div><div class="v" id="system">—</div></div>
  <div class="card"><div class="k">Step</div><div class="v" id="step">—</div></div>
  <div class="card"><div class="k">Targets</div><div class="v" id="targets">—</div></div>
  <div class="card"><div class="k">Overview</div><div class="v" id="overview">—</div></div>
  <div class="card"><div class="k">Motions</div><div class="v" id="motions">—</div></div>
  <div class="card"><div class="k">Last parse</div><div class="v" id="age">—</div></div>
  <div class="card"><div class="k">Strategy</div><div class="v small" id="strategy">—</div></div>
  <div class="card"><div class="k">Stage · state</div><div class="v small" id="strategyState">—</div></div>
</div>
<h2>Strategy status</h2>
<ul id="strategyStatus" class="status"><li class="muted">—</li></ul>
<h2>Readiness</h2>
<ul id="readiness"><li class="muted">—</li></ul>
<h2>Current plan</h2>
<ul id="intents"><li class="muted">—</li></ul>
<h2>Recent changes</h2>
<ul id="history" class="history"><li class="muted">—</li></ul>
<div id="errorBox"></div>
<div class="muted" id="foot"></div>
<script>
function bar(id, lbl, name, v){
  document.getElementById(id).style.width = (v==null?0:v)+'%';
  document.getElementById(lbl).textContent = name+' '+(v==null?'—':v+'%');
}
function esc(v){ return String(v??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }
async function tick(){
  try {
    const r = await fetch('/state.json',{cache:'no-store'});
    const {serverNowMs, snapshot:s, overall, events} = await r.json();
    const st = s.strategyStatus || {};
    document.getElementById('mode').textContent = '· '+s.profile+' · '+s.mode+' · pid '+s.pid;
    const rl = document.getElementById('readiness');
    rl.innerHTML = (s.readiness&&s.readiness.length)
      ? s.readiness.map(r=>'<li style="border-left-color:'+(r.ok?'#3fb950':'#f85149')+'">'
          +(r.ok?'✓':'✗')+' '+r.name+' — <span class="muted">'+r.detail.replace(/</g,'&lt;')+'</span></li>').join('')
      : '<li class="muted">—</li>';
    const ageMs = serverNowMs - s.updatedAtMs;
    const goodAgeMs = serverNowMs - s.lastGoodParseMs;
    const b = document.getElementById('banner');
    if (s.stepIndex===0){ b.className='banner warn'; b.textContent='starting…'; }
    else if (!s.parseOk || goodAgeMs>6000){ b.className='banner bad';
      b.textContent = 'PARSING/READ BROKEN — intervene ('+Math.round(goodAgeMs/1000)+'s stale)'; }
    else if (s.lastError || overall.state==='Error'){ b.className='banner bad'; b.textContent=overall.summary; }
    else if (overall.state==='Warning'){ b.className='banner warn'; b.textContent=overall.summary; }
    else { b.className='banner ok'; b.textContent=overall.summary; }
    bar('armBar','armLbl','armor',s.armor); bar('shdBar','shdLbl','shield',s.shield); bar('strBar','strLbl','struct',s.struct);
    document.getElementById('system').textContent = s.system ?? '—';
    document.getElementById('step').textContent = s.stepIndex;
    document.getElementById('targets').textContent = s.targetsLocked;
    document.getElementById('overview').textContent = s.overviewEntries;
    document.getElementById('motions').textContent = s.motionCount + (s.executed?' ✓':'');
    document.getElementById('age').textContent = (ageMs/1000).toFixed(1)+'s';
    document.getElementById('strategy').textContent = st.strategy || '—';
    document.getElementById('strategyState').textContent = (st.stage||'—')+' · '+(st.state||'—');
    document.getElementById('strategyStatus').innerHTML =
      '<li><b>'+esc(st.summary||'No summary')+'</b></li>'+
      '<li style="border-left-color:#388bfd"><span class="muted">action:</span> '+esc(st.action||'none')+'</li>';
    const ul = document.getElementById('intents');
    ul.innerHTML = (s.intents&&s.intents.length) ? s.intents.map(i=>'<li>'+i.replace(/</g,'&lt;')+'</li>').join('')
                                                 : '<li class="muted">no plan this step</li>';
    document.getElementById('history').innerHTML = (events&&events.length)
      ? events.slice(0,20).map(e=>'<li><time>'+new Date(e.atUnixMs).toLocaleTimeString()+'</time>'+
          '<b>'+esc(e.state)+'</b> · '+esc(e.summary)+(e.action?' → '+esc(e.action):'')+'</li>').join('')
      : '<li class="muted">no status changes yet</li>';
    document.getElementById('errorBox').innerHTML = s.lastError ? '<h2>Last error</h2><ul><li class="err">'+s.lastError.replace(/</g,'&lt;')+'</li></ul>' : '';
    document.getElementById('foot').textContent = 'uptime '+Math.round(s.uptimeSec)+'s · "'+(s.title||'')+'"';
  } catch(e){
    const b=document.getElementById('banner'); b.className='banner bad'; b.textContent='dashboard cannot reach runner';
  }
}
tick(); setInterval(tick, 1000);
</script>
</body>
</html>
""";
}
