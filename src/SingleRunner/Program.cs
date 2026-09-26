// SingleRunner — drive ONE EVE client end-to-end with the shared ClientAgent engine.
//
// Thin wrapper: locate a client, create one ClientAgent, run it in a loop, log each
// step, and publish its snapshot to the remote dashboard. All the real work (attach,
// read+parse, bot brain, motion execution) lives in AbotEngine.ClientAgent so the same
// engine powers the multi-window FleetOrchestrator.
//
// SAFETY: dry-run by default. --live moves the mouse and clicks in the real client.
//
// Usage:
//   SingleRunner                    dry-run, auto-detect the single exefile.exe
//   SingleRunner <pid>              target a specific client
//   SingleRunner --live             actually control the ship
//   SingleRunner --belt-test        simple on-grid combat behavior (lock->orbit->drones)
//   SingleRunner --profile Worm_T1  select a run profile
//   SingleRunner --interval 1500    step period in ms
//   SingleRunner --port 5005        dashboard port
//   SingleRunner --rescan           force a fresh UIRoot scan (ignore the cache)
//   SingleRunner --observe          write a rich per-tick .jsonl (ship state + events + bot plan)
//   SingleRunner --inspect-hangar   open Inventory once, dump its parsed/raw state, then exit
//   SingleRunner --undock-only      click only Undock, verify Ship UI, dump calibration, then exit
//   SingleRunner --probe-only        open Probe Scanner with Alt+P, dump calibration, then exit
//   SingleRunner --space-menu-only   right-click clear space, dump the first menu level, then exit
//   SingleRunner --laser-menu-only   right-click the grouped beam laser and log its charge menu
//   SingleRunner --crystal-distance 28000 [--expected-orbit 500] [--prefer-gleam]
//                                    select/reload one crystal with no hostile present, then exit
//   SingleRunner --dump-once         read and dump raw + parsed UI once; never emit input

using System.Diagnostics;
using AbotEngine;
using AbotEngine.Fleet;
using Eve64;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.Interface.MemoryStruct;
using SingleRunner;

static void Log(string m) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {m}");

// DPI-aware before any window interaction (else clicks land off on a scaled display).
try { EveOnline64.WinApi.SetProcessDPIAware(); } catch { /* older OS */ }

// --- args ---------------------------------------------------------------------
var live = false;
var intervalMs = 1500;
var port = 5005;
var profileName = "Worm_T1";
var forceRescan = false;
var beltTest = false;
var anomaly = false;
var observe = false;
var dump = false;
var inspectHangar = false;
var undockOnly = false;
var probeOnly = false;
var spaceMenuOnly = false;
var laserMenuOnly = false;
int? crystalDistance = null;
int? expectedOrbit = null;
var preferGleam = false;
var dumpOnce = false;
var stopFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "abot-STOP");
int? explicitPid = null;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--live": live = true; break;
        case "--interval" when i + 1 < args.Length: intervalMs = int.Parse(args[++i]); break;
        case "--port" when i + 1 < args.Length: port = int.Parse(args[++i]); break;
        case "--profile" when i + 1 < args.Length: profileName = args[++i]; break;
        case "--rescan": forceRescan = true; break;
        case "--belt-test": beltTest = true; break;
        case "--anomaly": anomaly = true; break;
        case "--observe": observe = true; break;
        case "--dump": dump = true; break;
        case "--inspect-hangar": inspectHangar = true; break;
        case "--undock-only": undockOnly = true; break;
        case "--probe-only": probeOnly = true; break;
        case "--space-menu-only": spaceMenuOnly = true; break;
        case "--laser-menu-only": laserMenuOnly = true; break;
        case "--crystal-distance" when i + 1 < args.Length: crystalDistance = int.Parse(args[++i]); break;
        case "--expected-orbit" when i + 1 < args.Length: expectedOrbit = int.Parse(args[++i]); break;
        case "--prefer-gleam": preferGleam = true; break;
        case "--dump-once": dumpOnce = true; break;
        case "--stop-file" when i + 1 < args.Length: stopFile = args[++i]; break;
        default:
            if (int.TryParse(args[i], out var pid)) explicitPid = pid;
            break;
    }
}

var profile = ProfilesRegistry.GetByName(profileName);
Log("=== SingleRunner ===");

// --- locate client ------------------------------------------------------------
Process? process;
if (explicitPid.HasValue)
{
    process = Process.GetProcesses().FirstOrDefault(p => p.Id == explicitPid.Value);
    if (process is null) { Log($"No process with id {explicitPid}."); return 1; }
}
else
{
    var candidates = Process.GetProcessesByName("exefile");
    if (candidates.Length == 0) { Log("No 'exefile.exe' client found. Log into the game first."); return 1; }
    if (candidates.Length > 1)
    {
        Log($"Found {candidates.Length} clients; pass the pid you want:");
        foreach (var c in candidates) Log($"    pid={c.Id}");
        return 1;
    }
    process = candidates[0];
}

// --- dashboard ----------------------------------------------------------------
_ = Dashboard.Start(port);
foreach (var ip in LocalIPv4Addresses())
    Log($"Dashboard: http://{ip}:{port}  (open from your phone / another PC on this network)");
Log($"Dashboard: http://localhost:{port}");

// --- observation log (optional) -----------------------------------------------
ObservationLog? obsLog = null;
if (observe)
{
    var obsPath = System.IO.Path.GetFullPath($"observation-{process.Id}-{DateTime.Now:yyyyMMdd-HHmmss}.jsonl");
    obsLog = new ObservationLog(obsPath);
    Log($"Observation log: {obsPath}");
}

// --- the agent ----------------------------------------------------------------
if (dump) live = false; // diagnostic read-only
Action<IMemoryMeasurement, AgentSnapshot>? observeCb = dump ? DumpState : (obsLog != null ? obsLog.Write : null);
using var agent = new ClientAgent(process, profile, live, beltTest,
    role: anomaly ? "anomaly" : beltTest ? "belt-test" : "solo",
    trace: Log, observe: observeCb, anomaly: anomaly);
Log($"Target: pid={agent.Pid}, title=\"{agent.Title}\", profile={profile.Name}" +
    (beltTest ? " (BELT TEST: lock -> orbit 10km -> drones at 5km)" : $", filament={profile.FilamentName}"));

if (live)
{
    Log("");
    Log("!!! LIVE MODE: the bot will move the mouse and click in the real EVE client. !!!");
    Log("    Do not touch the mouse. Starting in 5s — Ctrl+C to abort.");
    Thread.Sleep(5000);
}
else
{
    Log("DRY-RUN: decisions logged, no input sent. Add --live to control the ship.");
}

Log("Attaching (cached UIRoot if available, else ~10-90s scan)...");
if (!agent.Attach(forceRescan))
{
    Log("Could not attach (no UIRoot — login screen? 32-bit?). Exiting.");
    return 2;
}

if (dumpOnce)
{
    var dir = System.IO.Path.GetFullPath($"dump-once-{process.Id}-{DateTime.Now:yyyyMMdd-HHmmss}");
    agent.DumpDiagnostics(dir);
    Log($"Read-only UI snapshot written to {dir}");
    return 0;
}

if (dump)
{
    var rawPath = System.IO.Path.GetFullPath($"rawtree-{process.Id}.json");
    agent.DumpRawTree(rawPath);
    Log($"Raw tree written to {rawPath}");
}

if (inspectHangar)
{
    Log($"Hangar inspection: {agent.OpenInventoryForInspection()}");
    Thread.Sleep(1800);
    var dir = System.IO.Path.GetFullPath($"hangar-{process.Id}-{DateTime.Now:yyyyMMdd-HHmmss}");
    agent.DumpDiagnostics(dir);
    Log($"Hangar inspection written to {dir}");
    return 0;
}

if (undockOnly)
{
    Log($"Launch calibration: {agent.UndockForCalibration()}");
    AgentSnapshot launch = new();
    for (var attempt = 0; attempt < 20; attempt++)
    {
        Thread.Sleep(1000);
        launch = agent.Step(); // runner is dry-run: perception/decision only, no strategy input is emitted
        DashboardState.Current = launch;
        Log($"Launch calibration: attempt={attempt + 1} parse={launch.ParseOk} inSpace={launch.InSpace} " +
            $"A{launch.Armor}/S{launch.Shield}/H{launch.Struct}");
        if (launch.InSpace) break;
    }

    var dir = System.IO.Path.GetFullPath($"launch-{process.Id}-{DateTime.Now:yyyyMMdd-HHmmss}");
    agent.DumpDiagnostics(dir);
    Log($"Launch calibration written to {dir}");
    return launch.InSpace ? 0 : 3;
}

if (probeOnly)
{
    Log($"Probe Scanner calibration: {agent.OpenProbeScannerForCalibration()}");
    Thread.Sleep(1800);
    var dir = System.IO.Path.GetFullPath($"probe-{process.Id}-{DateTime.Now:yyyyMMdd-HHmmss}");
    agent.DumpDiagnostics(dir);
    Log($"Probe Scanner calibration written to {dir}");
    return 0;
}

if (spaceMenuOnly)
{
    Log($"Space menu calibration: {agent.OpenSpaceContextMenuForCalibration()}");
    Thread.Sleep(900);
    var dir = System.IO.Path.GetFullPath($"space-menu-{process.Id}-{DateTime.Now:yyyyMMdd-HHmmss}");
    agent.DumpDiagnostics(dir);
    Log($"Space menu calibration written to {dir}");
    return 0;
}

if (laserMenuOnly)
{
    Log($"Laser menu calibration: {agent.InspectLaserCrystalMenuForCalibration()}");
    return 0;
}

if (crystalDistance is int calibrationDistance)
{
    var parsedCrystal = RetributionCrystalCatalog
        .FromTypeId(agent.ReadLaserChargeTypeIdForCalibration())?.Crystal ?? LaserCrystal.KeepCurrent;
    var selection = RetributionCrystalSelector.Decide(
        calibrationDistance,
        expectedOrbit,
        parsedCrystal,
        preferGleam);
    Log($"Crystal selector: {selection.Crystal}; {selection.Reason}; loaded={parsedCrystal}");
    Log($"Crystal live calibration: {agent.ActuateLaserCrystalForCalibration(selection.Crystal)}");
    return 0;
}

// --- emergency stop -----------------------------------------------------------
try { if (File.Exists(stopFile)) File.Delete(stopFile); } catch { /* stale/locked */ }
HotkeyStop.Start();

// --- loop ---------------------------------------------------------------------
Log("");
Log($"EMERGENCY STOP: press {HotkeyStop.Combo} anytime (works while EVE is focused) — or create {stopFile}");
Log($"SNAPSHOT: press {HotkeyStop.SnapshotCombo} to dump a screenshot + raw/parsed UI state to disk for markup.");
Log("Entering step loop. Ctrl+C to stop.");
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

while (!cts.IsCancellationRequested)
{
    if (HotkeyStop.Triggered || File.Exists(stopFile))
    {
        Log($"EMERGENCY STOP ({(HotkeyStop.Triggered ? HotkeyStop.Combo : "stop file")}) — halting. Mouse released; take manual control.");
        try { File.Delete(stopFile); } catch { }
        break;
    }

    if (HotkeyStop.ConsumeSnapshotRequest())
        TakeSnapshot();

    var snap = agent.Step();
    DashboardState.Current = snap;

    if (dump && snap.ParseOk) { Log("Dump complete."); break; }

    if (!snap.ParseOk)
    {
        Log($"step {snap.StepIndex}: read/parse failed — {snap.LastError}");
    }
    else
    {
        Log($"--- step {snap.StepIndex} | {snap.System ?? "?"} | " +
            $"A{snap.Armor}/S{snap.Shield}/H{snap.Struct} | locked={snap.TargetsLocked} | motions={snap.MotionCount}" +
            (snap.Executed ? " (executed)" : snap.MotionCount > 0 ? " (dry-run)" : "") + " ---");
        Log($"    [{snap.StrategyStatus.State}] {snap.StrategyStatus.Summary}" +
            (!string.IsNullOrWhiteSpace(snap.StrategyStatus.Action)
                ? $" -> {snap.StrategyStatus.Action}"
                : ""));
        foreach (var intent in snap.Intents)
            Log($"    {intent}");
        foreach (var problem in snap.MotionProblems)
            Log($"    ! {problem}");
        if (snap.LastError != null)
            Log($"    bot error: {snap.LastError}");
    }

    try { Task.Delay(intervalMs, cts.Token).Wait(); } catch { break; }
}

obsLog?.Dispose();
if (obsLog != null) Log($"Observation log written: {obsLog.Path}");
Log("Stopped.");
return 0;

// Operator snapshot (Ctrl+Alt+S): screenshot the client + dump raw/parsed UI state for later markup.
void TakeSnapshot()
{
    try
    {
        var dir = System.IO.Path.GetFullPath($"snapshot-{process!.Id}-{DateTime.Now:yyyyMMdd-HHmmss}");
        System.IO.Directory.CreateDirectory(dir);
        var shot = System.IO.Path.Combine(dir, "screenshot.bmp");
        if (WindowCapture.Capture(process.MainWindowHandle, shot, out var err))
            Log($"snapshot: screenshot -> {shot}");
        else
            Log($"snapshot: screenshot FAILED ({err}) — is the EVE window visible (not minimized/covered)?");
        var files = agent.DumpDiagnostics(dir);
        Log($"snapshot: state -> {dir}  (rawtree.json + parsed.json)");
    }
    catch (Exception e)
    {
        Log($"snapshot FAILED: {e.Message}");
    }
}

// Diagnostic: print exactly what the parser sees for drones and inventory (ground truth for readiness).
static void DumpState(IMemoryMeasurement m, AgentSnapshot s)
{
    void P(string t) => Console.WriteLine($"[dump] {t}");
    P("===== DRONE WINDOW =====");
    var dv = m.WindowDroneView;
    if (dv == null) P("  WindowDroneView: null (drone window closed?)");
    else
    {
        var groups = dv.DroneGroups ?? new List<IDronesWindowEntryGroupStructure>();
        P($"  groups: {groups.Count}");
        foreach (var g in groups)
            P($"    header='{g.Header?.MainText}'  children={g.Children?.Count ?? 0}");
        P($"  DroneGroupInBay.header  = '{dv.DroneGroupInBay?.Header?.MainText}'  children={dv.DroneGroupInBay?.Children?.Count ?? 0}");
        P($"  DroneGroupInSpace.header= '{dv.DroneGroupInSpace?.Header?.MainText}'  children={dv.DroneGroupInSpace?.Children?.Count ?? 0}");
    }

    P("===== INVENTORY =====");
    var wins = m.WindowInventory ?? Array.Empty<IWindowInventory>();
    P($"  inventory windows: {wins.Length}");
    foreach (var wi in wins)
    {
        P($"  --- window: subCaption='{wi.SubCaptionLabelText}' ---");
        var items = wi.SelectedContainerInventory?.ItemsView;
        if (items == null) { P("    SelectedContainerInventory.ItemsView: null"); continue; }
        P($"    items: {items.Count}");
        var n = 0;
        foreach (var it in items)
        {
            if (n++ >= 25) { P("    ...(truncated)"); break; }
            var cells = it.CellsTexts == null ? "" : string.Join(" | ", it.CellsTexts.Select(kv => $"{kv.Key}={kv.Value}"));
            P($"    [{n}] {cells}");
        }
    }
}

static IEnumerable<string> LocalIPv4Addresses()
{
    try
    {
        return System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
            .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .Select(a => a.ToString())
            .Where(a => !a.StartsWith("127."))
            .ToArray();
    }
    catch { return Array.Empty<string>(); }
}
