using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services.Situations;

namespace AbyssalBot.Domain.Tests.Situations;

public class CriticalDamageHandlerTests
{
    private readonly CriticalDamageHandler _handler;

    public CriticalDamageHandlerTests()
    {
        _handler = new CriticalDamageHandler();
    }

    [Fact]
    public void HandleCriticalDamage_NormalHitpoints_ReturnsNoAction()
    {
        // Arrange
        var status = CreateShipStatus(
            shieldPercentage: 80,
            armorPercentage: 100,
            hullPercentage: 100
        );

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        response.Priority.Should().Be(SituationPriority.Normal);
        response.HasDecisions.Should().BeFalse();
    }

    [Fact]
    public void HandleCriticalDamage_CriticalStructure_OverheatsAllModules()
    {
        // Arrange
        var status = CreateShipStatus(hullPercentage: 40);

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        response.Priority.Should().Be(SituationPriority.CriticalDamage);
        response.Situation.Should().Be("Critical Structure Damage");

        // Should overheat shield boosters
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.ShieldBooster &&
            md.Overload
        );

        // Should overheat hardeners
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.Hardener &&
            md.Overload
        );
    }

    [Fact]
    public void HandleCriticalDamage_CriticalShield_OverheatsShieldBoosters()
    {
        // Arrange
        var status = CreateShipStatus(shieldPercentage: 15);

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        response.Priority.Should().Be(SituationPriority.CriticalDamage);
        response.Situation.Should().Be("Critical Shield Damage");

        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.ShieldBooster &&
            md.Overload
        );
    }

    [Fact]
    public void HandleCriticalDamage_CriticalShield_OverheatsHardeners()
    {
        // Arrange
        var status = CreateShipStatus(shieldPercentage: 18);

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.Hardener &&
            md.Overload
        );
    }

    [Fact]
    public void HandleCriticalDamage_CriticalArmor_OverheatsTankModules()
    {
        // Arrange
        var status = CreateShipStatus(
            shieldPercentage: 0,
            armorPercentage: 25
        );

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        response.Priority.Should().Be(SituationPriority.CriticalDamage);
        response.Situation.Should().Be("Critical Armor Damage");

        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.Overload
        );
    }

    [Fact]
    public void HandleCriticalDamage_InactiveBoosters_ActivatesAndOverheats()
    {
        // Arrange
        var status = CreateShipStatus(
            shieldPercentage: 15,
            boostersActive: false
        );

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.ShieldBooster &&
            md.Activate &&
            md.Overload
        );
    }

    [Theory]
    [InlineData(51, false)] // Just above threshold
    [InlineData(50, false)] // At threshold
    [InlineData(49, true)]  // Below threshold
    [InlineData(25, true)]  // Well below threshold
    public void HandleCriticalDamage_StructureThresholds_CorrectlyIdentifiesCritical(
        double hullPercentage,
        bool shouldBeCritical)
    {
        // Arrange
        var status = CreateShipStatus(hullPercentage: hullPercentage);

        // Act
        var response = _handler.HandleCriticalDamage(status);

        // Assert
        if (shouldBeCritical)
        {
            response.Priority.Should().Be(SituationPriority.CriticalDamage);
        }
        else
        {
            response.Priority.Should().Be(SituationPriority.Normal);
        }
    }

    private ShipStatus CreateShipStatus(
        double shieldPercentage = 100,
        double armorPercentage = 100,
        double hullPercentage = 100,
        bool boostersActive = true)
    {
        var hitpoints = new ShipHitpointsAndEnergy(
            Shield: shieldPercentage * 10,
            Armor: armorPercentage * 10,
            Hull: hullPercentage * 5,
            Capacitor: 1000,
            MaxShield: 1000,
            MaxArmor: 1000,
            MaxHull: 500,
            MaxCapacitor: 1000
        );

        var modules = new List<ShipModule>
        {
            new ShipModule(ModuleType.ShieldBooster, IsActive: boostersActive),
            new ShipModule(ModuleType.Hardener, IsActive: true)
        };

        var fitting = new ShipFitting(
            HighSlots: Array.Empty<ShipModule>(),
            MidSlots: modules,
            LowSlots: Array.Empty<ShipModule>(),
            MaxTargetingRange: 50000,
            MaxTargets: 5,
            MaxDronesInSpace: 5
        );

        return new ShipStatus(hitpoints, fitting, 300);
    }
}
