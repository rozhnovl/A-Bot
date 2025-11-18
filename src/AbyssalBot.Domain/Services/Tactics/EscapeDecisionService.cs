using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for making escape and survival decisions in EVE Online
/// </summary>
/// <remarks>
/// Implements EVE Online escape mechanics:
/// - Warp scramble/disruptor detection
/// - Align to safe spot mechanics
/// - Overheat propulsion for escape
/// - Red boxing detection (being targeted)
/// - Emergency warp decision logic
/// - Filament exit mechanics (Abyssal specific)
/// </remarks>
public class EscapeDecisionService
{
    /// <summary>
    /// Determines if we should attempt to escape current situation
    /// </summary>
    /// <param name="currentHpPercentage">Current HP percentage</param>
    /// <param name="canWarp">Whether we can warp (not scrambled)</param>
    /// <param name="enemiesRemaining">Number of enemies remaining</param>
    /// <param name="timeToLive">Estimated seconds until death (-1 if holding)</param>
    /// <param name="capacitorPercentage">Current cap percentage</param>
    /// <returns>Escape decision with reasoning</returns>
    /// <remarks>
    /// Escape criteria:
    /// - HP critical and can't break enemy tank
    /// - Outnumbered and taking unsustainable damage
    /// - Cap dying and can't maintain active tank
    /// </remarks>
    public EscapeDecision ShouldEscape(
        double currentHpPercentage,
        bool canWarp,
        int enemiesRemaining,
        double timeToLive,
        double capacitorPercentage)
    {
        const double CriticalHpThreshold = 25;
        const double LowHpThreshold = 40;
        const double CriticalCapThreshold = 15;
        const double CriticalTimeToLive = 30; // seconds

        // Critical HP and short TTL - immediate escape
        if (currentHpPercentage < CriticalHpThreshold && timeToLive > 0 && timeToLive < CriticalTimeToLive)
        {
            if (canWarp)
            {
                return new EscapeDecision(
                    true,
                    EscapeMethod.Warp,
                    "Critical HP and short TTL - emergency warp");
            }
            else
            {
                return new EscapeDecision(
                    true,
                    EscapeMethod.Burn,
                    "Critical HP but scrambled - burn away and break scram range");
            }
        }

        // Low HP, many enemies, and can warp
        if (currentHpPercentage < LowHpThreshold && enemiesRemaining >= 5 && canWarp)
        {
            return new EscapeDecision(
                true,
                EscapeMethod.Warp,
                "Outnumbered with low HP - tactical withdrawal");
        }

        // Cap critical and HP low - can't sustain
        if (capacitorPercentage < CriticalCapThreshold && currentHpPercentage < LowHpThreshold)
        {
            if (canWarp)
            {
                return new EscapeDecision(
                    true,
                    EscapeMethod.Warp,
                    "Cap and HP both critical - cannot sustain combat");
            }
        }

        // Fight is winnable - stay
        return new EscapeDecision(
            false,
            EscapeMethod.None,
            "Situation manageable - continue fighting");
    }

    /// <summary>
    /// Detects if we are warp scrambled or disrupted
    /// </summary>
    /// <param name="scramblerCount">Number of scramblers on us</param>
    /// <param name="disruptorCount">Number of disruptors on us</param>
    /// <param name="warpCoreStrength">Our warp core strength (from stabs)</param>
    /// <returns>Warp disruption status</returns>
    /// <remarks>
    /// Scrambler = 2 points, Disruptor = 1 point, Warp Stab = -1 point
    /// If total points >= 1, cannot warp
    /// Scramblers also disable MWD
    /// </remarks>
    public WarpDisruptionStatus CheckWarpDisruption(
        int scramblerCount,
        int disruptorCount,
        int warpCoreStrength = 0)
    {
        var totalPoints = (scramblerCount * 2) + disruptorCount + warpCoreStrength;

        var canWarp = totalPoints < 1;
        var mwdDisabled = scramblerCount > 0;

        var method = canWarp
            ? "None - can warp freely"
            : scramblerCount > 0
                ? $"Warp Scrambled ({scramblerCount} scrams) - MWD disabled"
                : $"Warp Disrupted ({disruptorCount} points)";

        return new WarpDisruptionStatus(
            canWarp,
            mwdDisabled,
            totalPoints,
            method);
    }

    /// <summary>
    /// Calculates align time to warp
    /// </summary>
    /// <param name="shipMass">Ship mass (kg)</param>
    /// <param name="shipInertia">Ship inertia modifier</param>
    /// <param name="propModActive">Whether propulsion module is active</param>
    /// <param name="propModMassBonus">Mass addition from prop mod (%)</param>
    /// <returns>Time to align to warp speed (seconds)</returns>
    /// <remarks>
    /// EVE align formula: Time = -ln(0.25) * Inertia * Mass / 500000
    /// Where 0.25 represents 75% of max velocity (warp threshold)
    /// </remarks>
    public double CalculateAlignTime(
        double shipMass,
        double shipInertia,
        bool propModActive,
        double propModMassBonus = 0)
    {
        var effectiveMass = shipMass;
        if (propModActive)
            effectiveMass *= (1 + propModMassBonus / 100);

        var alignTime = -Math.Log(0.25) * shipInertia * effectiveMass / 500000;
        return alignTime;
    }

    /// <summary>
    /// Determines if prop mod should be overheated for escape
    /// </summary>
    /// <param name="escapeMethod">Current escape method</param>
    /// <param name="distanceToSafety">Distance to safety (gate, warp out range)</param>
    /// <param name="propModHeatLevel">Current heat level of prop mod</param>
    /// <param name="pursuersInRange">Number of pursuers in range</param>
    /// <returns>True if prop mod should be overheated</returns>
    /// <remarks>
    /// Overheat prop mod when:
    /// - Burning away from scram range
    /// - Need to align faster (MWD off, but afterburner can help)
    /// - Critical escape situation
    /// </remarks>
    public bool ShouldOverheatPropMod(
        EscapeMethod escapeMethod,
        double distanceToSafety,
        double propModHeatLevel,
        int pursuersInRange)
    {
        const double HeatDangerThreshold = 0.95;

        // Never overheat if about to burn out
        if (propModHeatLevel > HeatDangerThreshold)
            return false;

        // Burning away - overheat for maximum speed
        if (escapeMethod == EscapeMethod.Burn)
            return true;

        // Close pursuit - overheat to create separation
        if (pursuersInRange > 0 && distanceToSafety < 10000)
            return true;

        return false;
    }

    /// <summary>
    /// Identifies if we are being targeted (red boxed)
    /// </summary>
    /// <param name="lockingUs">Number of entities actively locking us</param>
    /// <param name="lockedUs">Number of entities with lock on us</param>
    /// <returns>Threat assessment</returns>
    /// <remarks>
    /// Red boxing = being targeted by hostile
    /// Multiple red boxes = high priority target or multiple hostiles
    /// Can use this to preemptively align/prepare to escape
    /// </remarks>
    public TargetingThreat AssessTargetingThreat(int lockingUs, int lockedUs)
    {
        var totalThreat = lockingUs + lockedUs;

        var threatLevel = totalThreat switch
        {
            0 => ThreatLevel.None,
            1 => ThreatLevel.Low,
            2 or 3 => ThreatLevel.Moderate,
            >= 4 => ThreatLevel.High
        };

        var description = threatLevel switch
        {
            ThreatLevel.None => "No hostile locks",
            ThreatLevel.Low => $"Single hostile lock ({lockingUs} locking, {lockedUs} locked)",
            ThreatLevel.Moderate => $"Multiple hostile locks ({lockingUs} locking, {lockedUs} locked)",
            ThreatLevel.High => $"Heavy hostile attention ({lockingUs} locking, {lockedUs} locked) - primary target",
            _ => "Unknown"
        };

        return new TargetingThreat(threatLevel, lockingUs, lockedUs, description);
    }

    /// <summary>
    /// Determines optimal safe spot type for current situation
    /// </summary>
    /// <param name="inAbyssalSpace">Whether currently in Abyssal Deadspace</param>
    /// <param name="nearGate">Whether near a gate</param>
    /// <param name="nearStation">Whether near a station</param>
    /// <param name="canUseFilament">Whether we have exit filament (Abyssal)</param>
    /// <returns>Recommended safe destination</returns>
    public SafeDestination DetermineEscapeDestination(
        bool inAbyssalSpace,
        bool nearGate,
        bool nearStation,
        bool canUseFilament)
    {
        // Abyssal space - special rules
        if (inAbyssalSpace)
        {
            if (canUseFilament)
            {
                return new SafeDestination(
                    DestinationType.Filament,
                    0,
                    "Use filament to exit Abyssal space");
            }
            else
            {
                return new SafeDestination(
                    DestinationType.Conduit,
                    0,
                    "Take conduit to next room (cannot exit without filament)");
            }
        }

        // Normal space - prefer station > gate > safe spot
        if (nearStation)
        {
            return new SafeDestination(
                DestinationType.Station,
                0,
                "Dock at nearby station");
        }

        if (nearGate)
        {
            return new SafeDestination(
                DestinationType.Gate,
                0,
                "Jump through gate");
        }

        return new SafeDestination(
            DestinationType.SafeSpot,
            150000000, // 150,000 km - typical safe spot distance
            "Warp to safe spot");
    }

    /// <summary>
    /// Calculates if we can break scram range before dying
    /// </summary>
    /// <param name="currentDistance">Current distance to scrambler</param>
    /// <param name="scramRange">Scram range (typically 7500-10000m)</param>
    /// <param name="ourSpeed">Our current speed (m/s)</param>
    /// <param name="scramblerSpeed">Scrambler's speed (m/s)</param>
    /// <param name="timeToLive">Our time to live (seconds)</param>
    /// <returns>True if we can escape scram range before dying</returns>
    public bool CanBreakScramRange(
        double currentDistance,
        double scramRange,
        double ourSpeed,
        double scramblerSpeed,
        double timeToLive)
    {
        if (timeToLive <= 0)
            return false; // Already holding, no rush

        // Distance to break scram
        var distanceToBreak = scramRange - currentDistance;

        if (distanceToBreak <= 0)
            return true; // Already out of range

        // Relative speed (if they're chasing)
        var relativeSpeed = ourSpeed - scramblerSpeed;

        if (relativeSpeed <= 0)
            return false; // They're faster, can't escape

        // Time to break scram range
        var timeToBreak = distanceToBreak / relativeSpeed;

        // Can we survive long enough?
        return timeToBreak < timeToLive * 0.8; // 20% safety margin
    }

    /// <summary>
    /// Determines priority order for killing scrams to enable escape
    /// </summary>
    /// <param name="scramblers">Entities scrambling us</param>
    /// <returns>Ordered list of scramblers to kill (highest priority first)</returns>
    /// <remarks>
    /// If we can't run, fight to break free
    /// Kill weakest/closest scramblers first
    /// </remarks>
    public IReadOnlyList<ScramblerTarget> PrioritizeScramblerKills(
        IEnumerable<ScramblerTarget> scramblers)
    {
        return scramblers
            .OrderBy(s => s.EstimatedHp) // Weakest first (quick kill)
            .ThenBy(s => s.Distance) // Closer first
            .ToList();
    }
}

/// <summary>
/// Escape decision result
/// </summary>
/// <param name="ShouldEscape">Whether to attempt escape</param>
/// <param name="Method">Recommended escape method</param>
/// <param name="Reason">Explanation</param>
public record EscapeDecision(
    bool ShouldEscape,
    EscapeMethod Method,
    string Reason
);

/// <summary>
/// Escape methods
/// </summary>
public enum EscapeMethod
{
    None,      // Stay and fight
    Warp,      // Warp to safe spot
    Burn,      // Burn away (scrambled, trying to break range)
    Gate,      // Jump through gate
    Dock,      // Dock at station
    Filament   // Use filament (Abyssal)
}

/// <summary>
/// Warp disruption status
/// </summary>
/// <param name="CanWarp">Whether we can warp</param>
/// <param name="MwdDisabled">Whether MWD is disabled (scrambled)</param>
/// <param name="TotalPoints">Total warp disruption points</param>
/// <param name="Description">Status description</param>
public record WarpDisruptionStatus(
    bool CanWarp,
    bool MwdDisabled,
    int TotalPoints,
    string Description
);

/// <summary>
/// Targeting threat assessment
/// </summary>
/// <param name="Level">Threat level</param>
/// <param name="LockingCount">Entities locking us</param>
/// <param name="LockedCount">Entities with lock on us</param>
/// <param name="Description">Threat description</param>
public record TargetingThreat(
    ThreatLevel Level,
    int LockingCount,
    int LockedCount,
    string Description
);

/// <summary>
/// Threat level enumeration
/// </summary>
public enum ThreatLevel
{
    None,
    Low,
    Moderate,
    High,
    Critical
}

/// <summary>
/// Safe destination for escape
/// </summary>
/// <param name="Type">Destination type</param>
/// <param name="Distance">Distance to destination (meters, 0 if nearby)</param>
/// <param name="Description">Destination description</param>
public record SafeDestination(
    DestinationType Type,
    double Distance,
    string Description
);

/// <summary>
/// Destination types
/// </summary>
public enum DestinationType
{
    Station,
    Gate,
    SafeSpot,
    Filament,
    Conduit
}

/// <summary>
/// Entity that is scrambling us
/// </summary>
/// <param name="Id">Entity ID</param>
/// <param name="Name">Entity name</param>
/// <param name="Distance">Distance to entity</param>
/// <param name="EstimatedHp">Estimated HP</param>
public record ScramblerTarget(
    long Id,
    string Name,
    double Distance,
    double EstimatedHp
);
