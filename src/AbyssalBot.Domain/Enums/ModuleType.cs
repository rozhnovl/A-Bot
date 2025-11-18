namespace AbyssalBot.Domain.Enums;

/// <summary>
/// Represents the type of ship module
/// </summary>
public enum ModuleType
{
    /// <summary>
    /// Defensive module that provides resistance
    /// </summary>
    Hardener,

    /// <summary>
    /// Offensive weapon module
    /// </summary>
    Weapon,

    /// <summary>
    /// Active shield repair module
    /// </summary>
    ShieldBooster,

    /// <summary>
    /// Microwarpdrive for speed boost
    /// </summary>
    MWD,

    /// <summary>
    /// Other module types
    /// </summary>
    Etc
}
