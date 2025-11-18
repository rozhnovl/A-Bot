namespace AbyssalBot.Domain.Interfaces;

/// <summary>
/// Provides access to ship's inventory and cargo
/// </summary>
public interface IInventoryProvider
{
    /// <summary>
    /// Checks if an item exists in inventory
    /// </summary>
    bool HasItem(string itemName);

    /// <summary>
    /// Gets the quantity of an item in inventory
    /// </summary>
    int GetItemQuantity(string itemName);

    /// <summary>
    /// Checks if the inventory window is open
    /// </summary>
    bool IsInventoryWindowOpen();

    /// <summary>
    /// Checks if there is a lootable container nearby
    /// </summary>
    bool HasLootableContainer();

    /// <summary>
    /// Checks if the loot window is empty
    /// </summary>
    bool IsLootWindowEmpty();
}
