using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services.Situations;

namespace AbyssalBot.Domain.Tests.Situations;

public class SituationCoordinatorTests
{
    private readonly SituationCoordinator _coordinator;

    public SituationCoordinatorTests()
    {
        _coordinator = new SituationCoordinator(
            new CriticalDamageHandler(),
            new LowCapacitorHandler(),
            new EwarSituationHandler(),
            new MultipleHostilesHandler()
        );
    }

    [Fact]
    public void HandleSituation_NormalConditions_ReturnsNoAction()
    {
        // Arrange
        var context = CreateNormalContext();

        // Act
        var response = _coordinator.HandleSituation(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.Normal);
        response.HasDecisions.Should().BeFalse();
    }

    [Fact]
    public void HandleSituation_CriticalDamage_TakesPriorityOverAll()
    {
        // Arrange - Critical hull, low cap, many enemies
        var context = CreateContext(
            hullPercentage: 40,
            capacitorPercentage: 15,
            enemyCount: 12
        );

        // Act
        var response = _coordinator.HandleSituation(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.CriticalDamage);
        response.Situation.Should().Contain("Critical");
    }

    [Fact]
    public void HandleSituation_CapacitorEmergency_TakesPriorityOverEnemies()
    {
        // Arrange - Emergency cap with many enemies
        var context = CreateContext(
            capacitorPercentage: 8,
            enemyCount: 12
        );

        // Act
        var response = _coordinator.HandleSituation(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.CapacitorEmergency);
    }

    [Fact]
    public void HandleSituation_EwarPresent_IncludedInResponse()
    {
        // Arrange
        var context = CreateNormalContext();
        var ewarSource = new Target(1, "Jammer", "Frigate", 10000, true);
        var ewarContext = new EwarContext(
            EwarType.Jammed,
            ewarSource,
            context.Hitpoints,
            new[] { ewarSource }
        );

        // Act
        var response = _coordinator.HandleSituation(context, ewarContext);

        // Assert
        response.Priority.Should().Be(SituationPriority.Ewar);
        response.Situation.Should().Contain("EWAR");
    }

    [Fact]
    public void HandleSituation_MultipleHostiles_ReturnsPriorityDecisions()
    {
        // Arrange
        var context = CreateContext(enemyCount: 11);
        var beacon = new Target(100, "Beacon", "Structure", 10000, false);

        // Act
        var response = _coordinator.HandleSituation(context, null, beacon);

        // Assert
        response.Priority.Should().Be(SituationPriority.MultipleHostiles);
        response.HasDecisions.Should().BeTrue();
    }

    [Fact]
    public void IsEmergencySituation_CriticalDamage_ReturnsTrue()
    {
        // Arrange
        var context = CreateContext(hullPercentage: 45);

        // Act
        var isEmergency = _coordinator.IsEmergencySituation(context);

        // Assert
        isEmergency.Should().BeTrue();
    }

    [Fact]
    public void IsEmergencySituation_CapacitorEmergency_ReturnsTrue()
    {
        // Arrange
        var context = CreateContext(capacitorPercentage: 5);

        // Act
        var isEmergency = _coordinator.IsEmergencySituation(context);

        // Assert
        isEmergency.Should().BeTrue();
    }

    [Fact]
    public void IsEmergencySituation_NormalConditions_ReturnsFalse()
    {
        // Arrange
        var context = CreateNormalContext();

        // Act
        var isEmergency = _coordinator.IsEmergencySituation(context);

        // Assert
        isEmergency.Should().BeFalse();
    }

    [Fact]
    public void GetAllSituations_MultipleSituations_ReturnsAllByPriority()
    {
        // Arrange - Low cap, many enemies
        var context = CreateContext(
            capacitorPercentage: 25,
            enemyCount: 11
        );
        var beacon = new Target(100, "Beacon", "Structure", 10000, false);

        // Act
        var situations = _coordinator.GetAllSituations(context, null, beacon);

        // Assert
        situations.Should().HaveCountGreaterThan(1);
        situations.Should().BeInDescendingOrder(s => s.Priority);
    }

    [Fact]
    public void HandleSituation_ComplexScenario_PrioritizesCorrectly()
    {
        // Arrange - Multiple issues: critical shield, low cap, many enemies, EWAR
        var context = CreateContext(
            shieldPercentage: 15,
            capacitorPercentage: 25,
            enemyCount: 12
        );

        var ewarSource = new Target(1, "Neutralizer", "Cruiser", 8000, true);
        var ewarContext = new EwarContext(
            EwarType.Neutralized,
            ewarSource,
            context.Hitpoints,
            new[] { ewarSource }
        );

        var beacon = new Target(100, "Beacon", "Structure", 10000, false);

        // Act
        var response = _coordinator.HandleSituation(context, ewarContext, beacon);

        // Assert - Critical damage should take priority
        response.Priority.Should().Be(SituationPriority.CriticalDamage);
        response.Situation.Should().Contain("Shield");
    }

    private CombatContext CreateNormalContext()
    {
        return CreateContext();
    }

    private CombatContext CreateContext(
        double shieldPercentage = 100,
        double armorPercentage = 100,
        double hullPercentage = 100,
        double capacitorPercentage = 100,
        int enemyCount = 2)
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
            new ShipModule(ModuleType.MWD)
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
            enemies.Add(new Target(i + 1, $"Enemy{i + 1}", "Frigate", 10000, true));
        }

        return new CombatContext(
            fitting,
            hitpoints,
            ShipManeuverType.Orbit,
            new TargetCollection(Array.Empty<Target>(), null),
            new DroneState(0, 5, true),
            enemyCount * 50, // Incoming DPS
            enemies
        );
    }
}
