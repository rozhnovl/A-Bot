namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents information about an NPC type
/// </summary>
public record NpcInfo(
    string TypeName,
    double Dps,
    int TargetPriority,
    bool IsOrbitBeacon = false
);
