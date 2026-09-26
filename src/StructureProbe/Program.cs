// StructureProbe — a standalone diagnostic for the A-Bot memory-reading pipeline.
//
// It attaches to a running EVE Online client (exefile.exe), reads the Python UI
// tree straight out of process memory, and prints a report describing what it
// found. The point is to answer one question quickly: does our reader + parser
// still understand the structure of the *current* EVE client?
//
// It has no dependency on Redis, Aspire or the bot logic — just Eve64.
//
// Usage:
//   StructureProbe                 attach to the single running exefile.exe
//   StructureProbe <pid>           attach to a specific process id
//   StructureProbe --dump <path>   also write the largest UI tree as JSON to <path>

using System.Diagnostics;
using System.Text.Json;
using Eve64;
using PythonStructures;
using Sanderling.Interface.MemoryStruct;

static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

string? dumpPath = null;
int? explicitPid = null;

for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--dump" && i + 1 < args.Length) { dumpPath = args[++i]; continue; }
    if (int.TryParse(args[i], out var pid)) { explicitPid = pid; }
}

Log("=== A-Bot StructureProbe ===");

// ---------------------------------------------------------------------------
// 1. Locate the EVE client process.
// ---------------------------------------------------------------------------
Process? process;
if (explicitPid.HasValue)
{
    process = Process.GetProcesses().FirstOrDefault(p => p.Id == explicitPid.Value);
    if (process == null) { Log($"No process with id {explicitPid}."); return 1; }
}
else
{
    var candidates = Process.GetProcessesByName("exefile");
    if (candidates.Length == 0)
    {
        Log("No 'exefile.exe' process found. Start & log in to an EVE client first,");
        Log("or pass a process id explicitly: StructureProbe <pid>.");
        return 1;
    }
    if (candidates.Length > 1)
    {
        Log($"Found {candidates.Length} EVE clients:");
        foreach (var c in candidates)
            Log($"    pid={c.Id}  title=\"{SafeTitle(c)}\"");
        Log("Multiple clients running — pass the pid you want: StructureProbe <pid>.");
        return 1;
    }
    process = candidates[0];
}

Log($"Attached to pid={process.Id}, title=\"{SafeTitle(process)}\", " +
    $"workingSet={process.WorkingSet64 / (1024 * 1024)} MB, 64-bit assumed.");

// ---------------------------------------------------------------------------
// 2. Find the UIRoot object(s) in memory.
// ---------------------------------------------------------------------------
var scanTimer = Stopwatch.StartNew();
Log("Scanning committed memory for UIRoot candidates (this can take 10-60s)...");

var rootAddresses = EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(process.Id);
scanTimer.Stop();

Log($"Found {rootAddresses.Count} UIRoot candidate address(es) in {scanTimer.Elapsed.TotalSeconds:F1}s.");
if (rootAddresses.Count == 0)
{
    Log("=> The memory scanner could not locate a 'UIRoot' Python type instance.");
    Log("   This usually means the client's memory layout changed, the client is");
    Log("   still on the character-select/login screen, or the process is 32-bit.");
    return 2;
}

// ---------------------------------------------------------------------------
// 3. Read the UI trees; pick the largest (the real, live one).
// ---------------------------------------------------------------------------
var reader = new EveOnline64.MemoryReaderFromLiveProcess(process.Id);

var trees = rootAddresses
    .Select(addr =>
    {
        var readTimer = Stopwatch.StartNew();
        var tree = EveOnline64.ReadUITreeFromAddress(addr, reader, 99);
        readTimer.Stop();
        var count = tree?.EnumerateSelfAndDescendants().Count() ?? 0;
        Log($"    root 0x{addr:X}: {count} nodes ({readTimer.Elapsed.TotalSeconds:F1}s).");
        return (addr, tree, count);
    })
    .Where(t => t.tree != null)
    .OrderByDescending(t => t.count)
    .ToList();

if (trees.Count == 0 || trees[0].tree == null)
{
    Log("=> Candidate addresses were found but no readable UI tree could be built.");
    return 2;
}

var largest = trees[0].tree!;
var allNodes = largest.EnumerateSelfAndDescendants().ToList();
Log("");
Log($"Selected largest UI tree at 0x{trees[0].addr:X}: {allNodes.Count} nodes total.");

// ---------------------------------------------------------------------------
// 4. Raw structure report: histogram of Python object type names.
//    This is the ground truth of "what the client exposes".
// ---------------------------------------------------------------------------
Log("");
Log("--- Node type histogram (top 40 by count) ---");
var histogram = allNodes
    .GroupBy(n => n.PythonObjectTypeName ?? "<null>")
    .Select(g => (Type: g.Key, Count: g.Count()))
    .OrderByDescending(g => g.Count)
    .ToList();

foreach (var (type, count) in histogram.Take(40))
    Log($"    {count,6}  {type}");
Log($"    ({histogram.Count} distinct type names in total.)");

// ---------------------------------------------------------------------------
// 5. Presence check for the key UI types the parser depends on.
//    A red line here points directly at what stopped being recognized.
// ---------------------------------------------------------------------------
Log("");
Log("--- Key UI type presence (types the parser looks for) ---");
var typeSet = allNodes.Select(n => n.PythonObjectTypeName).Where(t => t != null).ToHashSet()!;
string[][] keyTypes =
{
    new[] { "UIRoot" },
    new[] { "ShipUI" },
    new[] { "CapacitorContainer" },
    new[] { "ModuleButton" },
    new[] { "TargetInBar" },
    new[] { "InfoPanelContainer" },
    new[] { "InfoPanelLocationInfo" },
    new[] { "OverView", "OverviewWindow", "OverviewWindowOld" },
    new[] { "OverviewScrollEntry" },
    new[] { "DroneView", "DronesWindow" },
    new[] { "ActiveItem" },
    new[] { "Neocom" },
};
foreach (var group in keyTypes)
{
    var present = group.Where(t => typeSet.Contains(t)).ToList();
    var mark = present.Count > 0 ? "OK  " : "MISS";
    Log($"    [{mark}] {string.Join(" / ", group)}" +
        (present.Count > 0 ? $"  -> {string.Join(", ", present)}" : ""));
}

// ---------------------------------------------------------------------------
// 6. Run the actual high-level parser and report what it managed to extract.
//    This is the end-to-end answer: does the pipeline understand the client?
// ---------------------------------------------------------------------------
Log("");
Log("--- Parser output (high-level structures recognized) ---");
ParsedUserInterface? parsed = null;
try
{
    var withRegion = Parser.ParseUITreeWithDisplayRegionFromUITree(largest);
    parsed = Parser.ParseUserInterfaceFromUITree(withRegion);
}
catch (Exception e)
{
    Log($"    Parser threw: {e.GetType().Name}: {e.Message}");
}

if (parsed != null)
{
    var overviewEntries = parsed.WindowOverview?.SelectMany(w => w.Entries ?? new List<IOverviewEntry>()).Count() ?? 0;
    var droneGroups = parsed.WindowDroneView?.DroneGroups?.Count ?? 0;

    // The parser fills HitpointsPercent on the concrete ShipUi (not the interface's HitpointsAndEnergy).
    var hp = (parsed.ShipUi as Sanderling.Interface.MemoryStruct.ShipUi)?.HitpointsPercent;
    Report("ShipUi", parsed.ShipUi != null
        ? $"armor={hp?.Armor}% shield={hp?.Shield}% struct={hp?.Structure}% " +
          $"modules={parsed.ShipUi.ModuleButtons?.Count}"
        : null);
    Report("Current solar system", parsed.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName);
    Report("Targets locked", parsed.Target != null ? parsed.Target.Length.ToString() : null);
    Report("Overview windows", parsed.WindowOverview != null ? parsed.WindowOverview.Length.ToString() : null);
    Report("Overview entries", overviewEntries > 0 ? overviewEntries.ToString() : null);
    Report("Drone groups", droneGroups > 0 ? droneGroups.ToString() : null);
    Report("Context menus", parsed.Menu != null ? parsed.Menu.Length.ToString() : null);
    Report("Neocom", parsed.Neocom != null ? "present" : null);
    Report("Station window", parsed.WindowStation?.Length > 0 ? "present" : null);
    Report("Inventory windows", parsed.WindowInventory?.Length > 0 ? parsed.WindowInventory.Length.ToString() : null);
}

// ---------------------------------------------------------------------------
// 7. Optional: dump the full tree to JSON for offline inspection.
// ---------------------------------------------------------------------------
if (dumpPath != null)
{
    try
    {
        var json = JsonSerializer.Serialize(largest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(dumpPath, json);
        Log("");
        Log($"Wrote full UI tree JSON ({json.Length / 1024} KB) to {dumpPath}.");
    }
    catch (Exception e)
    {
        Log($"Failed to write dump: {e.Message}");
    }
}

Log("");
Log("=== Done ===");
return 0;

void Report(string label, string? value) =>
    Log($"    [{(value != null ? "OK  " : "MISS")}] {label}" + (value != null ? $": {value}" : ""));

static string SafeTitle(Process p)
{
    try { return p.MainWindowTitle; } catch { return "<unavailable>"; }
}
