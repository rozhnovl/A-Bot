using System.Text.Json;
using AbotEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetOrchestrator;

/// <summary>Thread-safe holder for the whole fleet's latest snapshots, keyed by pid.</summary>
public static class FleetState
{
    private static volatile IReadOnlyList<AgentSnapshot> current = Array.Empty<AgentSnapshot>();
    public static StatusJournal Journal { get; } = new();
    public static IReadOnlyList<AgentSnapshot> Current
    {
        get => current;
        set
        {
            current = value;
            Journal.Observe(value);
        }
    }
}

/// <summary>
/// A remote fleet dashboard: one card per client, each showing role, health, plan and a
/// per-agent status banner. Bound to 0.0.0.0 so the whole fleet can be watched from a
/// phone while live mode holds the local mouse hostage.
/// </summary>
public static class FleetDashboard
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static Task Start(int port)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        var app = builder.Build();

        app.MapGet("/", () => Results.Content(HtmlPage, "text/html"));
        object Payload()
        {
            var agents = FleetState.Current;
            return new
            {
                serverNowMs = Environment.TickCount64,
                agents,
                overall = StatusJournal.Overview(agents),
                events = FleetState.Journal.Recent(),
            };
        }
        app.MapGet("/state.json", () => Results.Content(JsonSerializer.Serialize(Payload(), JsonOpts), "application/json"));
        app.MapGet("/api/status", () => Results.Json(Payload(), JsonOpts));
        app.MapGet("/health", () =>
        {
            var agents = FleetState.Current;
            var overview = StatusJournal.Overview(agents);
            var ok = agents.Count > 0 && overview.State != "Error";
            return Results.Json(new { ok, overall = overview }, JsonOpts,
                statusCode: ok ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
        });

        return app.RunAsync();
    }

    private const string HtmlPage = """
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>A-Bot Fleet</title>
<style>
  :root { color-scheme: dark; }
  body { margin:0; font-family:system-ui,-apple-system,Segoe UI,Roboto,sans-serif; background:#0d1117; color:#c9d1d9; }
  header { padding:14px 18px; font-size:16px; font-weight:700; }
  .overall { margin:0 14px 12px; padding:12px 14px; border-radius:8px; font-weight:700; }
  .fleet { display:grid; grid-template-columns:repeat(auto-fill,minmax(300px,1fr)); gap:12px; padding:0 14px 18px; }
  .card { background:#161b22; border:1px solid #21262d; border-radius:10px; overflow:hidden; }
  .top { padding:8px 12px; font-weight:700; display:flex; justify-content:space-between; align-items:center; }
  .ok { background:#12401f; color:#7ee787; } .warn { background:#4d3b00; color:#f2cc60; } .bad { background:#4d1414; color:#ff7b72; }
  .role { font-size:11px; text-transform:uppercase; letter-spacing:.5px; opacity:.85; }
  .body { padding:10px 12px; }
  .row { display:flex; justify-content:space-between; font-size:13px; padding:2px 0; }
  .k { color:#8b949e; }
  .bars div { height:14px; border-radius:4px; background:#21262d; margin:4px 0; position:relative; }
  .bars span { position:absolute; inset:0; border-radius:4px; }
  .arm { background:#d29922; } .shd { background:#388bfd; } .str { background:#8957e5; }
  .plan { margin:6px 0 0; font-size:12px; color:#adbac7; }
  .plan div { border-left:2px solid #388bfd; padding:2px 6px; margin:3px 0; background:#0d1117; word-break:break-word; }
  .strategy { margin-top:8px; padding:7px; background:#0d1117; border-radius:6px; font-size:12px; }
  .strategy b { color:#f0f6fc; }
  .history { margin:0 14px 18px; background:#161b22; border:1px solid #21262d; border-radius:10px; padding:8px 12px; }
  .event { font-size:12px; padding:4px 0; border-bottom:1px solid #21262d; }
  .event:last-child { border-bottom:0; }
  .event time { color:#8b949e; margin-right:6px; }
  .err { color:#ff7b72; font-size:12px; margin-top:4px; }
  .muted { color:#8b949e; font-size:12px; padding:0 18px 16px; }
</style>
</head>
<body>
<header>A-Bot Fleet <span id="count" class="muted"></span></header>
<div id="overall" class="overall warn">connecting…</div>
<div id="fleet" class="fleet"></div>
<header>Recent strategy changes</header>
<div id="history" class="history"><div class="muted">—</div></div>
<div class="muted" id="foot"></div>
<script>
function bar(cls,v){ return '<div><span class="'+cls+'" style="width:'+(v==null?0:v)+'%"></span></div>'; }
function esc(v){ return String(v??'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }
async function tick(){
  try {
    const {serverNowMs, agents, overall, events} = await (await fetch('/state.json',{cache:'no-store'})).json();
    document.getElementById('count').textContent = '· '+agents.length+' client(s)';
    const ov=document.getElementById('overall');
    ov.className='overall '+(overall.state==='Error'?'bad':overall.state==='Warning'?'warn':'ok');
    ov.textContent=overall.state+' · '+overall.summary;
    document.getElementById('fleet').innerHTML = agents.map(s => {
      const goodAge = serverNowMs - s.lastGoodParseMs;
      const st=s.strategyStatus||{};
      let cls='ok', txt='OK';
      if (s.stepIndex===0){ cls='warn'; txt='starting'; }
      else if (!s.parseOk || goodAge>6000){ cls='bad'; txt='BROKEN '+Math.round(goodAge/1000)+'s'; }
      else if (s.lastError || st.state==='Error'){ cls='bad'; txt='ERROR'; }
      else if (st.state==='Warning' || !s.readyOk){ cls='warn'; txt='WARNING'; }
      else { txt=st.state||'OK'; }
      const plan = (s.intents&&s.intents.length) ? s.intents.map(i=>'<div>'+i.replace(/</g,'&lt;')+'</div>').join('') : '<div class="k">—</div>';
      return '<div class="card"><div class="top '+cls+'"><span><span class="role">'+(s.role||'')+'</span> '+(s.system||'?')+'</span><span>'+txt+'</span></div>'
        +'<div class="body">'
        +'<div class="row"><span class="k">pid '+s.pid+' · '+s.mode+'</span><span>step '+s.stepIndex+'</span></div>'
        +'<div class="bars">'+bar('arm',s.armor)+bar('shd',s.shield)+bar('str',s.struct)+'</div>'
        +'<div class="row"><span class="k">cap · incoming</span><span>'+(s.capacitor==null?'—':s.capacitor+'%')+' · '+s.incomingDps+' DPS / '+s.attackers+' attackers</span></div>'
        +'<div class="row"><span class="k">locked</span><span>'+s.targetsLocked+'</span></div>'
        +'<div class="row"><span class="k">overview</span><span>'+s.overviewEntries+'</span></div>'
        +'<div class="row"><span class="k">motions</span><span>'+s.motionCount+(s.executed?' ✓':'')+'</span></div>'
        +'<div class="strategy"><div><b>'+esc(st.strategy||'—')+'</b> · '+esc(st.stage||'—')+'</div>'
        +'<div>'+esc(st.summary||'No summary')+'</div>'
        +'<div class="k">action: '+esc(st.action||'none')+'</div></div>'
        +'<div class="plan">'+plan+'</div>'
        +(s.lastError?'<div class="err">'+s.lastError.replace(/</g,'&lt;')+'</div>':'')
        +'</div></div>';
    }).join('');
    document.getElementById('history').innerHTML = (events&&events.length)
      ? events.slice(0,30).map(e=>'<div class="event"><time>'+new Date(e.atUnixMs).toLocaleTimeString()+'</time>'+
          '<b>['+esc(e.role||e.pid)+'] '+esc(e.state)+'</b> · '+esc(e.summary)+(e.action?' → '+esc(e.action):'')+'</div>').join('')
      : '<div class="muted">no status changes yet</div>';
    document.getElementById('foot').textContent = 'updated '+new Date().toLocaleTimeString();
  } catch(e){ document.getElementById('count').textContent = '· cannot reach orchestrator'; }
}
tick(); setInterval(tick, 1000);
</script>
</body>
</html>
""";
}
