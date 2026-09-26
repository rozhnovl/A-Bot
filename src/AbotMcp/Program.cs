// AbotMcp — expose the A-Bot engine to Claude Code as an MCP server, so the operator's
// assistant can perceive the EVE client and drive it action-by-action in near real time.
//
// The process owns the game attachment (memory reader + screen input) and stays alive; Claude
// Code connects over Streamable HTTP (see /.mcp.json) and may disconnect/reconnect freely.
//
// SAFETY: dry-run by default — every input tool reports what it WOULD do. --live sends real
// mouse/keyboard input. Ctrl+Alt+K is a global emergency stop (also exposed as a tool).
//
// Usage:
//   AbotMcp                       dry-run, all running exefile.exe clients
//   AbotMcp --live                real input
//   AbotMcp --pid 1234 [--pid ..] only these clients
//   AbotMcp --port 5030           MCP endpoint http://127.0.0.1:<port>/mcp
//   AbotMcp --profile Worm_T1     run profile for bot_step / autopilot
//   AbotMcp --rescan              ignore the cached UIRoot addresses

using System.Diagnostics;
using AbotMcp;
using Eve64;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using Sanderling.ABot.Bot.Configuration;

static void Log(string m) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {m}");

try { EveOnline64.WinApi.SetProcessDPIAware(); } catch { /* older OS */ }

var live = false;
var port = 5030;
var profileName = "Worm_T1";
var rescan = false;
var noHotkey = false;
var pids = new List<int>();
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--live": live = true; break;
        case "--port" when i + 1 < args.Length: port = int.Parse(args[++i]); break;
        case "--profile" when i + 1 < args.Length: profileName = args[++i]; break;
        case "--pid" when i + 1 < args.Length: pids.Add(int.Parse(args[++i])); break;
        case "--rescan": rescan = true; break;
        case "--no-hotkey": noHotkey = true; break;
        default:
            if (int.TryParse(args[i], out var pid)) pids.Add(pid);
            break;
    }
}

var profile = ProfilesRegistry.GetByName(profileName);
Fleet.Live = live;
Fleet.Log = Log;
Log("=== AbotMcp ===");

// --- discover clients ---------------------------------------------------------
List<Process> processes;
if (pids.Count > 0)
{
    processes = new List<Process>();
    foreach (var pid in pids)
    {
        try { processes.Add(Process.GetProcessById(pid)); }
        catch { Log($"No process with id {pid}."); }
    }
}
else
{
    processes = Process.GetProcessesByName("exefile").OrderBy(p => p.Id).ToList();
}
Fleet.Profile = profile;
Fleet.Config = FleetConfig.Load("fleet.config.json");
if (processes.Count == 0)
    Log("No EVE client (exefile.exe) running yet — serving anyway; use the attach_clients tool once a client is logged in.");
foreach (var p in processes)
{
    var host = Fleet.AddClient(p);
    Log($"    pid={p.Id,-6} role={host.Agent.Role,-10} title=\"{host.Agent.Title}\"");
}
Log($"Mode: {(live ? "LIVE — input tools send real mouse/keyboard input" : "dry-run — input tools only report")}, profile={profile.Name}");

if (!noHotkey)
{
    HotkeyStop.Start(() => Fleet.TriggerEmergencyStop($"hotkey {HotkeyStop.Combo}"));
    Log($"Emergency stop: {HotkeyStop.Combo} (global; also the emergency_stop tool).");
}

// --- MCP server (Streamable HTTP) --------------------------------------------
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "HH:mm:ss "; });
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
builder.Services
    .AddMcpServer(o =>
    {
        o.ServerInfo = new Implementation { Name = "abot", Version = "1.0.0" };
        o.ServerInstructions = AbotTools.Instructions;
    })
    .WithHttpTransport(o => o.Stateless = true)
    .WithTools<AbotTools>();

var app = builder.Build();
app.MapMcp("/mcp");
app.MapGet("/", () => Results.Text(Fleet.StatusText()));
app.MapGet("/state.json", () => Results.Text(Fleet.StatusJson(), "application/json"));

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var web = app.RunAsync(cts.Token);
Log($"MCP endpoint: http://127.0.0.1:{port}/mcp   (status page: http://127.0.0.1:{port}/)");

// Attach after the endpoint is up so list_clients already answers during a slow UIRoot scan.
Fleet.AttachAllInBackground(rescan, () => Log("Ready. Ctrl+C to exit."));

try { await web; } catch (OperationCanceledException) { }
foreach (var host in Fleet.Agents) host.Dispose();
Log("Stopped.");
return 0;
