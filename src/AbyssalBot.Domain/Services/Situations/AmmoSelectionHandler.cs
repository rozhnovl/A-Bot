using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles intelligent ammo selection based on target and situation
/// </summary>
public class AmmoSelectionHandler : IAmmoSelectionHandler
{
    // Distance thresholds
    private const int CloseRange = 5000;
    private const int MediumRange = 15000;
    private const int LongRange = 30000;

    // Target size classification
    private static readonly HashSet<string> SmallTargets = new()
    {
        "Frigate", "Destroyer", "Drone", "Damavik"
    };

    private static readonly HashSet<string> MediumTargets = new()
    {
        "Cruiser", "Vedmak"
    };

    private static readonly HashSet<string> LargeTargets = new()
    {
        "Battlecruiser", "Battleship", "Leshak"
    };

    public AmmoType SelectBestAmmo(Target target, WeaponType weapon)
    {
        var targetSize = DetermineTargetSize(target);
        var range = DetermineRangeCategory(target.Distance, weapon);

        // For missiles/projectiles, consider both range and target size
        return (targetSize, range) switch
        {
            ("Small", "Close") => new AmmoType("Light/Short Range", weapon.PreferredDamageType, CloseRange, 1.2),
            ("Small", "Medium") => new AmmoType("Light/Medium Range", weapon.PreferredDamageType, MediumRange, 1.0),
            ("Small", "Long") => new AmmoType("Light/Long Range", weapon.PreferredDamageType, LongRange, 0.8),

            ("Medium", "Close") => new AmmoType("Medium/Short Range", weapon.PreferredDamageType, CloseRange, 1.3),
            ("Medium", "Medium") => new AmmoType("Medium/Standard", weapon.PreferredDamageType, MediumRange, 1.1),
            ("Medium", "Long") => new AmmoType("Medium/Long Range", weapon.PreferredDamageType, LongRange, 0.9),

            ("Large", "Close") => new AmmoType("Heavy/Short Range", weapon.PreferredDamageType, CloseRange, 1.5),
            ("Large", "Medium") => new AmmoType("Heavy/Standard", weapon.PreferredDamageType, MediumRange, 1.2),
            ("Large", "Long") => new AmmoType("Heavy/Long Range", weapon.PreferredDamageType, LongRange, 1.0),

            _ => new AmmoType("Standard", weapon.PreferredDamageType, weapon.BaseOptimalRange, 1.0)
        };
    }

    public string GetSelectionReasoning(Target target, WeaponType weapon)
    {
        var ammo = SelectBestAmmo(target, weapon);
        var targetSize = DetermineTargetSize(target);
        var range = DetermineRangeCategory(target.Distance, weapon);

        return $"Selected {ammo.Name} for {targetSize} target at {range.ToLower()} range ({target.Distance}m) - {ammo.DamageType} damage";
    }

    private string DetermineTargetSize(Target target)
    {
        var typeLower = target.Type.ToLower();
        var nameLower = target.Name.ToLower();

        // Check if name or type indicates size
        if (SmallTargets.Any(s => nameLower.Contains(s.ToLower()) || typeLower.Contains(s.ToLower())))
        {
            return "Small";
        }

        if (LargeTargets.Any(s => nameLower.Contains(s.ToLower()) || typeLower.Contains(s.ToLower())))
        {
            return "Large";
        }

        if (MediumTargets.Any(s => nameLower.Contains(s.ToLower()) || typeLower.Contains(s.ToLower())))
        {
            return "Medium";
        }

        // Default to medium if unknown
        return "Medium";
    }

    private string DetermineRangeCategory(int distance, WeaponType weapon)
    {
        // Determine range category based on weapon's optimal and falloff
        var optimalRange = weapon.BaseOptimalRange;

        if (distance <= optimalRange)
        {
            return "Close";
        }
        else if (distance <= optimalRange + weapon.BaseFalloffRange / 2)
        {
            return "Medium";
        }
        else
        {
            return "Long";
        }
    }
}
