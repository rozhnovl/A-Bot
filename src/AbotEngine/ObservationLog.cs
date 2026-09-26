using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using IMemoryMeasurement = Sanderling.Interface.MemoryStruct.IMemoryMeasurement;

namespace AbotEngine;

/// <summary>
/// Rich per-tick observation sink for a single client. Writes one JSON line per <see cref="ClientAgent.Step"/>
/// to a .jsonl file, capturing especially the SHIP STATE (hp / cap / speed / maneuver / module activity /
/// incoming EWAR), plus locked targets, on-grid overview entries, drones, and the bot's would-be plan.
///
/// It also derives an "events" list from deltas vs. the previous tick. In a dry-run the bot sends no input,
/// so those deltas are the OPERATOR's own manual actions surfaced through state changes — a new lock, a
/// module toggled on, a maneuver change, drones launched, a shield drop, incoming EWAR starting. That lets a
/// hand-played recording (e.g. a couple of Abyss runs) be compared afterwards against the bot's intended
/// scenario (the logged "botPlan").
/// </summary>
public sealed class ObservationLog : IDisposable
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly Regex ParenCount = new(@"\((\d+)\)", RegexOptions.Compiled);

    private readonly StreamWriter writer;
    private readonly object gate = new();
    private Prev? prev;

    public string Path { get; }

    public ObservationLog(string path)
    {
        Path = path;
        writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true,
        };
    }

    public void Write(IMemoryMeasurement parsed, AgentSnapshot snap)
    {
        lock (gate)
        {
            try { writer.WriteLine(JsonSerializer.Serialize(Build(parsed, snap), Json)); }
            catch (Exception e)
            {
                try { writer.WriteLine(JsonSerializer.Serialize(new { step = snap.StepIndex, error = e.Message }, Json)); }
                catch { /* never let logging crash the loop */ }
            }
        }
    }

    private object Build(IMemoryMeasurement parsed, AgentSnapshot snap)
    {
        var ship = parsed.ShipUi as ShipUi;
        var hp = ship?.HitpointsPercent;
        int? shield = hp?.Shield, armor = hp?.Armor, str = hp?.Structure;
        int? cap = ship?.Capacitor?.LevelFromPmarksPercent;
        // Speed and incoming-EWar stay in the log schema so hand-played comparisons keep their
        // columns, but the Eve64 parser does not fill the legacy members yet — always null/empty.
#pragma warning disable CS0618
        int? speedMs = ship?.SpeedMilli is long sm ? (int)(sm / 1000) : null;
#pragma warning restore CS0618
        var maneuver = ship?.Indication?.ManeuverType?.ToString() ?? (ship == null ? null : "None");

        var modules = ship?.ModuleButtons?.Select((m, i) => new ModuleObs
        {
            Slot = i,
            Active = m.IsActive,
            Weapon = m.ModuleInfo?.IsWeapon == true ? true : null,
            Busy = m.IsBusy ? true : null,
        }).ToList() ?? new List<ModuleObs>();
        var modulesActive = modules.Count(m => m.Active == true);

#pragma warning disable CS0618
        var incomingEwar = ship?.EWarElement?
            .Select(e => e?.EWarType).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).Distinct().ToList()
            ?? new List<string>();
#pragma warning restore CS0618

        var targets = parsed.Target?.Select(t => new TargetObs
        {
            Name = (t as ShipUiTarget)?.LabelText?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?.Trim(),
            DistM = t.Distance,
        }).ToList() ?? new List<TargetObs>();

        var enemies = (parsed.WindowOverview ?? Array.Empty<IWindowOverview>())
            .SelectMany(w => w?.Entries ?? new List<IOverviewEntry>())
            .Where(e => e != null)
            .Select(e =>
            {
                var stat = Sanderling.ABot.Bot.Configuration.NpcStats.Lookup(e.ObjectType ?? e.ObjectName);
                return new EnemyObs
                {
                    Name = e.ObjectName,
                    Type = e.ObjectType,
                    DistM = e.ObjectDistanceInMeters,
                    Targeting = Nz(e.CommonIndications?.Targeting),
                    AttackingMe = Nz(e.CommonIndications?.AttackingMe),
                    TargetedByMe = Nz(e.CommonIndications?.TargetedByMe),
                    JammingMe = Nz(e.CommonIndications?.IsJammingMe),
                    WarpDisrupt = Nz(e.CommonIndications?.IsWarpDisruptingMe),
                    Icons = e.RightAlignedIconsHints is { Count: > 0 } ic ? ic : null,
                    Ehp = stat?.EhpKin,
                    Dps = stat?.Dps,
                    Dmg = stat?.Dmg,
                    Ewar = stat?.Ewar is { Length: > 0 } ew ? ew : null,
                };
            })
            .ToList();

        var droneBay = DroneCount(parsed.WindowDroneView?.DroneGroupInBay?.Header?.MainText);
        var droneSpace = DroneCount(parsed.WindowDroneView?.DroneGroupInSpace?.Header?.MainText);
        var droneStatuses = parsed.WindowDroneView?.DroneGroupInSpace?.Children?
            .Select(d => d?.Entry?.MainText)
            .Select(StatusInParens).Where(s => s != null).Select(s => s!).Distinct().ToList();

        var cur = new Prev
        {
            Shield = shield, Armor = armor, Struct = str, Cap = cap, Maneuver = maneuver,
            ModulesActive = modulesActive, DroneSpace = droneSpace,
            TargetNames = targets.Select(t => t.Name ?? "?").ToHashSet(),
            EnemyNames = enemies.Select(e => e.Name ?? "?").ToHashSet(),
            Ewar = incomingEwar.ToHashSet(),
        };
        var events = Diff(prev, cur);
        prev = cur;

        return new
        {
            t = DateTime.Now.ToString("HH:mm:ss.fff"),
            step = snap.StepIndex,
            mode = snap.Mode,
            sys = snap.System,
            inSpace = ship != null,
            ship = ship == null ? null : new
            {
                shield, armor, str, cap, speedMs, maneuver,
                modulesActive, modules,
                incomingEwar = incomingEwar.Count > 0 ? incomingEwar : null,
            },
            targets = targets.Count > 0 ? targets : null,
            enemies = enemies.Count > 0 ? enemies : null,
            drones = new { bay = droneBay, space = droneSpace, statuses = droneStatuses },
            botPlan = snap.Intents is { Length: > 0 } ? snap.Intents : null,
            problems = snap.MotionProblems is { Length: > 0 } ? snap.MotionProblems : null,
            events = events.Count > 0 ? events : null,
        };
    }

    private static List<string> Diff(Prev? a, Prev cur)
    {
        var ev = new List<string>();
        if (a is null) return ev;

        if (a.Maneuver != cur.Maneuver) ev.Add($"maneuver {a.Maneuver}->{cur.Maneuver}");
        if (Changed(a.Shield, cur.Shield, 3)) ev.Add($"shield {a.Shield}->{cur.Shield}");
        if (Changed(a.Armor, cur.Armor, 3)) ev.Add($"armor {a.Armor}->{cur.Armor}");
        if (Changed(a.Struct, cur.Struct, 3)) ev.Add($"struct {a.Struct}->{cur.Struct}");
        if (Changed(a.Cap, cur.Cap, 5)) ev.Add($"cap {a.Cap}->{cur.Cap}");
        if (a.ModulesActive != cur.ModulesActive) ev.Add($"modulesActive {a.ModulesActive}->{cur.ModulesActive}");
        if (a.DroneSpace != cur.DroneSpace) ev.Add($"dronesInSpace {a.DroneSpace}->{cur.DroneSpace}");

        foreach (var n in cur.TargetNames.Except(a.TargetNames)) ev.Add($"lock+ {n}");
        foreach (var n in a.TargetNames.Except(cur.TargetNames)) ev.Add($"lock- {n}");
        foreach (var n in cur.EnemyNames.Except(a.EnemyNames)) ev.Add($"enemy+ {n}");
        foreach (var n in a.EnemyNames.Except(cur.EnemyNames)) ev.Add($"enemy- {n}");
        foreach (var n in cur.Ewar.Except(a.Ewar)) ev.Add($"ewar+ {n}");
        foreach (var n in a.Ewar.Except(cur.Ewar)) ev.Add($"ewar- {n}");
        return ev;
    }

    private static bool Changed(int? a, int? b, int minDelta) =>
        a != b && (a is null || b is null || Math.Abs(a.Value - b.Value) >= minDelta);

    private static bool Nz(bool? b) => b ?? false;

    private static int? DroneCount(string? caption)
    {
        if (caption is null) return null;
        var m = ParenCount.Match(caption);
        return m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : null;
    }

    private static string? StatusInParens(string? entryText)
    {
        if (entryText is null) return null;
        var m = ParenCount.Match(entryText);
        if (m.Success) return null; // "(5)" is a count, not a status
        var open = entryText.IndexOf('(');
        var close = entryText.LastIndexOf(')');
        if (open >= 0 && close > open)
            return Regex.Replace(entryText.Substring(open + 1, close - open - 1), "<.*?>", "").Trim();
        return null;
    }

    public void Dispose()
    {
        lock (gate) { try { writer.Flush(); writer.Dispose(); } catch { } }
    }

    private sealed class Prev
    {
        public int? Shield, Armor, Struct, Cap;
        public string? Maneuver;
        public int ModulesActive;
        public int? DroneSpace;
        public HashSet<string> TargetNames = new();
        public HashSet<string> EnemyNames = new();
        public HashSet<string> Ewar = new();
    }

    private sealed class ModuleObs
    {
        public int Slot { get; set; }
        public bool? Active { get; set; }
        public bool? Weapon { get; set; }
        public bool? Busy { get; set; }
    }

    private sealed class TargetObs
    {
        public string? Name { get; set; }
        public int? DistM { get; set; }
    }

    private sealed class EnemyObs
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int? DistM { get; set; }
        public bool Targeting { get; set; }
        public bool AttackingMe { get; set; }
        public bool TargetedByMe { get; set; }
        public bool JammingMe { get; set; }
        public bool WarpDisrupt { get; set; }
        public List<string>? Icons { get; set; }
        public long? Ehp { get; set; }
        public double? Dps { get; set; }
        public string? Dmg { get; set; }
        public string[]? Ewar { get; set; }
    }
}
