namespace AbotEngine.Fleet;

/// <summary>Nominal mixed EM/thermal damage needed to remove an NPC's remaining tank.</summary>
public readonly record struct RetributionVolleyEstimate(
    bool IsKnown,
    LaserCrystal Crystal,
    double Volleys,
    int Shots,
    double RawEmPerVolley,
    double RawThermalPerVolley,
    string Reason)
{
    public static RetributionVolleyEstimate Unknown(LaserCrystal crystal, string reason) =>
        new(false, crystal, 0, 0, 0, 0, reason);
}

/// <summary>
/// Converts NPC layer HP/resonances and the fitted beam/crystal profile into solo volleys to kill.
/// A fractional volley is carried into the next layer, matching a single shot consuming shield,
/// armor and hull in sequence. Tracking and hit-quality randomness are deliberately represented by
/// the weapon profile's application factor rather than being hidden inside an EHP constant.
/// </summary>
public static class RetributionDeaconVolleyEstimator
{
    public static RetributionVolleyEstimate Estimate(
        RetributionDeaconEnemyState enemy,
        RetributionLaserWeaponProfile weapon,
        LaserCrystal requestedCrystal)
    {
        var crystal = requestedCrystal == LaserCrystal.KeepCurrent
            ? weapon.CurrentCrystal
            : requestedCrystal;
        if (requestedCrystal == LaserCrystal.KeepCurrent && !weapon.CurrentCrystalKnown)
            return RetributionVolleyEstimate.Unknown(crystal, "loaded crystal is unknown");
        var charge = ChargeDamage(crystal);
        if (charge is null)
            return RetributionVolleyEstimate.Unknown(crystal, "crystal damage is unknown");
        if (!enemy.Defense.IsKnown)
            return RetributionVolleyEstimate.Unknown(crystal, "NPC layer HP/resists are unknown");
        if (weapon.TurretCount <= 0 || weapon.PerTurretDamageMultiplier <= 0 ||
            weapon.ApplicationFactor <= 0)
            return RetributionVolleyEstimate.Unknown(crystal, "weapon damage profile is invalid");

        var groupMultiplier = weapon.TurretCount * weapon.PerTurretDamageMultiplier *
                              Math.Clamp(weapon.ApplicationFactor, 0.01, 1);
        var rawEm = charge.Value.Em * groupMultiplier;
        var rawThermal = charge.Value.Thermal * groupMultiplier;
        var volleyFractions = 0d;

        foreach (var layer in new[] { enemy.Defense.Shield, enemy.Defense.Armor, enemy.Defense.Hull })
        {
            var hp = layer.Hitpoints * Math.Clamp(layer.RemainingPct, 0, 100) / 100d;
            if (hp <= 0) continue;
            var applied = rawEm * layer.EmResonance + rawThermal * layer.ThermalResonance;
            if (applied <= 0)
                return RetributionVolleyEstimate.Unknown(crystal, "crystal cannot damage one tank layer");
            volleyFractions += hp / applied;
        }

        if (volleyFractions <= 0)
            return RetributionVolleyEstimate.Unknown(crystal, "NPC has no remaining HP");

        return new RetributionVolleyEstimate(
            true,
            crystal,
            volleyFractions,
            Math.Max(1, (int)Math.Ceiling(volleyFractions - 1e-9)),
            rawEm,
            rawThermal,
            "");
    }

    /// <summary>
    /// Exact SDE charge damage for the crystals carried by the T3 beginner fit. Standard and
    /// Multifrequency mean their Imperial Navy variants used by that fit.
    /// </summary>
    private static (double Em, double Thermal)? ChargeDamage(LaserCrystal crystal)
    {
        var profile = RetributionCrystalCatalog.For(crystal);
        return profile is null ? null : (profile.EmDamage, profile.ThermalDamage);
    }
}
