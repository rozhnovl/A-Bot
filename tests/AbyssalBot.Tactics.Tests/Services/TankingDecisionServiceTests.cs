using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Tactics.Tests.Services;

public class TankingDecisionServiceTests
{
    private readonly TankingDecisionService _sut;

    public TankingDecisionServiceTests()
    {
        _sut = new TankingDecisionService();
    }

    public class DecideShieldBoosting : TankingDecisionServiceTests
    {
        [Fact]
        public void Should_ActivateAllBoosters_When_ShieldCritical()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(100)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                d.Should().BeOfType<ModuleDecision>();
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
                moduleDecision.ShouldOverload.Should().BeTrue();
            });
        }

        [Fact]
        public void Should_ActivateAllBoosters_When_HighDpsAndLowShield()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(500)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 200, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
                moduleDecision.ShouldOverload.Should().BeFalse();
            });
        }

        [Fact]
        public void Should_ActivateSingleBooster_When_LowDpsAndLowShield()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(500)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(1);
            var decision = (ModuleDecision)decisions[0];
            decision.ShouldActivate.Should().BeTrue();
            decision.ShouldOverload.Should().BeFalse();
        }

        [Fact]
        public void Should_DeactivateBoosters_When_ShieldSafe()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(850)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeFalse();
            });
        }

        [Fact]
        public void Should_DeactivateBoosters_When_CapacitorLow()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(650)
                .WithCapacitor(350)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().HaveCount(1);
            var decision = (ModuleDecision)decisions[0];
            decision.ShouldActivate.Should().BeFalse();
        }

        [Fact]
        public void Should_DeactivateExtraBoosters_When_LowDpsAndLowShield()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(500)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().HaveCountGreaterThan(0);
            decisions.Should().Contain(d => ((ModuleDecision)d).ShouldActivate == false);
        }

        [Fact]
        public void Should_ReturnNoDecisions_When_NoBoostersAvailable()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(100)
                .Build();

            var boosters = Array.Empty<ShipModule>();

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().BeEmpty();
        }

        [Theory]
        [InlineData(50, 200, 2)]  // Critical shield, high DPS
        [InlineData(100, 200, 2)] // Very low shield, high DPS
        [InlineData(500, 200, 2)] // Low shield, high DPS
        [InlineData(500, 50, 1)]  // Low shield, low DPS
        public void Should_ActivateCorrectNumberOfBoosters_ForVariousDpsLevels(
            double shield, double dps, int expectedMinActivations)
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(shield)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, dps, boosters).ToList();

            // Assert
            var activationDecisions = decisions
                .OfType<ModuleDecision>()
                .Count(d => d.ShouldActivate);
            activationDecisions.Should().BeGreaterOrEqualTo(expectedMinActivations);
        }
    }

    public class DecideHardeners : TankingDecisionServiceTests
    {
        [Fact]
        public void Should_ActivateAllInactiveHardeners()
        {
            // Arrange
            var hardeners = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Inactive().Build(),
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Active().Build()
            };

            // Act
            var decisions = _sut.DecideHardeners(hardeners).ToList();

            // Assert
            decisions.Should().HaveCount(2);
            decisions.Should().AllSatisfy(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                moduleDecision.ShouldActivate.Should().BeTrue();
            });
        }

        [Fact]
        public void Should_ReturnNoDecisions_When_AllHardenersActive()
        {
            // Arrange
            var hardeners = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Active().Build(),
                new ShipModuleBuilder().WithType(ModuleType.Hardener).Active().Build()
            };

            // Act
            var decisions = _sut.DecideHardeners(hardeners).ToList();

            // Assert
            decisions.Should().BeEmpty();
        }

        [Fact]
        public void Should_ReturnNoDecisions_When_NoHardenersAvailable()
        {
            // Arrange
            var hardeners = Array.Empty<ShipModule>();

            // Act
            var decisions = _sut.DecideHardeners(hardeners).ToList();

            // Assert
            decisions.Should().BeEmpty();
        }
    }

    public class CapacitorManagementScenarios : TankingDecisionServiceTests
    {
        [Theory]
        [InlineData(30)]
        [InlineData(20)]
        [InlineData(10)]
        [InlineData(5)]
        public void Should_PreserveCapacitor_AtVariousLowLevels(double capPercentage)
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(650) // Not safe, but not critical
                .WithCapacitorPercentage(capPercentage)
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate == false;
            }, "should deactivate boosters to preserve capacitor");
        }

        [Fact]
        public void Should_OverrideCapacitorConcerns_When_ShieldCritical()
        {
            // Arrange
            var hitpoints = new ShipHitpointsBuilder()
                .WithShield(100) // Critical
                .WithCapacitorPercentage(5) // Very low cap
                .Build();

            var boosters = new[]
            {
                new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
            };

            // Act
            var decisions = _sut.DecideShieldBoosting(hitpoints, 200, boosters).ToList();

            // Assert
            decisions.Should().Contain(d =>
            {
                var moduleDecision = (ModuleDecision)d;
                return moduleDecision.ShouldActivate && moduleDecision.ShouldOverload;
            }, "should activate with overload despite low capacitor");
        }
    }
}
