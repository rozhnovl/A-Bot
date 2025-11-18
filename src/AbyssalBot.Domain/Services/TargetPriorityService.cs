using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services;

/// <summary>
/// Service for calculating target priorities and managing target selection
/// </summary>
public class TargetPriorityService
{
    private readonly NpcInformationService _npcInfoService;

    public TargetPriorityService(NpcInformationService npcInfoService)
    {
        _npcInfoService = npcInfoService;
    }

    /// <summary>
    /// Prioritizes targets based on threat level, type, and distance
    /// </summary>
    public IReadOnlyList<PrioritizedTarget> PrioritizeTargets(
        IEnumerable<Target> targets,
        int maxTargetingRange)
    {
        return targets
            .Where(t => t.IsEnemy)
            .Where(t => !t.Name.Contains("Extraction")) // Ignore extraction nodes
            .Where(t => t.Type != "Vila Swarmer") // Ignore swarmers
            .Where(t => t.Distance <= maxTargetingRange)
            .Select(target => new PrioritizedTarget(
                target,
                _npcInfoService.CalculateTargetPriority(target.Name, target.Type),
                _npcInfoService.IsKnownNpc(target.Type)
                    ? _npcInfoService.GetNpcDps(target.Type)
                    : 0
            ))
            .OrderBy(pt => pt)
            .ToList();
    }

    /// <summary>
    /// Gets the best target to attack from a list of targets
    /// </summary>
    public PrioritizedTarget? GetBestTarget(
        IEnumerable<Target> targets,
        int maxTargetingRange)
    {
        var prioritized = PrioritizeTargets(targets, maxTargetingRange);
        return prioritized.FirstOrDefault();
    }

    /// <summary>
    /// Gets targets that should be locked up to the max target limit
    /// </summary>
    public IReadOnlyList<Target> GetTargetsToLock(
        IEnumerable<Target> availableTargets,
        int maxTargetingRange,
        int maxTargets,
        int currentTargetCount)
    {
        var slotsAvailable = maxTargets - currentTargetCount;
        if (slotsAvailable <= 0)
            return Array.Empty<Target>();

        return PrioritizeTargets(availableTargets, maxTargetingRange)
            .Where(pt => !pt.Target.IsTargeted && !pt.Target.IsTargeting)
            .Take(slotsAvailable)
            .Select(pt => pt.Target)
            .ToList();
    }

    /// <summary>
    /// Calculates total incoming DPS from a list of targets
    /// </summary>
    public double CalculateIncomingDps(IEnumerable<Target> enemies)
    {
        return enemies
            .Where(t => t.IsEnemy)
            .Select(t => _npcInfoService.IsKnownNpc(t.Type)
                ? _npcInfoService.GetNpcDps(t.Type)
                : 0)
            .Sum();
    }

    /// <summary>
    /// Checks if a target should be used as an orbit beacon
    /// </summary>
    public bool IsOrbitBeacon(Target target)
    {
        return _npcInfoService.IsOrbitBeacon(target.Name);
    }
}
