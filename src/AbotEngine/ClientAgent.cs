using System.Collections.Immutable;
using System.Diagnostics;
using Bib3.Geometrik;
using AbotEngine.Fleet;
using BotEngine.Interface;
using Eve64;
using PythonStructures;
using Sanderling.ABot;
using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using WindowsInput.Native;
using IMemoryReader = Eve64.IMemoryReader;
using OverviewEntry = Sanderling.Interface.MemoryStruct.IOverviewEntry;

namespace AbotEngine;

/// <summary>
/// The self-contained engine for ONE EVE client window: attach (with cached UIRoot
/// addresses), read + parse its UI, run the bot brain over it, and — in live mode —
/// execute the resulting input against the client. Each <see cref="Step"/> returns a
/// compact <see cref="AgentSnapshot"/> for observers.
///
/// It owns no cadence, no dashboard, no console — the caller drives it. That is what
/// makes it reusable: SingleRunner runs one in a loop and serves a dashboard;
/// FleetOrchestrator runs several (one per role) and shows a fleet view.
/// </summary>
public sealed class ClientAgent : IDisposable
{
    public int Pid { get; }
    public string Title { get; }
    public RunProfile Profile { get; }
    public string Role { get; private set; }
    public bool Live { get; }

    public AgentSnapshot Snapshot { get; private set; } = new();

    private readonly Process process;
    private readonly Bot bot;
    private readonly IMemoryReader reader;
    private readonly Stopwatch sw = Stopwatch.StartNew();
    private readonly Action<string>? trace;
    private readonly Action<IMemoryMeasurement, AgentSnapshot>? observe;
    private readonly Func<bool>? stopRequested;
    private readonly string modeLabel;
    private readonly bool monitorOnly;

    private IImmutableList<ulong>? roots;
    private ulong? pinnedRoot;
    // Type-object-pointer -> Python type name, reused across ticks (see Eve64 ReadUITreeFromAddress overload).
    private readonly Dictionary<ulong, string> typeNameCache = new();
    private long lastGoodParseTick;
    private long step;
    private string? lastSelectedTargetName;
    private long? lastSelectedTargetId;
    private long? laserTargetId;
    private readonly Dictionary<int, string> repairTargetByModuleIndex = new();
    private readonly Dictionary<string, long> lastModuleToggleAt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (bool DesiredActive, long IssuedAt)> pendingModuleToggles =
        new(StringComparer.OrdinalIgnoreCase);
    // Bridges only the short interval between our Ctrl-click and EVE publishing the
    // targeting indicator in the overview. After that, the UI is the source of truth.
    private readonly HashSet<long> pendingTargetLocks = new();
    private readonly HashSet<string> protectionActivationIssued = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> visitedAsteroidBelts = new(StringComparer.OrdinalIgnoreCase);
    private string lastOverlaySignature = "";
    private long moduleActivationSequence;

    // Mouse and keyboard input are global OS resources. A named mutex protects them not only
    // between the three ClientAgent instances, but also against another A-Bot runner process.
    private static readonly Mutex ScreenInputMutex = new(false, @"Local\A-Bot.ScreenInput");

    public ClientAgent(
        Process process,
        RunProfile profile,
        bool live,
        bool beltTest = false,
        string role = "",
        Action<string>? trace = null,
        Action<IMemoryMeasurement, AgentSnapshot>? observe = null,
        bool anomaly = false,
        bool monitorOnly = false,
        Func<bool>? stopRequested = null)
    {
        this.process = process;
        Pid = process.Id;
        Title = SafeTitle(process);
        Profile = profile;
        Live = live;
        Role = role;
        this.trace = trace;
        this.observe = observe;
        this.stopRequested = stopRequested;
        this.monitorOnly = monitorOnly;
        modeLabel = live ? "LIVE" : "dry-run";

        reader = new EveOnline64.MemoryReaderFromLiveProcess(Pid);
        bot = monitorOnly ? Bot.Monitor(profile)
            : anomaly ? Bot.Anomaly(profile)
            : beltTest ? Bot.BeltTest(profile)
            : new Bot(profile, role, Pid);
    }

    public bool IsAttached => roots is { Count: > 0 };
    public int VisitedAsteroidBeltCount => visitedAsteroidBelts.Count;

    /// <summary>
    /// Identify the known 2x Retribution + Deacon hull instance by its fitted-module signature.
    /// EVE window titles can be temporarily blank after a client restart, so PID ordering is not
    /// a safe role assignment. These signatures deliberately describe the operator's three
    /// calibrated ships: Tiara's T2 remote reps, Gil's abyssal heat sinks, and Fenreire's T2
    /// heat sinks.
    /// </summary>
    public string? AutoAssignRetributionDeaconRole()
    {
        var parsed = ReadAndParse();
        if (parsed?.ShipUi is not ShipUi ship) return null;
        var typeIds = (ship.ModuleButtons ?? new List<ShipUIModuleButton>())
            .Select(m => m.ModuleInfo?.ModuleId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var detected = typeIds.Contains(26912) ? "deacon"
            : typeIds.Contains(3033) && typeIds.Contains(16435) && typeIds.Contains(49726) ? "tank-retri"
            : typeIds.Contains(3033) && typeIds.Contains(16435) && typeIds.Contains(2364) ? "wing-retri"
            : null;
        if (detected is not null)
        {
            Role = detected;
            // Fenreire's first-session Ctrl+F1 attempt did not produce ramp_active, while
            // lower-circle clicks are calibrated. Start this client on the click half of the
            // alternating hotkey/click sequence; the other two clients still start on hotkey.
            if (detected == "wing-retri" && moduleActivationSequence == 0)
                moduleActivationSequence = 1;
        }
        return detected;
    }

    /// <summary>
    /// Locate the client's UIRoot addresses: reuse the on-disk cache when it still reads a
    /// live tree, otherwise run the (slow) memory scan and cache the result. Returns whether
    /// the agent is attached.
    /// </summary>
    public bool Attach(bool forceRescan = false)
    {
        var cached = forceRescan ? null : RootCache.Load(process);
        if (cached is { Count: > 0 } && TreeReadable(cached))
        {
            roots = cached;
            trace?.Invoke($"pid {Pid}: reused {roots.Count} cached UIRoot address(es).");
            return true;
        }

        if (cached is { Count: > 0 })
            trace?.Invoke($"pid {Pid}: cached UIRoot addresses stale; rescanning.");

        var scan = Stopwatch.StartNew();
        roots = EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(Pid);
        scan.Stop();
        trace?.Invoke($"pid {Pid}: found {roots.Count} UIRoot candidate(s) in {scan.Elapsed.TotalSeconds:F1}s.");

        if (roots.Count == 0) return false;
        RootCache.Save(process, roots);
        return true;
    }

    /// <summary>Run one perceive -> decide -> (optionally) act iteration. Updates <see cref="Snapshot"/>.</summary>
    public AgentSnapshot Step()
    {
        var t0 = sw.ElapsedMilliseconds;
        var parsed = ReadAndParse();

        if (parsed is null)
        {
            Snapshot = Snapshot with
            {
                StepIndex = step,
                Mode = modeLabel,
                Profile = Profile.Name,
                Role = Role,
                Pid = Pid,
                Title = Title,
                ParseOk = false,
                ReadyOk = false,
                Readiness = Array.Empty<ReadyItem>(),
                UpdatedAtMs = Environment.TickCount64,
                LastGoodParseMs = lastGoodParseTick,
                UptimeSec = sw.Elapsed.TotalSeconds,
                Intents = Array.Empty<string>(),
                StrategyStatus = ErrorStatus("UI perception", "UI read/parse returned nothing"),
                MotionCount = 0,
                Executed = false,
                Overview = Array.Empty<AgentOverviewEntity>(),
                MotionProblems = Array.Empty<string>(),
                LastError = "UI read/parse returned nothing",
            };
            step++;
            return Snapshot;
        }
        lastGoodParseTick = Environment.TickCount64;

        var nowMs = sw.ElapsedMilliseconds;
        var input = new BotStepInput
        {
            TimeMilli = nowMs,
            FromProcessMemoryMeasurement =
                new FromProcessMeasurement<IMemoryMeasurement>(parsed, t0, nowMs, Pid),
            StepLastMotionResult = null,
        };

        BotStepResult result;
        try { result = bot.Step(input); }
        catch (Exception e)
        {
            Snapshot = Snapshot with
            {
                StepIndex = step, Mode = modeLabel, Profile = Profile.Name, Role = Role,
                Pid = Pid, Title = Title, ParseOk = true,
                UpdatedAtMs = Environment.TickCount64, LastGoodParseMs = lastGoodParseTick,
                UptimeSec = sw.Elapsed.TotalSeconds, LastError = $"bot.Step: {e.Message}",
                Intents = Array.Empty<string>(), StrategyStatus = ErrorStatus("Bot.Step", e.Message),
                MotionCount = 0, Executed = false,
            };
            step++;
            return Snapshot;
        }

        var motions = result.ListMotion ?? Array.Empty<MotionRecommendation>();
        var intents = DiagnosticMessages(result.OutputListTaskPath).ToArray();

        var executed = false;
        var motionProblems = Array.Empty<string>();
        if (Live && motions.Length > 0)
        {
            motionProblems = ExecuteMotions(motions, parsed).ToArray();
            executed = true;
        }

        var hp = (parsed.ShipUi as ShipUi)?.HitpointsPercent;
        var readiness = monitorOnly
            ? new ReadinessReport(new[]
            {
                new ReadinessItem("Fleet rehearsal", true, "monitor-only; fit actuation intentionally disabled"),
            })
            : ReadinessCheck.Evaluate(parsed, Profile);

        var overview = (parsed.WindowOverview ?? Array.Empty<IWindowOverview>())
            .SelectMany(w => w?.Entries ?? new List<OverviewEntry>())
            .Where(e => e?.ObjectDistanceInMeters is not null)
            .Select(e =>
            {
                var stat = NpcStats.Lookup(e.ObjectType ?? e.ObjectName);
                var lockedTarget = FindTarget(
                    parsed.Target ?? Array.Empty<IShipUiTarget>(),
                    e.ObjectName ?? e.ObjectType ?? "",
                    e.ObjectDistanceInMeters);
                var color = e.IconSpriteColorPercent;
                var isEnemy = color is not null &&
                              color.BPercent < color.RPercent / 3 &&
                              color.GPercent < color.RPercent / 3 &&
                              color.RPercent > 80;
                return new AgentOverviewEntity
                {
                    Id = e.Id,
                    Name = e.ObjectName ?? e.ObjectType ?? "",
                    Type = e.ObjectType ?? e.ObjectName ?? "",
                    DistanceMeters = e.ObjectDistanceInMeters ?? 0,
                    IsEnemy = isEnemy,
                    TargetingMe = e.CommonIndications?.Targeting ?? false,
                    AttackingMe = e.CommonIndications?.AttackingMe ?? false,
                    TargetedByMe = e.CommonIndications?.TargetedByMe ?? false,
                    EstimatedEhp = stat?.EhpTh ?? stat?.EhpOmni ?? 0,
                    Defense = ToDefenseProfile(stat, lockedTarget?.Hitpoints),
                    ApproxDps = (stat?.Dps ?? 0) * (stat?.Ramp == true ? 2 : 1),
                    Ewar = stat?.Ewar ?? Array.Empty<string>(),
                };
            })
            .ToArray();
        var hostileAttackers = overview.Where(e => e.IsEnemy && e.AttackingMe).ToArray();

        Snapshot = new AgentSnapshot
        {
            StepIndex = step,
            Mode = modeLabel,
            Profile = Profile.Name,
            Role = Role,
            Pid = Pid,
            Title = Title,
            ParseOk = true,
            ReadyOk = readiness.AllOk,
            Readiness = readiness.Items.Select(r => new ReadyItem(r.Name, r.Ok, r.Detail)).ToArray(),
            UpdatedAtMs = Environment.TickCount64,
            LastGoodParseMs = lastGoodParseTick,
            UptimeSec = sw.Elapsed.TotalSeconds,
            System = parsed.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName,
            Armor = hp?.Armor,
            Shield = hp?.Shield,
            Struct = hp?.Structure,
            Capacitor = (parsed.ShipUi as ShipUi)?.Capacitor?.LevelFromPmarksPercent,
            IncomingDps = (int)Math.Round(hostileAttackers.Sum(e => e.ApproxDps)),
            Attackers = hostileAttackers.Length,
            TargetsLocked = parsed.Target?.Length ?? 0,
            LaserChargeTypeId = (parsed.ShipUi as ShipUi)?.ModuleButtons?
                .FirstOrDefault(m => m.ModuleInfo?.ModuleId == 3033)?.ModuleInfo?.ChargeTypeId,
            OverviewEntries = parsed.WindowOverview?.Sum(w => w.Entries?.Count ?? 0) ?? 0,
            InSpace = parsed.ShipUi != null,
            Maneuver = (parsed.ShipUi as ShipUi)?.Indication?.ManeuverType?.ToString(),
            Overview = overview,
            StrategyStatus = ToAgentStatus(result.StrategyStatus),
            Intents = intents,
            MotionCount = motions.Length,
            Executed = executed,
            MotionProblems = motionProblems,
            LastError = result.Exception?.Message,
        };
        try { observe?.Invoke(parsed, Snapshot); } catch { /* observation must never break the loop */ }
        step++;
        return Snapshot;
    }

    // --- manual control surface (used by the AbotMcp server) -------------------------

    /// <summary>Read + parse the UI once WITHOUT running the bot brain. Null when the read fails.</summary>
    public ParsedUserInterface? Perceive() => ReadAndParse();

    /// <summary>Read the raw UI tree with display regions (to search windows the parser does not model yet).</summary>
    public UITreeNodeWithDisplayRegion? PerceiveRawWithRegion()
    {
        try
        {
            var raw = ReadRawTree();
            return raw is null ? null : Parser.ParseUITreeWithDisplayRegionFromUITree(raw);
        }
        catch (Exception e)
        {
            trace?.Invoke($"pid {Pid}: raw read error: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Execute operator-composed motions against the client through the same path the bot uses
    /// (screen-input lease, occlusion model, emergency stop). In dry-run mode nothing is sent.
    /// </summary>
    public string ExecuteManual(IEnumerable<MotionParam> motions, ParsedUserInterface parsed, string action)
    {
        var list = motions.Where(m => m is not null).Select(m => m.AsRecommendation()).ToArray();
        if (list.Length == 0) return $"{action}: no motion";
        if (!Live) return $"dry-run (no input sent): {action}";
        if (StopRequested) return $"{action}: emergency stop";
        var problems = ExecuteMotions(list, parsed).ToArray();
        return problems.Length == 0 ? $"ok: {action}" : $"{action}: {string.Join("; ", problems)}";
    }

    /// <summary>Native window handle of the client (IntPtr.Zero when unavailable).</summary>
    public IntPtr MainWindowHandle
    {
        get => WindowHandleResolver.Resolve(process, trace);
    }

    /// <summary>Read the largest raw UI tree and write it to <paramref name="path"/> as JSON (diagnostic).</summary>
    public void DumpRawTree(string path)
    {
        if (roots is null) return;
        try
        {
            var largest = ReadRawTree();
            if (largest is null) { trace?.Invoke("raw tree dump: no tree"); return; }
            var json = System.Text.Json.JsonSerializer.Serialize(largest,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(path, json);
            trace?.Invoke($"raw tree dumped ({json.Length / 1024} KB) to {path}");
        }
        catch (Exception e) { trace?.Invoke($"raw tree dump failed: {e.Message}"); }
    }

    /// <summary>
    /// One-shot operator inspection action: open the Inventory window, without running any strategy.
    /// Used while docked to discover the active ship/hangar contents before a live fleet launch.
    /// </summary>
    public string OpenInventoryForInspection()
    {
        if (!IsAttached) return "not attached";
        var parsed = ReadAndParse();
        if (parsed is null) return "UI read/parse returned nothing";
        var button = parsed.Neocom?.InventoryButton;
        if (button is null) return "Neocom inventory button not found";

        var motion = button.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation();
        var problems = ExecuteMotions(new[] { motion }, parsed).ToArray();
        return problems.Length == 0 ? "inventory opened" : string.Join("; ", problems);
    }

    /// <summary>
    /// One-shot launch calibration: click only the station Undock button. No strategy is evaluated and
    /// no combat/module motion can be emitted by this method.
    /// </summary>
    public string UndockForCalibration()
    {
        if (!IsAttached) return "not attached";
        var parsed = ReadAndParse();
        if (parsed is null) return "UI read/parse returned nothing";
        if (parsed.ShipUi is not null) return "already in space";
        var station = parsed.WindowStation?.FirstOrDefault();
        if (station is null) return "station window not found (session transition?)";
        var button = station.UndockButton;
        if (button is null) return "station Undock button not found (already undocking?)";

        var motion = button.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation();
        var problems = ExecuteMotions(new[] { motion }, parsed).ToArray();
        return problems.Length == 0 ? "undock clicked" : string.Join("; ", problems);
    }

    /// <summary>
    /// One-shot navigation calibration: open the Probe Scanner with EVE's default Alt+P shortcut.
    /// This deliberately performs no scan-result selection and cannot initiate warp.
    /// </summary>
    public string OpenProbeScannerForCalibration()
    {
        if (!IsAttached) return "not attached";
        var parsed = ReadAndParse();
        if (parsed is null) return "UI read/parse returned nothing";
        if (parsed.ShipUi is null) return "ship is not in space";

        var motions = new HotkeyTask(VirtualKeyCode.VK_P, VirtualKeyCode.MENU)
            .ClientActions?.ToArray() ?? Array.Empty<MotionRecommendation>();
        var problems = ExecuteMotions(motions, parsed).ToArray();
        return problems.Length == 0 ? "Alt+P sent" : string.Join("; ", problems);
    }

    /// <summary>
    /// One-shot navigation calibration: right-click a known clear part of the in-space viewport.
    /// It opens only the first context-menu level and cannot select or initiate warp.
    /// </summary>
    public string OpenSpaceContextMenuForCalibration()
    {
        if (!IsAttached) return "not attached";
        var parsed = ReadAndParse();
        if (parsed is null) return "UI read/parse returned nothing";
        if (parsed.ShipUi is null) return "ship is not in space";

        // Current clients are 1374x773. This patch is above the ship HUD, left of the overview,
        // and outside the chat/route panels. A tiny absolute region avoids clicking a bracket.
        var clearSpace = new UIElement
        {
            Region = new RectInt(480, 180, 490, 190),
            InTreeIndex = int.MaxValue,
            ChildLastInTreeIndex = int.MaxValue,
        };
        var motion = clearSpace.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right).AsRecommendation();
        var problems = ExecuteMotions(new[] { motion }, parsed).ToArray();
        return problems.Length == 0 ? "space context menu opened" : string.Join("; ", problems);
    }

    /// <summary>
    /// Open the grouped beam-laser context menu and report every visible entry. This calibration
    /// action cannot fire the weapon or choose a charge; it is used to learn the exact menu labels
    /// exposed by the current EVE client before enabling crystal reload actuation.
    /// </summary>
    public string InspectLaserCrystalMenuForCalibration()
    {
        if (!Live) return "live mode required to open the laser menu";
        if (!IsAttached) return "not attached";
        var parsed = ReadAndParse();
        if (parsed?.ShipUi is not ShipUi ship) return "ship is not in space";
        var laser = ModuleByType(ship, 3033);
        if (laser is null) return $"{ModuleTypes.NameOrId(3033)} not found";

        var open = ExecuteOne(
            laser.UINode.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right).AsRecommendation(),
            parsed,
            "open laser context menu");
        if (open.Contains(": ", StringComparison.Ordinal)) return open;
        Thread.Sleep(500);

        var withMenu = ReadAndParse();
        return withMenu is null
            ? $"charge={laser.ModuleInfo?.ChargeTypeId?.ToString() ?? "unknown"}; menu parse failed"
            : $"charge={laser.ModuleInfo?.ChargeTypeId?.ToString() ?? "unknown"}; {MenuSnapshot(withMenu)}";
    }

    /// <summary>Read the grouped laser's loaded charge without running the ship strategy.</summary>
    public int? ReadLaserChargeTypeIdForCalibration()
    {
        var parsed = IsAttached ? ReadAndParse() : null;
        return (parsed?.ShipUi as ShipUi)?.ModuleButtons?
            .FirstOrDefault(m => m.ModuleInfo?.ModuleId == 3033)?.ModuleInfo?.ChargeTypeId;
    }

    /// <summary>
    /// Safely exercise the crystal UI with no combat target. The caller supplies a pure selector
    /// result; this method only reloads the grouped laser and confirms success from ChargeTypeId.
    /// </summary>
    public string ActuateLaserCrystalForCalibration(LaserCrystal desired, int maxActions = 12)
    {
        if (!Live) return "live mode required for crystal calibration";
        if (StopRequested) return "emergency stop";
        if (!IsAttached) return "not attached";
        if (desired == LaserCrystal.KeepCurrent) return "no crystal requested";

        var leaseHeld = false;
        try
        {
            try { leaseHeld = ScreenInputMutex.WaitOne(TimeSpan.FromSeconds(5)); }
            catch (AbandonedMutexException) { leaseHeld = true; }
            if (!leaseHeld) return "screen input lease timeout";

            var actions = new List<string>();
            for (var attempt = 0; attempt < Math.Max(1, maxActions) && !StopRequested; attempt++)
            {
                var parsed = ReadAndParse();
                if (parsed?.ShipUi is not ShipUi ship) return "ship is not in space";
                var hostiles = (parsed.WindowOverview ?? Array.Empty<IWindowOverview>())
                    .SelectMany(w => w?.Entries ?? new List<OverviewEntry>())
                    .Where(e => e is not null && IsHostileOverviewEntry(e))
                    .ToArray();
                if (hostiles.Length > 0)
                    return $"calibration refused: {hostiles.Length} hostile overview row(s) present";

                var laser = ModuleByType(ship, 3033);
                if (laser is null) return $"{ModuleTypes.NameOrId(3033)} not found";
                var desiredTypeId = CrystalTypeId(desired);
                if (laser.ModuleInfo?.ChargeTypeId == desiredTypeId)
                    return $"crystal confirmed: {desired} type={desiredTypeId}" +
                           (actions.Count == 0 ? " (already loaded)" : $" ({string.Join(" -> ", actions)})");
                if (laser.IsActive == true)
                    return "calibration refused: grouped laser is active";

                var action = RequestLaserCrystal(laser, parsed, desired);
                actions.Add(action);
                if (action.Contains("blocked", StringComparison.OrdinalIgnoreCase) ||
                    action.Contains(": ", StringComparison.Ordinal))
                    return string.Join(" -> ", actions);

                Thread.Sleep(action.StartsWith("load crystal", StringComparison.OrdinalIgnoreCase) ? 500 : 250);
            }
            return StopRequested
                ? "emergency stop"
                : $"crystal not confirmed: {desired} ({string.Join(" -> ", actions)})";
        }
        finally
        {
            if (leaseHeld) ScreenInputMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Select the first Serpentis combat anomaly through the in-space context menu. A personal warp is
    /// deliberately not used as a fallback: the final action must explicitly be a fleet warp.
    /// </summary>
    public string ActuateNextSerpentisAnomalyBurst(int maxActions = 8)
    {
        if (!Live) return "dry-run";
        if (StopRequested) return "emergency stop";
        if (!IsAttached) return "not attached";

        var leaseHeld = false;
        try
        {
            try { leaseHeld = ScreenInputMutex.WaitOne(TimeSpan.FromSeconds(5)); }
            catch (AbandonedMutexException) { leaseHeld = true; }
            if (!leaseHeld) return "screen input lease timeout";

            var results = new List<string>();
            var hoveredSite = false;
            for (var i = 0; i < Math.Max(1, maxActions) && !StopRequested; i++)
            {
                var parsed = ReadAndParse();
                if (parsed?.ShipUi is null) return "ship UI unavailable";
                var menus = (parsed.Menu ?? Array.Empty<IMenu>())
                    .Where(m => m?.Entry?.Any() == true).ToArray();

                if (menus.Length == 0)
                {
                    var clearSpace = new UIElement
                    {
                        Region = new RectInt(480, 180, 490, 190),
                        InTreeIndex = int.MaxValue,
                        ChildLastInTreeIndex = int.MaxValue,
                    };
                    results.Add(ExecuteOne(
                        clearSpace.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right).AsRecommendation(),
                        parsed, "open space menu"));
                }
                else
                {
                    var anomalies = menus[0].Entry.FirstOrDefault(e =>
                        string.Equals(e?.Text?.Trim(), "Anomalies", StringComparison.OrdinalIgnoreCase));
                    if (anomalies is null)
                        return $"navigation blocked: Anomalies entry absent ({string.Join(" | ", results)})";

                    if (menus.Length < 2)
                    {
                        results.Add(ExecuteOne(
                            anomalies.MouseClick(BotEngine.Motor.MouseButtonIdEnum.None).AsRecommendation(),
                            parsed, "hover Anomalies"));
                    }
                    else
                    {
                        var site = menus[1].Entry.FirstOrDefault(e =>
                            e?.Text?.Contains("Serpentis", StringComparison.OrdinalIgnoreCase) == true);
                        if (site is null)
                            return $"no Serpentis anomaly in space menu ({string.Join(" | ", results)})";
                        var siteName = site.Text?.Trim() ?? "Serpentis anomaly";

                        if (menus.Length < 3)
                        {
                            if (hoveredSite && site.HighlightVisible == true)
                                return $"navigation blocked: {siteName} has no fleet-warp submenu";
                            hoveredSite = true;
                            results.Add(ExecuteOne(
                                site.MouseClick(BotEngine.Motor.MouseButtonIdEnum.None).AsRecommendation(),
                                parsed, $"hover {siteName}"));
                        }
                        else
                        {
                            // Current EVE clients expose Warp Fleet (Point) as another submenu.
                            // Hover it first and only report success after clicking the final range.
                            // This avoids treating "submenu opened" as a warp request.
                            var zeroRange = menus.Skip(3).SelectMany(m => m.Entry)
                                .FirstOrDefault(e => string.Equals(
                                    CleanOverlayText(e?.Text), "Within 0 m",
                                    StringComparison.OrdinalIgnoreCase));
                            if (zeroRange is not null)
                            {
                                var click = ExecuteOne(
                                    zeroRange.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation(),
                                    parsed, $"warp fleet to {siteName} at 0 m");
                                return click.Contains(": ", StringComparison.Ordinal)
                                    ? click
                                    : $"warp fleet requested: {siteName}";
                            }

                            // Search every visible deeper level because EVE keeps parent menus alive
                            // while opening the child.
                            var fleetWarpEntries = menus.Skip(2).SelectMany(m => m.Entry)
                                .Where(e => e?.Text?.Contains(
                                    "Warp Fleet", StringComparison.OrdinalIgnoreCase) == true)
                                .ToArray();
                            var fleetWarpToWithin = fleetWarpEntries.FirstOrDefault(e =>
                                CleanOverlayText(e?.Text).Contains(
                                    "to Within", StringComparison.OrdinalIgnoreCase));
                            if (fleetWarpToWithin is not null)
                            {
                                results.Add(ExecuteOne(
                                    fleetWarpToWithin.MouseClick(BotEngine.Motor.MouseButtonIdEnum.None)
                                        .AsRecommendation(),
                                    parsed, $"hover Warp Fleet to Within for {siteName}"));
                                goto SettleSerpentisMenu;
                            }

                            var directFleetWarp = fleetWarpEntries.FirstOrDefault();
                            if (directFleetWarp is null)
                                return $"navigation blocked: no Warp Fleet action for {siteName}; {MenuSnapshot(parsed)}";

                            var directClick = ExecuteOne(
                                directFleetWarp.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left)
                                    .AsRecommendation(),
                                parsed, $"direct warp fleet to {siteName}");
                            return directClick.Contains(": ", StringComparison.Ordinal)
                                ? directClick
                                : $"warp fleet requested: {siteName}";
                        }
                    }
                }

                SettleSerpentisMenu:
                for (var settle = 0; settle < 7 && !StopRequested; settle++)
                    Thread.Sleep(50);
            }
            return StopRequested ? "emergency stop" : string.Join(" -> ", results);
        }
        finally
        {
            if (leaseHeld) ScreenInputMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Visit each asteroid belt exposed by the in-space context menu once. The route is entirely
    /// UI-driven: right-click empty space, hover Asteroid Belts, hover the first unvisited belt,
    /// then require the explicit Warp Fleet action. Completed belt names are retained for this
    /// agent lifetime, so an empty/cleared belt cannot become an endless first-entry loop.
    /// </summary>
    public string ActuateNextAsteroidBeltBurst(int maxActions = 8)
    {
        if (!Live) return "dry-run";
        if (StopRequested) return "emergency stop";
        if (!IsAttached) return "not attached";

        var leaseHeld = false;
        try
        {
            try { leaseHeld = ScreenInputMutex.WaitOne(TimeSpan.FromSeconds(5)); }
            catch (AbandonedMutexException) { leaseHeld = true; }
            if (!leaseHeld) return "screen input lease timeout";

            var results = new List<string>();
            var hoveredBelt = false;
            for (var i = 0; i < Math.Max(1, maxActions) && !StopRequested; i++)
            {
                var parsed = ReadAndParse();
                if (parsed?.ShipUi is null) return "ship UI unavailable";
                var menus = (parsed.Menu ?? Array.Empty<IMenu>())
                    .Where(m => m?.Entry?.Any() == true).ToArray();

                if (menus.Length == 0)
                {
                    var clearSpace = new UIElement
                    {
                        Region = new RectInt(480, 180, 490, 190),
                        InTreeIndex = int.MaxValue,
                        ChildLastInTreeIndex = int.MaxValue,
                    };
                    results.Add(ExecuteOne(
                        clearSpace.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right).AsRecommendation(),
                        parsed, "open space menu"));
                }
                else
                {
                    var asteroidMenu = menus[0].Entry.FirstOrDefault(e =>
                        CleanOverlayText(e?.Text).StartsWith("Asteroid", StringComparison.OrdinalIgnoreCase));
                    if (asteroidMenu is null)
                        return $"navigation blocked: Asteroid entry absent; {MenuSnapshot(parsed)}";

                    if (menus.Length < 2)
                    {
                        results.Add(ExecuteOne(
                            asteroidMenu.MouseClick(BotEngine.Motor.MouseButtonIdEnum.None).AsRecommendation(),
                            parsed, $"hover {CleanOverlayText(asteroidMenu.Text)}"));
                    }
                    else
                    {
                        var beltEntries = menus[1].Entry
                            .Where(e => CleanOverlayText(e?.Text)
                                .Contains("Asteroid Belt", StringComparison.OrdinalIgnoreCase))
                            .ToArray();
                        if (beltEntries.Length == 0)
                            return $"navigation blocked: no asteroid belts in submenu; {MenuSnapshot(parsed)}";

                        var belt = beltEntries.FirstOrDefault(e =>
                            !visitedAsteroidBelts.Contains(CleanOverlayText(e?.Text)));
                        if (belt is null)
                            return $"all asteroid belts visited ({visitedAsteroidBelts.Count})";
                        var beltName = CleanOverlayText(belt.Text);

                        if (menus.Length < 3)
                        {
                            if (hoveredBelt && belt.HighlightVisible == true)
                                return $"navigation blocked: {beltName} has no fleet-warp submenu";
                            hoveredBelt = true;
                            results.Add(ExecuteOne(
                                belt.MouseClick(BotEngine.Motor.MouseButtonIdEnum.None).AsRecommendation(),
                                parsed, $"hover {beltName}"));
                        }
                        else
                        {
                            var zeroRange = menus.Skip(3).SelectMany(m => m.Entry)
                                .FirstOrDefault(e => string.Equals(
                                    CleanOverlayText(e?.Text), "Within 0 m",
                                    StringComparison.OrdinalIgnoreCase));
                            if (zeroRange is not null)
                            {
                                var click = ExecuteOne(
                                    zeroRange.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation(),
                                    parsed, $"warp fleet to {beltName} at 0 m");
                                if (click.Contains(": ", StringComparison.Ordinal)) return click;

                                visitedAsteroidBelts.Add(beltName);
                                return $"warp fleet requested: {beltName}";
                            }

                            var fleetWarpEntries = menus.Skip(2).SelectMany(m => m.Entry)
                                .Where(e => CleanOverlayText(e?.Text)
                                    .Contains("Warp Fleet", StringComparison.OrdinalIgnoreCase))
                                .ToArray();
                            var fleetWarpToWithin = fleetWarpEntries.FirstOrDefault(e =>
                                CleanOverlayText(e?.Text).Contains(
                                    "to Within", StringComparison.OrdinalIgnoreCase));
                            if (fleetWarpToWithin is not null)
                            {
                                results.Add(ExecuteOne(
                                    fleetWarpToWithin.MouseClick(BotEngine.Motor.MouseButtonIdEnum.None)
                                        .AsRecommendation(),
                                    parsed, $"hover Warp Fleet to Within for {beltName}"));
                                goto SettleBeltMenu;
                            }

                            var directFleetWarp = fleetWarpEntries.FirstOrDefault();
                            if (directFleetWarp is null)
                                return $"navigation blocked: no Warp Fleet action for {beltName}; {MenuSnapshot(parsed)}";

                            var directClick = ExecuteOne(
                                directFleetWarp.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left)
                                    .AsRecommendation(),
                                parsed, $"direct warp fleet to {beltName}");
                            if (directClick.Contains(": ", StringComparison.Ordinal)) return directClick;

                            visitedAsteroidBelts.Add(beltName);
                            return $"warp fleet requested: {beltName}";
                        }
                    }
                }

                SettleBeltMenu:
                for (var settle = 0; settle < 7 && !StopRequested; settle++)
                    Thread.Sleep(50);
            }
            return StopRequested ? "emergency stop" : string.Join(" -> ", results);
        }
        finally
        {
            if (leaseHeld) ScreenInputMutex.ReleaseMutex();
        }
    }

    /// <summary>Right-click the fleet commander header in the Fleet window and invoke Regroup.</summary>
    public string ActuateFleetRegroupBurst(string commanderName, int maxActions = 5)
    {
        if (!Live) return "dry-run";
        if (StopRequested) return "emergency stop";
        if (!IsAttached) return "not attached";

        var leaseHeld = false;
        try
        {
            try { leaseHeld = ScreenInputMutex.WaitOne(TimeSpan.FromSeconds(5)); }
            catch (AbandonedMutexException) { leaseHeld = true; }
            if (!leaseHeld) return "screen input lease timeout";

            for (var i = 0; i < Math.Max(1, maxActions) && !StopRequested; i++)
            {
                var parsed = ReadAndParse();
                if (parsed?.ShipUi is null) return "ship UI unavailable";
                var regroup = (parsed.Menu ?? Array.Empty<IMenu>())
                    .SelectMany(m => m?.Entry ?? Array.Empty<IMenuEntry>())
                    .FirstOrDefault(e => e?.Text?.Contains("Regroup", StringComparison.OrdinalIgnoreCase) == true);
                if (regroup is not null)
                    return ExecuteOne(
                        regroup.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation(),
                        parsed, "fleet Regroup");

                var raw = ReadRawTree();
                if (raw is null) return "fleet regroup: UI tree unavailable";
                var displayTree = Parser.ParseUITreeWithDisplayRegionFromUITree(raw);
                var commanderHeader = new[] { displayTree }.Concat(displayTree.ListDescendantsWithDisplayRegion())
                    .FirstOrDefault(n => n.UiNode.PythonObjectTypeName == "FleetHeader" &&
                        string.Equals(
                            Parser.GetStringPropertyFromDictEntries("commanderName", n.UiNode),
                            commanderName,
                            StringComparison.OrdinalIgnoreCase));
                var commanderElement = commanderHeader.AsUiElement();
                if (commanderElement is null)
                    return $"fleet regroup: commander header {commanderName} not found";

                var open = ExecuteOne(
                    commanderElement.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right).AsRecommendation(),
                    parsed, $"open fleet menu on {commanderName}");
                if (open.Contains(": ", StringComparison.Ordinal)) return open;
                for (var settle = 0; settle < 7 && !StopRequested; settle++)
                    Thread.Sleep(50);
            }
            return StopRequested ? "emergency stop" : "fleet regroup: Regroup action not found";
        }
        finally
        {
            if (leaseHeld) ScreenInputMutex.ReleaseMutex();
        }
    }

    /// <summary>
    /// Execute at most one doctrine action against the current UI. The caller supplies the canonical
    /// primary name because overview object ids are client-local. Remote repairs are target-verified and
    /// are never activated unless the brain explicitly assigned them after observed armor damage.
    /// </summary>
    public string ActuateRetributionDeaconBurst(
        RetributionDeaconOrders order,
        string? primaryTargetName,
        int? primaryTargetDistanceMeters,
        IReadOnlyDictionary<int, string> fleetMemberNames,
        int maxActions = 4)
    {
        if (!Live) return "dry-run";
        if (StopRequested) return "emergency stop";
        var leaseHeld = false;
        try
        {
            try { leaseHeld = ScreenInputMutex.WaitOne(TimeSpan.FromSeconds(5)); }
            catch (AbandonedMutexException) { leaseHeld = true; }
            if (!leaseHeld) return "screen input lease timeout";

            var results = new List<string>();
            for (var i = 0; i < Math.Max(1, maxActions); i++)
            {
                if (StopRequested)
                {
                    results.Add("emergency stop");
                    break;
                }
                var result = ActuateRetributionDeaconOrder(
                    order, primaryTargetName, primaryTargetDistanceMeters, fleetMemberNames);
                results.Add(result);
                if (!LooksLikeExecutedDoctrineAction(result)) break;

                // Hotkeys finish faster than EVE updates the module/target tree. Keep the same window
                // focused and let the new state settle before deciding the next action in this burst.
                for (var settle = 0; settle < 5 && !StopRequested; settle++)
                    Thread.Sleep(50);
            }
            return string.Join(" -> ", results);
        }
        finally
        {
            if (leaseHeld) ScreenInputMutex.ReleaseMutex();
        }
    }

    public string ActuateRetributionDeaconOrder(
        RetributionDeaconOrders order,
        string? primaryTargetName,
        int? primaryTargetDistanceMeters,
        IReadOnlyDictionary<int, string> fleetMemberNames)
    {
        if (!Live) return "dry-run";
        if (StopRequested) return "emergency stop";
        if (!IsAttached) return "not attached";
        var parsed = ReadAndParse();
        if (parsed?.ShipUi is not ShipUi ship) return "ship UI unavailable";

        var overview = (parsed.WindowOverview ?? Array.Empty<IWindowOverview>())
            .SelectMany(w => w?.Entries ?? new List<OverviewEntry>()).Where(e => e is not null).ToArray();
        var targets = parsed.Target ?? Array.Empty<IShipUiTarget>();
        RefreshSelectedTarget(targets);
        RefreshPendingModuleToggles(ship);
        RefreshPendingTargetLocks(overview);
        var reps = RepairModules(ship).ToArray();
        var desiredRepIndexes = order.Repairs.Select(r => r.ModuleIndex).ToHashSet();

        // Protection modules are persistent while undocked. The Deacon fit has an additional active
        // hardener (type 11646) beside the reactive hardener. These are handled before combat/repair
        // work; afterburners remain controlled exclusively by order.Propulsion below.
        var protectionTypeIds = Role == "deacon" ? new[] { 4403, 11646 } : new[] { 4403 };
        foreach (var protectionTypeId in protectionTypeIds)
        {
            var protection = ModuleByType(ship, protectionTypeId);
            if (protection is null) continue;
            var protectionKey = $"{protection.Rack}:{protection.SlotIndex}:{protectionTypeId}";
            if (protection.IsActive == true)
            {
                // A confirmed active -> inactive edge is allowed to request one new activation.
                protectionActivationIssued.Remove(protectionKey);
                continue;
            }

            // ramp_active is absent while idle. One activation attempt is therefore allowed for
            // false/unknown, then latched until we have actually observed an active ramp. This
            // prevents an unreadable hardener from being toggled on/off every few ticks.
            if (!protectionActivationIssued.Add(protectionKey))
                continue;
            return ToggleModule(protection, parsed, $"{ModuleTypes.NameOrId(protectionTypeId)} on", desiredActive: true);
        }

        // Forget assignments after a module finished or its target left the target bar.
        foreach (var (module, index) in reps.Select((module, index) => (module, index)))
            if (module.IsActive != true ||
                repairTargetByModuleIndex.TryGetValue(index, out var oldTarget) &&
                FindTarget(targets, oldTarget) is null)
                repairTargetByModuleIndex.Remove(index);

        // A repair cycle left running after armor reaches 100% is both unnecessary cap use and violates
        // the operator's explicit "logi only on armor damage" policy.
        var repToStop = reps.Select((module, index) => (module, index))
            .FirstOrDefault(x => x.module.IsActive == true && !desiredRepIndexes.Contains(x.index));
        if (repToStop.module is not null)
        {
            repairTargetByModuleIndex.Remove(repToStop.index);
            return ToggleModule(repToStop.module, parsed, $"RR[{repToStop.index + 1}] off", desiredActive: false);
        }

        if (order.Repairs.Count > 0)
        {
            var repair = order.Repairs[0];
            if (!fleetMemberNames.TryGetValue(repair.TargetPid, out var repairTargetName))
                return $"repair target pid {repair.TargetPid} has no character mapping";

            // If an already-running RR is known to be assigned to somebody else, stop it before
            // selecting the new recipient. Never churn the selected target while the desired RR
            // is already active: that was the cause of Fenreire clicking Deacon every tick.
            var wrongTarget = reps.Select((module, index) => (module, index))
                .FirstOrDefault(x => desiredRepIndexes.Contains(x.index) && x.module.IsActive == true &&
                    repairTargetByModuleIndex.TryGetValue(x.index, out var assigned) &&
                    !string.Equals(assigned, repairTargetName, StringComparison.OrdinalIgnoreCase));
            if (wrongTarget.module is not null)
            {
                repairTargetByModuleIndex.Remove(wrongTarget.index);
                return ToggleModule(wrongTarget.module, parsed, $"RR[{wrongTarget.index + 1}] retarget off", desiredActive: false);
            }

            var desired = reps.Select((module, index) => (module, index))
                .FirstOrDefault(x => desiredRepIndexes.Contains(x.index) && x.module.IsActive != true);
            if (desired.module is not null)
            {
                var target = FindTarget(targets, repairTargetName);
                if (target is null)
                {
                    var repairOverview = FindOverview(overview, repairTargetName);
                    if (repairOverview is null) return $"repair target {repairTargetName} absent from target bar and overview";
                    if (repairOverview.CommonIndications?.TargetedByMe != true)
                        return ExecuteTask(repairOverview.UiElement.ClickWithModifier(bot, VirtualKeyCode.CONTROL), parsed,
                            $"lock repair target {repairTargetName}");
                    return $"waiting for repair lock {repairTargetName}";
                }
                if (!TargetIsSelected(target, repairTargetName))
                    return SelectTarget(target, repairTargetName, parsed, $"select repair target {repairTargetName}");

                repairTargetByModuleIndex[desired.index] = repairTargetName;
                return ToggleModule(desired.module, parsed,
                    $"RR[{desired.index + 1}] on -> {repairTargetName}", desiredActive: true);
            }
        }

        // Wrecks are never valid doctrine targets. Clear one stale lock per action pass so they
        // cannot consume target slots or become the accidental active target.
        var wreck = targets.FirstOrDefault(t => TargetName(t).Contains("Wreck", StringComparison.OrdinalIgnoreCase));
        if (wreck is not null)
        {
            var wreckName = TargetName(wreck);
            if (lastSelectedTargetId == wreck.Id ||
                string.Equals(lastSelectedTargetName, wreckName, StringComparison.OrdinalIgnoreCase))
            {
                lastSelectedTargetName = null;
                lastSelectedTargetId = null;
            }
            return ExecuteTask(wreck.ClickWithModifier(bot, HotkeyRegistry.UnlockTargetModifier), parsed,
                $"unlock wreck {wreckName}");
        }

        if (Role is "tank-retri" or "wing-retri" && order.PrimaryTargetId is not null &&
            !string.IsNullOrWhiteSpace(primaryTargetName))
        {
            var laser = ModuleByType(ship, 3033);
            if (laser is null) return $"{ModuleTypes.NameOrId(3033)} not found";

            if (order.Crystal != LaserCrystal.KeepCurrent &&
                laser.ModuleInfo?.ChargeTypeId != CrystalTypeId(order.Crystal))
            {
                if (laser.IsActive == true)
                {
                    laserTargetId = null;
                    return ToggleModule(
                        laser,
                        parsed,
                        $"laser off; switch crystal to {order.Crystal}",
                        desiredActive: false);
                }
                if (laser.IsBusy)
                    return $"waiting crystal reload {order.Crystal}";
                return RequestLaserCrystal(laser, parsed, order.Crystal);
            }

            if (order.HoldFireForRange)
            {
                if (laser.IsActive == true)
                {
                    laserTargetId = null;
                    return ToggleModule(laser, parsed,
                        $"laser off; {primaryTargetName} beyond useful falloff", desiredActive: false);
                }
            }
            else
            {
                if (laser.IsActive != true)
                    laserTargetId = null;
                else if (laserTargetId is null)
                {
                    // We cannot safely infer an assignment for a module that was already active when
                    // the coordinator attached. Stop it once and establish a tracked assignment.
                    return ToggleModule(laser, parsed, "laser assignment unknown; stop to retarget", desiredActive: false);
                }
                else if (laserTargetId != order.PrimaryTargetId)
                {
                    laserTargetId = null;
                    return ToggleModule(laser, parsed, $"laser off; switch primary to {primaryTargetName}", desiredActive: false);
                }

                var enemy = FindOverview(overview, primaryTargetName!, primaryTargetDistanceMeters);
                if (enemy is null) return $"primary {primaryTargetName} not on this overview";
                if (laser.IsActive != true)
                {
                    if (enemy.CommonIndications?.TargetedByMe != true)
                        return enemy.CommonIndications?.Targeting == true || PendingTargetLock(enemy.Id)
                            ? $"waiting for lock {primaryTargetName}"
                            : BatchLockHostiles(overview, targets, enemy, parsed);

                    var target = FindTarget(targets, primaryTargetName!, primaryTargetDistanceMeters);
                    if (target is null) return $"waiting for lock {primaryTargetName}";
                    if (!TargetIsSelected(target, primaryTargetName!))
                        return SelectTarget(target, primaryTargetName!, parsed, $"select {primaryTargetName}");

                    laserTargetId = order.PrimaryTargetId;
                    return ToggleModule(laser, parsed, $"fire on {primaryTargetName}", desiredActive: true);
                }
            }
        }

        var prop = ModuleByType(ship, 438);
        if (order.Propulsion == PropulsionOrder.On && prop?.IsActive != true)
            return ToggleModule(prop!, parsed, "prop on", desiredActive: true);
        if (order.Propulsion == PropulsionOrder.Off && prop?.IsActive == true)
            return ToggleModule(prop, parsed, "prop off", desiredActive: false);

        var positionTargetName = order.Positioning.Kind == RetributionDeaconPositioningKind.OrbitFleetmate &&
                                 fleetMemberNames.TryGetValue((int)order.Positioning.TargetId, out var fleetName)
            ? fleetName
            : order.Positioning.Kind is RetributionDeaconPositioningKind.OrbitEnemy or
                RetributionDeaconPositioningKind.ApproachEnemy or RetributionDeaconPositioningKind.CommitTank
                ? primaryTargetName
                : null;
        if (!string.IsNullOrWhiteSpace(positionTargetName))
        {
            var positionEntry = FindOverview(
                overview,
                positionTargetName!,
                order.Positioning.Kind == RetributionDeaconPositioningKind.OrbitFleetmate
                    ? null
                    : primaryTargetDistanceMeters);
            if (positionEntry is not null)
            {
                var maneuver = ship.Indication?.ManeuverType ?? ShipManeuverType.None;
                // Support formation is issued centrally by Gil-Gelad through Fleet -> Regroup after
                // each warp. Never replace it with per-client orbit commands.
                if (order.Positioning.Kind == RetributionDeaconPositioningKind.OrbitFleetmate)
                    return "formation via FC Regroup";
                if (order.Positioning.Kind == RetributionDeaconPositioningKind.OrbitEnemy &&
                    maneuver != ShipManeuverType.Orbit)
                    return ExecuteTask(positionEntry.UiElement.ClickWithModifier(bot, VirtualKeyCode.VK_W), parsed,
                        $"orbit {positionTargetName}");
                if (order.Positioning.Kind is RetributionDeaconPositioningKind.ApproachEnemy or RetributionDeaconPositioningKind.CommitTank &&
                    maneuver != ShipManeuverType.Approach)
                    return ExecuteTask(positionEntry.UiElement.ClickWithModifier(bot, VirtualKeyCode.VK_Q), parsed,
                        $"approach {positionTargetName}");
            }
        }

        return "stable";
    }

    private string RequestLaserCrystal(
        ShipUIModuleButton laser,
        ParsedUserInterface parsed,
        LaserCrystal desired)
    {
        var label = CrystalMenuLabel(desired);
        if (label is null) return $"crystal switch blocked: unsupported crystal {desired}";

        var laserMenu = (parsed.Menu ?? Array.Empty<IMenu>())
            .FirstOrDefault(m => m?.Entry?.Any(e =>
                CleanOverlayText(e?.Text).Equals("Clear group", StringComparison.OrdinalIgnoreCase)) == true);
        if (laserMenu is not null)
        {
            var entry = laserMenu.Entry.FirstOrDefault(e =>
            {
                var text = CleanOverlayText(e?.Text);
                return text.Equals(label, StringComparison.OrdinalIgnoreCase) ||
                       text.StartsWith(label + " ", StringComparison.OrdinalIgnoreCase);
            });
            if (entry is null)
                return $"crystal switch blocked: {label} absent; {MenuSnapshot(parsed)}";
            return ExecuteOne(
                entry.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation(),
                parsed,
                $"load crystal {desired}");
        }

        return ExecuteOne(
            laser.UINode.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right).AsRecommendation(),
            parsed,
            $"laser crystal menu for {desired}");
    }

    private static int? CrystalTypeId(LaserCrystal crystal) =>
        RetributionCrystalCatalog.For(crystal)?.TypeId;

    private static string? CrystalMenuLabel(LaserCrystal crystal) =>
        RetributionCrystalCatalog.For(crystal)?.MenuLabel;

    private static bool LooksLikeExecutedDoctrineAction(string result)
    {
        if (result.Contains(": no motion", StringComparison.OrdinalIgnoreCase) ||
            result.Contains(": motion[", StringComparison.OrdinalIgnoreCase) ||
            result.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            return false;

        return new[]
        {
            "RR[", "lock ", "select ", "unlock ", "reactive ", "laser ", "fire ",
            "load crystal ", "prop ", "orbit ", "approach ",
        }.Any(prefix => result.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshSelectedTarget(IEnumerable<IShipUiTarget> targets)
    {
        var targetArray = targets.ToArray();
        var selected = targetArray.FirstOrDefault(t => t.IsSelected == true);
        if (selected is not null)
        {
            lastSelectedTargetName = TargetName(selected);
            lastSelectedTargetId = selected.Id;
        }
        else if (lastSelectedTargetId is long selectedId && targetArray.All(t => t.Id != selectedId))
        {
            lastSelectedTargetName = null;
            lastSelectedTargetId = null;
        }
    }

    private bool TargetIsSelected(IShipUiTarget target, string name)
    {
        if (target.IsSelected == true)
        {
            lastSelectedTargetName = name;
            lastSelectedTargetId = target.Id;
            return true;
        }
        return lastSelectedTargetId == target.Id;
    }

    private string SelectTarget(IShipUiTarget target, string name, ParsedUserInterface parsed, string action)
    {
        var result = ExecuteOne(target.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left).AsRecommendation(), parsed, action);
        if (!result.Contains(": motion", StringComparison.OrdinalIgnoreCase) &&
            !result.Contains(": generation", StringComparison.OrdinalIgnoreCase) &&
            !result.Contains(": execute", StringComparison.OrdinalIgnoreCase))
        {
            lastSelectedTargetName = name;
            lastSelectedTargetId = target.Id;
        }
        return result;
    }

    /// <summary>
    /// Alternate physical input styles so the doctrine exercises both paths. Hotkeys are derived from
    /// the real rack/slot name (including empty gaps such as HighSlot5), never from the compacted list
    /// of fitted buttons. Clicks land at a random point inside the lower 70% of the module circle.
    /// </summary>
    private string ToggleModule(
        ShipUIModuleButton module,
        ParsedUserInterface parsed,
        string action,
        bool desiredActive)
    {
        var moduleKey = $"{module.Rack}:{module.SlotIndex}:{module.ModuleInfo?.ModuleId}";
        var now = Environment.TickCount64;
        if (pendingModuleToggles.TryGetValue(moduleKey, out var pending) &&
            pending.DesiredActive == desiredActive && now - pending.IssuedAt < 8_000)
            return $"await module state: {action}";
        if (lastModuleToggleAt.TryGetValue(moduleKey, out var previous) && now - previous < 1_000)
            return $"await module state: {action}";

        var hotkey = ModuleHotkey(module);
        var useHotkey = hotkey is not null && Interlocked.Increment(ref moduleActivationSequence) % 2 == 1;
        if (useHotkey)
        {
            lastModuleToggleAt[moduleKey] = now;
            pendingModuleToggles[moduleKey] = (desiredActive, now);
            return ExecuteOne(hotkey!.KeyboardPressCombined().AsRecommendation(), parsed, $"{action} [hotkey]");
        }

        var click = LowerSeventyPercentModuleClick(module);
        if (click is null)
            return $"{action}: module has no safe click region";
        lastModuleToggleAt[moduleKey] = now;
        pendingModuleToggles[moduleKey] = (desiredActive, now);
        return ExecuteOne(click.AsRecommendation(), parsed, $"{action} [click lower70]");
    }

    private void RefreshPendingModuleToggles(ShipUi ship)
    {
        foreach (var module in ship.ModuleButtons ?? new List<ShipUIModuleButton>())
        {
            var key = $"{module.Rack}:{module.SlotIndex}:{module.ModuleInfo?.ModuleId}";
            if (!pendingModuleToggles.TryGetValue(key, out var pending)) continue;
            var reached = pending.DesiredActive ? module.IsActive == true : module.IsActive != true;
            if (reached) pendingModuleToggles.Remove(key);
        }
    }

    private void RefreshPendingTargetLocks(IEnumerable<OverviewEntry> overview)
    {
        var entries = overview.ToArray();
        var visibleIds = entries.Select(entry => entry.Id).ToHashSet();
        pendingTargetLocks.RemoveWhere(id => !visibleIds.Contains(id));
        foreach (var entry in entries)
            if (entry.CommonIndications?.TargetedByMe == true || entry.CommonIndications?.Targeting == true)
                pendingTargetLocks.Remove(entry.Id);
    }

    private bool PendingTargetLock(long id) => pendingTargetLocks.Contains(id);

    /// <summary>
    /// Lock several hostile overview rows with one modifier hold: Ctrl down, N clicks, Ctrl up.
    /// A per-target pending latch prevents re-clicking while EVE is still resolving the locks.
    /// </summary>
    private string BatchLockHostiles(
        IReadOnlyList<OverviewEntry> overview,
        IReadOnlyCollection<IShipUiTarget> targets,
        OverviewEntry primary,
        ParsedUserInterface parsed)
    {
        const int conservativeTargetLimit = 5;
        const int maxBatch = 4;
        var locking = overview.Count(e => e.CommonIndications?.Targeting == true);
        var available = Math.Max(1, conservativeTargetLimit - targets.Count - locking);
        var candidates = overview
            .Where(IsHostileOverviewEntry)
            .Where(e => e.CommonIndications?.TargetedByMe != true && e.CommonIndications?.Targeting != true)
            .Where(e => !PendingTargetLock(e.Id))
            .DistinctBy(e => e.Id)
            .OrderBy(e => e.Id == primary.Id ? 0 : 1)
            .ThenBy(e => e.ObjectDistanceInMeters ?? int.MaxValue)
            .Take(Math.Min(maxBatch, available))
            .ToArray();
        if (candidates.Length == 0)
            return $"waiting for lock {primary.ObjectName ?? primary.ObjectType}";

        foreach (var entry in candidates)
            pendingTargetLocks.Add(entry.Id);
        var names = string.Join(", ", candidates.Select(e => e.ObjectName ?? e.ObjectType ?? e.Id.ToString()));
        return ExecuteTask(
            candidates.Select(e => e.UiElement).ClickWithModifier(VirtualKeyCode.CONTROL),
            parsed,
            $"batch lock {candidates.Length}: {names}");
    }

    private static bool IsHostileOverviewEntry(OverviewEntry entry)
    {
        var color = entry.IconSpriteColorPercent;
        return color is not null && color.RPercent > 80 &&
               color.BPercent < color.RPercent / 3 && color.GPercent < color.RPercent / 3;
    }

    private static VirtualKeyCode[]? ModuleHotkey(ShipUIModuleButton module)
    {
        if (module.SlotIndex is not int slot || slot < 0 || slot >= 8)
            return null;

        var key = slot switch
        {
            0 => VirtualKeyCode.F1,
            1 => VirtualKeyCode.F2,
            2 => VirtualKeyCode.F3,
            3 => VirtualKeyCode.F4,
            4 => VirtualKeyCode.F5,
            5 => VirtualKeyCode.F6,
            6 => VirtualKeyCode.F7,
            7 => VirtualKeyCode.F8,
            _ => throw new ArgumentOutOfRangeException(nameof(module)),
        };

        if (string.Equals(module.Rack, "High", StringComparison.OrdinalIgnoreCase))
            return new[] { key };
        if (string.Equals(module.Rack, "Medium", StringComparison.OrdinalIgnoreCase))
            return new[] { VirtualKeyCode.MENU, key };
        if (string.Equals(module.Rack, "Low", StringComparison.OrdinalIgnoreCase))
            return new[] { VirtualKeyCode.CONTROL, key };
        return null;
    }

    private static Sanderling.Motor.MotionParam? LowerSeventyPercentModuleClick(ShipUIModuleButton module)
    {
        var region = module.UINode.RegionInteraction?.Region ?? module.UINode.Region;
        if (region is not RectInt r || r.Max0 - r.Min0 < 8 || r.Max1 - r.Min1 < 8)
            return null;

        var width = r.Max0 - r.Min0;
        var height = r.Max1 - r.Min1;
        var centerX = (r.Min0 + r.Max0) / 2d;
        var centerY = (r.Min1 + r.Max1) / 2d;
        var radius = Math.Max(2d, Math.Min(width, height) / 2d - 3d);
        var lowerSeventyTop = r.Min1 + height * 0.30d;
        long x;
        long y;

        // Rejection sampling keeps the point in the circle, while the Y floor excludes its top 30%.
        do
        {
            x = (long)Math.Round(centerX + (Random.Shared.NextDouble() * 2d - 1d) * radius);
            y = (long)Math.Round(lowerSeventyTop + Random.Shared.NextDouble() *
                Math.Max(1d, r.Max1 - 2d - lowerSeventyTop));
        } while (Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2) > radius * radius);

        var pointRegion = new RectInt(x, y, x + 1, y + 1);
        return new Sanderling.Motor.MotionParam
        {
            MouseListWaypoint = new[]
            {
                new Sanderling.Motor.MotionParamMouseRegion
                {
                    UIElement = module.UINode,
                    RegionReplacementAbsolute = pointRegion,
                },
            },
            MouseButton = new[] { BotEngine.Motor.MouseButtonIdEnum.Left },
        };
    }

    private static string TargetName(IShipUiTarget target) =>
        target.LabelText is null ? "" : string.Join(" ", target.LabelText)
            .Replace("<center>", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

    private string ExecuteTask(IBotTask? task, ParsedUserInterface parsed, string action)
    {
        var motions = task?.ClientActions?.ToArray() ?? Array.Empty<MotionRecommendation>();
        if (motions.Length == 0) return $"{action}: no motion";
        var problems = ExecuteMotions(motions, parsed).ToArray();
        return problems.Length == 0 ? action : $"{action}: {string.Join("; ", problems)}";
    }

    private string ExecuteOne(MotionRecommendation motion, ParsedUserInterface parsed, string action)
    {
        var problems = ExecuteMotions(new[] { motion }, parsed).ToArray();
        return problems.Length == 0 ? action : $"{action}: {string.Join("; ", problems)}";
    }

    private IEnumerable<ShipUIModuleButton> RepairModules(ShipUi ship) => Role switch
    {
        "deacon" => ModulesByType(ship, 26912),
        "wing-retri" => ModulesByType(ship, 16435),
        _ => Array.Empty<ShipUIModuleButton>(),
    };

    private static ShipUIModuleButton? ModuleByType(ShipUi ship, int typeId) =>
        ship.ModuleButtons?.FirstOrDefault(m => m.ModuleInfo?.ModuleId == typeId);

    private static IEnumerable<ShipUIModuleButton> ModulesByType(ShipUi ship, int typeId) =>
        (ship.ModuleButtons ?? new List<ShipUIModuleButton>())
            .Where(m => m.ModuleInfo?.ModuleId == typeId)
            .OrderBy(m => m.UINode.Region?.Min0 ?? long.MaxValue);

    private static OverviewEntry? FindOverview(
        IEnumerable<OverviewEntry> entries,
        string name,
        int? expectedDistanceMeters = null)
    {
        var entryArray = entries.ToArray();
        var matches = entryArray
            .Where(e => string.Equals(e.ObjectName, name, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matches.Length == 0)
            matches = entryArray
                .Where(e => e.ObjectName?.Contains(name, StringComparison.OrdinalIgnoreCase) == true)
                .ToArray();
        return matches
            .OrderBy(e => expectedDistanceMeters is int expected
                ? Math.Abs((long)(e.ObjectDistanceInMeters ?? int.MaxValue) - expected)
                : 0)
            .FirstOrDefault();
    }

    private static IShipUiTarget? FindTarget(
        IEnumerable<IShipUiTarget>? targets,
        string name,
        int? expectedDistanceMeters = null) =>
        targets?
            .Where(t => (t.LabelText is null ? "" : string.Join(" ", t.LabelText))
                .Contains(name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => expectedDistanceMeters is int expected
                ? Math.Abs((long)(t.Distance ?? int.MaxValue) - expected)
                : 0)
            .FirstOrDefault();

    /// <summary>
    /// Operator debugging snapshot: write the full raw UI tree plus a focused, human-readable parse of the
    /// CURRENT state (which windows are present, overview entries + their brackets, drones, inventory, ship
    /// UI) into <paramref name="dir"/>. Paired with a screenshot by the caller, this is what the operator
    /// marks up with Claude when the bot misreads something — the raw tree is ground truth, parsed.json
    /// shows what we actually extracted, so a gap between them localizes the parser fix.
    /// </summary>
    public string[] DumpDiagnostics(string dir)
    {
        Directory.CreateDirectory(dir);
        var written = new List<string>();

        var rawPath = Path.Combine(dir, "rawtree.json");
        DumpRawTree(rawPath);
        if (File.Exists(rawPath)) written.Add(rawPath);

        var parsed = ReadAndParse();
        var parsedPath = Path.Combine(dir, "parsed.json");
        File.WriteAllText(parsedPath, System.Text.Json.JsonSerializer.Serialize(
            ParsedSummary(parsed), new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        written.Add(parsedPath);

        return written.ToArray();
    }

    /// <summary>Best-effort focused view of a parsed measurement for the debugging snapshot — every section
    /// is guarded so a null/oddity anywhere still yields a readable file.</summary>
    private static object ParsedSummary(IMemoryMeasurement? m)
    {
        if (m is null) return new { error = "UI read/parse returned nothing" };
        T? Try<T>(Func<T> f) { try { return f(); } catch { return default; } }

        return new
        {
            system = Try(() => m.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName),
            ship = Try<object?>(() =>
            {
                if (m.ShipUi is not ShipUi su) return null;
                var hp = su.HitpointsPercent;
                return new
                {
                    hp = hp == null ? null : new { hp.Shield, hp.Armor, hp.Structure },
                    maneuver = su.Indication?.ManeuverType?.ToString(),
                    maneuverTarget = su.Indication?.ManeuverTarget,
                    moduleButtons = su.ModuleButtons?.Count ?? 0,
                    modules = su.ModuleButtons?.Select(b => new
                    {
                        typeId = b.ModuleInfo?.ModuleId,
                        typeName = b.ModuleInfo?.ModuleId is int typeId
                            ? ModuleTypes.Lookup(typeId)?.Name
                            : null,
                        chargeTypeId = b.ModuleInfo?.ChargeTypeId,
                        b.Rack,
                        b.SlotIndex,
                        b.IsActive,
                    }).ToArray(),
                };
            }),
            windowsPresent = new
            {
                overview = Try(() => m.WindowOverview?.Length) ?? 0,
                inventory = Try(() => m.WindowInventory?.Length) ?? 0,
                droneView = Try(() => m.WindowDroneView != null),
                other = Try(() => m.WindowOther?.Length) ?? 0,
                targets = Try(() => m.Target?.Length) ?? 0,
            },
            overview = Try(() => m.WindowOverview?.SelectMany(w =>
                    (IEnumerable<OverviewEntry>?)w?.Entries ?? Enumerable.Empty<OverviewEntry>())
                .Select(e => new
                {
                    name = e.ObjectName,
                    type = e.ObjectType,
                    distM = e.ObjectDistanceInMeters,
                    bracket = new
                    {
                        attackingMe = e.CommonIndications?.AttackingMe ?? false,
                        targeting = e.CommonIndications?.Targeting ?? false,
                        targetedByMe = e.CommonIndications?.TargetedByMe ?? false,
                        jammingMe = e.CommonIndications?.IsJammingMe ?? false,
                        warpDisruptingMe = e.CommonIndications?.IsWarpDisruptingMe ?? false,
                    },
                    iconHints = e.RightAlignedIconsHints,
                }).ToArray()),
            targets = Try(() => m.Target?.Select(t => new
            {
                label = t.LabelText is null ? "" : string.Join(" ", t.LabelText),
                t.Distance,
                shieldPermille = t.Hitpoints?.Shield,
                armorPermille = t.Hitpoints?.Armor,
                hullPermille = t.Hitpoints?.Struct,
                t.IsSelected,
            }).ToArray()),
            drones = Try(() => m.WindowDroneView == null ? null : new
            {
                inBay = m.WindowDroneView.DroneGroupInBay?.Header?.MainText,
                inSpace = m.WindowDroneView.DroneGroupInSpace?.Header?.MainText,
                spaceStatuses = m.WindowDroneView.DroneGroupInSpace?.Children?
                    .Select(c => c?.Entry?.MainText).ToArray(),
            }),
            inventory = Try(() => m.WindowInventory?.Select(wi => new
            {
                subCaption = wi.SubCaptionLabelText,
                items = wi.SelectedContainerInventory?.ItemsView?
                    .Select(it => it.CellsTexts == null ? null : string.Join(" | ", it.CellsTexts.Select(kv => $"{kv.Key}={kv.Value}")))
                    .Take(60).ToArray(),
            }).ToArray()),
            windowOtherButtons = Try(() => m.WindowOther?.Select(w => w?.ToString()).ToArray()),
            menus = Try(() => m.Menu?.Select(menu => menu?.Entry?
                .Select(entry => new { entry.Text, entry.HighlightVisible }).ToArray()).ToArray()),
        };
    }

    // --- internals ------------------------------------------------------------

    private ParsedUserInterface? ReadAndParse()
    {
        try
        {
            var largest = ReadRawTree();
            if (largest is null) return null;
            var parsed = Parser.ParseUserInterfaceFromUITree(Parser.ParseUITreeWithDisplayRegionFromUITree(largest));
            TraceUiOverlays(parsed);
            return parsed;
        }
        catch (Exception e)
        {
            trace?.Invoke($"pid {Pid}: read/parse error: {e.Message}");
            return null;
        }
    }

    private void TraceUiOverlays(ParsedUserInterface parsed)
    {
        var menuLevels = (parsed.Menu ?? Array.Empty<IMenu>())
            .Where(m => m?.Entry?.Any() == true)
            .Select((m, index) =>
                $"L{index}: " + string.Join(" | ", m.Entry.Select(e =>
                    $"{CleanOverlayText(e?.Text)}{(e?.HighlightVisible == true ? " [hover]" : "")}")))
            .ToArray();
        var popupButtons = (parsed.WindowOther ?? Array.Empty<IWindow>())
            .SelectMany(w => w?.ButtonText ?? Array.Empty<IUIElementText?>())
            .Select(b => CleanOverlayText(b?.Text))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToArray();
        var signature = string.Join(" || ", menuLevels) + " ## " + string.Join(" | ", popupButtons);
        if (string.Equals(signature, lastOverlaySignature, StringComparison.Ordinal)) return;
        lastOverlaySignature = signature;

        foreach (var level in menuLevels)
            trace?.Invoke($"pid {Pid}: CONTEXT {level}");
        if (popupButtons.Length > 0)
            trace?.Invoke($"pid {Pid}: POPUP buttons: {string.Join(" | ", popupButtons)}");
        if (menuLevels.Length == 0 && popupButtons.Length == 0)
            trace?.Invoke($"pid {Pid}: overlays closed");
    }

    private static string MenuSnapshot(IMemoryMeasurement parsed)
    {
        var levels = (parsed.Menu ?? Array.Empty<IMenu>())
            .Where(m => m?.Entry?.Any() == true)
            .Select((m, index) =>
                $"L{index}=[{string.Join(" | ", m.Entry.Select(e => CleanOverlayText(e?.Text)))}]");
        return string.Join("; ", levels);
    }

    private static string CleanOverlayText(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? "<blank>"
            : System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " ")
                .Replace('\r', ' ').Replace('\n', ' ').Trim();

    /// <summary>
    /// Read the client's UI tree, reading ONLY the pinned winning UIRoot each tick (chosen once from the
    /// candidates, re-chosen if it goes stale after a client UI teardown). Previously every candidate root
    /// was read in full every tick and all but the largest discarded — N× the memory syscalls. Threads the
    /// persistent type-name cache so stable type names aren't re-read.
    /// </summary>
    private UITreeNode? ReadRawTree()
    {
        if (roots is null || roots.Count == 0) return null;

        if (pinnedRoot is ulong pin)
        {
            var t = EveOnline64.ReadUITreeFromAddress(pin, reader, 99, typeNameCache);
            if (t != null && t.EnumerateSelfAndDescendants().Count() > 50)
                return t;
            trace?.Invoke($"pid {Pid}: pinned UIRoot 0x{pin:X} went stale; re-selecting.");
            pinnedRoot = null;
        }

        UITreeNode? best = null;
        var bestCount = 0;
        ulong bestAddr = 0;
        foreach (var addr in roots)
        {
            var t = EveOnline64.ReadUITreeFromAddress(addr, reader, 99, typeNameCache);
            var c = t?.EnumerateSelfAndDescendants().Count() ?? 0;
            if (c > bestCount) { bestCount = c; best = t; bestAddr = addr; }
        }
        if (best != null)
        {
            pinnedRoot = bestAddr;
            trace?.Invoke($"pid {Pid}: pinned UIRoot 0x{bestAddr:X} ({bestCount} nodes).");
        }
        return best;
    }

    private bool TreeReadable(IImmutableList<ulong> candidateRoots)
    {
        try
        {
            var best = candidateRoots
                .Select(addr => EveOnline64.ReadUITreeFromAddress(addr, reader, 99))
                .Where(t => t != null)
                .Select(t => t!.EnumerateSelfAndDescendants().Count())
                .DefaultIfEmpty(0)
                .Max();
            return best > 50;
        }
        catch { return false; }
    }

    private IEnumerable<string> ExecuteMotions(MotionRecommendation[] motions, ParsedUserInterface measurement)
    {
        var problems = new List<string>();
        if (StopRequested)
            return new[] { "emergency stop" };
        var inputLeaseHeld = false;
        try
        {
            try
            {
                inputLeaseHeld = ScreenInputMutex.WaitOne(TimeSpan.FromSeconds(5));
            }
            catch (AbandonedMutexException)
            {
                // The prior runner died while holding input. Ownership transfers to this process.
                inputLeaseHeld = true;
            }
            if (!inputLeaseHeld)
                return new[] { "screen input lease timeout" };

            var windowHandle = MainWindowHandle;
            if (windowHandle == IntPtr.Zero)
            {
                problems.Add(
                    "no usable EVE window handle; keep the client visible on the interactive desktop " +
                    "and retry");
                return problems;
            }

            var motor = new WindowMotor(windowHandle);
            var i = 0;
            foreach (var motion in motions)
            {
                if (StopRequested)
                {
                    problems.Add("emergency stop");
                    break;
                }
                try
                {
                    var seq = motion.MotionParam.AsSequenceMotion(measurement).ToList();
                    var r = motor.ActSequenceMotion(seq);
                    if (r?.Success != true)
                        problems.Add($"motion[{i}] execute: {r?.Exception?.GetType().Name}: {r?.Exception?.Message ?? "no result"}");
                }
                catch (Exception e)
                {
                    problems.Add($"motion[{i}] generation: {e.GetType().Name}: {e.Message}");
                }
                i++;
            }
            return problems;
        }
        finally
        {
            if (inputLeaseHeld)
                ScreenInputMutex.ReleaseMutex();
        }
    }

    private bool StopRequested => stopRequested?.Invoke() == true;

    private static IEnumerable<string> DiagnosticMessages(IBotTask[][]? taskPaths)
    {
        if (taskPaths is null) yield break;
        foreach (var path in taskPaths)
        {
            var leaf = path?.LastOrDefault();
            if (leaf is DiagnosticTask d && !string.IsNullOrWhiteSpace(d.MessageText))
                yield return d.MessageText;
            else if (leaf is ISerializableBotTask s)
                yield return s.ToJson();
            else if (leaf != null)
                yield return leaf.GetType().Name;
        }
    }

    private static AgentStrategyStatus ToAgentStatus(StrategyStatus? status) => status is null
        ? new AgentStrategyStatus
        {
            State = "Warning",
            Summary = "Strategy returned no structured status",
            UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        }
        : new AgentStrategyStatus
        {
            Strategy = status.Strategy,
            Stage = status.Stage,
            State = status.State.ToString(),
            Summary = status.Summary,
            Action = status.Action,
            Details = status.Details,
            UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

    private static NpcDefenseProfile ToDefenseProfile(
        NpcStat? stat,
        IShipHitpointsAndEnergy? observedHitpoints)
    {
        NpcDamageLayer Layer(string key, int? observedPermille) => stat is null
            ? new NpcDamageLayer()
            : new NpcDamageLayer
            {
                Hitpoints = stat.HitpointsFor(key),
                EmResonance = stat.ResonanceFor(key, "em"),
                ThermalResonance = stat.ResonanceFor(key, "th"),
                RemainingPct = observedPermille is int value
                    ? Math.Clamp(value / 10d, 0, 100)
                    : 100,
            };

        return new NpcDefenseProfile
        {
            Shield = Layer("s", observedHitpoints?.Shield),
            Armor = Layer("a", observedHitpoints?.Armor),
            Hull = Layer("h", observedHitpoints?.Struct),
            RemainingObserved = observedHitpoints is not null &&
                                (observedHitpoints.Shield.HasValue || observedHitpoints.Armor.HasValue ||
                                 observedHitpoints.Struct.HasValue),
        };
    }

    private static AgentStrategyStatus ErrorStatus(string strategy, string summary) => new()
    {
        Strategy = strategy,
        State = "Error",
        Summary = summary,
        UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    };

    private static string SafeTitle(Process p)
    {
        try { return p.MainWindowTitle; } catch { return "<unavailable>"; }
    }

    public void Dispose()
    {
        (reader as IDisposable)?.Dispose();
    }
}
