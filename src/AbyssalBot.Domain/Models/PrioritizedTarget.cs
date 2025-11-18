namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents a target with calculated priority
/// </summary>
public record PrioritizedTarget(
    Target Target,
    int Priority,
    double EstimatedDps
) : IComparable<PrioritizedTarget>
{
    /// <summary>
    /// Compares priorities (lower value = higher priority)
    /// </summary>
    public int CompareTo(PrioritizedTarget? other)
    {
        if (other == null) return -1;

        var priorityComparison = Priority.CompareTo(other.Priority);
        if (priorityComparison != 0) return priorityComparison;

        // If same priority, sort by distance (closer = higher priority)
        return Target.Distance.CompareTo(other.Target.Distance);
    }

    /// <summary>
    /// Checks if this is a high priority target
    /// </summary>
    public bool IsHighPriority => Priority < 100;

    /// <summary>
    /// Checks if this is a critical priority target
    /// </summary>
    public bool IsCriticalPriority => Priority < 10;
}
