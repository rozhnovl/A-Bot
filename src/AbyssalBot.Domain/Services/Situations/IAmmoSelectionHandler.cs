using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles ammo selection for projectile/missile weapons
/// </summary>
public interface IAmmoSelectionHandler
{
    /// <summary>
    /// Selects the best ammo type for the target
    /// </summary>
    /// <param name="target">Current target</param>
    /// <param name="weapon">Weapon being used</param>
    /// <returns>Best ammo type to use</returns>
    AmmoType SelectBestAmmo(Target target, WeaponType weapon);

    /// <summary>
    /// Gets reasoning for ammo selection
    /// </summary>
    /// <param name="target">Target</param>
    /// <param name="weapon">Weapon</param>
    /// <returns>Reasoning string</returns>
    string GetSelectionReasoning(Target target, WeaponType weapon);
}
