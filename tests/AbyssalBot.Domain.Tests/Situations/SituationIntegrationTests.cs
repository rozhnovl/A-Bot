using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services.Situations;

namespace AbyssalBot.Domain.Tests.Situations;

/// <summary>
/// Integration tests for complex multi-situation scenarios
/// </summary>
public class SituationIntegrationTests
{
    private readonly SituationCoordinator _coordinator;
    private readonly ILootPrioritizationHandler _lootHandler;
    private readonly IAbyssRoomStrategyHandler _roomStrategyHandler;
    private readonly IReloadTimingHandler _reloadHandler;

    public SituationIntegrationTests()
    {
        _coordinator = new SituationCoordinator(
            new CriticalDamageHandler(),
            new LowCapacitorHandler(),
            new EwarSituationHandler(),
            new MultipleHostilesHandler()
        );
        _lootHandler = new LootPrioritizationHandler();
        _roomStrategyHandler = new AbyssRoomStrategyHandler();
        _reloadHandler = new ReloadTimingHandler();
    }

    [Fact]
    public void Scenario_TriglavianRoom_KillsLogiFirst()
    {
        // Arrange - Triglavian room with Renewing Logi
        var enemies = new List<Target>
        {
            new Target(1, "Damavik", "Frigate", 8000, true),
            new Target(2, "Renewing Logi", "Frigate", 12000, true),
            new Target(3, "Vedmak", "Cruiser", 15000, true)
        };

        var room = new AbyssRoom("Triglavian", enemies, null, true);

        // Act
        var strategy = _roomStrategyHandler.DetermineStrategy(room);

        // Assert
        strategy.StrategyName.Should().Be("Triglavian Room");
        strategy.PriorityTargets.First().Name.Should().Be("Renewing Logi");
        strategy.Reasoning.Should().Contain("Logi");
    }

    [Fact]
    public void Scenario_LowCapWithManyEnemies_ManagesBothSituations()
    {
        // Arrange - Low capacitor with many enemies
        var context = CreateCombatContext(
            capacitorPercentage: 22,
            enemyCount: 11,
            mwdActive: true
        );

        var beacon = new Target(100, "Cache", "Structure", 10000, false);

        // Act
        var response = _coordinator.HandleSituation(context, null, beacon);

        // Assert
        response.Priority.Should().BeOneOf(
            SituationPriority.LowCapacitor,
            SituationPriority.MultipleHostiles
        );
        response.HasDecisions.Should().BeTrue();
    }

    [Fact]
    public void Scenario_CriticalDamageWithNeutralization_PrioritizesSurvival()
    {
        // Arrange - Critical shield + being neutralized
        var context = CreateCombatContext(
            shieldPercentage: 18,
            capacitorPercentage: 25
        );

        var neut = new Target(1, "Nullifier", "Cruiser", 8000, true);
        var ewarContext = new EwarContext(
            EwarType.Neutralized,
            neut,
            context.Hitpoints,
            new[] { neut }
        );

        // Act
        var response = _coordinator.HandleSituation(context, ewarContext);

        // Assert - Critical damage takes priority
        response.Priority.Should().Be(SituationPriority.CriticalDamage);
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md && md.Overload
        );
    }

    [Fact]
    public void Scenario_CacheRoom_ClearsAndLoots()
    {
        // Arrange - Cache room with defenders
        var enemies = new List<Target>
        {
            new Target(1, "Defender1", "Frigate", 5000, true),
            new Target(2, "Defender2", "Frigate", 7000, true)
        };

        var cache = new Target(10, "Bioadaptive Cache", "Structure", 15000, false);
        var room = new AbyssRoom("Cache", enemies, cache, false);

        // Act
        var strategy = _roomStrategyHandler.DetermineStrategy(room);

        // Assert
        strategy.StrategyName.Should().Be("Cache Room");
        strategy.ShouldLootCache.Should().BeTrue();
        strategy.Reasoning.Should().Contain("loot");
    }

    [Fact]
    public void Scenario_LootPrioritization_ValueOverVolume()
    {
        // Arrange - Multiple loot items with varying value
        var loot = new List<LootTarget>
        {
            new LootTarget(1, "Unstable Mutaplasmid", "Mutaplasmid", 1, 50000000, 100),
            new LootTarget(2, "Triglavian Datasheet", "Data", 10, 500000, 100),
            new LootTarget(3, "Abyssal Filament", "Filament", 0.1, 5000000, 100),
            new LootTarget(4, "Random Module", "Module", 50, 1000000, 100)
        };

        var space = new InventorySpace(50, 100, 0);

        // Act
        var prioritized = _lootHandler.PrioritizeLoot(loot, space);

        // Assert
        prioritized.Should().NotBeEmpty();
        prioritized.First().Type.Should().Contain("Mutaplasmid");
        prioritized.Should().Contain(l => l.Type.Contains("Filament"));
    }

    [Fact]
    public void Scenario_ReloadDuringLull_SafeToReloadWeapons()
    {
        // Arrange - No enemies nearby
        var context = CreateCombatContext(
            enemyCount: 0,
            capacitorPercentage: 80
        );

        // Act
        var shouldReload = _reloadHandler.ShouldReloadNow(context, ModuleType.Weapon);

        // Assert
        shouldReload.Should().BeTrue();
    }

    [Fact]
    public void Scenario_ReloadDuringCombat_NotSafeToReload()
    {
        // Arrange - Active combat target in range
        var target = new Target(1, "Enemy", "Frigate", 5000, true, true);
        var context = CreateCombatContext(
            enemyCount: 1,
            activeTarget: target
        );

        // Act
        var shouldReload = _reloadHandler.ShouldReloadNow(context, ModuleType.Weapon);

        // Assert
        shouldReload.Should().BeFalse();
    }

    [Fact]
    public void Scenario_EwarRoom_PrioritizesNeutralizers()
    {
        // Arrange - EWAR room with neutralizers and jammers
        var enemies = new List<Target>
        {
            new Target(1, "Starving Nullifier", "Frigate", 10000, true),
            new Target(2, "Jammer", "Frigate", 12000, true),
            new Target(3, "Dissipator", "Cruiser", 15000, true),
            new Target(4, "Regular Enemy", "Frigate", 8000, true)
        };

        var room = new AbyssRoom("EWAR", enemies, null, true);

        // Act
        var strategy = _roomStrategyHandler.DetermineStrategy(room);

        // Assert
        strategy.StrategyName.Should().Be("EWAR Room");
        strategy.PriorityTargets.First().Name.Should().Contain("Nullifier");
        strategy.Reasoning.Should().Contain("neutralizer");
    }

    [Fact]
    public void Scenario_FullCargo_SkipsLowValueLoot()
    {
        // Arrange - Nearly full cargo with high value items
        var loot = new List<LootTarget>
        {
            new LootTarget(1, "Trash Item", "Junk", 5, 10000, 100),
            new LootTarget(2, "Another Trash", "Junk", 5, 10000, 100)
        };

        var space = new InventorySpace(95, 100, 60000000); // 95% full, 60M ISK value

        // Act
        var prioritized = _lootHandler.PrioritizeLoot(loot, space);

        // Assert
        prioritized.Should().BeEmpty();
    }

    [Fact]
    public void Scenario_ComplexBattle_AllSystemsCoordinated()
    {
        // Arrange - Complex scenario:
        // - Shield at 25% (not critical yet)
        // - Cap at 28% (low)
        // - 8 enemies (moderate)
        // - Being webbed
        var context = CreateCombatContext(
            shieldPercentage: 25,
            capacitorPercentage: 28,
            enemyCount: 8,
            mwdActive: true
        );

        var tackler = new Target(1, "Tackler", "Frigate", 3000, true, false);
        var ewarContext = new EwarContext(
            EwarType.Webbed,
            tackler,
            context.Hitpoints,
            new[] { tackler }
        );

        var beacon = new Target(100, "Cache", "Structure", 10000, false);

        // Act
        var allSituations = _coordinator.GetAllSituations(context, ewarContext, beacon);

        // Assert
        allSituations.Should().NotBeEmpty();
        allSituations.Should().Contain(s => s.Priority == SituationPriority.Ewar);
        allSituations.Should().Contain(s => s.Priority == SituationPriority.LowCapacitor);

        // Should prioritize tackle (EWAR) over low cap
        var topPriority = allSituations.First();
        topPriority.Priority.Should().Be(SituationPriority.Ewar);
    }

    [Fact]
    public void Scenario_DroneRoom_PrioritizesClosestTargets()
    {
        // Arrange - Drone swarm (many small targets)
        var enemies = new List<Target>();
        for (int i = 0; i < 15; i++)
        {
            enemies.Add(new Target(i, $"Drone{i}", "Drone", 3000 + (i * 500), true));
        }

        var room = new AbyssRoom("Drone", enemies, null, true);

        // Act
        var strategy = _roomStrategyHandler.DetermineStrategy(room);

        // Assert
        strategy.StrategyName.Should().Be("Drone Room");
        strategy.PriorityTargets.First().Distance.Should().BeLessThan(5000);
        strategy.Reasoning.Should().Contain("closest");
    }

    [Fact]
    public void Scenario_EmergencyEvacuation_AllDefensiveModulesActive()
    {
        // Arrange - Critical hull damage
        var context = CreateCombatContext(
            hullPercentage: 35,
            shieldPercentage: 0,
            armorPercentage: 15
        );

        // Act
        var response = _coordinator.HandleSituation(context);

        // Assert
        response.IsEmergency.Should().BeTrue();
        response.Priority.Should().Be(SituationPriority.CriticalDamage);

        // Should overheat all tank modules
        var overheatDecisions = response.Decisions
            .OfType<ModuleDecision>()
            .Where(d => d.Overload)
            .ToList();

        overheatDecisions.Should().NotBeEmpty();
    }

    private CombatContext CreateCombatContext(
        double shieldPercentage = 100,
        double armorPercentage = 100,
        double hullPercentage = 100,
        double capacitorPercentage = 100,
        int enemyCount = 2,
        bool mwdActive = false,
        Target? activeTarget = null)
    {
        var hitpoints = new ShipHitpointsAndEnergy(
            Shield: shieldPercentage * 10,
            Armor: armorPercentage * 10,
            Hull: hullPercentage * 5,
            Capacitor: capacitorPercentage * 10,
            MaxShield: 1000,
            MaxArmor: 1000,
            MaxHull: 500,
            MaxCapacitor: 1000
        );

        var modules = new List<ShipModule>
        {
            new ShipModule(ModuleType.Weapon),
            new ShipModule(ModuleType.ShieldBooster),
            new ShipModule(ModuleType.Hardener),
            new ShipModule(ModuleType.MWD, IsActive: mwdActive)
        };

        var fitting = new ShipFitting(
            HighSlots: new[] { modules[0] },
            MidSlots: modules.Skip(1).ToList(),
            LowSlots: Array.Empty<ShipModule>(),
            MaxTargetingRange: 50000,
            MaxTargets: 5,
            MaxDronesInSpace: 5
        );

        var enemies = new List<Target>();
        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(new Target(i + 1, $"Enemy{i + 1}", "Frigate", 8000 + (i * 1000), true));
        }

        var targets = activeTarget != null
            ? new TargetCollection(new[] { activeTarget }, activeTarget)
            : new TargetCollection(Array.Empty<Target>(), null);

        return new CombatContext(
            fitting,
            hitpoints,
            ShipManeuverType.Orbit,
            targets,
            new DroneState(0, 5, true),
            enemyCount * 50,
            enemies
        );
    }
}
