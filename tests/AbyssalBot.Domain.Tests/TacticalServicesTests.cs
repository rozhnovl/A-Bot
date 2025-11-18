using AbyssalBot.Domain.Services.Tactics;

namespace AbyssalBot.Domain.Tests;

/// <summary>
/// Unit tests for tactical combat services
/// </summary>
public class TacticalServicesTests
{
    #region DamageApplicationService Tests

    [Fact]
    public void DamageApplication_TurretWithPerfectTracking_ReturnsFullDps()
    {
        // Arrange
        var service = new DamageApplicationService();
        var baseDps = 500.0;
        var tracking = 0.1;
        var angularVelocity = 0.001; // Very low
        var optimalRange = 10000.0;
        var falloffRange = 5000.0;
        var distance = 8000.0;
        var targetSig = 40.0;
        var weaponSigResolution = 40.0;

        // Act
        var effectiveDps = service.CalculateTurretEffectiveDps(
            baseDps, tracking, angularVelocity, optimalRange, falloffRange,
            distance, targetSig, weaponSigResolution);

        // Assert
        effectiveDps.Should().BeGreaterThan(baseDps * 0.95); // Near perfect hit
    }

    [Fact]
    public void DamageApplication_TurretBeyondFalloff_ReducedDps()
    {
        // Arrange
        var service = new DamageApplicationService();
        var baseDps = 500.0;
        var distance = 20000.0; // Way beyond optimal + falloff

        // Act
        var effectiveDps = service.CalculateTurretEffectiveDps(
            baseDps, 0.1, 0.001, 10000, 5000,
            distance, 40, 40);

        // Assert
        effectiveDps.Should().BeLessThan(baseDps * 0.5); // Significant reduction
    }

    [Fact]
    public void DamageApplication_MissileVsLargeTarget_FullDamage()
    {
        // Arrange
        var service = new DamageApplicationService();
        var baseDps = 400.0;
        var explosionRadius = 50.0;
        var explosionVelocity = 100.0;
        var targetSig = 200.0; // Large target
        var targetVelocity = 50.0; // Slow

        // Act
        var effectiveDps = service.CalculateMissileEffectiveDps(
            baseDps, explosionRadius, explosionVelocity, targetSig, targetVelocity);

        // Assert
        effectiveDps.Should().BeGreaterThan(baseDps * 0.9); // Nearly full damage
    }

    [Fact]
    public void DamageApplication_MissileVsSmallFastTarget_ReducedDamage()
    {
        // Arrange
        var service = new DamageApplicationService();
        var baseDps = 400.0;
        var explosionRadius = 100.0;
        var explosionVelocity = 100.0;
        var targetSig = 40.0; // Small frigate
        var targetVelocity = 500.0; // Fast

        // Act
        var effectiveDps = service.CalculateMissileEffectiveDps(
            baseDps, explosionRadius, explosionVelocity, targetSig, targetVelocity);

        // Assert
        effectiveDps.Should().BeLessThan(baseDps * 0.5); // Significantly reduced
    }

    [Fact]
    public void DamageApplication_ShouldOrbitForDefense_HighDpsGoodTracking_ReturnsTrue()
    {
        // Arrange
        var service = new DamageApplicationService();
        var tracking = 0.05;
        var angularVelocity = 0.01; // High but manageable
        var targetDps = 250.0; // High

        // Act
        var result = service.ShouldOrbitForDefense(tracking, angularVelocity, targetDps);

        // Assert
        result.Should().BeTrue("high DPS and tracking can handle it");
    }

    [Fact]
    public void DamageApplication_CalculateOptimalOrbitRadius_ReturnsReasonableValue()
    {
        // Arrange
        var service = new DamageApplicationService();
        var optimalRange = 10000.0;
        var tracking = 0.05;
        var ourSig = 50.0;
        var targetSig = 50.0;

        // Act
        var orbitRadius = service.CalculateOptimalOrbitRadius(
            optimalRange, tracking, ourSig, targetSig);

        // Assert
        orbitRadius.Should().BeInRange(2000, 15000, "orbit should be within practical ranges");
        orbitRadius.Should().BeLessOrEqualTo((int)(optimalRange * 0.8), "should stay within optimal");
    }

    #endregion

    #region CapacitorManagementService Tests

    [Fact]
    public void CapacitorManagement_StableCapacitor_ReturnsPositiveStability()
    {
        // Arrange
        var service = new CapacitorManagementService();
        var currentCap = 500.0;
        var maxCap = 1000.0;
        var rechargeRate = 10.0; // GJ/s
        var usageRate = 8.0; // GJ/s (using less than recharge)

        // Act
        var stability = service.CalculateStableCapPercentage(
            currentCap, maxCap, rechargeRate, usageRate);

        // Assert
        stability.Should().BeGreaterThan(0, "cap is stable");
    }

    [Fact]
    public void CapacitorManagement_UnstableCapacitor_ReturnsNegative()
    {
        // Arrange
        var service = new CapacitorManagementService();
        var rechargeRate = 10.0;
        var usageRate = 15.0; // Using more than recharge

        // Act
        var stability = service.CalculateStableCapPercentage(500, 1000, rechargeRate, usageRate);

        // Assert
        stability.Should().Be(-1, "cap is unstable");
    }

    [Fact]
    public void CapacitorManagement_CriticalCap_SuggestsDeactivatingPropulsion()
    {
        // Arrange
        var service = new CapacitorManagementService();
        var capPercentage = 15.0; // Critical
        var isStable = false;
        var timeUntilEmpty = 20.0;

        // Act
        var toDeactivate = service.GetModulesToDeactivate(capPercentage, isStable, timeUntilEmpty);

        // Assert
        toDeactivate.Should().Contain("Propulsion", "should turn off prop mod to save cap");
    }

    [Fact]
    public void CapacitorManagement_ShouldUseCapBooster_EmergencySituation_ReturnsTrue()
    {
        // Arrange
        var service = new CapacitorManagementService();
        var capPercentage = 20.0; // Low
        var isStable = false;
        var charges = 10;
        var inCombat = true;

        // Act
        var shouldUse = service.ShouldUseCapBooster(capPercentage, isStable, charges, inCombat);

        // Assert
        shouldUse.Should().BeTrue("cap is critical and we're in combat");
    }

    [Fact]
    public void CapacitorManagement_OverheatRecommendation_CriticalShield_RecommendsOverheat()
    {
        // Arrange
        var service = new CapacitorManagementService();
        var capPercentage = 60.0; // Good cap
        var isStable = true;
        var shieldPercentage = 25.0; // Critical

        // Act
        var recommendation = service.GetOverheatRecommendation(capPercentage, isStable, shieldPercentage);

        // Assert
        recommendation.ShouldOverheat.Should().BeTrue();
        recommendation.ModulesToOverheat.Should().Contain("Shield Booster");
    }

    #endregion

    #region ManeuverOptimizationService Tests

    [Fact]
    public void ManeuverOptimization_CalculateAngularVelocity_ReturnsCorrectValue()
    {
        // Arrange
        var service = new ManeuverOptimizationService();
        var transversal = 500.0; // m/s
        var distance = 5000.0; // meters

        // Act
        var angularVelocity = service.CalculateAngularVelocity(transversal, distance);

        // Assert
        angularVelocity.Should().Be(0.1); // 500/5000 = 0.1 rad/s
    }

    [Fact]
    public void ManeuverOptimization_OptimalOrbitRadius_FrigateRockets_ReturnsSmallOrbit()
    {
        // Arrange
        var service = new ManeuverOptimizationService();

        // Act
        var orbitRadius = service.CalculateOptimalOrbitRadius(
            ShipClass.Frigate,
            WeaponType.Rockets,
            5000, // Optimal range
            0.1,  // Tracking
            0);   // Angular velocity

        // Assert
        orbitRadius.Should().BeLessOrEqualTo(3000, "frigates with rockets orbit close");
    }

    [Fact]
    public void ManeuverOptimization_ShouldIncreaseTransversal_HighDpsLowShield_ReturnsTrue()
    {
        // Arrange
        var service = new ManeuverOptimizationService();
        var incomingDps = 250.0; // High
        var currentAngular = 0.0001; // Very low
        var shieldPercentage = 35.0; // Low
        var weaponTracking = 0.05;

        // Act
        var result = service.ShouldIncreaseTransversal(
            incomingDps, currentAngular, shieldPercentage, weaponTracking);

        // Assert
        result.Should().BeTrue("need more defense with high DPS and low shield");
    }

    [Fact]
    public void ManeuverOptimization_SpiralApproach_BeyondRange_RecommendSpiral()
    {
        // Arrange
        var service = new ManeuverOptimizationService();
        var currentDistance = 30000.0;
        var targetDistance = 10000.0;
        var shipSpeed = 500.0;

        // Act
        var spiral = service.CalculateSpiralApproach(currentDistance, targetDistance, shipSpeed);

        // Assert
        spiral.UseSpiral.Should().BeTrue();
        spiral.InitialOrbitRadius.Should().BeGreaterThan(spiral.FinalOrbitRadius);
    }

    [Fact]
    public void ManeuverOptimization_MWDRecommendation_LowCap_ReturnsFalse()
    {
        // Arrange
        var service = new ManeuverOptimizationService();
        var distance = 15000.0;
        var incomingDps = 100.0;
        var hasWeb = false;
        var capPercentage = 20.0; // Low cap
        var desiredRange = 10000.0;

        // Act
        var recommendation = service.ShouldActivateMWD(
            distance, incomingDps, hasWeb, capPercentage, desiredRange);

        // Assert
        recommendation.ActivateMWD.Should().BeFalse("cap too low");
    }

    #endregion

    #region EwarTargetPriorityService Tests

    [Fact]
    public void EwarPriority_JammerTarget_ReceivesHighestPriority()
    {
        // Arrange
        var service = new EwarTargetPriorityService();
        var jammer = new EwarTarget(
            1, "Jammer", "Lucid Entangling", 5000, true, 100, 200, 50,
            IsJammer: true);

        // Act
        var priority = service.CalculateEwarPriority(jammer, "Gila", false, false);

        // Assert
        priority.Should().Be(1, "jammers are highest priority");
    }

    [Fact]
    public void EwarPriority_PaintedTarget_GetsPriorityBoost()
    {
        // Arrange
        var service = new EwarTargetPriorityService();
        var target = new EwarTarget(
            1, "Enemy", "Damavik", 5000, true, 150, 200, 50);

        // Act
        var normalPriority = service.CalculateEwarPriority(target, "Gila", false, false);
        var paintedPriority = service.CalculateEwarPriority(target, "Gila", true, false);

        // Assert
        paintedPriority.Should().BeLessThan(normalPriority, "painted targets easier to kill");
    }

    [Fact]
    public void EwarPriority_CalculateWarpDisruptionPoints_Scrambled_CannotWarp()
    {
        // Arrange
        var service = new EwarTargetPriorityService();
        var scramblers = 1;
        var disruptors = 0;
        var warpStrength = 0;

        // Act
        var points = service.CalculateWarpDisruptionPoints(scramblers, disruptors, warpStrength);

        // Assert
        points.Should().Be(2, "1 scrambler = 2 points");
    }

    #endregion

    #region TankOptimizationService Tests

    [Fact]
    public void TankOptimization_ShouldActivateBooster_LowShieldHighDps_ReturnsTrue()
    {
        // Arrange
        var service = new TankOptimizationService();
        var currentShield = 400.0;
        var maxShield = 1000.0;
        var boosterAmount = 200.0;
        var cycleTime = 5.0;
        var incomingDps = 250.0; // High

        // Act
        var shouldActivate = service.ShouldActivateShieldBooster(
            currentShield, maxShield, boosterAmount, cycleTime, incomingDps);

        // Assert
        shouldActivate.Should().BeTrue("shield low and taking high DPS");
    }

    [Fact]
    public void TankOptimization_ShouldActivateBooster_FullShield_ReturnsFalse()
    {
        // Arrange
        var service = new TankOptimizationService();
        var currentShield = 990.0;
        var maxShield = 1000.0;
        var boosterAmount = 200.0;
        var cycleTime = 5.0;
        var incomingDps = 100.0;

        // Act
        var shouldActivate = service.ShouldActivateShieldBooster(
            currentShield, maxShield, boosterAmount, cycleTime, incomingDps);

        // Assert
        shouldActivate.Should().BeFalse("shield nearly full, would waste cycle");
    }

    [Fact]
    public void TankOptimization_OverheatRecommendation_CriticalHp_RecommendsOverheat()
    {
        // Arrange
        var service = new TankOptimizationService();
        var hpPercentage = 20.0; // Critical
        var incomingDps = 200.0;
        var repairPerSecond = 100.0;
        var capPercentage = 50.0;

        // Act
        var recommendation = service.ShouldOverheatTank(
            hpPercentage, incomingDps, repairPerSecond, capPercentage);

        // Assert
        recommendation.ShouldOverheat.Should().BeTrue();
        recommendation.OverheatBoosters.Should().BeTrue();
    }

    [Fact]
    public void TankOptimization_CalculateEffectiveResistance_ReturnsWeightedAverage()
    {
        // Arrange
        var service = new TankOptimizationService();
        var resistances = new ResistanceProfile(0.7, 0.6, 0.5, 0.4);
        var damageProfile = new DamageProfile(100, 100, 100, 100); // Equal damage

        // Act
        var effectiveResist = service.CalculateEffectiveResistance(resistances, damageProfile);

        // Assert
        effectiveResist.Should().Be(55.0); // (70+60+50+40)/4 = 55%
    }

    [Fact]
    public void TankOptimization_IdentifyResistanceHole_ReturnsWeakestResist()
    {
        // Arrange
        var service = new TankOptimizationService();
        var resistances = new ResistanceProfile(0.7, 0.6, 0.3, 0.5); // Kinetic weakest
        var damageProfile = new DamageProfile(100, 100, 100, 100);

        // Act
        var hole = service.IdentifyResistanceHole(resistances, damageProfile);

        // Assert
        hole.Should().Be(DamageType.Kinetic, "taking most damage from kinetic");
    }

    [Fact]
    public void TankOptimization_CalculateTimeToLive_TankHolding_ReturnsNegative()
    {
        // Arrange
        var service = new TankOptimizationService();
        var currentHp = 500.0;
        var incomingDps = 100.0;
        var repairPerSecond = 120.0; // Repairing more than incoming

        // Act
        var ttl = service.CalculateTimeToLive(currentHp, incomingDps, repairPerSecond);

        // Assert
        ttl.Should().Be(-1, "tank is holding");
    }

    #endregion

    #region WeaponManagementService Tests

    [Fact]
    public void WeaponManagement_ShouldChangeTarget_MidCycle_ReturnsFalse()
    {
        // Arrange
        var service = new WeaponManagementService();
        var currentTarget = 123L;
        var newTarget = 456L;
        var cycleProgress = 0.5; // Mid-cycle
        var targetHealth = 50.0;

        // Act
        var shouldChange = service.ShouldChangeWeaponTarget(
            currentTarget, newTarget, cycleProgress, targetHealth);

        // Assert
        shouldChange.Should().BeFalse("don't switch mid-cycle");
    }

    [Fact]
    public void WeaponManagement_ShouldChangeTarget_TargetAlmostDead_ReturnsTrue()
    {
        // Arrange
        var service = new WeaponManagementService();
        var currentTarget = 123L;
        var newTarget = 456L;
        var cycleProgress = 0.5;
        var targetHealth = 3.0; // Almost dead

        // Act
        var shouldChange = service.ShouldChangeWeaponTarget(
            currentTarget, newTarget, cycleProgress, targetHealth);

        // Assert
        shouldChange.Should().BeTrue("avoid overkill");
    }

    [Fact]
    public void WeaponManagement_ShouldReload_InCombatWithAmmo_ReturnsFalse()
    {
        // Arrange
        var service = new WeaponManagementService();
        var currentCharges = 50;
        var maxCharges = 100;
        var inCombat = true;
        var enemiesRemaining = 5;
        var reloadTime = 10.0;

        // Act
        var recommendation = service.ShouldReload(
            currentCharges, maxCharges, inCombat, enemiesRemaining, reloadTime);

        // Assert
        recommendation.ShouldReload.Should().BeFalse("in combat - maintain DPS");
    }

    [Fact]
    public void WeaponManagement_ShouldReload_EmptyWeapon_ReturnsTrue()
    {
        // Arrange
        var service = new WeaponManagementService();
        var currentCharges = 0;
        var maxCharges = 100;

        // Act
        var recommendation = service.ShouldReload(currentCharges, maxCharges, false, 0, 10);

        // Assert
        recommendation.ShouldReload.Should().BeTrue("weapon empty");
    }

    [Fact]
    public void WeaponManagement_SelectMissileAmmo_SmallTarget_RecommendsPrecision()
    {
        // Arrange
        var service = new WeaponManagementService();
        var targetSig = 40.0; // Small frigate
        var distance = 10000.0;
        var velocity = 600.0; // Fast

        // Act
        var recommendation = service.SelectOptimalAmmo(
            WeaponSystem.Missiles, targetSig, distance, 15000, velocity);

        // Assert
        recommendation.AmmoType.Should().Be("Precision");
    }

    [Fact]
    public void WeaponManagement_OverheatWeapons_CriticalShield_ReturnsTrue()
    {
        // Arrange
        var service = new WeaponManagementService();
        var timeToKill = 15.0;
        var shieldPercentage = 25.0; // Critical
        var enemiesRemaining = 3;
        var heatLevel = 0.3; // Safe

        // Act
        var recommendation = service.ShouldOverheatWeapons(
            timeToKill, shieldPercentage, enemiesRemaining, heatLevel);

        // Assert
        recommendation.ShouldOverheat.Should().BeTrue("critical shield - kill faster");
    }

    #endregion

    #region EscapeDecisionService Tests

    [Fact]
    public void EscapeDecision_CriticalHpShortTTL_CanWarp_RecommendsWarp()
    {
        // Arrange
        var service = new EscapeDecisionService();
        var hpPercentage = 20.0; // Critical
        var canWarp = true;
        var enemies = 5;
        var timeToLive = 20.0; // Short
        var capPercentage = 50.0;

        // Act
        var decision = service.ShouldEscape(hpPercentage, canWarp, enemies, timeToLive, capPercentage);

        // Assert
        decision.ShouldEscape.Should().BeTrue();
        decision.Method.Should().Be(EscapeMethod.Warp);
    }

    [Fact]
    public void EscapeDecision_CheckWarpDisruption_OneScrambler_CannotWarp()
    {
        // Arrange
        var service = new EscapeDecisionService();

        // Act
        var status = service.CheckWarpDisruption(1, 0, 0);

        // Assert
        status.CanWarp.Should().BeFalse("scrambled");
        status.MwdDisabled.Should().BeTrue("scrambler disables MWD");
        status.TotalPoints.Should().Be(2);
    }

    [Fact]
    public void EscapeDecision_CalculateAlignTime_ReturnsReasonableValue()
    {
        // Arrange
        var service = new EscapeDecisionService();
        var mass = 1200000.0; // Cruiser mass
        var inertia = 0.5;
        var propModActive = false;
        var massBonus = 0.0;

        // Act
        var alignTime = service.CalculateAlignTime(mass, inertia, propModActive, massBonus);

        // Assert
        alignTime.Should().BeGreaterThan(0);
        alignTime.Should().BeLessThan(20, "reasonable align time for cruiser");
    }

    [Fact]
    public void EscapeDecision_AssessTargetingThreat_MultipleHostiles_ReturnsHigh()
    {
        // Arrange
        var service = new EscapeDecisionService();
        var locking = 2;
        var locked = 3;

        // Act
        var threat = service.AssessTargetingThreat(locking, locked);

        // Assert
        threat.Level.Should().Be(ThreatLevel.High);
    }

    [Fact]
    public void EscapeDecision_CanBreakScramRange_FasterThanScrambler_ReturnsTrue()
    {
        // Arrange
        var service = new EscapeDecisionService();
        var currentDistance = 5000.0;
        var scramRange = 10000.0;
        var ourSpeed = 1000.0;
        var scramblerSpeed = 400.0;
        var timeToLive = 30.0;

        // Act
        var canBreak = service.CanBreakScramRange(
            currentDistance, scramRange, ourSpeed, scramblerSpeed, timeToLive);

        // Assert
        canBreak.Should().BeTrue("we're faster and have time");
    }

    #endregion

    #region AbyssRoomAnalysisService Tests

    [Fact]
    public void AbyssRoom_AnalyzeWeather_Electrical_CorrectEffects()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();

        // Act
        var analysis = service.AnalyzeWeather(FilamentType.Electrical, "Gila", 4);

        // Assert
        analysis.Penalties.Should().Contain(p => p.Contains("shield recharge"));
        analysis.Benefits.Should().Contain(b => b.Contains("capacitor recharge"));
    }

    [Fact]
    public void AbyssRoom_PrioritizeCaches_Bioadaptive_HighestPriority()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();
        var caches = new[]
        {
            new CacheTarget(1, CacheType.Storage, 5000),
            new CacheTarget(2, CacheType.Bioadaptive, 10000),
            new CacheTarget(3, CacheType.Biocombinative, 7000)
        };

        // Act
        var prioritized = service.PrioritizeCaches(caches, 1000);

        // Assert
        prioritized[0].Type.Should().Be(CacheType.Bioadaptive);
        prioritized[1].Type.Should().Be(CacheType.Biocombinative);
        prioritized[2].Type.Should().Be(CacheType.Storage);
    }

    [Fact]
    public void AbyssRoom_ShouldTakeConduit_EnemiesRemaining_ReturnsFalse()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();

        // Act
        var decision = service.ShouldTakeConduit(5, 2, 2, 80, 80, 120, 600);

        // Assert
        decision.ShouldTake.Should().BeFalse();
        decision.Reason.Should().Be(ConduitReason.EnemiesRemaining);
    }

    [Fact]
    public void AbyssRoom_ShouldTakeConduit_AllClearGoodStatus_ReturnsTrue()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();

        // Act
        var decision = service.ShouldTakeConduit(
            enemiesRemaining: 0,
            cachesClaimed: 2,
            totalCaches: 2,
            currentHpPercentage: 90,
            currentCapPercentage: 95,
            roomTimeElapsed: 180,
            totalRunTimeElapsed: 600);

        // Assert
        decision.ShouldTake.Should().BeTrue();
        decision.Reason.Should().Be(ConduitReason.RoomComplete);
    }

    [Fact]
    public void AbyssRoom_TrackInvulnerability_RecentlyJumped_IsInvulnerable()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();
        var timeSinceJump = 10.0; // Just jumped

        // Act
        var status = service.TrackInvulnerability(timeSinceJump);

        // Assert
        status.IsInvulnerable.Should().BeTrue();
        status.TimeRemaining.Should().BeGreaterThan(45);
    }

    [Fact]
    public void AbyssRoom_AssessFilamentDifficulty_Tier5_RequiresHighStats()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();

        // Act
        var assessment = service.AssessFilamentDifficulty(5, "Gila", 700, 500);

        // Assert
        assessment.Tier.Should().Be(5);
        assessment.IsReady.Should().BeTrue("stats meet requirements");
        assessment.RecommendedMinDps.Should().BeGreaterThan(500);
    }

    [Fact]
    public void AbyssRoom_AssessFilamentDifficulty_InsufficientStats_NotReady()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();

        // Act
        var assessment = service.AssessFilamentDifficulty(6, "Gila", 300, 200);

        // Assert
        assessment.IsReady.Should().BeFalse("stats below requirements");
        assessment.Warnings.Should().NotBeEmpty();
    }

    [Fact]
    public void AbyssRoom_AnalyzeSpawnPattern_ManyFrigates_DetectsSwarm()
    {
        // Arrange
        var service = new AbyssRoomAnalysisService();
        var enemies = new[]
        {
            "Starving Damavik", "Starving Damavik", "Starving Damavik",
            "Vila Swarmer", "Vila Swarmer", "Vila Swarmer", "Vila Swarmer"
        };

        // Act
        var analysis = service.AnalyzeSpawnPattern(enemies);

        // Assert
        analysis.Pattern.Should().Be(SpawnPattern.Swarm);
        analysis.TacticalRecommendations.Should().Contain(r => r.Contains("drones"));
    }

    #endregion
}
