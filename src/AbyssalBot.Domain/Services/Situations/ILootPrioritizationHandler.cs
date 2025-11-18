using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles loot prioritization decisions
/// </summary>
public interface ILootPrioritizationHandler
{
    /// <summary>
    /// Prioritizes loot items based on value and available space
    /// </summary>
    /// <param name="available">Available loot targets</param>
    /// <param name="space">Current inventory space</param>
    /// <returns>Prioritized list of loot targets to collect</returns>
    IReadOnlyList<LootTarget> PrioritizeLoot(
        IReadOnlyList<LootTarget> available,
        InventorySpace space);

    /// <summary>
    /// Gets reasoning for loot prioritization decisions
    /// </summary>
    /// <param name="available">Available loot</param>
    /// <param name="space">Inventory space</param>
    /// <returns>Reasoning string</returns>
    string GetPrioritizationReasoning(
        IReadOnlyList<LootTarget> available,
        InventorySpace space);
}
