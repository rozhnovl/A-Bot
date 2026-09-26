namespace AbotEngine.Fleet;

/// <summary>
/// Executable doctrine checks. These are intentionally dependency-free so operators can run them with
/// <c>FleetOrchestrator --brain-selftest</c> before attaching to any EVE client.
/// </summary>
public static class RetributionDeaconBrainScenarios
{
    public static void Run(Action<string> log)
    {
        NpcDatabaseContainsLayerResists(log);
        ModuleDatabaseResolvesActiveHardeners(log);
        OpeningTankFirst(log);
        LogiOnlyAfterArmorDamage(log);
        FireShuffleWeakTargets(log);
        FireUsesCrystalDamageAndLayerResists(log);
        CrystalCatalogMatchesBuildAndFalloffMath(log);
        CrystalSelectionUsesCurrentAndExpectedRange(log);
        FireFocusesDangerousAndToughTargets(log);
        KikimoraAllIn(log);
        DeaconAggroCrossRep(log);
        VhetaguthHazardAndOrder(log);
        LeshakOrder(log);
        DeepwatcherOrder(log);
        KarybdisWebThenNeut(log);
        OvermindNeedlesAndNoNeedles(log);
        AngelsAllIn(log);
        RogueDroneFrigateCrystals(log);
        RogueDroneBattlecruiser(log);
        StarvingVedmakKillsMwd(log);
        GateChecklist(log);
        TrainingDoesNotEnableAbyssTactics(log);
        log("");
        log("Retribution/Deacon doctrine checks: PASS");
    }

    private static void ModuleDatabaseResolvesActiveHardeners(Action<string> log)
    {
        var reactive = Sanderling.ABot.Bot.Configuration.ModuleTypes.Lookup(4403);
        var explosive = Sanderling.ABot.Bot.Configuration.ModuleTypes.Lookup(11646);
        Check(reactive?.Name == "Reactive Armor Hardener" &&
              explosive?.Name == "Explosive Armor Hardener II",
            "bundled module database must resolve both doctrine hardeners");
        log($"Module DB: 4403={reactive!.Name}; 11646={explosive!.Name}");
    }

    private static void NpcDatabaseContainsLayerResists(Action<string> log)
    {
        var stat = Sanderling.ABot.Bot.Configuration.NpcStats.Lookup("Coreli Safeguard");
        Check(stat is not null && stat.HitpointsFor("s") > 0 &&
              stat.ResonanceFor("a", "em") is > 0 and < 1,
            "bundled NPC database must contain raw layer HP and resonances");
        log($"NPC layer DB: Coreli Safeguard shield={stat!.HitpointsFor("s"):0}, " +
            $"armor EM resonance={stat.ResonanceFor("a", "em"):0.00}");
    }

    private static void LogiOnlyAfterArmorDamage(Action<string> log)
    {
        var enemy = Enemy(80, "Striking Damavik", frigate: true);
        var brain = PrimedBrain(enemy);
        var fullArmor = brain.Decide(Perception(enemy, tankAttackers: 4, tankIncoming: 500, tankArmor: 100));
        Check(fullArmor.Orders[3].Repairs.Count == 0 && fullArmor.Orders[2].Repairs.Count == 0,
            "logi must stay off while armor is undamaged, even under incoming DPS");

        var damaged = brain.Decide(Perception(enemy, tankAttackers: 4, tankIncoming: 500, tankArmor: 99));
        Check(damaged.Orders[3].Repairs.Count > 0 && damaged.Orders[2].Repairs.Count > 0,
            "logi may engage after armor damage is observed");
        Check(damaged.Orders[2].PrimaryTargetId == enemy.Id,
            "wing Retribution must keep its combat primary while cross-repairing");
        Print(log, "logi gate: armor damage only", damaged);
    }

    private static void OpeningTankFirst(Action<string> log)
    {
        var brain = new RetributionDeaconBrain();
        var p = Perception(Enemy(1, "Striking Damavik", frigate: true));
        var d = brain.Decide(p);

        Check(d.OpeningCommit, "fresh grid must be in opening commit");
        Check(d.Orders[1].Positioning.Kind == RetributionDeaconPositioningKind.CommitTank,
            "tank must commit first");
        Check(d.Orders[2].Positioning.Kind == RetributionDeaconPositioningKind.Hold &&
              d.Orders[3].Positioning.Kind == RetributionDeaconPositioningKind.Hold,
            "wing and Deacon must hold on first tick");
        Check(d.Orders.Values.All(o => o.PrimaryTargetId is null), "nobody should shoot on commit tick");
        Print(log, "opening: tank first", d);
    }

    private static void FireShuffleWeakTargets(Action<string> log)
    {
        var enemies = new[]
        {
            Enemy(901, "Coreli Defender", distance: 6000, ehp: 900),
            Enemy(902, "Coreli Infantry", distance: 4500, ehp: 1100),
            Enemy(903, "Coreli Safeguard", distance: 15000, ehp: 2400),
        };
        var brain = new RetributionDeaconBrain();
        brain.Decide(Perception(enemies, environment: RetributionDeaconEnvironment.AnomalyTraining));
        var d = brain.Decide(Perception(enemies, tankAttackers: 2,
            environment: RetributionDeaconEnvironment.AnomalyTraining));

        Check(d.FireMode == RetributionDeaconFireMode.Shuffle,
            "weak non-EWAR anomaly rats should use shuffle fire");
        Check(d.Orders[1].PrimaryTargetId == 901 && d.Orders[2].PrimaryTargetId == 902,
            "tank takes lowest-EHP primary while wing takes a distinct nearby weak target");
        Check(d.Orders[1].Intent.Contains("2 shots") &&
              d.Orders[1].Intent.Contains("Multifrequency") &&
              d.Orders[2].Intent.Contains("4500m"),
            "fire intent should expose the crystal/volley/distance inputs used by the allocator");
        Print(log, "fire allocator: weak targets shuffle", d);
    }

    private static void FireUsesCrystalDamageAndLayerResists(Action<string> log)
    {
        var far = new[]
        {
            Enemy(905, "Coreli Far One", distance: 28000, ehp: 1900),
            Enemy(906, "Coreli Far Two", distance: 28000, ehp: 1900),
        };
        var brain = new RetributionDeaconBrain();
        brain.Decide(Perception(far, environment: RetributionDeaconEnvironment.AnomalyTraining,
            currentCrystal: LaserCrystal.Aurora));
        var aurora = brain.Decide(Perception(far, tankAttackers: 1,
            environment: RetributionDeaconEnvironment.AnomalyTraining,
            currentCrystal: LaserCrystal.Aurora));
        Check(aurora.FireMode == RetributionDeaconFireMode.FocusFire &&
              aurora.Orders[1].Crystal == LaserCrystal.Aurora &&
              aurora.Orders[1].SoloShotsToKill == 6,
            "Aurora target above five solo volleys must receive focus fire");

        var near = far.Select(e => e with { DistanceMeters = 7000 }).ToArray();
        brain = new RetributionDeaconBrain();
        brain.Decide(Perception(near, environment: RetributionDeaconEnvironment.AnomalyTraining));
        var multifrequency = brain.Decide(Perception(near, tankAttackers: 1,
            environment: RetributionDeaconEnvironment.AnomalyTraining));
        Check(multifrequency.FireMode == RetributionDeaconFireMode.Shuffle &&
              multifrequency.Orders[1].Crystal == LaserCrystal.Multifrequency &&
              multifrequency.Orders[1].SoloShotsToKill == 3,
            "the same HP should shuffle when the in-range crystal kills it within five volleys");

        var resisted = near.Select(e => e with { Defense = UniformDefense(1900, 0.5) }).ToArray();
        brain = new RetributionDeaconBrain();
        brain.Decide(Perception(resisted, environment: RetributionDeaconEnvironment.AnomalyTraining));
        var resistedDecision = brain.Decide(Perception(resisted, tankAttackers: 1,
            environment: RetributionDeaconEnvironment.AnomalyTraining));
        Check(resistedDecision.FireMode == RetributionDeaconFireMode.FocusFire &&
              resistedDecision.Orders[1].SoloShotsToKill == 6,
            "layer resonances must turn the same raw HP into more applied volleys");

        brain = new RetributionDeaconBrain();
        brain.Decide(Perception(near, environment: RetributionDeaconEnvironment.AnomalyTraining,
            currentCrystal: LaserCrystal.KeepCurrent));
        var unknownAmmo = brain.Decide(Perception(near, tankAttackers: 1,
            environment: RetributionDeaconEnvironment.AnomalyTraining,
            currentCrystal: LaserCrystal.KeepCurrent));
        Check(unknownAmmo.FireMode == RetributionDeaconFireMode.FocusFire &&
              unknownAmmo.Orders[1].SoloShotsToKill is null,
            "unknown loaded ammo must fall back to focus fire instead of guessing from EHP");
        Print(log, "fire allocator: current crystal + layer resists", resistedDecision);
    }

    private static void CrystalSelectionUsesCurrentAndExpectedRange(Action<string> log)
    {
        var farWhileClosing = RetributionCrystalSelector.Decide(
            32_000, 500, LaserCrystal.Multifrequency, preferGleam: true);
        Check(farWhileClosing.Crystal == LaserCrystal.Aurora &&
              farWhileClosing.EngagementRangeMeters == 32_000,
            "a future tight orbit must not load a short crystal while the target is still far away");

        var targetWillPullRange = RetributionCrystalSelector.Decide(
            7_000, 20_000, LaserCrystal.Multifrequency, preferGleam: false);
        Check(targetWillPullRange.Crystal == LaserCrystal.Xray &&
              targetWillPullRange.EngagementRangeMeters == 20_000,
            "expected enemy orbit must pre-emptively select the highest applied-damage crystal at that range");

        var settledBoss = RetributionCrystalSelector.Decide(
            4_000, 500, LaserCrystal.Multifrequency, preferGleam: true);
        Check(settledBoss.Crystal == LaserCrystal.Gleam,
            "a close boss orbit should select Gleam only after both current and expected range fit it");

        var boundaryJitter = RetributionCrystalSelector.Decide(
            24_000, null, LaserCrystal.Aurora, preferGleam: false);
        Check(boundaryJitter.Crystal == LaserCrystal.Aurora,
            "range hysteresis must avoid crystal churn around the Standard/Aurora boundary");

        var tooFar = RetributionCrystalSelector.Decide(
            50_000, null, LaserCrystal.Aurora, preferGleam: false);
        Check(tooFar.Crystal == LaserCrystal.Aurora && !tooFar.CurrentRangeUseful,
            "normal fire must be held when even the selected crystal is beyond optimal plus one falloff");
        log("Crystal selector: current distance + expected orbit + hysteresis PASS");
    }

    private static void CrystalCatalogMatchesBuildAndFalloffMath(Action<string> log)
    {
        var expectedRanges = new Dictionary<LaserCrystal, (int Optimal, int FalloffEnd)>
        {
            [LaserCrystal.Aurora] = (45_000, 48_000),
            [LaserCrystal.Xray] = (19_000, 22_000),
            [LaserCrystal.Standard] = (25_000, 28_000),
            [LaserCrystal.Gamma] = (15_000, 18_000),
            [LaserCrystal.Gleam] = (6_000, 9_000),
            [LaserCrystal.Multifrequency] = (12_000, 15_000),
        };

        Check(RetributionCrystalCatalog.Carried.Count == expectedRanges.Count,
            "the carried-crystal catalog must contain exactly the six calibrated charges");
        foreach (var (crystal, ranges) in expectedRanges)
        {
            var profile = RetributionCrystalCatalog.For(crystal);
            Check(profile is not null &&
                  profile.OptimalMeters == ranges.Optimal &&
                  profile.FalloffEndMeters == ranges.FalloffEnd &&
                  profile.FalloffMeters == 3_000,
                $"{crystal} must retain its calibrated optimal/falloff values");
            Check(Math.Abs(profile!.RangeHitChance(profile.OptimalMeters) - 1) < 0.0001,
                $"{crystal} must have no range penalty at optimal");
            Check(Math.Abs(profile.RangeHitChance(profile.FalloffEndMeters) - 0.5) < 0.0001,
                $"{crystal} must have 50% range-only hit chance at optimal plus one falloff");
            Check(Math.Abs(profile.RangeHitChance(profile.OptimalMeters + 2 * profile.FalloffMeters) - 0.0625) < 0.0001,
                $"{crystal} must have 6.25% range-only hit chance at optimal plus two falloff");
        }

        log("Crystal catalog: six calibrated ranges + falloff curve PASS");
    }

    private static void FireFocusesDangerousAndToughTargets(Action<string> log)
    {
        var webber = Enemy(911, "Coreli Guardian", distance: 8000, web: true, ehp: 1200);
        var trash = Enemy(912, "Coreli Infantry", distance: 5000, ehp: 900);
        var brain = new RetributionDeaconBrain();
        brain.Decide(Perception(new[] { webber, trash },
            environment: RetributionDeaconEnvironment.AnomalyTraining));
        var dangerous = brain.Decide(Perception(new[] { webber, trash }, tankAttackers: 1,
            environment: RetributionDeaconEnvironment.AnomalyTraining));
        Check(dangerous.FireMode == RetributionDeaconFireMode.FocusFire &&
              dangerous.Orders[1].PrimaryTargetId == 911 && dangerous.Orders[2].PrimaryTargetId == 911,
            "EWAR target must receive focus fire even when its database EHP is low");

        var tough = new[]
        {
            Enemy(921, "Corelior Chief", distance: 7000, ehp: 7000),
            Enemy(922, "Corelior Sentinel", distance: 9000, ehp: 8000),
        };
        brain = new RetributionDeaconBrain();
        brain.Decide(Perception(tough, environment: RetributionDeaconEnvironment.AnomalyTraining));
        var thick = brain.Decide(Perception(tough, tankAttackers: 1,
            environment: RetributionDeaconEnvironment.AnomalyTraining));
        Check(thick.FireMode == RetributionDeaconFireMode.FocusFire &&
              thick.Orders[1].PrimaryTargetId == thick.Orders[2].PrimaryTargetId,
            "targets above five one-Retri volleys must receive focus fire");
        Print(log, "fire allocator: EWAR/tough targets focus", dangerous);
    }

    private static void KikimoraAllIn(Action<string> log)
    {
        var brain = PrimedBrain(Enumerable.Range(0, 7)
            .Select(i => Enemy(100 + i, $"Kikimora {i}", frigate: true))
            .Append(Enemy(90, "Starving Damavik", frigate: true, neut: true))
            .ToArray());
        var p = Perception(
            Enumerable.Range(0, 7).Select(i => Enemy(100 + i, $"Kikimora {i}", frigate: true))
                .Append(Enemy(90, "Starving Damavik", frigate: true, neut: true)).ToArray(),
            tankAttackers: 7, tankIncoming: 360, tankArmor: 65);
        var d = brain.Decide(p);

        Check(d.RoomKind == RetributionDeaconRoomKind.Kikimora, "Kikimora room detection");
        Check(d.PrimaryTargetId == 90, "starving Damavik must die before Kikimoras");
        Check(d.Orders[3].Repairs.Count == 3 && d.Orders[3].Repairs.All(r => r.TargetPid == 1),
            "all three Deacon reps must land on tank in a large Kiki pack");
        Check(d.Orders[2].Repairs.Count == 1 && d.Orders[2].Repairs[0].TargetPid == 1,
            "wing Retri RR must supplement tank reps");
        Check(d.Orders[1].OverheatThermalHardener, "thermal hardener should overheat under Kiki spike");
        Print(log, "Kikimora: starving first, all-in reps", d);
    }

    private static void DeaconAggroCrossRep(Action<string> log)
    {
        var enemy = Enemy(201, "Rogue Drone Frigate", frigate: true);
        var brain = PrimedBrain(enemy);
        var p = new RetributionDeaconPerception
        {
            Environment = RetributionDeaconEnvironment.T3Electrical,
            Members = new[]
            {
                Member(1, RetributionDeaconRole.TankRetribution),
                Member(2, RetributionDeaconRole.WingRetribution),
                Member(3, RetributionDeaconRole.Deacon, armor: 52, attackers: 4, incoming: 260),
            },
            Enemies = new[] { enemy },
            Room = new RetributionDeaconRoomState { Index = 1 },
        };
        var d = brain.Decide(p);

        Check(d.AggroTargetPid == 3, "Deacon should be recognized as aggro target");
        Check(d.Orders[3].Repairs.Count == 0, "Deacon cannot remote-repair itself");
        Check(d.Orders[2].Repairs.Single().TargetPid == 3, "wing Retribution must repair Deacon");
        Print(log, "Deacon aggro: wing cross-rep", d);
    }

    private static void VhetaguthHazardAndOrder(Action<string> log)
    {
        var enemies = new[]
        {
            Enemy(301, "Harrowing Vet'akh", cruiser: true, paint: true),
            Enemy(302, "Starving Vet'akh", cruiser: true, neut: true),
            Enemy(303, "Ghosting Damavik", frigate: true, td: true),
            Enemy(304, "Starving Damavik", frigate: true, neut: true),
        };
        var brain = PrimedBrain(enemies);
        var d = brain.Decide(Perception(enemies, tankAttackers: 2,
            room: new RetributionDeaconRoomState { Index = 1, NearBlueCloud = true, NearTrackingPylon = true }));

        Check(d.RoomKind == RetributionDeaconRoomKind.Vhetaguth, "Vet'akh room detection");
        Check(d.PrimaryTargetId == 304, "Vet'akh room order starts with starving Damavik");
        Check(d.Orders.Values.All(o => o.Positioning.Kind == RetributionDeaconPositioningKind.ClearHazards),
            "whole fleet must clear tracking pylon/blue cloud");
        Print(log, "Vet'akh: clear hazards, exact kill order", d);
    }

    private static void KarybdisWebThenNeut(Action<string> log)
    {
        var enemies = new[]
        {
            Enemy(401, "Karybdis Tyrannos", battleship: true),
            Enemy(402, "Ephialtes Dissipator", frigate: true, neut: true),
            Enemy(403, "Ephialtes Entangler", frigate: true, web: true),
        };
        var brain = PrimedBrain(enemies);
        var d = brain.Decide(Perception(enemies, tankAttackers: 2));
        Check(d.RoomKind == RetributionDeaconRoomKind.Karybdis, "Karybdis room detection");
        Check(d.PrimaryTargetId == 403, "Karen room must kill web before neut");
        Print(log, "Karen: web before neut", d);

        var afterWeb = new[] { enemies[0], enemies[1] };
        d = brain.Decide(Perception(afterWeb, tankAttackers: 2));
        Check(d.PrimaryTargetId == 402, "Karen room must kill neut second");
    }

    private static void LeshakOrder(Action<string> log)
    {
        var enemies = new[]
        {
            Enemy(351, "Striking Leshak", battleship: true),
            Enemy(352, "Blinding Leshak", battleship: true, damp: true),
            Enemy(353, "Starving Leshak", battleship: true, neut: true),
        };
        var brain = PrimedBrain(enemies);
        var d = brain.Decide(Perception(enemies, tankAttackers: 1));
        Check(d.RoomKind == RetributionDeaconRoomKind.Leshak && d.PrimaryTargetId == 353,
            "Leshak order starts with starving");
        var afterStarving = new[] { enemies[0], enemies[1] };
        d = brain.Decide(Perception(afterStarving, tankAttackers: 1));
        Check(d.PrimaryTargetId == 352, "blinding Leshak dies after starving");
        Print(log, "Leshak: starving then blinding", d);
    }

    private static void DeepwatcherOrder(Action<string> log)
    {
        var deep = Enemy(361, "Lucid Deepwatcher", battleship: true, elite: true);
        var escort = Enemy(362, "Lucid Escort", frigate: true);
        var brain = PrimedBrain(deep, escort);
        var d = brain.Decide(Perception(new[] { deep, escort }, tankAttackers: 1));
        Check(d.RoomKind == RetributionDeaconRoomKind.Deepwatcher && d.PrimaryTargetId == 362,
            "incoming frigate/cruiser dies before Deepwatcher");
        d = brain.Decide(Perception(deep, tankAttackers: 1));
        Check(d.PrimaryTargetId == 361 &&
              d.Orders[1].Positioning.Kind == RetributionDeaconPositioningKind.OrbitEnemy &&
              d.Orders[1].Crystal == LaserCrystal.Multifrequency,
            "Deepwatcher must keep a crystal that reaches while closing to its tight orbit");
        var closeDeep = deep with { DistanceMeters = 4_000 };
        d = brain.Decide(Perception(closeDeep, tankAttackers: 1));
        Check(d.Orders[1].Crystal == LaserCrystal.Gleam,
            "Deepwatcher switches to Gleam once the current distance also fits the tight orbit");
        Print(log, "Deepwatcher: escorts then tight Gleam", d);
    }

    private static void OvermindNeedlesAndNoNeedles(Action<string> log)
    {
        var overmind = Enemy(501, "Bathyic Abyssal Overmind", battleship: true);
        var brain = PrimedBrain(overmind);
        var withNeedles = brain.Decide(Perception(overmind, tankAttackers: 1,
            room: new RetributionDeaconRoomState { Index = 1, SparkneedlesPresent = true }));
        Check(withNeedles.Orders[1].Positioning.Kind == RetributionDeaconPositioningKind.OrbitEnemy &&
              withNeedles.Orders[1].Positioning.RangeMeters == 500,
            "Overmind with Sparkneedles uses 500m orbit");

        var withoutNeedles = brain.Decide(Perception(overmind, tankAttackers: 1));
        Check(withoutNeedles.Orders[1].Positioning.Kind == RetributionDeaconPositioningKind.PushEnemyToBoundary,
            "Overmind without Sparkneedles is pushed to boundary");
        Print(log, "Overmind: needles/no-needles branches", withoutNeedles);
    }

    private static void RogueDroneBattlecruiser(Action<string> log)
    {
        var enemies = new[]
        {
            Enemy(601, "Sparkgrip Tessera", battlecruiser: true),
            Enemy(602, "Snarecaster Tessella", frigate: true, web: true),
        };
        var brain = PrimedBrain(enemies);
        var d = brain.Decide(Perception(enemies, tankAttackers: 1));
        Check(d.RoomKind == RetributionDeaconRoomKind.RogueDroneBattlecruiser, "rogue BC room detection");
        Check(d.PrimaryTargetId == 602, "snare caster dies first");
        Check(d.Orders[1].Positioning.Kind == RetributionDeaconPositioningKind.KeepRangeFromEnemy &&
              d.Orders[1].Positioning.RangeMeters == 25000,
            "rogue BC room keeps 25km");
        Print(log, "rogue BC: snare first, 25km", d);
    }

    private static void AngelsAllIn(Action<string> log)
    {
        var regular = Enemy(551, "Lucid Escort", cruiser: true);
        var elite = Enemy(552, "Elite Lucid Watchman", cruiser: true, elite: true);
        var brain = PrimedBrain(regular, elite);
        var d = brain.Decide(Perception(new[] { regular, elite }, tankAttackers: 1, tankIncoming: 40,
            tankArmor: 99));
        Check(d.RoomKind == RetributionDeaconRoomKind.AngelsOrSleepers && d.PrimaryTargetId == 551,
            "non-elite sleeper dies before elite");
        Check(d.Orders[3].Repairs.Count == 3 && d.Orders[2].Repairs.Count == 1,
            "T3 Angels/Sleepers uses all-in reps on first tank Retri");
        Print(log, "Angels/Sleepers: non-elite first, all-in reps", d);
    }

    private static void RogueDroneFrigateCrystals(Action<string> log)
    {
        var enemies = Enumerable.Range(0, 4)
            .Select(i => Enemy(570 + i, $"Rogue Drone Frigate {i}", frigate: true, distance: 28000))
            .ToArray();
        var brain = PrimedBrain(enemies);
        var d = brain.Decide(Perception(enemies, tankAttackers: 1));
        Check(d.RoomKind == RetributionDeaconRoomKind.RogueDroneFrigates &&
              d.Orders[1].Crystal == LaserCrystal.Aurora,
            "rogue frigate pack starts by kiting with Aurora");
        var remainder = enemies.Take(2).Select(e => e with { DistanceMeters = 8000 }).ToArray();
        d = brain.Decide(Perception(remainder, tankAttackers: 1));
        Check(d.Orders[1].Crystal == LaserCrystal.Multifrequency,
            "tanky rogue-frigate remainder switches to Multifrequency");
        Print(log, "rogue frigates: Aurora kite then Multifrequency", d);
    }

    private static void StarvingVedmakKillsMwd(Action<string> log)
    {
        var enemy = Enemy(701, "Starving Vedmak", cruiser: true, neut: true, distance: 18000);
        var brain = PrimedBrain(enemy);
        var d = brain.Decide(Perception(enemy, tankAttackers: 1));
        Check(d.RoomKind == RetributionDeaconRoomKind.Vedmak, "Vedmak room detection");
        Check(d.Orders[1].Propulsion == PropulsionOrder.Off, "MWD must stay off orbiting starving Vedmak");
        Print(log, "starving Vedmak: MWD off", d);
    }

    private static void GateChecklist(Action<string> log)
    {
        var brain = new RetributionDeaconBrain();
        var notReady = brain.Decide(new RetributionDeaconPerception
        {
            Environment = RetributionDeaconEnvironment.T3Electrical,
            Members = Members(cap: 30),
            Room = new RetributionDeaconRoomState
            {
                Index = 1, TagsPresent = true, LootPending = true,
                FormationReady = false, ReactiveHardenersReset = false,
            },
        });
        Check(!notReady.GateReady && notReady.Orders.Values.All(o => o.HoldGate),
            "gate must be held while checklist is incomplete");
        Check(notReady.Orders.Values.All(o => o.ClearSequenceTags && o.ResetReactiveHardener),
            "gate prep must clear tags and reset reactive hardeners");

        var ready = brain.Decide(new RetributionDeaconPerception
        {
            Environment = RetributionDeaconEnvironment.T3Electrical,
            Members = Members(cap: 80),
            Room = new RetributionDeaconRoomState
            {
                Index = 1, FormationReady = true, ReactiveHardenersReset = true,
            },
        });
        Check(ready.GateReady && ready.Orders.Values.All(o => !o.HoldGate),
            "gate releases only after checklist passes");
        Print(log, "gate checklist", ready);
    }

    private static void TrainingDoesNotEnableAbyssTactics(Action<string> log)
    {
        var enemies = Enumerable.Range(0, 7)
            .Select(i => Enemy(800 + i, $"Kikimora {i}", frigate: true)).ToArray();
        var brain = new RetributionDeaconBrain();
        var training = Perception(enemies, environment: RetributionDeaconEnvironment.AnomalyTraining);
        brain.Decide(training); // opening commit
        var d = brain.Decide(Perception(enemies, tankAttackers: 7,
            environment: RetributionDeaconEnvironment.AnomalyTraining));
        Check(d.RoomKind == RetributionDeaconRoomKind.Generic,
            "anomaly rollout must not enable Abyss room classifier");
        Check(d.Orders[2].Repairs.Count == 0,
            "training mode should not infer the Abyss >6 Kiki all-in rule without actual damage");
        Print(log, "anomaly rollout isolation", d);
    }

    private static RetributionDeaconBrain PrimedBrain(params RetributionDeaconEnemyState[] enemies)
    {
        var brain = new RetributionDeaconBrain();
        brain.Decide(Perception(enemies));
        return brain;
    }

    private static RetributionDeaconPerception Perception(
        RetributionDeaconEnemyState enemy,
        int tankAttackers = 0,
        int tankIncoming = 0,
        int tankArmor = 100,
        RetributionDeaconRoomState? room = null,
        RetributionDeaconEnvironment environment = RetributionDeaconEnvironment.T3Electrical,
        LaserCrystal currentCrystal = LaserCrystal.Multifrequency) =>
        Perception(new[] { enemy }, tankAttackers, tankIncoming, tankArmor, room, environment, currentCrystal);

    private static RetributionDeaconPerception Perception(
        RetributionDeaconEnemyState[] enemies,
        int tankAttackers = 0,
        int tankIncoming = 0,
        int tankArmor = 100,
        RetributionDeaconRoomState? room = null,
        RetributionDeaconEnvironment environment = RetributionDeaconEnvironment.T3Electrical,
        LaserCrystal currentCrystal = LaserCrystal.Multifrequency) => new()
    {
        Environment = environment,
        Members = new[]
        {
            Member(1, RetributionDeaconRole.TankRetribution, tankArmor, tankAttackers, tankIncoming,
                currentCrystal: currentCrystal),
            Member(2, RetributionDeaconRole.WingRetribution, currentCrystal: currentCrystal),
            Member(3, RetributionDeaconRole.Deacon),
        },
        Enemies = enemies,
        Room = room ?? new RetributionDeaconRoomState { Index = 1 },
    };

    private static RetributionDeaconMemberState[] Members(int cap) => new[]
    {
        Member(1, RetributionDeaconRole.TankRetribution, cap: cap),
        Member(2, RetributionDeaconRole.WingRetribution, cap: cap),
        Member(3, RetributionDeaconRole.Deacon, cap: cap),
    };

    private static RetributionDeaconMemberState Member(
        int pid,
        RetributionDeaconRole role,
        int armor = 100,
        int attackers = 0,
        int incoming = 0,
        int cap = 80,
        LaserCrystal currentCrystal = LaserCrystal.Multifrequency) => new()
    {
        Pid = pid,
        Role = role,
        InSpace = true,
        ShieldPct = 100,
        ArmorPct = armor,
        StructPct = 100,
        CapPct = cap,
        Attackers = attackers,
        IncomingDps = incoming,
        Weapon = new RetributionLaserWeaponProfile { CurrentCrystal = currentCrystal },
    };

    private static RetributionDeaconEnemyState Enemy(
        long id,
        string name,
        int distance = 10000,
        bool frigate = false,
        bool cruiser = false,
        bool battlecruiser = false,
        bool battleship = false,
        bool elite = false,
        bool neut = false,
        bool web = false,
        bool paint = false,
        bool scram = false,
        bool damp = false,
        bool td = false,
        bool rep = false,
        int? ehp = null)
    {
        var syntheticHp = ehp ?? (frigate ? 2000 : cruiser ? 8000 : battleship ? 40000 : 12000);
        return new RetributionDeaconEnemyState
        {
            Id = id,
            Name = name,
            DistanceMeters = distance,
            EstimatedEhp = syntheticHp,
            Defense = UniformDefense(syntheticHp),
            IsFrigate = frigate,
            IsCruiser = cruiser,
            IsBattlecruiser = battlecruiser,
            IsBattleship = battleship,
            IsElite = elite,
            IsNeuting = neut,
            IsWebbing = web,
            IsPainting = paint,
            IsScrambling = scram,
            IsDamping = damp,
            IsTrackingDisrupting = td,
            IsRepairing = rep,
        };
    }

    private static global::AbotEngine.NpcDefenseProfile UniformDefense(double hitpoints, double resonance = 1) =>
        new()
        {
            Hull = new global::AbotEngine.NpcDamageLayer
            {
                Hitpoints = hitpoints,
                EmResonance = resonance,
                ThermalResonance = resonance,
            },
        };

    private static void Print(Action<string> log, string title, RetributionDeaconDecision d)
    {
        log("");
        log($"=== Retri/Deacon: {title} ===");
        log($"    {d.Summary}");
        foreach (var order in d.Orders.Values.OrderBy(o => o.Pid))
            log($"    pid {order.Pid}: {order.Intent}");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"Retribution/Deacon doctrine check failed: {message}");
    }
}
