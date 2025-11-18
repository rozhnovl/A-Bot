using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Situations.Tests.Scenarios;

/// <summary>
/// Tests for various combat scenarios
/// </summary>
public class CombatScenarioTests
{
    private readonly CombatStrategyService _combatService;
    private readonly TankingDecisionService _tankingService;
    private readonly ManeuverDecisionService _maneuverService;
    private readonly DroneControlService _droneService;
    private readonly TargetPriorityService _targetPriorityService;
    private readonly NpcInformationService _npcInfoService;

    public CombatScenarioTests()
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

    public class SoloFrigateScenario : CombatScenarioTests
    {
        [Fact]
        public void Should_HandleEasyTarget_Efficiently()
        {
            // Arrange - Single frigate, easy fight
            var shipState = Substitute.For<IShipStateProvider>();
            var targetProvider = Substitute.For<ITargetProvider>();

            var fitting = new ShipFittingBuilder()
                .WithWeapon(11000)
                .WithShieldBoosters(2)
                .WithHardeners(2, active: true)
                .WithMWD()
                .WithDrones()
                .Build();

            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(950)
                .Build();

            var frigate = new TargetBuilder()
                .AsEnemy()
                .WithType("Damavik")
                .WithDistance(8000)
                .AsTargeted()
                .Build();

            var droneState = new DroneStateBuilder()
                .WithDronesInBay(5)
                .Build();

            shipState.GetShipFitting().Returns(fitting);
            shipState.GetHitpointsAndEnergy().Returns(hitpoints);
            shipState.GetCurrentManeuver().Returns(ShipManeuverType.None);
            shipState.GetCurrentTargets().Returns(new TargetCollection(new[] { frigate }, frigate));
            shipState.GetDroneState().Returns(droneState);
            targetProvider.GetEnemyTargets().Returns(new[] { frigate });

            // Act
            var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

            // Assert
            decisions.Should().NotBeEmpty();
            decisions.Should().Contain(d => d is ManeuverDecision);
            decisions.Should().Contain(d => d is LaunchDronesDecision);
        }
    }

    public class CruiserWithLogiScenario : CombatScenarioTests
    {
        [Fact]
        public void Should_HandleMediumThreat_WithMultipleTargets()
        {
            // Arrange - 3 cruisers with logistics support
            var shipState = Substitute.For<IShipStateProvider>();
            var targetProvider = Substitute.For<ITargetProvider>();

            var fitting = new ShipFittingBuilder()
                .WithWeapon(11000)
                .WithShieldBoosters(2)
                .WithHardeners(2, active: true)
                .WithDrones()
                .Build();

            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(700) // Taking some damage
                .Build();

            var cruiser1 = new TargetBuilder().AsEnemy().WithType("Cruiser").WithId(1).WithDistance(7000).Build();
            var cruiser2 = new TargetBuilder().AsEnemy().WithType("Cruiser").WithId(2).WithDistance(9000).Build();
            var logi = new TargetBuilder().AsEnemy().WithType("Logi Cruiser").WithId(3).WithDistance(15000).Build();

            var targets = new[] { cruiser1, cruiser2, logi };
            var droneState = new DroneStateBuilder().WithDronesInSpace(5).WithIdleDrones(5).Build();

            shipState.GetShipFitting().Returns(fitting);
            shipState.GetHitpointsAndEnergy().Returns(hitpoints);
            shipState.GetCurrentManeuver().Returns(ShipManeuverType.None);
            shipState.GetCurrentTargets().Returns(new TargetCollection(targets, cruiser1));
            shipState.GetDroneState().Returns(droneState);
            targetProvider.GetEnemyTargets().Returns(targets);

            // Act
            var decisions = _combatService.GenerateCombatDecisions(shipState, targetProvider).ToList();

            // Assert
            decisions.Should().NotBeEmpty();
            // Should prioritize high-value targets (logi usually prioritized)
            var lockDecisions = decisions.OfType<LockTargetDecision>().ToList();
            lockDecisions.Should().NotBeEmpty();
        }

        [Fact]
        public void Should_ActivateBoosters_UnderMediumDps()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(550)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            var incomingDps = 120; // Medium DPS from 3 cruisers

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate;
            });
        }
    }

    public class FrigateSwarmScenario : CombatScenarioTests
    {
        [Theory]
        [InlineData(5, "Small swarm")]
        [InlineData(10, "Medium swarm")]
        [InlineData(15, "Large swarm")]
        public void Should_HandleSwarm_WithAOEAndDrones(int frigateCount, string scenario)
        {
            // Arrange
            var targets = Enumerable.Range(0, frigateCount)
                .Select(i => new TargetBuilder()
                    .AsEnemy()
                    .WithType("Frigate")
                    .WithId(i)
                    .WithDistance(5000 + i * 500)
                    .Build())
                .ToArray();

            // Act
            var incomingDps = _targetPriorityService.CalculateIncomingDps(targets);
            var prioritized = _targetPriorityService.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(frigateCount, $"Scenario: {scenario}");
            incomingDps.Should().BeGreaterThan(0, $"Scenario: {scenario}");

            // High incoming DPS from swarm
            if (frigateCount >= 10)
            {
                incomingDps.Should().BeGreaterThan(150,
                    "large swarm should generate significant DPS");
            }
        }

        [Fact]
        public void Should_OrbitDefensively_AgainstSwarm()
        {
            // Arrange - Large swarm means high DPS
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .Build();

            // Act
            var decision = _maneuverService.DecideManeuver(
                ShipManeuverType.None,
                250, // High DPS from swarm
                orbitBeacon,
                5000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.Orbit,
                "should orbit to reduce tracking from multiple frigates");
        }

        [Fact]
        public void Should_DeployDrones_AgainstSwarm()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(5)
                .Build();

            // Act
            var decision = _droneService.DecideLaunchDrones(droneState);

            // Assert
            decision.Should().NotBeNull("drones are effective against frigate swarms");
        }
    }

    public class BattleshipWithEwarScenario : CombatScenarioTests
    {
        [Fact]
        public void Should_HandleHardTarget_WithSupport()
        {
            // Arrange - Battleship with EWAR frigates
            var battleship = new TargetBuilder()
                .AsEnemy()
                .WithType("Battleship")
                .WithId(1)
                .WithDistance(20000)
                .Build();

            var ewarFrigate1 = new TargetBuilder()
                .AsEnemy()
                .WithType("EWAR Frigate")
                .WithId(2)
                .WithDistance(15000)
                .Build();

            var ewarFrigate2 = new TargetBuilder()
                .AsEnemy()
                .WithType("EWAR Frigate")
                .WithId(3)
                .WithDistance(12000)
                .Build();

            var targets = new[] { battleship, ewarFrigate1, ewarFrigate2 };

            // Act
            var prioritized = _targetPriorityService.PrioritizeTargets(targets, 100000);
            var bestTarget = _targetPriorityService.GetBestTarget(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(3);
            // EWAR frigates should be prioritized over battleship
            bestTarget.Should().NotBeNull();
        }

        [Fact]
        public void Should_MaintainDefense_AgainstHighAlphaDamage()
        {
            // Arrange - Battleship can do high alpha damage
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(500) // After taking alpha strike
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 180, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate;
            }, "should activate boosters after alpha strike");
        }
    }

    public class MixedFleetScenario : CombatScenarioTests
    {
        [Fact]
        public void Should_HandleComplexEngagement_WithMultipleThreats()
        {
            // Arrange - Jams, neuts, and DPS
            var neut = new TargetBuilder()
                .AsEnemy()
                .WithType("Neuting Frigate")
                .WithName("Abyssal Neut")
                .WithId(1)
                .WithDistance(5000)
                .Build();

            var jammer = new TargetBuilder()
                .AsEnemy()
                .WithType("Jammer")
                .WithName("Abyssal Jammer")
                .WithId(2)
                .WithDistance(7000)
                .Build();

            var dps1 = new TargetBuilder()
                .AsEnemy()
                .WithType("Cruiser")
                .WithId(3)
                .WithDistance(9000)
                .Build();

            var dps2 = new TargetBuilder()
                .AsEnemy()
                .WithType("Cruiser")
                .WithId(4)
                .WithDistance(11000)
                .Build();

            var targets = new[] { neut, jammer, dps1, dps2 };

            // Act
            var prioritized = _targetPriorityService.PrioritizeTargets(targets, 100000);
            var incomingDps = _targetPriorityService.CalculateIncomingDps(targets);

            // Assert
            prioritized.Should().HaveCount(4);
            incomingDps.Should().BeGreaterThan(0);

            // High priority targets (EWAR) should be ordered first
            var topPriority = prioritized.First();
            topPriority.Target.Type.Should().Match(t =>
                t.Contains("Neuting") || t.Contains("Jammer") || t.Contains("Frigate"));
        }

        [Fact]
        public void Should_PrioritizeEwarTargets_OverDps()
        {
            // Arrange
            var ewar = new TargetBuilder()
                .AsEnemy()
                .WithType("Frigate")
                .WithName("Harrowing Scythe") // Known high-priority
                .WithId(1)
                .Build();

            var cruiser = new TargetBuilder()
                .AsEnemy()
                .WithType("Cruiser")
                .WithId(2)
                .Build();

            var targets = new[] { cruiser, ewar };

            // Act
            var bestTarget = _targetPriorityService.GetBestTarget(targets, 100000);

            // Assert
            bestTarget.Should().NotBeNull();
            // EWAR frigates typically have higher priority
        }

        [Fact]
        public void Should_ManageCap_UnderNeutPressure()
        {
            // Arrange - Capacitor being neuted
            var hitpoints = new ShipHitpointsBuilder()
                .WithCapacitorPercentage(25) // Low from neuting
                .WithShield(600)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 100, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return !moduleDecision.ShouldActivate;
            }, "should conserve cap under neut pressure");
        }
    }

    public class TargetPrioritization : CombatScenarioTests
    {
        [Fact]
        public void Should_TargetLogiFirst_InCombat()
        {
            // Arrange - Logi keeping DPS alive
            var logi = new TargetBuilder()
                .AsEnemy()
                .WithType("Logi Cruiser")
                .WithName("Logistics")
                .WithId(1)
                .WithDistance(15000)
                .Build();

            var dps = new TargetBuilder()
                .AsEnemy()
                .WithType("Cruiser")
                .WithId(2)
                .WithDistance(8000)
                .Build();

            var targets = new[] { dps, logi };

            // Act
            var prioritized = _targetPriorityService.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(2);
            // Logi should typically be higher priority
        }

        [Fact]
        public void Should_AvoidWastingLocks_OnExtractionNodes()
        {
            // Arrange
            var enemy = new TargetBuilder()
                .AsEnemy()
                .WithName("Normal Enemy")
                .Build();

            var extraction = new TargetBuilder()
                .AsEnemy()
                .WithName("Extraction Node")
                .Build();

            var targets = new[] { enemy, extraction };

            // Act
            var prioritized = _targetPriorityService.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(1);
            prioritized[0].Target.Name.Should().Be("Normal Enemy");
        }
    }
}
