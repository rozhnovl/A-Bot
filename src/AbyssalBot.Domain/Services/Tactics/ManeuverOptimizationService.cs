using System;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for optimizing ship maneuvers based on transversal, angular velocity, and combat situation
/// </summary>
/// <remarks>
/// Implements EVE Online movement mechanics:
/// - Optimal orbit radius calculation
/// - Spiral approach patterns
/// - Transversal management for defense and damage application
/// - Angular velocity calculations
/// </remarks>
public class ManeuverOptimizationService
{
    private const double RadiansPerDegree = Math.PI / 180.0;

    /// <summary>
    /// Calculates transversal velocity between two objects
    /// </summary>
    /// <param name="ourVelocityX">Our velocity X component (m/s)</param>
    /// <param name="ourVelocityY">Our velocity Y component (m/s)</param>
    /// <param name="ourVelocityZ">Our velocity Z component (m/s)</param>
    /// <param name="targetVelocityX">Target velocity X component (m/s)</param>
    /// <param name="targetVelocityY">Target velocity Y component (m/s)</param>
    /// <param name="targetVelocityZ">Target velocity Z component (m/s)</param>
    /// <param name="distanceToTarget">Distance to target (meters)</param>
    /// <returns>Transversal velocity in m/s</returns>
    /// <remarks>
    /// Transversal is the component of relative velocity perpendicular to the line of sight
    /// </remarks>
    public double CalculateTransversalVelocity(
        double ourVelocityX, double ourVelocityY, double ourVelocityZ,
        double targetVelocityX, double targetVelocityY, double targetVelocityZ,
        double distanceToTarget)
    {
        // Relative velocity
        var relVelX = ourVelocityX - targetVelocityX;
        var relVelY = ourVelocityY - targetVelocityY;
        var relVelZ = ourVelocityZ - targetVelocityZ;

        var relativeSpeed = Math.Sqrt(relVelX * relVelX + relVelY * relVelY + relVelZ * relVelZ);

        // Simplified transversal calculation (would need position vectors for exact calculation)
        // For orbit, transversal ≈ orbital velocity
        return relativeSpeed; // Approximation when orbiting
    }

    /// <summary>
    /// Calculates angular velocity to target
    /// </summary>
    /// <param name="transversalVelocity">Transversal velocity (m/s)</param>
    /// <param name="distanceToTarget">Distance to target (meters)</param>
    /// <returns>Angular velocity in radians per second</returns>
    public double CalculateAngularVelocity(double transversalVelocity, double distanceToTarget)
    {
        if (distanceToTarget <= 0)
            return 0;

        return transversalVelocity / distanceToTarget;
    }

    /// <summary>
    /// Calculates optimal orbit radius based on ship and weapon characteristics
    /// </summary>
    /// <param name="shipClass">Ship class (Frigate, Destroyer, Cruiser, etc.)</param>
    /// <param name="weaponType">Weapon system type</param>
    /// <param name="weaponOptimalRange">Weapon optimal range (meters)</param>
    /// <param name="weaponTracking">Weapon tracking (rad/s)</param>
    /// <param name="targetAngularVelocity">Expected target angular velocity (rad/s)</param>
    /// <returns>Optimal orbit radius in meters</returns>
    /// <remarks>
    /// Balance between:
    /// - Staying within weapon optimal range
    /// - Maintaining enough angular velocity for defense
    /// - Not exceeding our tracking capability
    /// </remarks>
    public int CalculateOptimalOrbitRadius(
        ShipClass shipClass,
        WeaponType weaponType,
        double weaponOptimalRange,
        double weaponTracking,
        double targetAngularVelocity = 0)
    {
        // Base orbit on ship class
        var baseOrbit = shipClass switch
        {
            ShipClass.Frigate => 2500,      // Small, fast, close orbit
            ShipClass.Destroyer => 5000,    // Medium orbit
            ShipClass.Cruiser => 7500,      // Larger orbit
            ShipClass.Battlecruiser => 10000,
            ShipClass.Battleship => 15000,
            _ => 5000
        };

        // Adjust for weapon type
        var weaponMultiplier = weaponType switch
        {
            WeaponType.Rockets => 0.5,       // Close range
            WeaponType.LightMissiles => 0.8,
            WeaponType.HeavyMissiles => 1.2,
            WeaponType.AutoCannon => 0.7,    // Close range
            WeaponType.Artillery => 1.3,
            WeaponType.Blaster => 0.6,       // Very close
            WeaponType.RailGun => 1.4,
            WeaponType.Beam => 1.3,
            WeaponType.Pulse => 0.9,
            WeaponType.Drones => 1.0,
            _ => 1.0
        };

        var calculatedOrbit = baseOrbit * weaponMultiplier;

        // Don't exceed weapon optimal range
        calculatedOrbit = Math.Min(calculatedOrbit, weaponOptimalRange * 0.8);

        // Ensure we're not too close
        calculatedOrbit = Math.Max(calculatedOrbit, 1000);

        return (int)calculatedOrbit;
    }

    /// <summary>
    /// Determines if we should increase transversal for defense
    /// </summary>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <param name="currentAngularVelocity">Our current angular velocity (rad/s)</param>
    /// <param name="shieldPercentage">Current shield percentage</param>
    /// <param name="weaponTracking">Our weapon tracking (rad/s)</param>
    /// <returns>True if we should increase transversal (orbit faster, use MWD)</returns>
    /// <remarks>
    /// Increase transversal when:
    /// - Taking high DPS
    /// - Shield is getting low
    /// - We can afford to lose some damage application for survival
    /// </remarks>
    public bool ShouldIncreaseTransversal(
        double incomingDps,
        double currentAngularVelocity,
        double shieldPercentage,
        double weaponTracking)
    {
        const double HighDpsThreshold = 200;
        const double CriticalShieldThreshold = 40;
        const double LowAngularVelocity = 0.001; // rad/s

        // High DPS and low shield = need more defense
        if (incomingDps > HighDpsThreshold && shieldPercentage < CriticalShieldThreshold)
            return true;

        // High DPS and not moving much = need to start moving
        if (incomingDps > HighDpsThreshold && currentAngularVelocity < LowAngularVelocity)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if we should reduce transversal for better damage application
    /// </summary>
    /// <param name="effectiveDpsRatio">Ratio of effective DPS to max DPS (0-1)</param>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <param name="currentAngularVelocity">Our current angular velocity (rad/s)</param>
    /// <returns>True if we should reduce transversal (stop MWD, approach instead of orbit)</returns>
    /// <remarks>
    /// Reduce transversal when:
    /// - Our damage application is suffering
    /// - Incoming DPS is manageable
    /// - We're moving too fast for our weapons to track
    /// </remarks>
    public bool ShouldReduceTransversal(
        double effectiveDpsRatio,
        double incomingDps,
        double currentAngularVelocity)
    {
        const double LowDpsThreshold = 150;
        const double PoorApplicationThreshold = 0.6;
        const double HighAngularVelocity = 0.01; // rad/s

        // Low threat + poor damage application = reduce transversal
        if (incomingDps < LowDpsThreshold &&
            effectiveDpsRatio < PoorApplicationThreshold &&
            currentAngularVelocity > HighAngularVelocity)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates spiral approach parameters
    /// </summary>
    /// <param name="currentDistance">Current distance to target</param>
    /// <param name="targetDistance">Desired final distance</param>
    /// <param name="shipSpeed">Ship speed with current modules (m/s)</param>
    /// <returns>Spiral approach recommendation</returns>
    /// <remarks>
    /// Spiral approach maintains transversal while closing distance.
    /// Used when target is beyond weapon range but we want to maintain defense.
    /// </remarks>
    public SpiralApproach CalculateSpiralApproach(
        double currentDistance,
        double targetDistance,
        double shipSpeed)
    {
        if (currentDistance <= targetDistance)
        {
            return new SpiralApproach(
                false,
                0,
                0,
                "Already in range - no spiral needed");
        }

        // Start with wide orbit and gradually tighten as we approach
        var initialOrbitRadius = Math.Min(currentDistance * 0.3, 10000);
        var finalOrbitRadius = Math.Max(targetDistance * 0.8, 2000);

        var distanceToClose = currentDistance - targetDistance;
        var estimatedTime = distanceToClose / (shipSpeed * 0.7); // 0.7 because not straight line

        return new SpiralApproach(
            true,
            (int)initialOrbitRadius,
            (int)finalOrbitRadius,
            $"Spiral from {initialOrbitRadius:F0}m to {finalOrbitRadius:F0}m over ~{estimatedTime:F0}s");
    }

    /// <summary>
    /// Determines if MWD should be active based on tactical situation
    /// </summary>
    /// <param name="distanceToTarget">Distance to target (meters)</param>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <param name="targetHasWebifier">Whether target has webifier</param>
    /// <param name="capacitorPercentage">Current cap percentage</param>
    /// <param name="desiredRange">Desired combat range</param>
    /// <returns>MWD usage recommendation</returns>
    /// <remarks>
    /// MWD increases speed (good for transversal/closing distance) but:
    /// - Increases signature radius (easier to hit)
    /// - Uses significant capacitor
    /// - Can make you harder to track vs high alpha weapons
    /// </remarks>
    public MwdRecommendation ShouldActivateMWD(
        double distanceToTarget,
        double incomingDps,
        bool targetHasWebifier,
        double capacitorPercentage,
        double desiredRange)
    {
        const double MinCapForMWD = 30;
        const double HighAlphaThreshold = 300; // High burst damage

        // Never use MWD if cap is too low
        if (capacitorPercentage < MinCapForMWD)
        {
            return new MwdRecommendation(
                false,
                "Capacitor too low for MWD");
        }

        // Don't use MWD if webbed - wastes cap
        if (targetHasWebifier)
        {
            return new MwdRecommendation(
                false,
                "Webified - MWD ineffective");
        }

        // Use MWD to close distance when far
        if (distanceToTarget > desiredRange * 1.5)
        {
            return new MwdRecommendation(
                true,
                "Closing distance to weapon range");
        }

        // Use MWD for defense against high alpha (increases sig but also speed)
        if (incomingDps > HighAlphaThreshold && capacitorPercentage > 50)
        {
            return new MwdRecommendation(
                true,
                "High incoming damage - speed tank");
        }

        // In range and moderate threat - save cap
        return new MwdRecommendation(
            false,
            "In range - conserving capacitor");
    }

    /// <summary>
    /// Calculates signature radius bloom from MWD
    /// </summary>
    /// <param name="baseSignature">Base ship signature (meters)</param>
    /// <param name="mwdSignatureMultiplier">MWD signature radius bonus (typically 500%)</param>
    /// <returns>Signature radius with MWD active</returns>
    public double CalculateMWDSignature(double baseSignature, double mwdSignatureMultiplier = 5.0)
    {
        return baseSignature * (1 + mwdSignatureMultiplier);
    }
}

/// <summary>
/// Ship class categories
/// </summary>
public enum ShipClass
{
    Frigate,
    Destroyer,
    Cruiser,
    Battlecruiser,
    Battleship
}

/// <summary>
/// Weapon system types
/// </summary>
public enum WeaponType
{
    Rockets,
    LightMissiles,
    HeavyMissiles,
    AutoCannon,
    Artillery,
    Blaster,
    RailGun,
    Beam,
    Pulse,
    Drones,
    Unknown
}

/// <summary>
/// Spiral approach recommendation
/// </summary>
/// <param name="UseSpiral">Whether to use spiral approach</param>
/// <param name="InitialOrbitRadius">Starting orbit radius (meters)</param>
/// <param name="FinalOrbitRadius">Ending orbit radius (meters)</param>
/// <param name="Reason">Explanation</param>
public record SpiralApproach(
    bool UseSpiral,
    int InitialOrbitRadius,
    int FinalOrbitRadius,
    string Reason
);

/// <summary>
/// MWD activation recommendation
/// </summary>
/// <param name="ActivateMWD">Whether to activate MWD</param>
/// <param name="Reason">Explanation</param>
public record MwdRecommendation(
    bool ActivateMWD,
    string Reason
);
