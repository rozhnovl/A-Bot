using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services.Situations;

namespace AbyssalBot.Domain.Tests.Situations;

public class LowCapacitorHandlerTests
{
    private readonly LowCapacitorHandler _handler;

    public LowCapacitorHandlerTests()
    {
        _handler = new LowCapacitorHandler();
    }

    [Fact]
    public void HandleLowCapacitor_NormalCap_ReturnsNoAction()
    {
        // Arrange
        var context = CreateContext(capacitorPercentage: 50);

        // Act
        var response = _handler.HandleLowCapacitor(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.Normal);
        response.HasDecisions.Should().BeFalse();
    }

    [Fact]
    public void HandleLowCapacitor_EmergencyCap_DisablesMWD()
    {
        // Arrange
        var context = CreateContext(
            capacitorPercentage: 8,
            mwdActive: true
        );

        // Act
        var response = _handler.HandleLowCapacitor(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.CapacitorEmergency);
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.MWD &&
            !md.Activate
        );
    }

    [Fact]
    public void HandleLowCapacitor_CriticalCap_DisablesWeapons()
    {
        // Arrange
        var context = CreateContext(
            capacitorPercentage: 4,
            weaponActive: true
        );

        // Act
        var response = _handler.HandleLowCapacitor(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.CapacitorEmergency);
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.Weapon &&
            !md.Activate
        );
    }

    [Fact]
    public void HandleLowCapacitor_EmergencyCap_KitesAway()
    {
        // Arrange
        var target = new Target(1, "Enemy", "Frigate", 10000, true, true);
        var context = CreateContext(
            capacitorPercentage: 8,
            activeTarget: target
        );

        // Act
        var response = _handler.HandleLowCapacitor(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ManeuverDecision md &&
            md.ManeuverType == ShipManeuverType.KeepAtRange &&
            md.Distance == 20000
        );
    }

    [Fact]
    public void HandleLowCapacitor_LowCap_DisablesMWDAtCloseRange()
    {
        // Arrange
        var target = new Target(1, "Enemy", "Frigate", 5000, true, true);
        var context = CreateContext(
            capacitorPercentage: 25,
            mwdActive: true,
            activeTarget: target
        );

        // Act
        var response = _handler.HandleLowCapacitor(context);

        // Assert
        response.Priority.Should().Be(SituationPriority.LowCapacitor);
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.MWD &&
            !md.Activate
        );
    }

    [Fact]
    public void HandleLowCapacitor_LowCap_ReducesShieldBoosters()
    {
        // Arrange
        var context = CreateContext(
            capacitorPercentage: 25,
            shieldPercentage: 40,
            multipleBoostersActive: true
        );

        // Act
        var response = _handler.HandleLowCapacitor(context);

        // Assert
        response.Decisions.Should().Contain(d =>
            d is ModuleDecision md &&
            md.ModuleType == ModuleType.ShieldBooster &&
            !md.Activate
        );
    }

    private CombatContext CreateContext(
        double capacitorPercentage = 100,
        double shieldPercentage = 100,
        bool mwdActive = false,
        bool weaponActive = false,
        bool multipleBoostersActive = false,
        Target? activeTarget = null)
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

        var modules = new List<ShipModule>();

        if (mwdActive || weaponActive || multipleBoostersActive)
        {
            modules.Add(new ShipModule(ModuleType.MWD, IsActive: mwdActive));
            modules.Add(new ShipModule(ModuleType.Weapon, IsActive: weaponActive));

            if (multipleBoostersActive)
            {
                modules.Add(new ShipModule(ModuleType.ShieldBooster, IsActive: true));
                modules.Add(new ShipModule(ModuleType.ShieldBooster, IsActive: true));
            }
        }

        var fitting = new ShipFitting(
            HighSlots: modules.Where(m => m.Type == ModuleType.Weapon).ToList(),
            MidSlots: modules.Where(m => m.Type == ModuleType.MWD || m.Type == ModuleType.ShieldBooster).ToList(),
            LowSlots: Array.Empty<ShipModule>(),
            MaxTargetingRange: 50000,
            MaxTargets: 5,
            MaxDronesInSpace: 5
        );

        var targets = activeTarget != null
            ? new TargetCollection(new[] { activeTarget }, activeTarget)
            : new TargetCollection(Array.Empty<Target>(), null);

        return new CombatContext(
            fitting,
            hitpoints,
            ShipManeuverType.Orbit,
            targets,
            new DroneState(0, 5, true),
            0,
            activeTarget != null ? new[] { activeTarget } : Array.Empty<Target>()
        );
    }
}
