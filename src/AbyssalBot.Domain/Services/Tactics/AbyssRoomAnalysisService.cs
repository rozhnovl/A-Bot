using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for analyzing Abyssal Deadspace rooms and making tactical decisions
/// </summary>
/// <remarks>
/// Implements Abyssal-specific mechanics:
/// - Weather effects from filament types (electrical, firestorm, etc.)
/// - Cache priority (bioadaptive caches > biocombinative caches)
/// - Conduit timing (when to take gate to next room)
/// - Invulnerability timer tracking
/// - Filament tier and difficulty assessment
/// - Room spawn pattern recognition
/// </remarks>
public class AbyssRoomAnalysisService
{
    /// <summary>
    /// Analyzes weather effects and their impact on our ship
    /// </summary>
    /// <param name="filamentType">Type of Abyssal filament used</param>
    /// <param name="ourShipType">Our ship type (Gila, Sacrilege, etc.)</param>
    /// <param name="weatherTier">Weather intensity tier (1-6)</param>
    /// <returns>Weather analysis with bonuses/penalties</returns>
    /// <remarks>
    /// Weather effects vary by filament type:
    /// - Electrical: -50% shield regen, +100% cap recharge
    /// - Firestorm: +100% armor/hull HP, explosion radius/velocity penalties
    /// - Dark: -50% targeting range, +100% ship velocity
    /// - Exotic: +100% armor resistances, -50% shield resistances
    /// - Gamma: +100% shield resistances, -50% armor resistances
    /// - Chaotic: Random effects
    /// </remarks>
    public WeatherAnalysis AnalyzeWeather(
        FilamentType filamentType,
        string ourShipType,
        int weatherTier)
    {
        var benefits = new List<string>();
        var penalties = new List<string>();
        var recommendations = new List<string>();

        switch (filamentType)
        {
            case FilamentType.Electrical:
                penalties.Add("50% reduced shield recharge rate");
                benefits.Add("100% increased capacitor recharge rate");

                if (ourShipType.Contains("Gila") || ourShipType.Contains("Cerberus"))
                    recommendations.Add("Shield recharge penalty hurts passive tank - use active tank");

                recommendations.Add("Excellent cap stability - can run all modules");
                break;

            case FilamentType.Firestorm:
                benefits.Add("100% increased armor and hull HP");
                penalties.Add("50% reduced explosion radius and velocity (missiles)");

                if (ourShipType.Contains("Sacrilege") || ourShipType.Contains("Zealot"))
                    benefits.Add("Armor HP bonus synergizes with armor tank");

                recommendations.Add("Missile damage application reduced - prefer guns");
                break;

            case FilamentType.Dark:
                penalties.Add("50% reduced targeting range");
                benefits.Add("100% increased ship velocity");

                penalties.Add("May need to get closer to lock targets");
                recommendations.Add("Speed bonus helps with kiting and transversal");
                break;

            case FilamentType.Exotic:
                benefits.Add("100% increased armor resistances");
                penalties.Add("50% reduced shield resistances");

                if (ourShipType.Contains("Sacrilege") || ourShipType.Contains("Ishtar"))
                    benefits.Add("Excellent for armor-tanked ships");
                else
                    penalties.Add("Shield tank is significantly weaker");
                break;

            case FilamentType.Gamma:
                benefits.Add("100% increased shield resistances");
                penalties.Add("50% reduced armor resistances");

                if (ourShipType.Contains("Gila") || ourShipType.Contains("Cerberus"))
                    benefits.Add("Excellent for shield-tanked ships");
                else
                    penalties.Add("Armor tank is significantly weaker");
                break;

            case FilamentType.Chaotic:
                recommendations.Add("Random weather effects - adapt tactics as needed");
                recommendations.Add("High risk, high reward - valuable loot");
                break;
        }

        return new WeatherAnalysis(filamentType, weatherTier, benefits, penalties, recommendations);
    }

    /// <summary>
    /// Determines cache priority in current room
    /// </summary>
    /// <param name="caches">Available caches in room</param>
    /// <param name="roomTimeRemaining">Time remaining in room (20 min max)</param>
    /// <returns>Ordered list of caches by priority</returns>
    /// <remarks>
    /// Cache priority:
    /// 1. Bioadaptive Cache - Contains filaments, very valuable
    /// 2. Biocombinative Cache - Contains mutaplasmids, valuable
    /// 3. Regular Storage - Standard loot
    ///
    /// Always loot bioadaptive if present
    /// </remarks>
    public IReadOnlyList<CacheTarget> PrioritizeCaches(
        IEnumerable<CacheTarget> caches,
        double roomTimeRemaining)
    {
        var prioritized = caches
            .OrderBy(c => c.Type switch
            {
                CacheType.Bioadaptive => 1,      // Highest priority - filaments
                CacheType.Biocombinative => 2,   // High priority - mutaplasmids
                CacheType.Storage => 3,           // Normal priority
                _ => 4
            })
            .ThenBy(c => c.Distance) // Closer first
            .ToList();

        // Add time warnings
        foreach (var cache in prioritized)
        {
            if (roomTimeRemaining < 300) // Less than 5 minutes
                cache.Notes.Add("Limited time - loot quickly");
        }

        return prioritized;
    }

    /// <summary>
    /// Determines optimal time to take conduit (gate to next room)
    /// </summary>
    /// <param name="enemiesRemaining">Number of enemies remaining</param>
    /// <param name="cachesClaimed">Number of caches looted</param>
    /// <param name="totalCaches">Total caches available</param>
    /// <param name="currentHpPercentage">Current HP percentage</param>
    /// <param name="currentCapPercentage">Current cap percentage</param>
    /// <param name="roomTimeElapsed">Time spent in current room (seconds)</param>
    /// <param name="totalRunTimeElapsed">Total time in Abyssal (seconds)</param>
    /// <returns>Conduit decision</returns>
    /// <remarks>
    /// Take conduit when:
    /// - All enemies dead
    /// - All valuable caches looted
    /// - HP/Cap recovered
    /// - Not too much time spent (20 min total timer)
    ///
    /// Don't take conduit if:
    /// - Enemies still alive
    /// - Bioadaptive/biocombinative cache not looted
    /// - Need to recover HP/cap
    /// - Timer not critical
    /// </remarks>
    public ConduitDecision ShouldTakeConduit(
        int enemiesRemaining,
        int cachesClaimed,
        int totalCaches,
        double currentHpPercentage,
        double currentCapPercentage,
        double roomTimeElapsed,
        double totalRunTimeElapsed)
    {
        const double TotalTimeLimit = 1200; // 20 minutes
        const double TimeWarningThreshold = 900; // 15 minutes

        // Still fighting - don't take conduit
        if (enemiesRemaining > 0)
        {
            return new ConduitDecision(
                false,
                ConduitReason.EnemiesRemaining,
                $"Still fighting {enemiesRemaining} enemies");
        }

        // Valuable caches not looted
        if (cachesClaimed < totalCaches)
        {
            // But if time is critical, skip loot
            if (totalRunTimeElapsed > TimeWarningThreshold)
            {
                return new ConduitDecision(
                    true,
                    ConduitReason.TimeLimit,
                    $"Time critical ({totalRunTimeElapsed:F0}s / {TotalTimeLimit}s) - skip remaining loot");
            }

            return new ConduitDecision(
                false,
                ConduitReason.LootRemaining,
                $"Caches remaining ({cachesClaimed}/{totalCaches})");
        }

        // HP too low - recover before next room
        if (currentHpPercentage < 75)
        {
            // Unless time critical
            if (totalRunTimeElapsed > TimeWarningThreshold)
            {
                if (currentHpPercentage > 50)
                {
                    return new ConduitDecision(
                        true,
                        ConduitReason.TimeLimit,
                        "HP acceptable (>50%) and time critical");
                }
            }

            return new ConduitDecision(
                false,
                ConduitReason.RecoverHp,
                $"Recovering HP ({currentHpPercentage:F1}% -> 90%+)");
        }

        // Cap too low - recover
        if (currentCapPercentage < 80)
        {
            if (totalRunTimeElapsed > TimeWarningThreshold)
            {
                return new ConduitDecision(
                    true,
                    ConduitReason.TimeLimit,
                    "Cap acceptable and time critical");
            }

            return new ConduitDecision(
                false,
                ConduitReason.RecoverCap,
                $"Recovering capacitor ({currentCapPercentage:F1}% -> 95%+)");
        }

        // All clear - proceed
        return new ConduitDecision(
            true,
            ConduitReason.RoomComplete,
            $"Room complete - proceed to next room (Time: {totalRunTimeElapsed:F0}s / {TotalTimeLimit}s)");
    }

    /// <summary>
    /// Tracks invulnerability timer after taking conduit
    /// </summary>
    /// <param name="timeSinceConduitJump">Seconds since jumping through conduit</param>
    /// <returns>Invulnerability status</returns>
    /// <remarks>
    /// After taking conduit, you have ~60 seconds of invulnerability in next room
    /// Use this time to:
    /// - Assess room threats
    /// - Position ship
    /// - Select initial targets
    /// - Prepare modules
    /// </remarks>
    public InvulnerabilityStatus TrackInvulnerability(double timeSinceConduitJump)
    {
        const double InvulnerabilityDuration = 60; // seconds

        var remaining = InvulnerabilityDuration - timeSinceConduitJump;
        var isInvulnerable = remaining > 0;

        var recommendation = remaining switch
        {
            > 45 => "Plan your approach and target priority",
            > 30 => "Position ship and select initial targets",
            > 15 => "Activate modules and prepare to engage",
            > 5 => "Invulnerability ending soon - ready for combat",
            _ => "Vulnerable - engage combat protocols"
        };

        return new InvulnerabilityStatus(
            isInvulnerable,
            Math.Max(0, remaining),
            recommendation);
    }

    /// <summary>
    /// Assesses filament tier difficulty
    /// </summary>
    /// <param name="tier">Filament tier (1-6)</param>
    /// <param name="ourShipType">Our ship type</param>
    /// <param name="ourDps">Our DPS</param>
    /// <param name="ourTank">Our tank (EHP/s)</param>
    /// <returns>Difficulty assessment</returns>
    /// <remarks>
    /// Tier 1-2: Easy, T1 frigates acceptable
    /// Tier 3-4: Moderate, T2 frigates or T1 cruisers
    /// Tier 5: Hard, T2 cruisers or HACs
    /// Tier 6: Very hard, HACs or specially fitted ships
    /// </remarks>
    public DifficultyAssessment AssessFilamentDifficulty(
        int tier,
        string ourShipType,
        double ourDps,
        double ourTank)
    {
        var difficulty = tier switch
        {
            1 => "Trivial",
            2 => "Easy",
            3 => "Moderate",
            4 => "Challenging",
            5 => "Difficult",
            6 => "Very Difficult",
            _ => "Unknown"
        };

        // Recommended minimums by tier
        var (minDps, minTank) = tier switch
        {
            1 => (100.0, 50.0),
            2 => (200.0, 100.0),
            3 => (300.0, 150.0),
            4 => (450.0, 250.0),
            5 => (600.0, 400.0),
            6 => (800.0, 600.0),
            _ => (0.0, 0.0)
        };

        var adequateDps = ourDps >= minDps;
        var adequateTank = ourTank >= minTank;
        var isReady = adequateDps && adequateTank;

        var warnings = new List<string>();
        if (!adequateDps)
            warnings.Add($"DPS below recommended ({ourDps:F0} < {minDps:F0})");
        if (!adequateTank)
            warnings.Add($"Tank below recommended ({ourTank:F0} < {minTank:F0})");

        return new DifficultyAssessment(
            tier,
            difficulty,
            isReady,
            minDps,
            minTank,
            warnings);
    }

    /// <summary>
    /// Identifies room spawn patterns and recommends tactics
    /// </summary>
    /// <param name="enemyTypes">Types of enemies in room</param>
    /// <returns>Spawn pattern analysis and tactical recommendations</returns>
    /// <remarks>
    /// Common patterns:
    /// - Swarm: Many small enemies (frigates) - use drones/smartbombs
    /// - Heavy: Few large enemies (battleships) - focus fire
    /// - Mixed: Variety of sizes - prioritize correctly
    /// - EWAR: Electronic warfare focus - kill EWAR first
    /// </remarks>
    public SpawnPatternAnalysis AnalyzeSpawnPattern(IEnumerable<string> enemyTypes)
    {
        var enemies = enemyTypes.ToList();
        var pattern = SpawnPattern.Mixed; // Default

        var frigateCount = enemies.Count(e => e.Contains("Damavik") || e.Contains("Vila"));
        var cruiserCount = enemies.Count(e => e.Contains("Vedmak") || e.Contains("Rodiva"));
        var battleshipCount = enemies.Count(e => e.Contains("Leshak") || e.Contains("Drekavac"));

        // Pattern detection
        if (frigateCount > 5 && cruiserCount == 0 && battleshipCount == 0)
            pattern = SpawnPattern.Swarm;
        else if (battleshipCount >= 2 && frigateCount <= 2)
            pattern = SpawnPattern.Heavy;
        else if (enemies.Any(e => e.Contains("Renewing") || e.Contains("Entangling")))
            pattern = SpawnPattern.Ewar;

        var tactics = pattern switch
        {
            SpawnPattern.Swarm => new[]
            {
                "Use drones for small targets",
                "Consider smartbombs if equipped",
                "Orbit and maintain transversal",
                "Kill frigates quickly before they accumulate damage"
            },
            SpawnPattern.Heavy => new[]
            {
                "Focus fire on battleships one at a time",
                "Use heavy drones if available",
                "Maintain optimal range",
                "Expect high alpha damage - keep transversal"
            },
            SpawnPattern.Ewar => new[]
            {
                "Priority: Kill EWAR ships first (Renewing, Entangling)",
                "Expect reduced effectiveness from webs/neuts",
                "Bring cap boosters",
                "Quick kills on support ships critical"
            },
            _ => new[]
            {
                "Assess and prioritize by threat",
                "Focus fire on priority targets",
                "Maintain good positioning"
            }
        };

        return new SpawnPatternAnalysis(pattern, frigateCount, cruiserCount, battleshipCount, tactics);
    }
}

/// <summary>
/// Filament types for Abyssal Deadspace
/// </summary>
public enum FilamentType
{
    Electrical,
    Firestorm,
    Dark,
    Exotic,
    Gamma,
    Chaotic
}

/// <summary>
/// Cache types in Abyssal rooms
/// </summary>
public enum CacheType
{
    Bioadaptive,      // Filaments
    Biocombinative,   // Mutaplasmids
    Storage           // Regular loot
}

/// <summary>
/// Weather analysis result
/// </summary>
/// <param name="Type">Filament type</param>
/// <param name="Tier">Weather tier (1-6)</param>
/// <param name="Benefits">List of beneficial effects</param>
/// <param name="Penalties">List of penalties</param>
/// <param name="Recommendations">Tactical recommendations</param>
public record WeatherAnalysis(
    FilamentType Type,
    int Tier,
    IReadOnlyList<string> Benefits,
    IReadOnlyList<string> Penalties,
    IReadOnlyList<string> Recommendations
);

/// <summary>
/// Cache target for looting
/// </summary>
/// <param name="Id">Cache ID</param>
/// <param name="Type">Cache type</param>
/// <param name="Distance">Distance to cache</param>
public record CacheTarget(
    long Id,
    CacheType Type,
    double Distance)
{
    public List<string> Notes { get; } = new();
}

/// <summary>
/// Conduit decision reasoning
/// </summary>
public enum ConduitReason
{
    RoomComplete,
    EnemiesRemaining,
    LootRemaining,
    RecoverHp,
    RecoverCap,
    TimeLimit
}

/// <summary>
/// Conduit (gate) decision
/// </summary>
/// <param name="ShouldTake">Whether to take conduit</param>
/// <param name="Reason">Reason code</param>
/// <param name="Description">Detailed explanation</param>
public record ConduitDecision(
    bool ShouldTake,
    ConduitReason Reason,
    string Description
);

/// <summary>
/// Invulnerability timer status
/// </summary>
/// <param name="IsInvulnerable">Whether currently invulnerable</param>
/// <param name="TimeRemaining">Seconds of invulnerability remaining</param>
/// <param name="Recommendation">What to do with remaining time</param>
public record InvulnerabilityStatus(
    bool IsInvulnerable,
    double TimeRemaining,
    string Recommendation
);

/// <summary>
/// Filament difficulty assessment
/// </summary>
/// <param name="Tier">Filament tier</param>
/// <param name="Difficulty">Difficulty description</param>
/// <param name="IsReady">Whether ship meets recommended minimums</param>
/// <param name="RecommendedMinDps">Recommended minimum DPS</param>
/// <param name="RecommendedMinTank">Recommended minimum tank (EHP/s)</param>
/// <param name="Warnings">Warning messages if not ready</param>
public record DifficultyAssessment(
    int Tier,
    string Difficulty,
    bool IsReady,
    double RecommendedMinDps,
    double RecommendedMinTank,
    IReadOnlyList<string> Warnings
);

/// <summary>
/// Spawn pattern types
/// </summary>
public enum SpawnPattern
{
    Swarm,   // Many small enemies
    Heavy,   // Few large enemies
    Mixed,   // Variety
    Ewar     // Electronic warfare focus
}

/// <summary>
/// Spawn pattern analysis
/// </summary>
/// <param name="Pattern">Detected pattern</param>
/// <param name="FrigateCount">Number of frigates</param>
/// <param name="CruiserCount">Number of cruisers</param>
/// <param name="BattleshipCount">Number of battleships</param>
/// <param name="TacticalRecommendations">Recommended tactics</param>
public record SpawnPatternAnalysis(
    SpawnPattern Pattern,
    int FrigateCount,
    int CruiserCount,
    int BattleshipCount,
    IReadOnlyList<string> TacticalRecommendations
);
