using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services.Situations;

namespace AbyssalBot.Domain.Tests.Situations;

public class EwarSituationHandlerTests
{
    private readonly EwarSituationHandler _handler;

    public EwarSituationHandlerTests()
    {
        _handler = new EwarSituationHandler();
    }

    [Fact]
    public void HandleEwar_NoEwar_ReturnsNoAction()
    {
        // Arrange
        var context = CreateEwarContext(EwarType.None);

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.Normal);
        response.HasDecisions.Should().BeFalse();
    }

    [Fact]
    public void HandleEwar_Jammed_RetreatsWhenDamaged()
    {
        // Arrange
        var context = CreateEwarContext(
            EwarType.Jammed,
            shieldPercentage: 40
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.Ewar);
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.KeepAtRange
        );
    }

    [Fact]
    public void HandleEwar_Jammed_ApproachesJammerWhenHealthy()
    {
        // Arrange
        var jammer = new Target(1, "Jammer", "Frigate", 20000, true);
        var context = CreateEwarContext(
            EwarType.Jammed,
            shieldPercentage: 80,
            ewarSource: jammer
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.Approach &&
            md.Target == jammer
        );
    }

    [Fact]
    public void HandleEwar_Dampened_ClosesRange()
    {
        // Arrange
        var enemy = new Target(2, "Enemy", "Cruiser", 15000, true);
        var context = CreateEwarContext(
            EwarType.Dampened,
            enemies: new[] { enemy }
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.Approach
        );
    }

    [Fact]
    public void HandleEwar_TrackingDisrupted_IncreasesRange()
    {
        // Arrange
        var target = new Target(3, "Target", "Frigate", 5000, true, true);
        var context = CreateEwarContext(
            EwarType.TrackingDisrupted,
            enemies: new[] { target }
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.KeepAtRange &&
            md.Distance == 15000
        );
    }

    [Fact]
    public void HandleEwar_Webbed_LocksTackleSource()
    {
        // Arrange
        var tackle = new Target(4, "Tackle", "Frigate", 3000, true, false);
        var context = CreateEwarContext(
            EwarType.Webbed,
            ewarSource: tackle
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is LockTargetDecision ltd &&
            ltd.Target == tackle
        );
    }

    [Fact]
    public void HandleEwar_Scrambled_OverheatsHardeners()
    {
        // Arrange
        var context = CreateEwarContext(EwarType.Scrambled);

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.Hardener &&
            md.Overload
        );
    }

    [Fact]
    public void HandleEwar_Neutralized_LocksNeutSource()
    {
        // Arrange
        var neut = new Target(5, "Neutralizer", "Cruiser", 10000, true, false);
        var context = CreateEwarContext(
            EwarType.Neutralized,
            ewarSource: neut
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is LockTargetDecision ltd &&
            ltd.Target == neut
        );
    }

    [Fact]
    public void HandleEwar_Neutralized_DisablesMWDWhenLowCap()
    {
        // Arrange
        var context = CreateEwarContext(
            EwarType.Neutralized,
            capacitorPercentage: 25
        );

        // Act
        var response = _handler.HandleEwar(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.MWD &&
            !md.Activate
        );
    }

    private EwarContext CreateEwarContext(
        EwarType ewarType,
        double shieldPercentage = 100,
        double capacitorPercentage = 100,
        Target? ewarSource = null,
        Target[]? enemies = null)
    {
        var hitpoints = new ShipHitpointsAndEnergy(
            Shield: shieldPercentage * 10,
            Armor: 1000,
            Hull: 500,
            Capacitor: capacitorPercentage * 10,
            MaxShield: 1000,
            MaxArmor: 1000,
            MaxHull: 500,
            MaxCapacitor: 1000
        );

        var availableTargets = enemies ?? Array.Empty<Target>();

        return new EwarContext(
            ewarType,
            ewarSource,
            hitpoints,
            availableTargets
        );
    }
}
