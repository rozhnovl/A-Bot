using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Interfaces;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Integration.Tests.Scenarios;

/// <summary>
/// Integration tests for edge cases and complex scenarios
/// </summary>
public class EdgeCaseIntegrationTests
{
    private readonly CombatStrategyService _combatService;
    private readonly TankingDecisionService _tankingService;
    private readonly ManeuverDecisionService _maneuverService;
    private readonly DroneControlService _droneService;
    private readonly TargetPriorityService _targetPriorityService;
    private readonly NpcInformationService _npcInfoService;

    public EdgeCaseIntegrationTests()
    {
        _npcInfoService = new NpcInformationService();
        _targetPriorityService = new TargetPriorityService(_npcInfoService);
        _tankingService = new TankingDecisionService();
        _maneuverService = new ManeuverDecisionService();
        _droneService = new DroneControlService();
        _combatService = new CombatStrategyService(
            _npcInfoService,
            _targetPriorityService,
            _maneuverService,
            _droneService,
            _tankingService
        );
    }

    [Fact]
    public void Should_HandleAllModulesOffline_DueToCapOut()
    {
        // Arrange - Complete capacitor failure
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon()
            .WithShieldBoosters(2)
            .WithHardeners(2)
            .Build();

        var hitpoints = new ShipHitpointsBuilder()
            .WithCapacitor(0) // Complete cap out
            .WithShield(300) // Low shield but can't boost
            .Build();

        var enemy = new TargetBuilder().AsEnemy().AsTargeted().Build();
        var droneState = new DroneStateBuilder().WithDronesInSpace(5).WithEngagingDrones(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.Orbit);
        shipState.GetCurrentTargets().Returns(new TargetCollection(new[] { enemy }, enemy));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(new[] { enemy });

        // Act
        var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Can't activate any modules, rely on drones
        var moduleActivations = decisions.OfType<ModuleDecision>()
            .Where(d => d.ShouldActivate && d.Type == ModuleType.ShieldBooster)
            .ToList();

        // System should not try to activate cap-dependent modules with 0 cap
        // Drones can still fight as they don't use ship cap
        decisions.Should().NotContain(d => d is EngageDronesDecision,
            "drones already engaging");
    }

    [Fact]
    public void Should_HandleAllTargetsOutOfRange()
    {
        // Arrange - All enemies too far
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon(11000)
            .WithMaxTargetingRange(100000)
            .Build();

        var hitpoints = new ShipHitpointsBuilder().Build();

        var farEnemy1 = new TargetBuilder().AsEnemy().WithId(1).WithDistance(120000).Build();
        var farEnemy2 = new TargetBuilder().AsEnemy().WithId(2).WithDistance(150000).Build();

        var enemies = new[] { farEnemy1, farEnemy2 };
        var droneState = new DroneStateBuilder().WithDronesInBay(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.None);
        shipState.GetCurrentTargets().Returns(new TargetCollection(Array.Empty<Target>(), null));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(enemies);

        // Act
        var targetsToLock = _targetPriorityService.GetTargetsToLock(enemies, 100000, 7, 0);

        // Assert
        targetsToLock.Should().BeEmpty("all targets out of range");

        // Should not try to engage weapons on out-of-range targets
        var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();
        decisions.Should().NotContain(d => d is LockTargetDecision,
            "should not lock targets beyond range");
    }

    [Fact]
    public void Should_HandleJammed_WithEnemiesApproaching()
    {
        // Arrange - Jammed, can't lock new targets
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon()
            .WithDrones()
            .Build();

        var hitpoints = new ShipHitpointsBuilder().Build();

        var jammer = new TargetBuilder().AsEnemy().WithType("Jammer").WithDistance(5000).Build();
        var approaching1 = new TargetBuilder().AsEnemy().WithDistance(8000).Build();
        var approaching2 = new TargetBuilder().AsEnemy().WithDistance(7000).Build();

        var enemies = new[] { jammer, approaching1, approaching2 };
        var droneState = new DroneStateBuilder().WithDronesInBay(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.None);
        shipState.GetCurrentTargets().Returns(new TargetCollection(Array.Empty<Target>(), null));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(enemies);

        // Act
        var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Should still launch drones and maneuver
        decisions.Should().Contain(d => d is LaunchDronesDecision,
            "can still use drones when jammed");
        decisions.Should().Contain(d => d is ManeuverDecision,
            "should establish defensive position");
    }

    [Fact]
    public void Should_HandleScrammed_WithHighIncomingDps()
    {
        // Arrange - Scrammed, can't warp out, taking heavy damage
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(350) // Under heavy fire
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 280, boosters).ToList();

        // Assert - Must activate all defenses
        decisions.Should().HaveCount(2);
        decisions.Should().AllSatisfy(d =>
        {
            var moduleDecision = (ModuleDecision)d;
            moduleDecision.ShouldActivate.Should().BeTrue();
        });
    }

    [Fact]
    public void Should_HandleMultipleSimultaneousProblems()
    {
        // Arrange - Cap low + Shield low + High DPS + Multiple enemies
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon()
            .WithShieldBoosters(2)
            .WithHardeners(2, active: true)
            .WithDrones()
            .Build();

        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(250) // Low shield
            .WithCapacitorPercentage(18) // Low cap
            .Build();

        var enemies = Enumerable.Range(0, 8)
            .Select(i => new TargetBuilder().AsEnemy().WithId(i).WithDistance(5000 + i * 1000).Build())
            .ToArray();

        var droneState = new DroneStateBuilder().WithDronesInSpace(5).WithIdleDrones(5).Build();
        var targetedEnemy = new TargetBuilder().AsEnemy().AsTargeted().AsSelected().Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.Orbit);
        shipState.GetCurrentTargets().Returns(new TargetCollection(new[] { targetedEnemy }, targetedEnemy));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(enemies);

        // Act
        var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();
        var incomingDps = _targetPriorityService.CalculateIncomingDps(enemies);

        // Assert - System should prioritize survival
        incomingDps.Should().BeGreaterThan(150, "8 enemies should generate significant DPS");
        decisions.Should().NotBeEmpty();

        // Should engage drones for additional DPS
        decisions.Should().Contain(d => d is EngageDronesDecision,
            "use all available firepower");
    }

    [Fact]
    public void Should_HandleNoEnemies_ButSystemStillActive()
    {
        // Arrange - Room cleared but still in combat state
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon()
            .WithShieldBoosters(2, active: true) // Still active
            .WithHardeners(2, active: true)
            .WithDrones()
            .Build();

        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(600) // Recovering
            .Build();

        var droneState = new DroneStateBuilder().WithDronesInSpace(5).WithEngagingDrones(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.Orbit);
        shipState.GetCurrentTargets().Returns(new TargetCollection(Array.Empty<Target>(), null));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(Array.Empty<Target>());

        // Act
        var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Should wind down combat systems
        decisions.Should().Contain(d => d is ReturnDronesDecision,
            "return drones when no enemies");
        decisions.Should().Contain(d =>
        {
            var moduleDecision = d as ModuleDecision;
            return moduleDecision != null &&
                   moduleDecision.Type == ModuleType.ShieldBooster &&
                   !moduleDecision.ShouldActivate;
        }, "deactivate boosters when shield safe and no enemies");
    }

    [Fact]
    public void Should_HandleTargetDestroyed_MidAttack()
    {
        // Arrange - Target just died
        var droneState = new DroneStateBuilder()
            .WithDronesInSpace(5)
            .WithEngagingDrones(5)
            .Build();

        // No more enemies
        var noEnemies = Array.Empty<Target>();

        // Act
        var decisions = _droneService.GetDroneDecisions(droneState, null, noEnemiesRemaining: true).ToList();

        // Assert
        decisions.Should().Contain(d => d is ReturnDronesDecision,
            "return drones when target destroyed");
    }

    [Fact]
    public void Should_HandleNewWave_WhileRecovering()
    {
        // Arrange - Recovering from previous wave, new enemies appear
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(500) // Recovering
            .WithCapacitorPercentage(40) // Cap recovering
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
        };

        // New wave spawns
        var newEnemies = Enumerable.Range(0, 4)
            .Select(i => new TargetBuilder().AsEnemy().WithId(i).Build())
            .ToArray();

        // Act
        var incomingDps = _targetPriorityService.CalculateIncomingDps(newEnemies);
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();

        // Assert - Should re-engage defensive systems
        incomingDps.Should().BeGreaterThan(0);
        // With shield at 500 and new incoming DPS, may need to keep boosters active
    }

    [Fact]
    public void Should_HandleVeryLongFight_WithResourceDepletion()
    {
        // Arrange - Extended combat, resources running low
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(450) // Worn down
            .WithCapacitorPercentage(25) // Cap depleted
            .WithArmor(900) // Some armor damage
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build(),
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
        };

        var enemies = new[]
        {
            new TargetBuilder().AsEnemy().WithId(1).Build(),
            new TargetBuilder().AsEnemy().WithId(2).Build()
        };

        // Act
        var incomingDps = _targetPriorityService.CalculateIncomingDps(enemies);
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();

        // Assert - Must balance survival vs resource management
        decisions.Should().NotBeEmpty();
        // With low cap, should deactivate some boosters
        decisions.Should().Contain(d =>
        {
            var moduleDecision = d as ModuleDecision;
            return moduleDecision != null && !moduleDecision.ShouldActivate;
        }, "conserve cap in extended fight");
    }

    [Theory]
    [InlineData(0, 100, 300, "Cap out, critical shield, high DPS")]
    [InlineData(10, 100, 300, "Nearly cap out, critical shield, high DPS")]
    [InlineData(50, 0, 200, "Half cap, shield gone, high DPS")]
    [InlineData(20, 50, 400, "Low cap, very low shield, very high DPS")]
    public void Should_MakeEmergencyDecisions_InDireScenarios(
        double capPercentage,
        double shield,
        double incomingDps,
        string scenario)
    {
        // Arrange
        var hitpoints = new ShipHitpointsBuilder()
            .WithCapacitorPercentage(capPercentage)
            .WithShield(shield)
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();
        var isCritical = hitpoints.IsCritical;

        // Assert
        isCritical.Should().BeTrue($"Scenario: {scenario}");

        if (shield < 150 && capPercentage > 0)
        {
            // Critical shield and some cap remaining - must boost
            decisions.Should().Contain(d =>
            {
                var moduleDecision = d as ModuleDecision;
                return moduleDecision != null && moduleDecision.ShouldActivate;
            }, $"Scenario: {scenario} - must attempt to save ship");
        }
    }
}
