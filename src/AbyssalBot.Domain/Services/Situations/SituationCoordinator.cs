using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Coordinates all situation handlers and determines the highest priority response
/// </summary>
public class SituationCoordinator
{
    private readonly ICriticalDamageHandler _criticalDamageHandler;
    private readonly ILowCapacitorHandler _lowCapacitorHandler;
    private readonly IEwarSituationHandler _ewarHandler;
    private readonly IMultipleHostilesHandler _multipleHostilesHandler;

    public SituationCoordinator(
        ICriticalDamageHandler criticalDamageHandler,
        ILowCapacitorHandler lowCapacitorHandler,
        IEwarSituationHandler ewarHandler,
        IMultipleHostilesHandler multipleHostilesHandler)
    {
        _criticalDamageHandler = criticalDamageHandler;
        _lowCapacitorHandler = lowCapacitorHandler;
        _ewarHandler = ewarHandler;
        _multipleHostilesHandler = multipleHostilesHandler;
    }

    /// <summary>
    /// Evaluates all situations and returns the highest priority response
    /// </summary>
    /// <param name="context">Full combat context</param>
    /// <param name="ewarContext">EWAR context (if applicable)</param>
    /// <param name="orbitBeacon">Orbit beacon for defensive positioning</param>
    /// <returns>Highest priority situation response</returns>
    public SituationResponse HandleSituation(
        CombatContext context,
        EwarContext? ewarContext = null,
        Target? orbitBeacon = null)
    {
        var responses = new List<SituationResponse>();

        // Priority order (highest to lowest):
        // 1. Critical damage (structure < 50%)
        // 2. Capacitor emergency (< 10%)
        // 3. EWAR (jammed/neuted)
        // 4. Multiple hostiles (> 10)
        // 5. Low capacitor (< 30%)
        // 6. Normal combat

        // Check 1: Critical Damage (HIGHEST PRIORITY)
        var shipStatus = new ShipStatus(
            context.Hitpoints,
            context.Fitting,
            context.IncomingDps
        );
        var criticalDamageResponse = _criticalDamageHandler.HandleCriticalDamage(shipStatus);
        if (criticalDamageResponse.Priority >= SituationPriority.CriticalDamage)
        {
            responses.Add(criticalDamageResponse);
        }

        // Check 2: Capacitor situations (Emergency and Low)
        var capResponse = _lowCapacitorHandler.HandleLowCapacitor(context);
        if (capResponse.Priority >= SituationPriority.LowCapacitor)
        {
            responses.Add(capResponse);
        }

        // Check 3: EWAR situations
        if (ewarContext != null && ewarContext.EwarType != EwarType.None)
        {
            var ewarResponse = _ewarHandler.HandleEwar(ewarContext);
            if (ewarResponse.Priority >= SituationPriority.Ewar)
            {
                responses.Add(ewarResponse);
            }
        }

        // Check 4: Multiple hostiles
        var enemies = context.AvailableTargets.Where(t => t.IsEnemy).ToList();
        if (enemies.Count >= 5)
        {
            var hostilesResponse = _multipleHostilesHandler.HandleMultipleHostiles(
                enemies,
                context.IncomingDps,
                orbitBeacon
            );
            if (hostilesResponse.Priority >= SituationPriority.LowCapacitor)
            {
                responses.Add(hostilesResponse);
            }
        }

        // Return highest priority response
        if (responses.Any())
        {
            var highestPriority = responses.OrderByDescending(r => r.Priority).First();
            return highestPriority;
        }

        // No special situations detected
        return SituationResponse.NoAction("Normal combat operations");
    }

    /// <summary>
    /// Gets all active situations (for monitoring/logging)
    /// </summary>
    /// <param name="context">Combat context</param>
    /// <param name="ewarContext">EWAR context</param>
    /// <param name="orbitBeacon">Orbit beacon</param>
    /// <returns>All detected situations</returns>
    public IReadOnlyList<SituationResponse> GetAllSituations(
        CombatContext context,
        EwarContext? ewarContext = null,
        Target? orbitBeacon = null)
    {
        var responses = new List<SituationResponse>();

        // Check all handlers
        var shipStatus = new ShipStatus(context.Hitpoints, context.Fitting, context.IncomingDps);
        var criticalDamageResponse = _criticalDamageHandler.HandleCriticalDamage(shipStatus);
        if (criticalDamageResponse.HasDecisions)
        {
            responses.Add(criticalDamageResponse);
        }

        var capResponse = _lowCapacitorHandler.HandleLowCapacitor(context);
        if (capResponse.HasDecisions)
        {
            responses.Add(capResponse);
        }

        if (ewarContext != null)
        {
            var ewarResponse = _ewarHandler.HandleEwar(ewarContext);
            if (ewarResponse.HasDecisions)
            {
                responses.Add(ewarResponse);
            }
        }

        var enemies = context.AvailableTargets.Where(t => t.IsEnemy).ToList();
        var hostilesResponse = _multipleHostilesHandler.HandleMultipleHostiles(
            enemies,
            context.IncomingDps,
            orbitBeacon
        );
        if (hostilesResponse.HasDecisions)
        {
            responses.Add(hostilesResponse);
        }

        return responses.OrderByDescending(r => r.Priority).ToList();
    }

    /// <summary>
    /// Checks if ship is in an emergency situation
    /// </summary>
    /// <param name="context">Combat context</param>
    /// <returns>True if in emergency</returns>
    public bool IsEmergencySituation(CombatContext context)
    {
        var response = HandleSituation(context);
        return response.IsEmergency;
    }
}
