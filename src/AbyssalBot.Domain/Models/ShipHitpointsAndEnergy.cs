namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents the ship's hitpoints and energy status
/// </summary>
public record ShipHitpointsAndEnergy(
    double Shield,
    double Armor,
    double Hull,
    double Capacitor,
    double MaxShield,
    double MaxArmor,
    double MaxHull,
    double MaxCapacitor
)
{
    /// <summary>
    /// Shield percentage (0-100)
    /// </summary>
    public double ShieldPercentage => MaxShield > 0 ? (Shield / MaxShield) * 100 : 0;

    /// <summary>
    /// Armor percentage (0-100)
    /// </summary>
    public double ArmorPercentage => MaxArmor > 0 ? (Armor / MaxArmor) * 100 : 0;

    /// <summary>
    /// Hull percentage (0-100)
    /// </summary>
    public double HullPercentage => MaxHull > 0 ? (Hull / MaxHull) * 100 : 0;

    /// <summary>
    /// Capacitor percentage (0-100)
    /// </summary>
    public double CapacitorPercentage => MaxCapacitor > 0 ? (Capacitor / MaxCapacitor) * 100 : 0;

    /// <summary>
    /// Checks if ship is in critical condition
    /// </summary>
    public bool IsCritical => Shield < 150 || Armor < 100 || Hull < 50;

    /// <summary>
    /// Checks if shield needs repairing
    /// </summary>
    public bool NeedsShieldRepair(double threshold = 600) => Shield < threshold;

    /// <summary>
    /// Checks if capacitor is low
    /// </summary>
    public bool IsCapacitorLow(double threshold = 400) => Capacitor < threshold;
}
