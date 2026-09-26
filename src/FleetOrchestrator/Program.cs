// FleetOrchestrator — run a fleet of EVE clients through the shared ClientAgent engine.
//
// Discovers every running client (exefile.exe), assigns each a role, creates one
// AbotEngine.ClientAgent per window (same engine SingleRunner uses), attaches them all,
// then steps them every tick and publishes a combined fleet view to a remote dashboard.
//
// Today each window runs its own bot independently. The shared coordination brain
// (roles calling a primary target, a healer reacting to the fleet) lives as scaffolding
// under Fleet/ (IFleetActivity, IFleetCoordination) and is the next layer to wire in.
//
// SAFETY: dry-run by default. --live lets every agent control its ship.
//
// Usage:
//   FleetOrchestrator                      dry-run, all detected clients
//   FleetOrchestrator --live               control all ships
//   FleetOrchestrator --profile Worm_T1    run profile for every client
//   FleetOrchestrator --interval 1500      step period per tick
//   FleetOrchestrator --port 5020          fleet dashboard port
//   FleetOrchestrator --rescan             force fresh UIRoot scans
//   FleetOrchestrator --setup retri-deacon --phase anomaly
//                                            rehearse 2x Retribution + Deacon on an anomaly
//   FleetOrchestrator --setup retri-deacon --phase abyss
//                                            dry-run the T3 Electrical room doctrine
//   FleetOrchestrator --live --setup retri-deacon --actuate-retri
//                                            execute the calibrated anomaly-training doctrine
//   FleetOrchestrator --live --setup retri-deacon --phase anomaly --actuate-retri --auto-belts
//                                            fleet-warp through every asteroid belt once

using System.Diagnostics;
using System.Text.Json;
using AbotEngine;
using AbotEngine.Fleet;
using Eve64;
using FleetOrchestrator;
using Sanderling.ABot.Bot.Configuration;

var liveLogPath = Path.GetFullPath("fleet-orchestrator.log");
var liveLogSync = new object();
void Log(string m)
{
    var line = $"[{DateTime.Now:HH:mm:ss}] {m}";
    Console.WriteLine(line);
    try
    {
        lock (liveLogSync) File.AppendAllText(liveLogPath, line + Environment.NewLine);
    }
    catch { /* logging must never stop live control */ }
}

// Offline: run both fleet doctrines through canned scenarios and exit.
if (args.Contains("--brain-selftest"))
{
    FleetBrainScenarios.Run(Log);
    RetributionDeaconBrainScenarios.Run(Log);
    FleetActivitySupervisorScenarios.Run(Log);
    StatusJournalScenarios.Run(Log);
    Sanderling.ABot.Bot.AmmoScenarios.Run(Log);
    return 0;
}

try { EveOnline64.WinApi.SetProcessDPIAware(); } catch { /* older OS */ }

// --- args ---------------------------------------------------------------------
var live = false;
var intervalMs = 1500;
var port = 5020;
var profileName = "Worm_T1";
var forceRescan = false;
var setupName = "independent";
var phaseName = "anomaly";
var actuateRetri = false;
var autoSerpentis = false;
var autoBelts = false;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--live": live = true; break;
        case "--interval" when i + 1 < args.Length: intervalMs = int.Parse(args[++i]); break;
        case "--port" when i + 1 < args.Length: port = int.Parse(args[++i]); break;
        case "--profile" when i + 1 < args.Length: profileName = args[++i]; break;
        case "--setup" when i + 1 < args.Length: setupName = args[++i]; break;
        case "--phase" when i + 1 < args.Length: phaseName = args[++i]; break;
        case "--actuate-retri": actuateRetri = true; break;
        case "--auto-serpentis": autoSerpentis = true; break;
        case "--auto-belts": autoBelts = true; break;
        case "--rescan": forceRescan = true; break;
    }
}

var profile = ProfilesRegistry.GetByName(profileName);
var retriDeaconSetup = string.Equals(setupName, "retri-deacon", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(setupName, "2retri-deacon", StringComparison.OrdinalIgnoreCase);
var retriDeaconEnvironment = string.Equals(phaseName, "abyss", StringComparison.OrdinalIgnoreCase)
    ? RetributionDeaconEnvironment.T3Electrical
    : RetributionDeaconEnvironment.AnomalyTraining;
Log("=== FleetOrchestrator ===");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
GlobalHotkeyStop.Start(() =>
{
    Log($"EMERGENCY STOP ({GlobalHotkeyStop.Combo}) — input halted; manual control is safe.");
    cts.Cancel();
});
Log($"Emergency stop: {GlobalHotkeyStop.Combo} (global; works while an EVE window has focus).");

// The first rollout stage is observation/rehearsal. Do not imply that remote-rep and sequence-tag
// motions are live before their UI adapter has been validated on recorded anomaly sessions.
if (retriDeaconSetup && live && !actuateRetri)
{
    Log("Refusing --live for setup 'retri-deacon': this rollout is monitor-only until anomaly recordings validate target/rep actuation.");
    Log("Run without --live, manually fly anomalies, and compare the fleet doctrine shown in the log/dashboard.");
    return 3;
}
if (actuateRetri && (!retriDeaconSetup || retriDeaconEnvironment != RetributionDeaconEnvironment.AnomalyTraining))
{
    Log("Refusing --actuate-retri outside '--setup retri-deacon --phase anomaly'. Abyss actuation is not calibrated.");
    return 3;
}
if (autoSerpentis && autoBelts)
{
    Log("Refusing simultaneous --auto-serpentis and --auto-belts routes.");
    return 3;
}
var autoNavigation = autoSerpentis || autoBelts;
if (autoNavigation && (!live || !actuateRetri || !retriDeaconSetup ||
                       retriDeaconEnvironment != RetributionDeaconEnvironment.AnomalyTraining))
{
    Log("Refusing automatic navigation without live calibrated retri-deacon anomaly actuation.");
    return 3;
}

// --- discover clients & assign roles -----------------------------------------
var config = LoadConfig();
var processes = Process.GetProcessesByName("exefile").OrderBy(p => p.Id).ToList();
if (processes.Count == 0) { Log("No 'exefile.exe' clients found. Log characters into the game first."); return 1; }

var agents = new List<ClientAgent>();
for (var i = 0; i < processes.Count; i++)
{
    var role = AssignRole(processes[i], i, processes.Count, config, retriDeaconSetup);
    agents.Add(new ClientAgent(processes[i], profile, live, beltTest: false, role: role, trace: Log,
        monitorOnly: retriDeaconSetup, stopRequested: () => cts.IsCancellationRequested));
}

Log($"Discovered {agents.Count} client(s), profile={profile.Name}, mode={(live ? "LIVE" : "dry-run")}" +
    (retriDeaconSetup ? $", setup=2x Retribution + Deacon, phase={retriDeaconEnvironment}" : "") + ":");
foreach (var a in agents)
    Log($"    pid={a.Pid,-6} role={a.Role,-7} title=\"{a.Title}\"");
if (!string.IsNullOrWhiteSpace(config?.FleetCommander))
    Log($"Fleet commander: {config.FleetCommander}");
if (retriDeaconSetup && agents.Count != 3)
    Log($"WARNING: retri-deacon requires exactly 3 clients; found {agents.Count}. Doctrine will report invalid composition.");

// --- dashboard ----------------------------------------------------------------
_ = FleetDashboard.Start(port);
foreach (var ip in LocalIPv4Addresses())
    Log($"Fleet dashboard: http://{ip}:{port}  (open from your phone / another PC)");
Log($"Fleet dashboard: http://localhost:{port}");

if (live)
{
    Log("");
    Log("!!! LIVE MODE: every agent will move the mouse in its client. Do not touch the mouse. 5s... !!!");
    try { Task.Delay(5000, cts.Token).Wait(); } catch { }
}

if (cts.IsCancellationRequested)
{
    Log("Stopped before client attachment.");
    return 0;
}

// --- attach all (cached UIRoot => fast; first-ever scan is ~90s each) ---------
Log("Attaching clients (cached UIRoot reused when available)...");
var attached = new List<ClientAgent>();
foreach (var a in agents)
{
    if (cts.IsCancellationRequested) break;
    if (a.Attach(forceRescan)) attached.Add(a);
    else Log($"    pid={a.Pid}: could not attach (skipped).");
}
if (attached.Count == 0) { Log("No client could be attached. Exiting."); return 2; }

if (retriDeaconSetup)
{
    foreach (var a in attached)
    {
        var oldRole = a.Role;
        var detectedRole = a.AutoAssignRetributionDeaconRole();
        Log(detectedRole is null
            ? $"    pid={a.Pid}: module signature unknown; retained role={oldRole}"
            : $"    pid={a.Pid}: module signature => role={detectedRole}" +
              (oldRole == detectedRole ? "" : $" (replaced unsafe fallback {oldRole})"));
    }

    var requiredRoles = new[] { "tank-retri", "wing-retri", "deacon" };
    var invalidRoles = requiredRoles
        .Where(role => attached.Count(a => string.Equals(a.Role, role, StringComparison.OrdinalIgnoreCase)) != 1)
        .ToArray();
    if (invalidRoles.Length > 0)
    {
        Log($"ROLE SAFETY HOLD: expected exactly one of each doctrine role; invalid: {string.Join(", ", invalidRoles)}.");
        if (live) return 4;
    }
}

// --- loop ---------------------------------------------------------------------
Log("");
Log($"Stepping {attached.Count} agent(s). {GlobalHotkeyStop.Combo} or Ctrl+C to stop.");

var tick = 0;
var doctrineBrain = retriDeaconSetup ? new RetributionDeaconBrain() : null;
var doctrineRoomIndex = 0;
var doctrineHadEnemies = false;
var fleetMemberNames = attached.ToDictionary(a => a.Pid, a =>
{
    var parsedName = CharacterName(a.Title);
    if (!string.IsNullOrWhiteSpace(parsedName)) return parsedName;
    return config?.Assignments?.FirstOrDefault(x =>
        string.Equals(NormalizeRetributionDeaconRole(x.Role), a.Role, StringComparison.OrdinalIgnoreCase))
        ?.TitleContains ?? "";
});
var navigationRunState = autoNavigation ? "pending" : "disabled";
var navigationStateChangedAt = DateTime.UtcNow;
var navigationSawWarp = false;
var navigationSawEnemies = false;
while (!cts.IsCancellationRequested)
{
    var snapshots = new List<AgentSnapshot>(attached.Count);
    foreach (var a in attached)
        snapshots.Add(a.Step());

    RetributionDeaconDecision? doctrineDecision = null;
    if (doctrineBrain is not null)
    {
        var perception = BuildRetributionDeaconPerception(
            snapshots, retriDeaconEnvironment, doctrineRoomIndex, config);
        var hasEnemies = perception.Enemies.Count > 0;
        if (hasEnemies && !doctrineHadEnemies)
            doctrineRoomIndex++;
        doctrineHadEnemies = hasEnemies;
        if (navigationRunState == "running" && hasEnemies)
            navigationSawEnemies = true;
        perception = perception with { Room = perception.Room with { Index = doctrineRoomIndex } };
        doctrineDecision = doctrineBrain.Decide(perception);

        var actuation = new Dictionary<int, string>();
        if (live && actuateRetri)
        {
            var canonical = snapshots.FirstOrDefault(s => RoleFrom(s.Role) == RetributionDeaconRole.TankRetribution && s.ParseOk)
                            ?? snapshots.FirstOrDefault(s => s.ParseOk);
            foreach (var a in attached)
            {
                if (!doctrineDecision.Orders.TryGetValue(a.Pid, out var liveOrder)) continue;
                // In shuffle mode the wing has its own canonical target id. Non-shooting clients
                // (Deacon) retain the fleet primary name for hardener/combat-presence checks.
                var targetId = liveOrder.PrimaryTargetId ?? doctrineDecision.PrimaryTargetId;
                var canonicalTarget = targetId is long id
                    ? canonical?.Overview.FirstOrDefault(e => e.Id == id)
                    : null;
                // Each client gets a short, exclusive focus burst (for example lock -> select -> fire)
                // before the coordinator moves to the next window. This prevents inter-client focus
                // churn from splitting a multi-step action across different EVE clients.
                actuation[a.Pid] = a.ActuateRetributionDeaconBurst(
                    liveOrder,
                    canonicalTarget?.Name,
                    canonicalTarget?.DistanceMeters,
                    fleetMemberNames,
                    maxActions: 4);
            }

            var tankAgent = attached.FirstOrDefault(a =>
                RoleFrom(a.Role) == RetributionDeaconRole.TankRetribution);
            var tankSnapshot = snapshots.FirstOrDefault(s =>
                RoleFrom(s.Role) == RetributionDeaconRole.TankRetribution);
            var commanderName = !string.IsNullOrWhiteSpace(config?.FleetCommander)
                ? config!.FleetCommander!
                : tankAgent is null ? "Gil-Gelad" : fleetMemberNames.GetValueOrDefault(tankAgent.Pid, "Gil-Gelad");

            if (autoNavigation && tankAgent is not null && tankSnapshot is not null)
            {
                if (navigationRunState == "pending" && !hasEnemies)
                {
                    var navigation = autoBelts
                        ? tankAgent.ActuateNextAsteroidBeltBurst()
                        : tankAgent.ActuateNextSerpentisAnomalyBurst();
                    actuation[tankAgent.Pid] = navigation;
                    if (navigation.StartsWith("warp fleet requested:", StringComparison.OrdinalIgnoreCase))
                    {
                        navigationRunState = "warp-issued";
                        navigationStateChangedAt = DateTime.UtcNow;
                        navigationSawWarp = false;
                        navigationSawEnemies = false;
                    }
                    else if (autoBelts && navigation.StartsWith(
                                 "all asteroid belts visited", StringComparison.OrdinalIgnoreCase))
                        navigationRunState = "complete";
                }
                else if (navigationRunState is "warp-issued" or "in-warp")
                {
                    var inWarp = string.Equals(tankSnapshot.Maneuver, "Warp", StringComparison.OrdinalIgnoreCase);
                    if (inWarp)
                    {
                        navigationSawWarp = true;
                        navigationRunState = "in-warp";
                    }
                    var landed = navigationSawWarp && !inWarp ||
                                 hasEnemies ||
                                 navigationRunState == "warp-issued" &&
                                 DateTime.UtcNow - navigationStateChangedAt > TimeSpan.FromSeconds(8);
                    if (landed)
                    {
                        var regroup = tankAgent.ActuateFleetRegroupBurst(commanderName);
                        actuation[tankAgent.Pid] = regroup;
                        if (string.Equals(regroup, "fleet Regroup", StringComparison.OrdinalIgnoreCase))
                        {
                            navigationRunState = "running";
                            navigationStateChangedAt = DateTime.UtcNow;
                            navigationSawEnemies = hasEnemies;
                        }
                    }
                }
                else if (navigationRunState == "running" && !hasEnemies &&
                         (navigationSawEnemies || autoBelts) &&
                         DateTime.UtcNow - navigationStateChangedAt >
                         TimeSpan.FromSeconds(navigationSawEnemies ? 5 : 8))
                {
                    navigationRunState = "pending";
                    navigationStateChangedAt = DateTime.UtcNow;
                }
            }
        }

        snapshots = snapshots.Select(s =>
        {
            var doctrineIntent = doctrineDecision.Orders.TryGetValue(s.Pid, out var order)
                ? order.Intent
                : s.InSpace
                    ? "no doctrine order (role/composition unavailable)"
                    : "docked; waiting for safe fleet launch";
            return s with
            {
                Intents = s.Intents.Concat(new[]
                {
                    $"FLEET {doctrineDecision.Summary}",
                    doctrineIntent,
                    actuation.TryGetValue(s.Pid, out var liveAction) ? $"EXEC {liveAction}" : "EXEC monitor-only",
                }).ToArray(),
                StrategyStatus = new AgentStrategyStatus
                {
                    Strategy = nameof(RetributionDeaconBrain),
                    Stage = $"{retriDeaconEnvironment}/{doctrineDecision.RoomKind}",
                    State = !s.InSpace
                        ? "Waiting"
                        : doctrineDecision.Summary.Contains("INVALID COMPOSITION", StringComparison.OrdinalIgnoreCase)
                        ? "Warning"
                        : order?.HoldGate == true || doctrineDecision.OpeningCommit ? "Waiting" : "Acting",
                    Summary = doctrineDecision.Summary,
                    Action = doctrineIntent,
                    Details = s.Intents,
                    UpdatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            };
        }).ToList();
    }
    FleetState.Current = snapshots;

    Log($"--- tick {tick} ---");
    if (doctrineDecision is not null)
        Log($"    FLEET: {doctrineDecision.Summary}");
    if (autoNavigation)
    {
        var routeName = autoBelts ? "Belts" : "Serpentis";
        var progress = autoBelts
            ? $", visited={attached.FirstOrDefault(a => RoleFrom(a.Role) == RetributionDeaconRole.TankRetribution)?.VisitedAsteroidBeltCount ?? 0}"
            : "";
        Log($"    NAV: {routeName}={navigationRunState}{progress}, stop={GlobalHotkeyStop.Combo}");
    }
    foreach (var s in snapshots)
    {
        var health = !s.ParseOk ? "READ-FAIL" : s.LastError != null ? "err" : "ok";
        var status = s.StrategyStatus;
        Log($"    [{s.Role,-7}] pid={s.Pid} {s.System ?? "?"} A{s.Armor}/S{s.Shield}/H{s.Struct} " +
            $"lock={s.TargetsLocked} motions={s.MotionCount}{(s.Executed ? "*" : "")} {health}/{status.State}" +
            (!string.IsNullOrWhiteSpace(status.Summary) ? $" :: {status.Summary}" : "") +
            (!string.IsNullOrWhiteSpace(status.Action) ? $" -> {status.Action}" : ""));
        var execIntent = s.Intents.LastOrDefault(i => i.StartsWith("EXEC ", StringComparison.Ordinal));
        if (execIntent is not null) Log($"        {execIntent}");
    }

    tick++;
    try { Task.Delay(intervalMs, cts.Token).Wait(); } catch { break; }
}

Log("Stopped.");
return 0;

// --- helpers ------------------------------------------------------------------
static string AssignRole(Process p, int index, int total, FleetConfig? config, bool retriDeaconSetup)
{
    if (config?.Assignments is { Count: > 0 } assignments)
    {
        var title = SafeTitle(p);
        var match = assignments.FirstOrDefault(a =>
            !string.IsNullOrEmpty(a.TitleContains) &&
            title.Contains(a.TitleContains, StringComparison.OrdinalIgnoreCase));
        if (match?.Role is { Length: > 0 })
            return retriDeaconSetup ? NormalizeRetributionDeaconRole(match.Role) : match.Role.ToLowerInvariant();
    }

    if (retriDeaconSetup)
        return index switch
        {
            0 => "tank-retri",
            1 => "wing-retri",
            2 => "deacon",
            _ => "unassigned",
        };
    if (total == 1) return "dps";
    if (index == 0) return "scout";
    if (index == total - 1) return "healer";
    return "dps";
}

static string NormalizeRetributionDeaconRole(string role)
{
    if (role.Contains("deacon", StringComparison.OrdinalIgnoreCase) ||
        role.Contains("healer", StringComparison.OrdinalIgnoreCase)) return "deacon";
    if (role.Contains("tank", StringComparison.OrdinalIgnoreCase)) return "tank-retri";
    if (role.Contains("wing", StringComparison.OrdinalIgnoreCase) ||
        role.Contains("second", StringComparison.OrdinalIgnoreCase) ||
        role.Contains("dps", StringComparison.OrdinalIgnoreCase)) return "wing-retri";
    return role.ToLowerInvariant();
}

static RetributionDeaconPerception BuildRetributionDeaconPerception(
    IReadOnlyList<AgentSnapshot> snapshots,
    RetributionDeaconEnvironment environment,
    int roomIndex,
    FleetConfig? config)
{
    var members = snapshots.Select(s => new RetributionDeaconMemberState
    {
        Pid = s.Pid,
        Role = RoleFrom(s.Role),
        InSpace = s.InSpace,
        ShieldPct = s.Shield ?? 0,
        ArmorPct = s.Armor ?? 0,
        StructPct = s.Struct ?? 0,
        CapPct = s.Capacitor ?? -1,
        IncomingDps = s.IncomingDps,
        Attackers = s.Attackers,
        Weapon = WeaponProfileFor(s, config),
    }).ToArray();

    // The tank's overview is canonical: it is the first ship into the room and owns sequence tags.
    // Fall back to another readable client only while the tank is loading.
    var canonical = snapshots.FirstOrDefault(s =>
                        RoleFrom(s.Role) == RetributionDeaconRole.TankRetribution && s.ParseOk)
                    ?? snapshots.FirstOrDefault(s => s.ParseOk);
    var overview = canonical?.Overview ?? Array.Empty<AgentOverviewEntity>();
    var enemies = overview.Where(e => e.IsEnemy && IsCombatEntity(e))
        .Select(e => WithFleetObservedDefense(e, snapshots))
        .Select(ToDoctrineEnemy)
        .ToArray();
    var cache = overview.FirstOrDefault(e =>
        Contains(e.Name, "Bioadaptive") || Contains(e.Name, "Biocombinative"));

    return new RetributionDeaconPerception
    {
        Environment = environment,
        Members = members,
        Enemies = enemies,
        Room = new RetributionDeaconRoomState
        {
            Index = roomIndex,
            CacheId = cache?.Id,
            NearBlueCloud = overview.Any(e => Contains(e.Name, "Cloud") && e.DistanceMeters <= 15000),
            NearTrackingPylon = overview.Any(e =>
                (Contains(e.Name, "Tracking Pylon") || Contains(e.Name, "Tracking Tower")) &&
                e.DistanceMeters <= 30000),
            SparkneedlesPresent = enemies.Any(e => Contains(e.Name, "Sparkneedle")),
            // Conservative in rehearsal: a visible cache means the operator still needs to confirm loot.
            LootPending = cache is not null,
            // Tags, reactive reset and formation are not yet parsed. Keeping them false prevents an
            // automatic gate release in the Abyss rehearsal phase.
            TagsPresent = false,
            FormationReady = false,
            ReactiveHardenersReset = false,
        },
    };
}

static AgentOverviewEntity WithFleetObservedDefense(
    AgentOverviewEntity canonical,
    IReadOnlyList<AgentSnapshot> snapshots)
{
    var observed = snapshots.SelectMany(s => s.Overview)
        .Where(e => e.Defense.RemainingObserved &&
                    (string.Equals(e.Name, canonical.Name, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(e.Type, canonical.Type, StringComparison.OrdinalIgnoreCase)))
        // Duplicate NPC names are common. Regroup keeps fleet distances close enough that nearest
        // name+distance is the safest cross-client identity available to the UI-only coordinator.
        .OrderBy(e => Math.Abs((long)e.DistanceMeters - canonical.DistanceMeters))
        .FirstOrDefault();
    return observed is null ? canonical : canonical with { Defense = observed.Defense };
}

static RetributionDeaconRole RoleFrom(string role) => role switch
{
    "tank-retri" => RetributionDeaconRole.TankRetribution,
    "wing-retri" => RetributionDeaconRole.WingRetribution,
    "deacon" => RetributionDeaconRole.Deacon,
    _ => RetributionDeaconRole.WingRetribution,
};

static RetributionDeaconEnemyState ToDoctrineEnemy(AgentOverviewEntity e)
{
    var n = string.IsNullOrWhiteSpace(e.Name) ? e.Type : e.Name;
    var ewar = e.Ewar.ToHashSet(StringComparer.OrdinalIgnoreCase);
    var frigate = ContainsAny(n, "Damavik", "Kikimora", "Tessella", "Frigate", "Needle");
    var cruiser = ContainsAny(n, "Vedmak", "Vhetaguth", "Vet'akh", "Vet’akh", "Cruiser", "Cynabal");
    var battlecruiser = ContainsAny(n, "Tessera", "Battlecruiser");
    var battleship = ContainsAny(n, "Leshak", "Deepwatcher", "Overmind", "Tyrannos", "Karybdis", "Battleship");
    var dedicatedRepairer = ContainsAny(n, "Renewing", "Rodiva", "Preserver", "Firewatcher", "Deepwatcher", "Fieldweaver", "Plateforger");

    return new RetributionDeaconEnemyState
    {
        Id = e.Id,
        Name = n,
        DistanceMeters = e.DistanceMeters,
        EstimatedEhp = (int)Math.Min(int.MaxValue, e.EstimatedEhp),
        Defense = e.Defense,
        ApproxDps = (int)Math.Round(e.ApproxDps),
        IsFrigate = frigate,
        IsCruiser = cruiser,
        IsBattlecruiser = battlecruiser,
        IsBattleship = battleship,
        IsElite = ContainsAny(n, "Elite", "Deepwatcher", "Watchman", "Upholder", "Sentinel", "Aegis"),
        IsNeuting = ewar.Contains("neut") || ContainsAny(n, "Starving", "Dissipator", "Discharger"),
        IsWebbing = ewar.Contains("web") || ContainsAny(n, "Tangling", "Entangler", "Snarecaster"),
        IsPainting = ewar.Contains("paint") || ContainsAny(n, "Harrowing", "Illuminator", "Spotlight"),
        IsScrambling = ewar.Contains("scram") || Contains(n, "Anchoring"),
        IsDamping = ewar.Contains("damp") || Contains(n, "Blinding"),
        IsTrackingDisrupting = ewar.Contains("td") || Contains(n, "Ghosting"),
        IsRepairing = dedicatedRepairer,
    };
}

static RetributionLaserWeaponProfile WeaponProfileFor(AgentSnapshot snapshot, FleetConfig? config)
{
    var assignment = config?.Assignments?.FirstOrDefault(a =>
        !string.IsNullOrWhiteSpace(a.TitleContains) &&
        snapshot.Title.Contains(a.TitleContains, StringComparison.OrdinalIgnoreCase));
    var loadedCrystal = RetributionCrystalCatalog.FromTypeId(snapshot.LaserChargeTypeId)?.Crystal ??
                        LaserCrystal.KeepCurrent;
    return new RetributionLaserWeaponProfile
    {
        TurretCount = assignment?.TurretCount is > 0 ? assignment.TurretCount.Value : 4,
        PerTurretDamageMultiplier = assignment?.PerTurretDamageMultiplier is > 0
            ? assignment.PerTurretDamageMultiplier.Value
            : 11.7142,
        ApplicationFactor = assignment?.ApplicationFactor is > 0 and <= 1
            ? assignment.ApplicationFactor.Value
            : 1,
        CurrentCrystal = loadedCrystal,
        CurrentCrystalKnown = loadedCrystal != LaserCrystal.KeepCurrent,
    };
}

static bool IsCombatEntity(AgentOverviewEntity e) =>
    !ContainsAny(e.Name, "Extraction", "Cache", "Conduit", "Suppressor", "Cloud", "Pylon", "Tower");

static bool Contains(string? value, string part) =>
    value?.Contains(part, StringComparison.OrdinalIgnoreCase) == true;

static bool ContainsAny(string? value, params string[] parts) => parts.Any(p => Contains(value, p));

static string SafeTitle(Process p)
{
    try { return p.MainWindowTitle; } catch { return ""; }
}

static string CharacterName(string title) =>
    title.StartsWith("EVE - ", StringComparison.OrdinalIgnoreCase) ? title[6..].Trim() : title.Trim();

static FleetConfig? LoadConfig()
{
    const string path = "fleet.config.json";
    if (!File.Exists(path)) return null;
    try
    {
        return JsonSerializer.Deserialize<FleetConfig>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    catch { return null; }
}

static IEnumerable<string> LocalIPv4Addresses()
{
    try
    {
        return System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
            .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .Select(a => a.ToString()).Where(a => !a.StartsWith("127.")).ToArray();
    }
    catch { return Array.Empty<string>(); }
}

sealed class FleetConfig
{
    public string? Activity { get; set; }
    public string? FleetCommander { get; set; }
    public List<RoleAssignment>? Assignments { get; set; }
}

sealed class RoleAssignment
{
    public string? TitleContains { get; set; }
    public string Role { get; set; } = "dps";
    /// <summary>Damage modifier shown on one fitted beam turret; defaults to the calibrated T3 fit.</summary>
    public double? PerTurretDamageMultiplier { get; set; }
    public int? TurretCount { get; set; }
    public double? ApplicationFactor { get; set; }
}
