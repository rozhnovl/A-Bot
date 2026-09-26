namespace AbotEngine.Fleet;

/// <summary>
/// Pure coordination brain for the asymmetric 2x Retribution + Deacon fleet.
///
/// The opening is intentionally stateful: on a fresh grid only the tank Retribution commits; on the
/// following tick it may tag/lock/fire; the wing and Deacon do not commit until the tank has observable
/// aggro. This is important for Kikimora and rogue-drone waves because they do not switch targets.
/// </summary>
public sealed class RetributionDeaconBrain
{
    private readonly RetributionDeaconThresholds t;
    private bool tankCommitIssued;
    private bool tankEngageIssued;
    private bool tankAggroEstablished;
    private bool previousTickHadEnemies;
    private int? lastRoomIndex;
    private RetributionDeaconEnvironment? lastEnvironment;
    private long? lastPrimaryId;
    private long? lastWingPrimaryId;

    public RetributionDeaconBrain(RetributionDeaconThresholds? thresholds = null) =>
        t = thresholds ?? new RetributionDeaconThresholds();

    public RetributionDeaconDecision Decide(RetributionDeaconPerception p)
    {
        var alive = p.Members.Where(m => m.InSpace).ToArray();
        if (alive.Length == 0)
            return Empty("no fleet members in space");

        var tank = alive.FirstOrDefault(m => m.Role == RetributionDeaconRole.TankRetribution);
        var wing = alive.FirstOrDefault(m => m.Role == RetributionDeaconRole.WingRetribution);
        var deacon = alive.FirstOrDefault(m => m.Role == RetributionDeaconRole.Deacon);
        var compositionOk = alive.Length == 3 &&
                            alive.Count(m => m.Role == RetributionDeaconRole.TankRetribution) == 1 &&
                            alive.Count(m => m.Role == RetributionDeaconRole.WingRetribution) == 1 &&
                            alive.Count(m => m.Role == RetributionDeaconRole.Deacon) == 1;

        var hasEnemies = p.Enemies.Count > 0;
        var freshEncounter = hasEnemies &&
            (!previousTickHadEnemies || lastRoomIndex != p.Room.Index || lastEnvironment != p.Environment);
        if (freshEncounter)
        {
            tankCommitIssued = false;
            tankEngageIssued = false;
            tankAggroEstablished = false;
            lastPrimaryId = null;
            lastWingPrimaryId = null;
        }
        previousTickHadEnemies = hasEnemies;
        lastRoomIndex = p.Room.Index;
        lastEnvironment = p.Environment;

        if (!hasEnemies)
            return GateOrClearDecision(p, alive, compositionOk);

        var roomKind = p.Environment == RetributionDeaconEnvironment.T3Electrical
            ? DetectRoom(p.Enemies)
            : RetributionDeaconRoomKind.Generic;

        // First action on every fresh grid belongs to the designated tank. Nobody locks an NPC yet.
        if (!tankCommitIssued)
        {
            tankCommitIssued = true;
            var openingOrders = alive.ToDictionary(
                m => m.Pid,
                m => new RetributionDeaconOrders
                {
                    Pid = m.Pid,
                    Positioning = m.Role == RetributionDeaconRole.TankRetribution
                        ? new RetributionDeaconPositioning(
                            RetributionDeaconPositioningKind.CommitTank,
                            p.Room.CacheId ?? 0)
                        : RetributionDeaconPositioning.Hold,
                    Intent = m.Role == RetributionDeaconRole.TankRetribution
                        ? "OPENING: tank Retribution commits first; establish aggro"
                        : "OPENING: hold — do not feed aggro before the tank commits",
                });

            return new RetributionDeaconDecision
            {
                Orders = openingOrders,
                RoomKind = roomKind,
                OpeningCommit = true,
                Summary = $"{roomKind}: opening commit; tank moves first" +
                          (compositionOk ? "" : "; INVALID COMPOSITION"),
            };
        }

        var primary = SelectPrimary(p.Enemies, roomKind);
        lastPrimaryId = primary?.Id;

        if (tank is not null && tank.Attackers > 0)
            tankAggroEstablished = true;
        // The opening gate exists to keep immutable-aggro spawns off the support ships. Once the
        // tank has visibly established aggro, ordinary anomaly NPCs may later switch targets; that
        // must not send the wing back into a permanent hold while a support ship is being shot.
        var tankHasAggro = tankAggroEstablished;
        var aggro = SelectAggroTarget(alive, tank);
        var dangerPack = IsDangerPack(p.Enemies, roomKind, aggro);
        var overheat = aggro is not null &&
            (aggro.ArmorPct <= t.OverheatArmorPct || aggro.IncomingDps >= t.OverheatIncomingDps);
        var deaconRepairs = RepairsFromDeacon(deacon, aggro, dangerPack, overheat);
        var wingRepairs = RepairsFromWing(wing, tank, deacon, aggro, dangerPack, overheat);
        var firePlan = SelectFirePlan(p, roomKind, primary, tank, wing, tankHasAggro);
        var fireMode = firePlan.Mode;
        var wingPrimary = firePlan.WingPrimary;

        // After the initial movement, the tank alone tags/locks/fires until red-box aggro is visible.
        // This prevents an immutable-aggro spawn selecting the wing or the Deacon.
        if (!tankHasAggro)
            tankEngageIssued = true;

        var orders = new Dictionary<int, RetributionDeaconOrders>();
        foreach (var member in alive)
        {
            var mayShoot = member.Role == RetributionDeaconRole.TankRetribution || tankHasAggro;
            var mayMove = member.Role == RetributionDeaconRole.TankRetribution || tankHasAggro;
            var assignedPrimary = member.Role == RetributionDeaconRole.WingRetribution &&
                                  fireMode == RetributionDeaconFireMode.Shuffle
                ? wingPrimary
                : primary;
            var repairs = member.Role switch
            {
                RetributionDeaconRole.Deacon => deaconRepairs,
                RetributionDeaconRole.WingRetribution => wingRepairs,
                _ => Array.Empty<RepairAssignment>(),
            };

            var positioning = mayMove
                ? PositionFor(member, tank, assignedPrimary, p, roomKind)
                : RetributionDeaconPositioning.Hold;
            var propulsion = mayMove ? PropulsionFor(member, assignedPrimary, p, roomKind) : PropulsionOrder.Off;
            var crystalDecision = member.Role == RetributionDeaconRole.Deacon
                ? new RetributionCrystalDecision(
                    LaserCrystal.KeepCurrent, 0, 0, null, "not a damage ship")
                : CrystalFor(member, assignedPrimary, p, roomKind, positioning);
            var crystal = crystalDecision.Crystal;
            var volleyEstimate = member.Role == RetributionDeaconRole.Deacon || assignedPrimary is null
                ? RetributionVolleyEstimate.Unknown(crystal, "not a damage ship")
                : RetributionDeaconVolleyEstimator.Estimate(
                    assignedPrimary, member.Weapon, LaserCrystal.KeepCurrent);

            orders[member.Pid] = new RetributionDeaconOrders
            {
                Pid = member.Pid,
                Repairs = repairs,
                PrimaryTargetId = mayShoot && member.Role != RetributionDeaconRole.Deacon ? assignedPrimary?.Id : null,
                FireMode = mayShoot && member.Role != RetributionDeaconRole.Deacon
                    ? fireMode
                    : RetributionDeaconFireMode.Hold,
                Positioning = positioning,
                Propulsion = propulsion,
                Crystal = crystal,
                RangeHitChance = member.Role == RetributionDeaconRole.Deacon || assignedPrimary is null
                    ? null
                    : crystalDecision.CurrentRangeHitChance,
                HoldFireForRange = member.Role != RetributionDeaconRole.Deacon && assignedPrimary is not null &&
                                   !crystalDecision.CurrentRangeUseful,
                SoloVolleysToKill = volleyEstimate.IsKnown ? volleyEstimate.Volleys : null,
                SoloShotsToKill = volleyEstimate.IsKnown ? volleyEstimate.Shots : null,
                OverheatThermalHardener = overheat && roomKind == RetributionDeaconRoomKind.Kikimora,
                Intent = BuildIntent(member, assignedPrimary, aggro, repairs, positioning, propulsion, crystalDecision,
                    roomKind, tankHasAggro, dangerPack, overheat, fireMode, volleyEstimate),
            };
        }

        var opening = !tankHasAggro;
        var summary =
            $"{p.Environment}/{roomKind}: " +
            $"primary={(primary is null ? "none" : $"{primary.Name}#{primary.Id}")}; " +
            $"fire={DescribeFirePlan(firePlan)}; " +
            $"aggro={(aggro is null ? "none" : $"pid {aggro.Pid}")}; " +
            $"tankAggro={(tankHasAggro ? "established" : tankEngageIssued ? "waiting after tank engage" : "waiting")}" +
            (dangerPack ? "; ALL-IN REPS" : "") +
            (compositionOk ? "" : "; INVALID COMPOSITION (need tank Retri + wing Retri + Deacon)");

        return new RetributionDeaconDecision
        {
            Orders = orders,
            RoomKind = roomKind,
            PrimaryTargetId = primary?.Id,
            FireMode = fireMode,
            AggroTargetPid = aggro?.Pid,
            OpeningCommit = opening,
            Summary = summary,
        };
    }

    private RetributionDeaconDecision GateOrClearDecision(
        RetributionDeaconPerception p,
        IReadOnlyList<RetributionDeaconMemberState> alive,
        bool compositionOk)
    {
        tankCommitIssued = false;
        tankEngageIssued = false;
        tankAggroEstablished = false;
        lastPrimaryId = null;
        lastWingPrimaryId = null;

        var abyss = p.Environment == RetributionDeaconEnvironment.T3Electrical;
        var capReady = alive.All(m => m.CapPct < 0 || m.CapPct >= t.GateCapPct);
        var gateReady = !abyss ||
            (compositionOk && capReady && !p.Room.LootPending && !p.Room.TagsPresent &&
             p.Room.FormationReady && p.Room.ReactiveHardenersReset);

        var orders = alive.ToDictionary(
            m => m.Pid,
            m => new RetributionDeaconOrders
            {
                Pid = m.Pid,
                Positioning = RetributionDeaconPositioning.Hold,
                Propulsion = PropulsionOrder.Off,
                ResetReactiveHardener = abyss && !p.Room.ReactiveHardenersReset,
                ClearSequenceTags = abyss && p.Room.TagsPresent,
                HoldGate = abyss && !gateReady,
                Intent = !abyss
                    ? "anomaly grid clear; regroup and recover"
                    : gateReady
                        ? "gate checklist complete; tank may activate the gate"
                        : $"HOLD GATE: reset reactive={(!p.Room.ReactiveHardenersReset ? "needed" : "ok")}, " +
                          $"tags={(p.Room.TagsPresent ? "clear" : "ok")}, cap={(capReady ? "ok" : $"need {t.GateCapPct}%")}, " +
                          $"loot={(p.Room.LootPending ? "pending" : "ok")}, formation={(p.Room.FormationReady ? "ok" : "align")}",
            });

        return new RetributionDeaconDecision
        {
            Orders = orders,
            RoomKind = RetributionDeaconRoomKind.Generic,
            GateReady = gateReady,
            Summary = !abyss
                ? "anomaly grid clear"
                : gateReady ? "room clear; gate checklist complete" : "room clear; HOLD GATE for checklist",
        };
    }

    private IReadOnlyList<RepairAssignment> RepairsFromDeacon(
        RetributionDeaconMemberState? deacon,
        RetributionDeaconMemberState? aggro,
        bool dangerPack,
        bool overheat)
    {
        // Operator policy: logi is called only after armor has actually taken damage. Red-box,
        // projected incoming DPS, or a dangerous room alone must not pre-activate remote reps.
        if (deacon is null || aggro is null || deacon.Pid == aggro.Pid || CapLocked(deacon) ||
            aggro.ArmorPct >= 100)
            return Array.Empty<RepairAssignment>();

        var modules = dangerPack
            ? t.DeaconRepModules
            : Math.Clamp((int)Math.Ceiling(Math.Max(1, aggro.IncomingDps) /
                                            (double)t.DeaconRepOutputPerModule), 1, t.DeaconRepModules);
        if (aggro.ArmorPct <= t.FocusArmorPct)
            modules = Math.Max(modules, 2);

        return Enumerable.Range(0, modules)
            .Select(i => new RepairAssignment(i, aggro.Pid, overheat))
            .ToArray();
    }

    private IReadOnlyList<RepairAssignment> RepairsFromWing(
        RetributionDeaconMemberState? wing,
        RetributionDeaconMemberState? tank,
        RetributionDeaconMemberState? deacon,
        RetributionDeaconMemberState? aggro,
        bool dangerPack,
        bool overheat)
    {
        if (wing is null || aggro is null || CapLocked(wing) || t.WingRepModules <= 0 ||
            aggro.ArmorPct >= 100)
            return Array.Empty<RepairAssignment>();

        // Gustav's explicit cross-rep safety: if the Deacon catches damage, the second Retri repairs it.
        if (deacon is not null && aggro.Pid == deacon.Pid)
            return new[] { new RepairAssignment(0, deacon.Pid, overheat) };

        // Large Kiki packs: Deacon's three reps plus the wing Retri's RR all land on the tank.
        if (dangerPack && tank is not null && aggro.Pid == tank.Pid)
            return new[] { new RepairAssignment(0, tank.Pid, overheat) };

        return aggro.ArmorPct <= t.FocusArmorPct && aggro.Pid != wing.Pid
            ? new[] { new RepairAssignment(0, aggro.Pid, overheat) }
            : Array.Empty<RepairAssignment>();
    }

    private bool CapLocked(RetributionDeaconMemberState m) => m.CapPct is >= 0 && m.CapPct <= t.LowCapPct;

    private bool IsDangerPack(
        IReadOnlyList<RetributionDeaconEnemyState> enemies,
        RetributionDeaconRoomKind room,
        RetributionDeaconMemberState? aggro) =>
        room == RetributionDeaconRoomKind.Kikimora &&
        enemies.Count(e => Contains(e.Name, "Kikimora")) >= t.KikimoraAllInCount ||
        room == RetributionDeaconRoomKind.AngelsOrSleepers &&
        aggro?.Role == RetributionDeaconRole.TankRetribution ||
        aggro is not null && (aggro.ArmorPct <= t.OverheatArmorPct || aggro.IncomingDps >= t.OverheatIncomingDps);

    private sealed record FireCandidate(
        RetributionDeaconEnemyState Enemy,
        RetributionVolleyEstimate Estimate);

    private sealed record FirePlan(
        RetributionDeaconFireMode Mode,
        RetributionDeaconEnemyState? TankPrimary,
        RetributionDeaconEnemyState? WingPrimary,
        RetributionVolleyEstimate TankEstimate,
        RetributionVolleyEstimate WingEstimate);

    private FirePlan SelectFirePlan(
        RetributionDeaconPerception p,
        RetributionDeaconRoomKind room,
        RetributionDeaconEnemyState? primary,
        RetributionDeaconMemberState? tank,
        RetributionDeaconMemberState? wing,
        bool tankHasAggro)
    {
        FirePlan Focus() => new(
            RetributionDeaconFireMode.FocusFire,
            primary,
            primary,
            EstimateFor(tank, primary),
            EstimateFor(wing, primary));

        // Before the tank owns aggro only it may fire. Dangerous named rooms, EWAR and remote repair
        // are always burned down together, independently of their nominal tank.
        if (!tankHasAggro || primary is null ||
            room is not (RetributionDeaconRoomKind.Generic or RetributionDeaconRoomKind.RogueDroneFrigates) ||
            PriorityBand(primary, room) < 14 || !MayShuffle(primary) || tank is null || wing is null)
        {
            lastWingPrimaryId = null;
            return Focus();
        }

        var tankEstimate = EstimateFor(tank, primary);
        if (!WithinSoloVolleyBudget(tankEstimate))
        {
            lastWingPrimaryId = null;
            return Focus();
        }

        var primaryBand = PriorityBand(primary, room);
        var candidates = p.Enemies
            .Where(e => e.Id != primary.Id && PriorityBand(e, room) == primaryBand && MayShuffle(e) &&
                        e.DistanceMeters <= t.ShuffleMaxTargetDistanceMeters)
            .Select(e => new FireCandidate(e, EstimateFor(wing, e)))
            .Where(c => WithinSoloVolleyBudget(c.Estimate))
            // With the fleet regrouped, canonical distance is a useful approximation for both Retris.
            .OrderBy(c => c.Enemy.DistanceMeters)
            .ThenBy(c => c.Estimate.Volleys)
            .ThenBy(c => c.Enemy.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var wingCandidate = lastWingPrimaryId is long stickyId
            ? candidates.FirstOrDefault(c => c.Enemy.Id == stickyId)
            : null;
        wingCandidate ??= candidates.FirstOrDefault();
        if (wingCandidate is null)
        {
            lastWingPrimaryId = null;
            return Focus();
        }

        lastWingPrimaryId = wingCandidate.Enemy.Id;
        return new FirePlan(
            RetributionDeaconFireMode.Shuffle,
            primary,
            wingCandidate.Enemy,
            tankEstimate,
            wingCandidate.Estimate);
    }

    private bool WithinSoloVolleyBudget(RetributionVolleyEstimate estimate) =>
        estimate.IsKnown && estimate.Volleys <= t.ShuffleMaxSoloVolleys;

    private static bool MayShuffle(RetributionDeaconEnemyState enemy)
    {
        if (enemy.IsNeuting || enemy.IsWebbing || enemy.IsPainting || enemy.IsScrambling ||
            enemy.IsDamping || enemy.IsTrackingDisrupting || enemy.IsRepairing)
            return false;

        // There is intentionally no EHP/name-size fallback here. If layer HP or resonances are absent,
        // the estimate is unknown and the fleet conservatively focuses the target.
        return enemy.Defense.IsKnown;
    }

    private static RetributionVolleyEstimate EstimateFor(
        RetributionDeaconMemberState? member,
        RetributionDeaconEnemyState? enemy)
    {
        if (member is null || enemy is null)
            return RetributionVolleyEstimate.Unknown(LaserCrystal.KeepCurrent, "ship or target unavailable");
        // Allocation uses what is actually loaded now. CrystalFor remains the requested next state;
        // after EVE reports that charge on a later tick the volley estimate automatically changes.
        return RetributionDeaconVolleyEstimator.Estimate(enemy, member.Weapon, LaserCrystal.KeepCurrent);
    }

    private static string DescribeFirePlan(FirePlan plan) => plan.Mode switch
    {
        RetributionDeaconFireMode.Shuffle =>
            $"shuffle tank={plan.TankPrimary?.Name ?? "none"}#{plan.TankPrimary?.Id} " +
            $"({FormatVolleyEstimate(plan.TankEstimate)}), " +
            $"wing={plan.WingPrimary?.Name ?? "none"}#{plan.WingPrimary?.Id} " +
            $"({FormatVolleyEstimate(plan.WingEstimate)})",
        RetributionDeaconFireMode.FocusFire =>
            $"focusFire ({FormatVolleyEstimate(plan.TankEstimate)} tank; " +
            $"{FormatVolleyEstimate(plan.WingEstimate)} wing)",
        _ => "hold",
    };

    private static string FormatVolleyEstimate(RetributionVolleyEstimate estimate) => estimate.IsKnown
        ? $"{estimate.Volleys:0.0} volleys/{estimate.Shots} shots {estimate.Crystal}"
        : $"shots unknown: {estimate.Reason}";

    private static RetributionDeaconMemberState? SelectAggroTarget(
        IReadOnlyList<RetributionDeaconMemberState> alive,
        RetributionDeaconMemberState? tank)
    {
        var attacked = alive.Where(m => m.Attackers > 0 || m.IncomingDps > 0)
            .OrderByDescending(m => m.Attackers)
            .ThenByDescending(m => m.IncomingDps)
            .ThenBy(m => m.ArmorPct)
            .FirstOrDefault();
        if (attacked is not null)
            return attacked;

        var damaged = alive.Where(m => m.ArmorPct < 100).OrderBy(m => m.ArmorPct).FirstOrDefault();
        return damaged ?? tank;
    }

    private RetributionDeaconEnemyState? SelectPrimary(
        IReadOnlyList<RetributionDeaconEnemyState> enemies,
        RetributionDeaconRoomKind room)
    {
        if (enemies.Count == 0)
            return null;

        var ranked = enemies
            .OrderBy(e => PriorityBand(e, room))
            .ThenBy(e => e.EstimatedEhp > 0 ? e.EstimatedEhp : int.MaxValue)
            .ThenBy(e => e.DistanceMeters)
            .ToArray();
        var top = ranked[0];

        if (lastPrimaryId is long lastId)
        {
            var last = enemies.FirstOrDefault(e => e.Id == lastId);
            if (last is not null && PriorityBand(last, room) <= PriorityBand(top, room))
                return last;
        }
        return top;
    }

    private static int PriorityBand(RetributionDeaconEnemyState e, RetributionDeaconRoomKind room)
    {
        var n = e.Name;
        switch (room)
        {
            case RetributionDeaconRoomKind.Kikimora:
                if (ContainsAll(n, "Starving", "Damavik")) return 0;
                if (Contains(n, "Kikimora")) return 1;
                if (e.IsRepairing) return 2;
                break;

            case RetributionDeaconRoomKind.Vhetaguth:
                if (ContainsAll(n, "Starving", "Damavik")) return 0;
                if (ContainsAll(n, "Ghosting", "Damavik")) return 1;
                if (IsVhetaguth(n) && Contains(n, "Starving")) return 2;
                if (IsVhetaguth(n) && Contains(n, "Harrowing")) return 3;
                if (Contains(n, "Damavik")) return 4;
                break;

            case RetributionDeaconRoomKind.Leshak:
                if (ContainsAll(n, "Starving", "Leshak")) return 0;
                if (ContainsAll(n, "Blinding", "Leshak")) return 1;
                if (Contains(n, "Leshak")) return 2;
                break;

            case RetributionDeaconRoomKind.Deepwatcher:
                if (!Contains(n, "Deepwatcher") && (e.IsFrigate || e.IsCruiser)) return 0;
                if (Contains(n, "Deepwatcher")) return 1;
                break;

            case RetributionDeaconRoomKind.Karybdis:
                if (e.IsWebbing || Contains(n, "Entangler") || Contains(n, "Snare")) return 0;
                if (e.IsNeuting || Contains(n, "Dissipator") || Contains(n, "Discharger")) return 1;
                if (e.IsPainting || Contains(n, "Illuminator") || Contains(n, "Spotlight")) return 2;
                if (IsKarybdis(n)) return 3;
                break;

            case RetributionDeaconRoomKind.Overmind:
                if (e.IsWebbing || e.IsPainting || Contains(n, "Snare") || Contains(n, "Spotlight")) return 0;
                if (e.IsDamping || e.IsTrackingDisrupting || Contains(n, "Fogcaster") || Contains(n, "Gazedimmer")) return 1;
                if (Contains(n, "Overmind")) return 2;
                break;

            case RetributionDeaconRoomKind.AngelsOrSleepers:
                if (IsSleeper(n) && !e.IsElite) return 0;
                if (IsSleeper(n) && e.IsElite) return 1;
                break;

            case RetributionDeaconRoomKind.RogueDroneBattlecruiser:
                if (Contains(n, "Snarecaster")) return 0;
                if (e.IsBattlecruiser || Contains(n, "Tessera")) return 1;
                break;

            case RetributionDeaconRoomKind.Vedmak:
                if (Contains(n, "Damavik") && (e.IsScrambling || e.IsWebbing || e.IsNeuting ||
                    Contains(n, "Anchoring") || Contains(n, "Tangling") || Contains(n, "Starving"))) return 0;
                if (ContainsAll(n, "Starving", "Vedmak")) return 1;
                if (ContainsAll(n, "Harrowing", "Vedmak")) return 2;
                break;
        }

        // Universal Electrical order: cap warfare -> web/painter -> remaining EWAR -> DPS by hull size.
        if (e.IsNeuting || Contains(n, "Starving")) return 10;
        if (e.IsWebbing || e.IsPainting || Contains(n, "Tangling") || Contains(n, "Illuminator") ||
            Contains(n, "Entangler") || Contains(n, "Harrowing")) return 11;
        if (e.IsScrambling || e.IsDamping || e.IsTrackingDisrupting || Contains(n, "Anchoring") ||
            Contains(n, "Blinding") || Contains(n, "Ghosting")) return 12;
        if (e.IsRepairing) return 13;
        if (e.IsFrigate) return 14;
        if (e.IsCruiser) return 15;
        if (e.IsBattlecruiser) return 16;
        if (e.IsBattleship) return 17;
        return 18;
    }

    private RetributionDeaconPositioning PositionFor(
        RetributionDeaconMemberState member,
        RetributionDeaconMemberState? tank,
        RetributionDeaconEnemyState? primary,
        RetributionDeaconPerception p,
        RetributionDeaconRoomKind room)
    {
        if (p.Environment == RetributionDeaconEnvironment.T3Electrical &&
            (p.Room.NearBlueCloud || room == RetributionDeaconRoomKind.Vhetaguth && p.Room.NearTrackingPylon))
            return new RetributionDeaconPositioning(RetributionDeaconPositioningKind.ClearHazards);

        // Vet'akh out-track themselves only when the fleet stops outside the pylon/cloud.
        if (room == RetributionDeaconRoomKind.Vhetaguth)
            return RetributionDeaconPositioning.Hold;

        if (member.Role == RetributionDeaconRole.Deacon)
            return tank is null
                ? RetributionDeaconPositioning.Hold
                : new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.OrbitFleetmate, tank.Pid, t.AnchorOrbitMeters);

        if (primary is null)
            return RetributionDeaconPositioning.Hold;

        if (p.Environment == RetributionDeaconEnvironment.AnomalyTraining)
            return member.Role == RetributionDeaconRole.TankRetribution
                ? new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.OrbitEnemy, primary.Id, t.TightOrbitMeters)
                : tank is null
                    ? RetributionDeaconPositioning.Hold
                    : new RetributionDeaconPositioning(
                        RetributionDeaconPositioningKind.OrbitFleetmate, tank.Pid, t.AnchorOrbitMeters);

        return room switch
        {
            RetributionDeaconRoomKind.Karybdis when IsKarybdis(primary.Name) =>
                new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.OrbitEnemy, primary.Id, t.TightOrbitMeters),
            RetributionDeaconRoomKind.Overmind when Contains(primary.Name, "Overmind") && p.Room.SparkneedlesPresent =>
                new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.OrbitEnemy, primary.Id, t.TightOrbitMeters),
            RetributionDeaconRoomKind.Overmind when Contains(primary.Name, "Overmind") =>
                new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.PushEnemyToBoundary, primary.Id),
            RetributionDeaconRoomKind.Deepwatcher when Contains(primary.Name, "Deepwatcher") =>
                new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.OrbitEnemy, primary.Id, t.TightOrbitMeters),
            RetributionDeaconRoomKind.RogueDroneBattlecruiser =>
                new RetributionDeaconPositioning(
                    RetributionDeaconPositioningKind.KeepRangeFromEnemy,
                    primary.Id,
                    t.RogueBattlecruiserRangeMeters),
            RetributionDeaconRoomKind.AngelsOrSleepers when p.Room.CacheId is long cacheId =>
                new RetributionDeaconPositioning(RetributionDeaconPositioningKind.ApproachCache, cacheId),
            _ => new RetributionDeaconPositioning(
                RetributionDeaconPositioningKind.ApproachEnemy, primary.Id),
        };
    }

    private static PropulsionOrder PropulsionFor(
        RetributionDeaconMemberState member,
        RetributionDeaconEnemyState? primary,
        RetributionDeaconPerception p,
        RetributionDeaconRoomKind room)
    {
        if (p.Room.NearBlueCloud || room == RetributionDeaconRoomKind.Vhetaguth && p.Room.NearTrackingPylon)
            return PropulsionOrder.On;
        if (member.Role == RetributionDeaconRole.Deacon || primary is null)
            return PropulsionOrder.Off;
        if (room == RetributionDeaconRoomKind.Vhetaguth)
            return PropulsionOrder.Off;
        if (room == RetributionDeaconRoomKind.Vedmak && ContainsAll(primary.Name, "Starving", "Vedmak"))
            return PropulsionOrder.Off;
        if (room == RetributionDeaconRoomKind.RogueDroneBattlecruiser)
            return PropulsionOrder.Off;
        return primary.DistanceMeters > 12000 || room is RetributionDeaconRoomKind.Karybdis or RetributionDeaconRoomKind.Overmind
            ? PropulsionOrder.On
            : PropulsionOrder.Off;
    }

    private static RetributionCrystalDecision CrystalFor(
        RetributionDeaconMemberState member,
        RetributionDeaconEnemyState? primary,
        RetributionDeaconPerception p,
        RetributionDeaconRoomKind room,
        RetributionDeaconPositioning positioning)
    {
        if (primary is null)
            return new RetributionCrystalDecision(
                LaserCrystal.KeepCurrent, 0, 0, null, "no primary target");

        int? expectedRange =
            positioning.Kind is (RetributionDeaconPositioningKind.OrbitEnemy or
                RetributionDeaconPositioningKind.KeepRangeFromEnemy) &&
            positioning.TargetId == primary.Id && positioning.RangeMeters > 0
                ? positioning.RangeMeters
                : null;

        // When we are not explicitly setting range, use only well-established room behaviour.
        // This prevents a close snapshot from loading Multifrequency just before a fast NPC pulls
        // into its normal orbit.
        expectedRange ??= room switch
        {
            RetributionDeaconRoomKind.Kikimora when Contains(primary.Name, "Kikimora") => 20_000,
            RetributionDeaconRoomKind.Vedmak when Contains(primary.Name, "Vedmak") => 18_000,
            RetributionDeaconRoomKind.RogueDroneFrigates when p.Enemies.Count > 3 => 20_000,
            _ => null,
        };

        var preferGleam =
            room == RetributionDeaconRoomKind.Deepwatcher && Contains(primary.Name, "Deepwatcher") ||
            room == RetributionDeaconRoomKind.Karybdis && IsKarybdis(primary.Name) ||
            room == RetributionDeaconRoomKind.Overmind && Contains(primary.Name, "Overmind") ||
            IsVhetaguth(primary.Name);
        return RetributionCrystalSelector.Decide(
            primary.DistanceMeters,
            expectedRange,
            member.Weapon.CurrentCrystalKnown ? member.Weapon.CurrentCrystal : LaserCrystal.KeepCurrent,
            preferGleam);
    }

    private static RetributionDeaconRoomKind DetectRoom(IReadOnlyList<RetributionDeaconEnemyState> enemies)
    {
        bool Any(Func<RetributionDeaconEnemyState, bool> predicate) => enemies.Any(predicate);
        if (Any(e => IsKarybdis(e.Name))) return RetributionDeaconRoomKind.Karybdis;
        if (Any(e => Contains(e.Name, "Overmind"))) return RetributionDeaconRoomKind.Overmind;
        if (Any(e => Contains(e.Name, "Kikimora"))) return RetributionDeaconRoomKind.Kikimora;
        if (Any(e => IsVhetaguth(e.Name))) return RetributionDeaconRoomKind.Vhetaguth;
        if (Any(e => Contains(e.Name, "Leshak"))) return RetributionDeaconRoomKind.Leshak;
        if (Any(e => Contains(e.Name, "Deepwatcher"))) return RetributionDeaconRoomKind.Deepwatcher;
        if (Any(e => Contains(e.Name, "Vedmak"))) return RetributionDeaconRoomKind.Vedmak;
        if (Any(e => e.IsBattlecruiser && IsRogueDrone(e.Name) || Contains(e.Name, "Tessera")))
            return RetributionDeaconRoomKind.RogueDroneBattlecruiser;
        if (Any(e => e.IsFrigate && IsRogueDrone(e.Name) || Contains(e.Name, "Tessella")))
            return RetributionDeaconRoomKind.RogueDroneFrigates;
        if (Any(e => IsSleeper(e.Name) || IsAngel(e.Name))) return RetributionDeaconRoomKind.AngelsOrSleepers;
        return RetributionDeaconRoomKind.Generic;
    }

    private static string BuildIntent(
        RetributionDeaconMemberState member,
        RetributionDeaconEnemyState? primary,
        RetributionDeaconMemberState? aggro,
        IReadOnlyList<RepairAssignment> repairs,
        RetributionDeaconPositioning positioning,
        PropulsionOrder propulsion,
        RetributionCrystalDecision crystalDecision,
        RetributionDeaconRoomKind room,
        bool tankHasAggro,
        bool dangerPack,
        bool overheat,
        RetributionDeaconFireMode fireMode,
        RetributionVolleyEstimate volleyEstimate)
    {
        var parts = new List<string>();
        if (!tankHasAggro && member.Role != RetributionDeaconRole.TankRetribution)
            parts.Add("hold until tank has red-box aggro");
        if (repairs.Count > 0)
            parts.Add($"RR[{string.Join(',', repairs.Select(r => r.ModuleIndex + 1))}] -> pid {repairs[0].TargetPid}" +
                      (repairs.Any(r => r.Overheat) ? " OVERHEAT" : ""));
        else if (member.Role == RetributionDeaconRole.Deacon && aggro?.Pid == member.Pid)
            parts.Add("cannot self-rep; wing Retri must repair Deacon");
        if (member.Role != RetributionDeaconRole.Deacon && primary is not null &&
            (member.Role == RetributionDeaconRole.TankRetribution || tankHasAggro))
            parts.Add($"{(fireMode == RetributionDeaconFireMode.Shuffle ? "shuffle" : "focus")} " +
                      $"{primary.Name}#{primary.Id} ({FormatVolleyEstimate(volleyEstimate)}, " +
                      $"{primary.DistanceMeters}m)");
        parts.Add(Describe(positioning));
        if (propulsion != PropulsionOrder.KeepCurrent) parts.Add($"prop {propulsion.ToString().ToLowerInvariant()}");
        var crystal = crystalDecision.Crystal;
        if (member.Role != RetributionDeaconRole.Deacon && primary is not null)
            parts.Add($"ammo={crystal} for {crystalDecision.EngagementRangeMeters}m; {crystalDecision.Reason}; " +
                      $"current-hit={crystalDecision.CurrentRangeHitChance:P0}" +
                      (crystalDecision.CurrentRangeUseful ? "" : "; HOLD FIRE beyond falloff-end"));
        if (crystal != LaserCrystal.KeepCurrent &&
            (!volleyEstimate.IsKnown || crystal != volleyEstimate.Crystal))
            parts.Add($"switch crystal -> {crystal}");
        if (dangerPack) parts.Add("all-in tank reps");
        if (overheat && room == RetributionDeaconRoomKind.Kikimora) parts.Add("overheat thermal hardener");
        return string.Join("; ", parts);
    }

    private static string Describe(RetributionDeaconPositioning p) => p.Kind switch
    {
        RetributionDeaconPositioningKind.Hold => "hold position",
        RetributionDeaconPositioningKind.ClearHazards => "clear blue cloud/tracking pylon, then stop",
        RetributionDeaconPositioningKind.CommitTank => "tank commits first",
        RetributionDeaconPositioningKind.OrbitFleetmate => $"orbit fleet pid {p.TargetId} @ {p.RangeMeters}m",
        RetributionDeaconPositioningKind.OrbitEnemy => $"orbit enemy {p.TargetId} @ {p.RangeMeters}m",
        RetributionDeaconPositioningKind.KeepRangeFromEnemy => $"keep {p.RangeMeters}m from enemy {p.TargetId}",
        RetributionDeaconPositioningKind.ApproachEnemy => $"approach enemy {p.TargetId}",
        RetributionDeaconPositioningKind.ApproachCache => $"approach cache {p.TargetId}",
        RetributionDeaconPositioningKind.PushEnemyToBoundary => $"push enemy {p.TargetId} toward boundary",
        _ => p.Kind.ToString(),
    };

    private static RetributionDeaconDecision Empty(string summary) => new()
    {
        Orders = new Dictionary<int, RetributionDeaconOrders>(),
        Summary = summary,
    };

    private static bool Contains(string? value, string part) =>
        value?.Contains(part, StringComparison.OrdinalIgnoreCase) == true;

    private static bool ContainsAll(string? value, params string[] parts) => parts.All(p => Contains(value, p));
    private static bool IsKarybdis(string? n) => Contains(n, "Karybdis") || Contains(n, "Tyrannos") || Contains(n, "Karen");
    private static bool IsVhetaguth(string? n) => Contains(n, "Vhetaguth") || Contains(n, "Vet'akh") || Contains(n, "Vet’akh");
    private static bool IsSleeper(string? n) => Contains(n, "Lucid") || Contains(n, "Ephialtes") || Contains(n, "Sleeper");
    private static bool IsAngel(string? n) => Contains(n, "Angel") || Contains(n, "Cynabal");
    private static bool IsRogueDrone(string? n) => Contains(n, "Rogue Drone") || Contains(n, "Drone");
}
