using System;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for calculating effective damage application based on EVE Online mechanics
/// </summary>
/// <remarks>
/// Implements signature resolution, tracking, and missile application formulas
/// to determine how well damage will apply to a target
/// </remarks>
public class DamageApplicationService
{
    /// <summary>
    /// Calculates effective DPS for turret-based weapons using tracking formula
    /// </summary>
    /// <param name="baseDps">Base DPS of the weapon</param>
    /// <param name="tracking">Weapon tracking speed (rad/s)</param>
    /// <param name="angularVelocity">Angular velocity to target (rad/s)</param>
    /// <param name="optimalRange">Optimal range of weapon (meters)</param>
    /// <param name="falloffRange">Falloff range of weapon (meters)</param>
    /// <param name="distanceToTarget">Current distance to target (meters)</param>
    /// <param name="targetSignature">Target signature radius (meters)</param>
    /// <param name="weaponSignatureResolution">Weapon signature resolution (meters)</param>
    /// <returns>Effective DPS after tracking and range calculations</returns>
    /// <remarks>
    /// Uses EVE's hit chance formula:
    /// ChanceToHit = 0.5 ^ ((((Transversal speed/(Range to target * Tracking speed)) * (Signature Resolution / Signature Radius))^2)
    ///                    + ((max(0, Range - Optimal))/ Falloff)^2)
    /// </remarks>
    public double CalculateTurretEffectiveDps(
        double baseDps,
        double tracking,
        double angularVelocity,
        double optimalRange,
        double falloffRange,
        double distanceToTarget,
        double targetSignature,
        double weaponSignatureResolution)
    {
        if (baseDps <= 0 || tracking <= 0 || distanceToTarget <= 0)
            return 0;

        // Calculate tracking component
        var trackingComponent = angularVelocity / (distanceToTarget * tracking);
        trackingComponent *= weaponSignatureResolution / Math.Max(targetSignature, 1);
        trackingComponent = Math.Pow(trackingComponent, 2);

        // Calculate range component
        var rangeComponent = Math.Max(0, distanceToTarget - optimalRange) / Math.Max(falloffRange, 1);
        rangeComponent = Math.Pow(rangeComponent, 2);

        // Calculate hit chance using EVE formula
        var hitChance = Math.Pow(0.5, trackingComponent + rangeComponent);

        return baseDps * hitChance;
    }

    /// <summary>
    /// Calculates effective DPS for missile-based weapons
    /// </summary>
    /// <param name="baseDps">Base DPS of the missile</param>
    /// <param name="explosionRadius">Missile explosion radius (meters)</param>
    /// <param name="explosionVelocity">Missile explosion velocity (m/s)</param>
    /// <param name="targetSignature">Target signature radius (meters)</param>
    /// <param name="targetVelocity">Target velocity (m/s)</param>
    /// <param name="damageReductionFactor">Damage reduction factor (default 5.5)</param>
    /// <param name="damageReductionSensitivity">Damage reduction sensitivity (default 0.25)</param>
    /// <returns>Effective DPS after missile application</returns>
    /// <remarks>
    /// Uses EVE's missile damage formula:
    /// Applied Damage = Full Damage * min(1, S/E, (VeS/VtE)^(log(drf)/log(drs)))
    /// Where: S = target signature, E = explosion radius, Ve = explosion velocity, Vt = target velocity
    /// drf = damage reduction factor, drs = damage reduction sensitivity
    /// </remarks>
    public double CalculateMissileEffectiveDps(
        double baseDps,
        double explosionRadius,
        double explosionVelocity,
        double targetSignature,
        double targetVelocity,
        double damageReductionFactor = 5.5,
        double damageReductionSensitivity = 0.25)
    {
        if (baseDps <= 0 || explosionRadius <= 0 || explosionVelocity <= 0)
            return 0;

        // Signature factor
        var signatureFactor = Math.Min(1.0, targetSignature / explosionRadius);

        // Velocity factor
        var velocityFactor = (targetVelocity * explosionRadius) / (explosionVelocity * targetSignature);
        var velocityExponent = Math.Log(damageReductionFactor) / Math.Log(damageReductionSensitivity);
        velocityFactor = Math.Pow(velocityFactor, velocityExponent);

        // Final damage multiplier is minimum of all factors
        var damageMultiplier = Math.Min(1.0, Math.Min(signatureFactor, velocityFactor));

        return baseDps * damageMultiplier;
    }

    /// <summary>
    /// Calculates angular velocity to a target
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
    /// Determines if orbiting is better than keeping at range for damage application
    /// </summary>
    /// <param name="tracking">Weapon tracking (rad/s)</param>
    /// <param name="targetAngularVelocity">Current angular velocity to target</param>
    /// <param name="targetDps">Target's DPS output</param>
    /// <returns>True if orbiting is recommended for defense without losing too much DPS</returns>
    /// <remarks>
    /// Orbiting trades some damage application for defense through transversal.
    /// Recommended when: enemy DPS is high AND our tracking can still hit reasonably well while orbiting
    /// </remarks>
    public bool ShouldOrbitForDefense(
        double tracking,
        double targetAngularVelocity,
        double targetDps)
    {
        const double HighDpsThreshold = 200;
        const double TrackingThreshold = 0.3; // If tracking ratio is above this, we can still apply good damage

        if (targetDps < HighDpsThreshold)
            return false;

        // If we're already moving and our tracking can handle it, orbit is good
        var trackingRatio = targetAngularVelocity / Math.Max(tracking, 0.001);
        return trackingRatio < TrackingThreshold;
    }

    /// <summary>
    /// Determines if we should reduce transversal for better damage application
    /// </summary>
    /// <param name="currentEffectiveDps">Current effective DPS being applied</param>
    /// <param name="baseDps">Maximum possible DPS</param>
    /// <param name="targetDps">Target's DPS output</param>
    /// <returns>True if we should reduce transversal (stop orbiting, approach target)</returns>
    /// <remarks>
    /// If we're applying significantly reduced damage due to tracking/application issues,
    /// and the enemy DPS is manageable, we should reduce transversal for better application
    /// </remarks>
    public bool ShouldReduceTransversal(
        double currentEffectiveDps,
        double baseDps,
        double targetDps)
    {
        const double LowDpsThreshold = 150;
        const double ApplicationThreshold = 0.6; // If we're applying less than 60% of max DPS

        if (targetDps > LowDpsThreshold)
            return false; // Too dangerous to reduce transversal

        var applicationRatio = currentEffectiveDps / Math.Max(baseDps, 1);
        return applicationRatio < ApplicationThreshold;
    }

    /// <summary>
    /// Calculates optimal orbit radius based on weapon system and ship class
    /// </summary>
    /// <param name="optimalRange">Weapon optimal range</param>
    /// <param name="tracking">Weapon tracking</param>
    /// <param name="shipSignature">Our ship's signature radius</param>
    /// <param name="targetSignature">Target's signature radius</param>
    /// <returns>Recommended orbit radius in meters</returns>
    /// <remarks>
    /// Balances staying within optimal range while maintaining enough angular velocity for defense
    /// </remarks>
    public int CalculateOptimalOrbitRadius(
        double optimalRange,
        double tracking,
        double shipSignature,
        double targetSignature)
    {
        // For Abyssal filaments, typical ranges are 5-10km for frigates, 10-20km for cruisers
        // Use weapon optimal as baseline, but clamp to reasonable ranges
        var baseOrbit = optimalRange * 0.7; // Stay within optimal but not too close

        // Adjust for signature sizes - larger sig = orbit closer for better tracking
        var signatureRatio = targetSignature / Math.Max(shipSignature, 1);
        if (signatureRatio > 2.0) // Target is much larger
            baseOrbit *= 0.8;
        else if (signatureRatio < 0.5) // Target is smaller
            baseOrbit *= 1.2;

        // Clamp to practical ranges for Abyssal combat
        return (int)Math.Clamp(baseOrbit, 2000, 15000);
    }

    /// <summary>
    /// Determines if we should use a spiral approach when beyond weapon range
    /// </summary>
    /// <param name="distanceToTarget">Current distance to target</param>
    /// <param name="optimalRange">Weapon optimal range</param>
    /// <param name="falloffRange">Weapon falloff range</param>
    /// <returns>True if spiral approach is recommended</returns>
    /// <remarks>
    /// Spiral approach maintains some transversal while closing distance,
    /// providing defense while getting into range
    /// </remarks>
    public bool ShouldUseSpiralApproach(
        double distanceToTarget,
        double optimalRange,
        double falloffRange)
    {
        var effectiveRange = optimalRange + (falloffRange * 0.5);
        return distanceToTarget > effectiveRange * 1.2;
    }
}
