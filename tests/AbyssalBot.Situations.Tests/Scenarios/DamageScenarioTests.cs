using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Situations.Tests.Scenarios;

/// <summary>
/// Tests for various damage scenarios affecting different tank layers
/// </summary>
public class DamageScenarioTests
{
    private readonly TankingDecisionService _tankingService;
    private readonly ManeuverDecisionService _maneuverService;
    private readonly TargetPriorityService _targetPriorityService;
    private readonly NpcInformationService _npcInfoService;

    public DamageScenarioTests()
    {
        _tankingService = new TankingDecisionService();
        _maneuverService = new ManeuverDecisionService();
        _npcInfoService = new NpcInformationService();
        _targetPriorityService = new TargetPriorityService(_npcInfoService);
    }

    public class ShieldTankingScenarios : DamageScenarioTests
    {
        [Fact]
        public void Should_HandleGradualDamage()
        {
            // Arrange - Shield slowly declining
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(550)
                .WithMaxShield(1000)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate && !moduleDecision.ShouldOverload;
            }, "gradual damage should activate single booster");
        }

        [Fact]
        public void Should_HandleSpikeDamage()
        {
            // Arrange - Sudden shield drop
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(120) // Critical after spike
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 250, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
                moduleDecision.ShouldOverload.Should().BeTrue();
            });
        }

        [Fact]
        public void Should_HandleSustainedHighDamage()
        {
            // Arrange - Continuous heavy damage
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(500)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 250, boosters).ToList();

            // Assert - High sustained DPS should activate all boosters
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
            });
        }

        [Theory]
        [InlineData(900, 50, false, "Shield high, low DPS - no action")]
        [InlineData(700, 50, false, "Shield medium, low DPS - no action")]
        [InlineData(550, 50, true, "Shield low, low DPS - activate booster")]
        [InlineData(550, 200, true, "Shield low, high DPS - activate boosters")]
        [InlineData(120, 200, true, "Shield critical, high DPS - emergency response")]
        public void Should_ResponseAppropriately_ToVariousDamageLevels(
            double shield,
            double incomingDps,
            bool shouldActivateBoosters,
            string scenario)
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(shield)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();

            // Assert
            var hasActivation = decisions.Any(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate;
            });

            hasActivation.Should().Be(shouldActivateBoosters, $"Scenario: {scenario}");
        }
    }

    public class StructureHitScenarios : DamageScenarioTests
    {
        [Fact]
        public void Should_RecognizeCriticalCondition_When_ArmorLow()
        {
            // Arrange - Shield depleted, armor damaged
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(0)
                .WithArmor(50)
                .Build();

            // Act
            var isCritical = hitpoints.IsCritical;

            // Assert
            isCritical.Should().BeTrue("low armor should be recognized as critical");
        }

        [Fact]
        public void Should_RecognizeCriticalCondition_When_HullDamaged()
        {
            // Arrange - Shield and armor gone, hull damaged
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(0)
                .WithArmor(0)
                .WithHull(30)
                .Build();

            // Act
            var isCritical = hitpoints.IsCritical;

            // Assert
            isCritical.Should().BeTrue("hull damage should be critical");
        }

        [Fact]
        public void Should_ActivateAllDefenses_When_TakingStructureDamage()
        {
            // Arrange - Critical situation with structure damage
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(80) // Very low shield
                .WithArmor(100)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 300, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
                moduleDecision.ShouldOverload.Should().BeTrue();
            });
        }

        [Fact]
        public void Should_AttemptDefensiveManeuvers_When_Critical()
        {
            // Arrange - Critical damage, need to evade
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .WithDistance(8000)
                .Build();

            // Act
            var decision = _maneuverService.DecideManeuver(
                ShipManeuverType.None,
                350, // Very high DPS
                orbitBeacon,
                8000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.Orbit,
                "should orbit for defensive angular velocity");
        }
    }

    public class CombinedDamageScenarios : DamageScenarioTests
    {
        [Fact]
        public void Should_HandleMultiLayerDamage()
        {
            // Arrange - Damage across multiple layers
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(100) // Critical
                .WithArmor(600) // Some damage
                .WithHull(450) // Slight damage
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 200, boosters).ToList();
            var isCritical = hitpoints.IsCritical;

            // Assert
            isCritical.Should().BeTrue();
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate && moduleDecision.ShouldOverload;
            });
        }

        [Fact]
        public void Should_PrioritizeImmediateThreat_OverFutureRisk()
        {
            // Arrange - Shield critical now, even though cap is low
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(90)
                .WithCapacitorPercentage(20)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, 200, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate;
            }, "immediate death threat overrides cap concerns");
        }

        [Theory]
        [InlineData(100, 50, 200, true, true, "Critical shield, low cap, high DPS")]
        [InlineData(500, 50, 200, true, false, "Low shield, low cap, high DPS")]
        [InlineData(500, 50, 50, true, false, "Low shield, low cap, low DPS")]
        [InlineData(850, 50, 50, false, false, "Safe shield, low cap, low DPS")]
        public void Should_MakeComplexDecisions_WithMultipleFactors(
            double shield,
            double capPercentage,
            double incomingDps,
            bool shouldActivate,
            bool shouldOverload,
            string scenario)
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(shield)
                .WithCapacitorPercentage(capPercentage)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideShieldBoosting(hitpoints, incomingDps, boosters).ToList();

            // Assert
            var activation = decisions.FirstOrDefault(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate;
            });

            if (shouldActivate)
            {
                activation.Should().NotBeNull($"Scenario: {scenario}");
                if (activation != null)
                {
                    var moduleDecision = (ModuleDecision)activation;
                    moduleDecision.ShouldOverload.Should().Be(shouldOverload, $"Scenario: {scenario}");
                }
            }
            else
            {
                activation.Should().BeNull($"Scenario: {scenario}");
            }
        }
    }

    public class DefensiveResponseScenarios : DamageScenarioTests
    {
        [Fact]
        public void Should_ActivateHardeners_BeforeBoostingShield()
        {
            // Arrange
            var hardeners = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Inactive().Build()
            };

            // Act
            var decisions = _tankingService.DecideHardeners(hardeners).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
            });
        }

        [Fact]
        public void Should_MaintainDefensiveOrbit_UnderHeavyFire()
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .Build();

            // Act
            var decision = _maneuverService.DecideManeuver(
                ShipManeuverType.None,
                300, // Heavy incoming DPS
                orbitBeacon,
                5000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.Orbit);
        }
    }
}
