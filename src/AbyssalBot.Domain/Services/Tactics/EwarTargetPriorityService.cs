using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Advanced target priority service with EWAR (Electronic Warfare) mechanics
/// </summary>
/// <remarks>
/// Implements EVE Online targeting priority with:
/// - EWAR threat assessment (jams, neuts, webs, scrams)
/// - Target painter/web synergy
/// - Warp disruption mechanics (scrams vs points)
/// - Logistics ship identification and priority
/// - Coordinated EWAR targeting
/// </remarks>
public class EwarTargetPriorityService
{
    /// <summary>
    /// Calculates target priority with EWAR considerations
    /// </summary>
    /// <param name="target">Target to evaluate</param>
    /// <param name="ourShipType">Our ship type for relative threat assessment</param>
    /// <param name="isPainted">Whether target is painted (easier to hit)</param>
    /// <param name="isWebbed">Whether target is webbed (slower, easier to hit)</param>
    /// <returns>Priority score (lower = higher priority)</returns>
    public int CalculateEwarPriority(
        EwarTarget target,
        string ourShipType,
        bool isPainted,
        bool isWebbed)
    {
        var basePriority = 100;

        // EWAR ships are highest priority - they force multiply enemy effectiveness
        if (target.IsJammer)
            basePriority = 1; // ECM breaks locks - absolutely critical
        else if (target.IsNeut)
            basePriority = 2; // Energy neuts kill cap-dependent ships
        else if (target.IsLogistics)
            basePriority = 3; // Logi keeps enemies alive - must die first
        else if (target.IsScrambler)
            basePriority = 4; // Scrams prevent escape
        else if (target.IsWebber)
            basePriority = 5; // Webs reduce speed/tracking
        else if (target.IsDampener)
            basePriority = 6; // Damps reduce lock range/speed
        else if (target.IsPainter)
            basePriority = 7; // Painters increase damage taken

        // Painted targets are easier to kill - boost priority slightly
        if (isPainted && basePriority > 10)
            basePriority -= 2;

        // Webbed targets are easier to kill - boost priority slightly
        if (isWebbed && basePriority > 10)
            basePriority -= 2;

        // High DPS ships
        if (target.EstimatedDps > 300)
            basePriority = Math.Min(basePriority, 8);
        else if (target.EstimatedDps > 200)
            basePriority = Math.Min(basePriority, 12);

        // Adjust for distance - closer threats are more urgent
        if (target.Distance < 5000)
            basePriority -= 1;
        else if (target.Distance > 20000)
            basePriority += 2;

        return Math.Max(1, basePriority);
    }

    /// <summary>
    /// Determines which EWAR module to prioritize
    /// </summary>
    /// <param name="targets">Available targets</param>
    /// <param name="ourEwarType">Type of EWAR we can apply</param>
    /// <returns>Best target for our EWAR</returns>
    /// <remarks>
    /// Synergize EWAR effects:
    /// - Paint the target we're shooting
    /// - Web the fastest/closest threat
    /// - Neut the ship that's scramming/neuting us
    /// </remarks>
    public EwarTarget? SelectEwarTarget(
        IEnumerable<EwarTarget> targets,
        EwarType ourEwarType)
    {
        var targetList = targets.Where(t => t.IsEnemy && t.Distance < 50000).ToList();

        if (!targetList.Any())
            return null;

        return ourEwarType switch
        {
            EwarType.TargetPainter => SelectPaintTarget(targetList),
            EwarType.StasisWeb => SelectWebTarget(targetList),
            EwarType.Neut => SelectNeutTarget(targetList),
            EwarType.WarpScrambler => SelectScramTarget(targetList),
            EwarType.Dampener => SelectDampTarget(targetList),
            _ => null
        };
    }

    /// <summary>
    /// Selects best target for target painter
    /// </summary>
    /// <remarks>
    /// Paint the target we're actively shooting for maximum damage
    /// Prefer small, fast targets that benefit most from painter
    /// </remarks>
    private EwarTarget? SelectPaintTarget(List<EwarTarget> targets)
    {
        // Paint our primary target if possible
        var primaryTarget = targets.FirstOrDefault(t => t.IsPrimaryTarget);
        if (primaryTarget != null && primaryTarget.SignatureRadius < 100)
            return primaryTarget;

        // Otherwise paint the smallest, highest-priority target
        return targets
            .Where(t => t.SignatureRadius < 150) // Small targets benefit most
            .OrderBy(t => CalculateEwarPriority(t, "", false, false))
            .ThenBy(t => t.SignatureRadius)
            .FirstOrDefault();
    }

    /// <summary>
    /// Selects best target for stasis webifier
    /// </summary>
    /// <remarks>
    /// Web fast-moving threats to reduce their transversal
    /// Or web high-priority targets to make them easier to hit
    /// </remarks>
    private EwarTarget? SelectWebTarget(List<EwarTarget> targets)
    {
        const double HighSpeedThreshold = 500; // m/s

        // Web fast movers that are threatening us
        var fastThreat = targets
            .Where(t => t.Velocity > HighSpeedThreshold)
            .OrderBy(t => CalculateEwarPriority(t, "", false, false))
            .FirstOrDefault();

        if (fastThreat != null)
            return fastThreat;

        // Otherwise web our primary target
        return targets.FirstOrDefault(t => t.IsPrimaryTarget);
    }

    /// <summary>
    /// Selects best target for energy neutralizer
    /// </summary>
    /// <remarks>
    /// Neut ships that are using active EWAR against us
    /// Or neut ships with active tank
    /// </remarks>
    private EwarTarget? SelectNeutTarget(List<EwarTarget> targets)
    {
        // Neut ships that are scramming us (break their scram)
        var scrambler = targets.FirstOrDefault(t => t.IsScrambler && t.IsNeutingUs);
        if (scrambler != null)
            return scrambler;

        // Neut ships that are neuting us (neut war)
        var neuter = targets.FirstOrDefault(t => t.IsNeut && t.IsNeutingUs);
        if (neuter != null)
            return neuter;

        // Neut active tankers
        var activeTanker = targets.FirstOrDefault(t => t.HasActiveTank);
        if (activeTanker != null)
            return activeTanker;

        return null;
    }

    /// <summary>
    /// Selects best target for warp scrambler
    /// </summary>
    /// <remarks>
    /// Scram ships trying to warp out
    /// Scram fast ships to turn off their MWD
    /// </remarks>
    private EwarTarget? SelectScramTarget(List<EwarTarget> targets)
    {
        const double FastSpeedThreshold = 1000; // Likely MWD active

        // Scram ships trying to escape
        var escaping = targets.FirstOrDefault(t => t.IsAligning || t.Velocity > FastSpeedThreshold);
        if (escaping != null)
            return escaping;

        // Scram primary target to prevent escape
        return targets.FirstOrDefault(t => t.IsPrimaryTarget);
    }

    /// <summary>
    /// Selects best target for sensor dampener
    /// </summary>
    /// <remarks>
    /// Damp long-range ships to reduce their effectiveness
    /// Damp logistics to reduce their lock range
    /// </remarks>
    private EwarTarget? SelectDampTarget(List<EwarTarget> targets)
    {
        // Damp logistics ships to reduce their support range
        var logi = targets.FirstOrDefault(t => t.IsLogistics);
        if (logi != null)
            return logi;

        // Damp long-range DPS ships
        return targets
            .Where(t => t.Distance > 20000)
            .OrderByDescending(t => t.EstimatedDps)
            .FirstOrDefault();
    }

    /// <summary>
    /// Identifies if target is receiving remote repairs (logistics support)
    /// </summary>
    /// <param name="target">Target to check</param>
    /// <param name="recentDamageDealt">Damage dealt to target recently</param>
    /// <param name="targetShieldChange">Change in target's shield HP</param>
    /// <returns>True if target appears to be receiving remote repairs</returns>
    /// <remarks>
    /// If target's shield is increasing despite taking damage, logistics is active
    /// </remarks>
    public bool IsReceivingRemoteRepairs(
        EwarTarget target,
        double recentDamageDealt,
        double targetShieldChange)
    {
        // If we dealt damage but their shield increased, they're being repped
        if (recentDamageDealt > 0 && targetShieldChange > 0)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if we should break target locks to prevent enemy EWAR
    /// </summary>
    /// <param name="beingJammed">Whether we're being jammed</param>
    /// <param name="beingDampened">Whether we're being dampened</param>
    /// <param name="lockedTargetsCount">Number of currently locked targets</param>
    /// <returns>True if we should unlock non-essential targets</returns>
    /// <remarks>
    /// Under EWAR pressure, reduce locked targets to essentials only
    /// </remarks>
    public bool ShouldReduceLockedTargets(
        bool beingJammed,
        bool beingDampened,
        int lockedTargetsCount)
    {
        // If jammed, we'll lose locks anyway
        if (beingJammed)
            return true;

        // If dampened and have many locks, reduce to prevent lock time issues
        if (beingDampened && lockedTargetsCount > 3)
            return true;

        return false;
    }

    /// <summary>
    /// Calculates effective warp disruption points on us
    /// </summary>
    /// <param name="scramsOnUs">Number of warp scramblers on us (2 points each)</param>
    /// <param name="pointsOnUs">Number of warp disruptors on us (1 point each)</param>
    /// <param name="ourWarpStrength">Our warp core strength (negative value)</param>
    /// <returns>Net warp disruption points. Positive = cannot warp</returns>
    /// <remarks>
    /// Scrams = 2 points, Disruptors = 1 point, Warp Stabs = -1 point each
    /// If total > 0, you cannot warp
    /// </remarks>
    public int CalculateWarpDisruptionPoints(
        int scramsOnUs,
        int pointsOnUs,
        int ourWarpStrength = 0)
    {
        var totalPoints = (scramsOnUs * 2) + pointsOnUs + ourWarpStrength;
        return totalPoints;
    }

    /// <summary>
    /// Prioritizes breaking logistics chains
    /// </summary>
    /// <param name="targets">All targets</param>
    /// <returns>Ordered list of logistics targets to kill</returns>
    /// <remarks>
    /// Logistics force multiplier can make fights unwinnable
    /// Must identify and eliminate logistics ships quickly
    /// </remarks>
    public IReadOnlyList<EwarTarget> PrioritizeLogisticsChain(IEnumerable<EwarTarget> targets)
    {
        return targets
            .Where(t => t.IsLogistics)
            .OrderBy(t => t.Distance) // Closer logi first
            .ThenBy(t => t.EstimatedDps) // Weaker logi first
            .ToList();
    }
}

/// <summary>
/// Target with EWAR capabilities and status
/// </summary>
public record EwarTarget(
    long Id,
    string Name,
    string Type,
    int Distance,
    bool IsEnemy,
    double EstimatedDps,
    double Velocity,
    double SignatureRadius,
    bool IsPrimaryTarget = false,
    bool IsJammer = false,
    bool IsNeut = false,
    bool IsWebber = false,
    bool IsScrambler = false,
    bool IsPainter = false,
    bool IsDampener = false,
    bool IsLogistics = false,
    bool HasActiveTank = false,
    bool IsAligning = false,
    bool IsNeutingUs = false
);

/// <summary>
/// Types of EWAR modules
/// </summary>
public enum EwarType
{
    TargetPainter,
    StasisWeb,
    Neut,
    WarpScrambler,
    WarpDisruptor,
    Dampener,
    ECM,
    None
}
