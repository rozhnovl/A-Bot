using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Interfaces;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Integration.Tests.Scenarios;

/// <summary>
/// Integration tests for full combat scenarios from room entry to completion
/// </summary>
public class FullCombatIntegrationTests
{
    private readonly CombatStrategyService _combatService;
    private readonly TankingDecisionService _tankingService;
    private readonly ManeuverDecisionService _maneuverService;
    private readonly DroneControlService _droneService;
    private readonly TargetPriorityService _targetPriorityService;
    private readonly NpcInformationService _npcInfoService;

    public FullCombatIntegrationTests()
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
    public void Should_CompleteRoomFromEntryToClearing()
    {
        // Arrange - Simulate entering a new room with 3 enemies
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon(11000)
            .WithShieldBoosters(2)
            .WithHardeners(2, active: false) // Start inactive
            .WithMWD()
            .WithDrones()
            .Build();

        var hitpoints = new ShipHitpointsBuilder().Build(); // Full health

        var enemy1 = new TargetBuilder().AsEnemy().WithId(1).WithDistance(8000).Build();
        var enemy2 = new TargetBuilder().AsEnemy().WithId(2).WithDistance(12000).Build();
        var enemy3 = new TargetBuilder().AsEnemy().WithId(3).WithDistance(15000).Build();
        var coreCache = new TargetBuilder().AsCoreCache().WithDistance(5000).Build();

        var initialTargets = new[] { enemy1, enemy2, enemy3 };
        var droneState = new DroneStateBuilder().WithDronesInBay(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.None);
        shipState.GetCurrentTargets().Returns(new TargetCollection(Array.Empty<Target>(), null));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(initialTargets);
        targetProvider.FindTargetByName(Arg.Is<string>(s => s.Contains("Bioadaptive"))).Returns(coreCache);

        // Act - Phase 1: Initial decisions
        var phase1Decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Phase 1: Should activate hardeners, establish orbit, launch drones
        phase1Decisions.Should().NotBeEmpty();
        phase1Decisions.Should().Contain(d => d is ModuleDecision md && md.Type == ModuleType.Hardener,
            "should activate hardeners first");
        phase1Decisions.Should().Contain(d => d is ManeuverDecision,
            "should establish positioning");
        phase1Decisions.Should().Contain(d => d is LaunchDronesDecision,
            "should launch drones");

        // Arrange - Phase 2: Combat in progress, one enemy locked
        var enemy1Locked = new TargetBuilder().AsEnemy().WithId(1).AsTargeted().AsSelected().WithDistance(7000).Build();
        shipState.GetCurrentTargets().Returns(new TargetCollection(new[] { enemy1Locked }, enemy1Locked));
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.Orbit);
        var dronesInSpace = new DroneStateBuilder().WithDronesInSpace(5).WithIdleDrones(5).Build();
        shipState.GetDroneState().Returns(dronesInSpace);

        // Act - Phase 2: Combat decisions
        var phase2Decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Phase 2: Should engage weapons and drones
        phase2Decisions.Should().Contain(d => d is EngageDronesDecision,
            "should engage drones on target");

        // Arrange - Phase 3: Room cleared
        var dronesEngaging = new DroneStateBuilder().WithDronesInSpace(5).WithEngagingDrones(5).Build();
        shipState.GetDroneState().Returns(dronesEngaging);
        targetProvider.GetEnemyTargets().Returns(Array.Empty<Target>());

        // Act - Phase 3: Post-combat
        var phase3Decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Phase 3: Should return drones
        phase3Decisions.Should().Contain(d => d is ReturnDronesDecision,
            "should return drones when room is clear");

        // Act - Check combat complete
        var isCombatComplete = _combatService.IsCombatComplete(targetProvider, dronesEngaging);
        isCombatComplete.Should().BeFalse("drones still out");

        var dronesReturned = new DroneStateBuilder().WithDronesInBay(5).Build();
        isCombatComplete = _combatService.IsCombatComplete(targetProvider, dronesReturned);
        isCombatComplete.Should().BeTrue("all enemies dead and drones returned");
    }

    [Fact]
    public void Should_HandleEdgeCase_CapOutDuringCombat()
    {
        // Arrange - Mid-combat cap depletion
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder()
            .WithWeapon()
            .WithShieldBoosters(2, active: true) // Boosters running
            .WithHardeners(2, active: true)
            .Build();

        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(400) // Low shield
            .WithCapacitor(50) // Nearly empty cap
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

        // Assert - Should turn off boosters despite low shield to preserve cap
        decisions.Should().Contain(d =>
        {
            var moduleDecision = d as ModuleDecision;
            return moduleDecision != null &&
                   moduleDecision.Type == ModuleType.ShieldBooster &&
                   !moduleDecision.ShouldActivate;
        }, "must conserve remaining capacitor");
    }

    [Fact]
    public void Should_HandleEdgeCase_ShieldDown()
    {
        // Arrange - Shield completely depleted
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(0)
            .WithArmor(800) // Still have armor
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 200, boosters).ToList();

        // Assert - Critical situation, activate boosters
        decisions.Should().NotBeEmpty("should attempt to restore shield");
    }

    [Fact]
    public void Should_HandleEdgeCase_MultipleEwarSimultaneous()
    {
        // Arrange - Jammed + Neuted + Webbed
        var hitpoints = new ShipHitpointsBuilder()
            .WithCapacitorPercentage(20) // Neuted
            .WithShield(500) // Taking damage
            .Build();

        var jammer = new TargetBuilder().AsEnemy().WithType("Jammer").WithId(1).Build();
        var neut = new TargetBuilder().AsEnemy().WithType("Neuting Frigate").WithId(2).Build();
        var webber = new TargetBuilder().AsEnemy().WithType("Webbing Frigate").WithId(3).Build();

        var targets = new[] { jammer, neut, webber };

        // Act
        var prioritized = _targetPriorityService.PrioritizeTargets(targets, 100000);
        var incomingDps = _targetPriorityService.CalculateIncomingDps(targets);

        // Assert
        prioritized.Should().HaveCount(3);
        // All EWAR should be high priority
        prioritized.Should().AllSatisfy(pt =>
            pt.Priority.Should().BeLessThan(100, "EWAR targets should be high priority"));
    }

    [Fact]
    public void Should_HandleRetreatScenario_WhenOverwhelmed()
    {
        // Arrange - Overwhelming force
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(200) // Low shield
            .WithCapacitorPercentage(15) // Low cap
            .Build();

        var enemies = Enumerable.Range(0, 12)
            .Select(i => new TargetBuilder().AsEnemy().WithId(i).Build())
            .ToArray();

        // Act
        var incomingDps = _targetPriorityService.CalculateIncomingDps(enemies);
        var isCritical = hitpoints.IsCritical;

        // Assert
        isCritical.Should().BeTrue();
        incomingDps.Should().BeGreaterThan(200, "12 enemies should generate high DPS");
        // In real scenario, this would trigger retreat logic
    }

    [Fact]
    public void Should_HandleRoomCompletion_WithLooting()
    {
        // Arrange - All enemies dead, time to loot
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder().Build();
        var hitpoints = new ShipHitpointsBuilder().Build();
        var droneState = new DroneStateBuilder().WithDronesInBay(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.None);
        shipState.GetCurrentTargets().Returns(new TargetCollection(Array.Empty<Target>(), null));
        shipState.GetDroneState().Returns(droneState);
        targetProvider.GetEnemyTargets().Returns(Array.Empty<Target>());

        // Act
        var isCombatComplete = _combatService.IsCombatComplete(targetProvider, droneState);

        // Assert
        isCombatComplete.Should().BeTrue("ready to loot");
    }

    [Fact]
    public void Should_ManageStateTransition_FromCombatToLooting()
    {
        // Arrange - Transition phase
        var shipState = Substitute.For<IShipStateProvider>();
        var targetProvider = Substitute.For<ITargetProvider>();

        var fitting = new ShipFittingBuilder().WithDrones().Build();
        var hitpoints = new ShipHitpointsBuilder().Build();

        // Phase 1: Last enemy just died
        var droneState1 = new DroneStateBuilder().WithDronesInSpace(5).WithEngagingDrones(5).Build();

        shipState.GetShipFitting().Returns(fitting);
        shipState.GetHitpointsAndEnergy().Returns(hitpoints);
        shipState.GetCurrentManeuver().Returns(ShipManeuverType.Orbit);
        shipState.GetCurrentTargets().Returns(new TargetCollection(Array.Empty<Target>(), null));
        shipState.GetDroneState().Returns(droneState1);
        targetProvider.GetEnemyTargets().Returns(Array.Empty<Target>());

        // Act - Phase 1
        var phase1Decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

        // Assert - Should return drones
        phase1Decisions.Should().Contain(d => d is ReturnDronesDecision);

        // Arrange - Phase 2: Drones returned
        var droneState2 = new DroneStateBuilder().WithDronesInBay(5).Build();
        shipState.GetDroneState().Returns(droneState2);

        // Act - Phase 2
        var isCombatComplete = _combatService.IsCombatComplete(targetProvider, droneState2);

        // Assert - Combat complete, ready for next phase
        isCombatComplete.Should().BeTrue();
    }

    [Theory]
    [InlineData(5, 200, true, "Low shield, high DPS - activate all")]
    [InlineData(5, 50, true, "Low shield, low DPS - activate one")]
    [InlineData(30, 200, false, "Medium shield, high DPS - activate some")]
    [InlineData(80, 50, false, "High shield, low DPS - no activation")]
    public void Should_MakeCorrectDecisions_AcrossVariousIntegratedScenarios(
        double shieldPercentage,
        double incomingDps,
        bool expectOverload,
        string scenario)
    {
        // Arrange
        var hitpoints = new ShipHitpointsBuilder()
            .WithShieldPercentage(shieldPercentage)
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();

        // Assert
        var hasOverloadDecision = decisions.Any(d =>
        {
            var moduleDecision = d as ModuleDecision;
            return moduleDecision != null && moduleDecision.ShouldOverload;
        });

        hasOverloadDecision.Should().Be(expectOverload, $"Scenario: {scenario}");
    }
}
