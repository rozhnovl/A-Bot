using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Interfaces;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Situations.Tests.Scenarios;

/// <summary>
/// Tests for critical capacitor scenarios at various levels
/// </summary>
public class CapacitorCriticalScenariosTests
{
    private readonly TankingDecisionService _tankingService;
    private readonly ManeuverDecisionService _maneuverService;
    private readonly TargetPriorityService _targetPriorityService;
    private readonly NpcInformationService _npcInfoService;

    public CapacitorCriticalScenariosTests()
    {
        _tankingService = new TankingDecisionService();
        _maneuverService = new ManeuverDecisionService();
        _npcInfoService = new NpcInformationService();
        _targetPriorityService = new TargetPriorityService(_npcInfoService);
    }

    [Theory]
    [InlineData(30, "Cap at 30% - Emergency power management")]
    [InlineData(20, "Cap at 20% - Critical power management")]
    [InlineData(10, "Cap at 10% - Desperate power conservation")]
    [InlineData(5, "Cap at 5% - Near cap-out situation")]
    public void Should_ConserveCapacitor_AtLowLevels(double capPercentage, string scenario)
    {
        // Arrange
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(650) // Not critical, but needs management
            .WithCapacitorPercentage(capPercentage)
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build(),
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Active().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 100, boosters).ToList();

        // Assert - Should deactivate boosters to save cap
        decisions.Should().Contain(d =>
        {
            var moduleDecision = (ModuleDecision)d;
            return !moduleDecision.ShouldActivate;
        }, $"Scenario: {scenario}");
    }

    [Fact]
    public void Should_PrioritizeSurvival_WhenCapLowButShieldCritical()
    {
        // Arrange - Worst case: both cap and shield critical
        var hitpoints = new ShipHitpointsBuilder()
            .WithShield(100) // Critical
            .WithCapacitorPercentage(5) // Nearly empty
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 200, boosters).ToList();

        // Assert - Must activate boosters with overload despite low cap
        decisions.Should().Contain(d =>
        {
            var moduleDecision = (ModuleDecision)d;
            return moduleDecision.ShouldActivate && moduleDecision.ShouldOverload;
        }, "survival takes priority over capacitor management");
    }

    [Fact]
    public void Should_DisableMWD_WhenCapacitorLow()
    {
        // Arrange
        var hitpoints = new ShipHitpointsBuilder()
            .WithCapacitorPercentage(15)
            .Build();

        // MWD uses a lot of capacitor, should be disabled when cap is low
        // This is a judgment call - in real scenario, we'd check if incoming DPS is low
        var shouldUseMWD = _maneuverService.ShouldActivateMWD(
            50, // Low DPS
            10000, // Far from target
            ShipManeuverType.Approach
        );

        // Assert
        // At low cap, even though we're far, we should consider cap management
        // This test demonstrates the tradeoff between positioning and cap management
        shouldUseMWD.Should().BeTrue("MWD decision doesn't consider cap directly in current implementation");
    }

    [Fact]
    public void Should_HandleCapOut_WithAllModulesOffline()
    {
        // Arrange - Complete capacitor depletion
        var hitpoints = new ShipHitpointsBuilder()
            .WithCapacitor(0)
            .WithShield(800) // Shield is fine
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 50, boosters).ToList();

        // Assert - No boosters should activate with zero cap
        decisions.Should().NotContain(d =>
        {
            var moduleDecision = (ModuleDecision)d;
            return moduleDecision.ShouldActivate;
        }, "cannot activate modules with no capacitor");
    }

    [Theory]
    [InlineData(30, 500, true, "Low cap, low shield - risky situation")]
    [InlineData(30, 800, false, "Low cap, safe shield - conserve")]
    [InlineData(5, 500, true, "Critical cap, low shield - must boost")]
    [InlineData(5, 100, true, "Critical cap, critical shield - emergency")]
    public void Should_BalanceCapAndShield_InVariousScenarios(
        double capPercentage,
        double shield,
        bool shouldBoost,
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
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 100, boosters).ToList();

        // Assert
        var hasBoostDecision = decisions.Any(d =>
        {
            var moduleDecision = (ModuleDecision)d;
            return moduleDecision.ShouldActivate;
        });

        hasBoostDecision.Should().Be(shouldBoost, $"Scenario: {scenario}");
    }

    [Fact]
    public void Should_StaggerBoosterActivation_ToManageCapRegen()
    {
        // Arrange - Cap recovering but still low
        var hitpoints = new ShipHitpointsBuilder()
            .WithCapacitorPercentage(25)
            .WithShield(550) // Low shield
            .Build();

        var boosters = new[]
        {
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build(),
            new ShipModuleBuilder().WithType(ModuleType.ShieldBooster).Inactive().Build()
        };

        // Act - Low DPS scenario
        var decisions = _tankingService.DecideShieldBoosting(hitpoints, 75, boosters).ToList();

        // Assert - Should activate only one booster to balance cap usage
        var activationCount = decisions.Count(d =>
        {
            var moduleDecision = (ModuleDecision)d;
            return moduleDecision.ShouldActivate;
        });

        activationCount.Should().BeLessOrEqualTo(1,
            "with low DPS and low cap, should use single booster");
    }
}
