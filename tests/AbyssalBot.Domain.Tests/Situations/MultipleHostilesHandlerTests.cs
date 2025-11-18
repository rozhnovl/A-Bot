using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services.Situations;

namespace AbyssalBot.Domain.Tests.Situations;

public class MultipleHostilesHandlerTests
{
    private readonly MultipleHostilesHandler _handler;

    public MultipleHostilesHandlerTests()
    {
        _handler = new MultipleHostilesHandler();
    }

    [Fact]
    public void HandleMultipleHostiles_FewEnemies_ReturnsNoAction()
    {
        // Arrange
        var hostiles = CreateHostiles(3);

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 100, null);

        // Assert
        response.Priority.Should().Be(SituationPriority.Normal);
        response.HasDecisions.Should().BeFalse();
    }

    [Fact]
    public void HandleMultipleHostiles_ModerateEnemies_MaintainsDefensivePosition()
    {
        // Arrange
        var hostiles = CreateHostiles(6);
        var beacon = new Target(100, "Beacon", "Structure", 20000, false);

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 200, beacon);

        // Assert
        response.Priority.Should().Be(SituationPriority.LowCapacitor);
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.Orbit &&
            md.Target == beacon
        );
    }

    [Fact]
    public void HandleMultipleHostiles_ManyEnemies_EntersDefensiveOrbit()
    {
        // Arrange
        var hostiles = CreateHostiles(12);
        var beacon = new Target(100, "Beacon", "Structure", 10000, false);

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 300, beacon);

        // Assert
        response.Priority.Should().Be(SituationPriority.MultipleHostiles);
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.Orbit &&
            md.Distance == 7500
        );
    }

    [Fact]
    public void HandleMultipleHostiles_ManyEnemiesHighDPS_OverheatsTank()
    {
        // Arrange
        var hostiles = CreateHostiles(11);

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 600, null);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.Hardener &&
            md.Overload
        );

        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.ShieldBooster &&
            md.Overload
        );
    }

    [Fact]
    public void HandleMultipleHostiles_PrioritizesClosestTarget()
    {
        // Arrange
        var hostiles = new List<Target>
        {
            new Target(1, "Enemy1", "Frigate", 15000, true, false),
            new Target(2, "Enemy2", "Frigate", 8000, true, false),
            new Target(3, "Enemy3", "Frigate", 20000, true, false),
            new Target(4, "Enemy4", "Frigate", 12000, true, false),
            new Target(5, "Enemy5", "Frigate", 5000, true, false),
        };

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 200, null);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is LockTargetDecision ltd &&
            ltd.Target.Distance == 5000
        );
    }

    [Theory]
    [InlineData(4, false)]  // Below threshold
    [InlineData(5, true)]   // At threshold
    [InlineData(6, true)]   // Above threshold
    [InlineData(10, true)]  // Well above threshold
    public void HandleMultipleHostiles_EnemyCountThresholds_CorrectlyIdentifies(
        int enemyCount,
        bool shouldTakAction)
    {
        // Arrange
        var hostiles = CreateHostiles(enemyCount);

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 200, null);

        // Assert
        if (shouldTakAction)
        {
            response.Priority.Should().BeGreaterThanOrEqualTo(SituationPriority.LowCapacitor);
        }
        else
        {
            response.Priority.Should().Be(SituationPriority.Normal);
        }
    }

    [Fact]
    public void HandleMultipleHostiles_NoBeacon_StillProvidesDecisions()
    {
        // Arrange
        var hostiles = CreateHostiles(12);

        // Act
        var response = _handler.HandleMultipleHostiles(hostiles, 550, null);

        // Assert
        response.Priority.Should().Be(SituationPriority.MultipleHostiles);
        // Should still overheat when DPS is high
        response.Decisions.Should().Contain(d => d is ModuleDecision);
    }

    private List<Target> CreateHostiles(int count)
    {
        var hostiles = new List<Target>();
        for (int i = 0; i < count; i++)
        {
            hostiles.Add(new Target(
                i + 1,
                $"Enemy{i + 1}",
                "Frigate",
                5000 + (i * 1000),
                true,
                false
            ));
        }
        return hostiles;
    }
}
