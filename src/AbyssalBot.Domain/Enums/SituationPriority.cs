namespace AbyssalBot.Domain.Enums;

/// <summary>
/// Represents the priority level of a situation
/// Higher values = higher priority
/// </summary>
public enum SituationPriority
{
    /// <summary>
    /// Normal combat operations
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Low capacitor warning (< 30%)
    /// </summary>
    LowCapacitor = 10,

    /// <summary>
    /// Multiple hostiles engaging (> 10)
    /// </summary>
    MultipleHostiles = 20,

    /// <summary>
    /// EWAR affecting ship (jammed/neuted)
    /// </summary>
    Ewar = 30,

    /// <summary>
    /// Capacitor emergency (< 10%)
    /// </summary>
    CapacitorEmergency = 40,

    /// <summary>
    /// Critical damage to structure (< 50%)
    /// </summary>
    CriticalDamage = 50
}
