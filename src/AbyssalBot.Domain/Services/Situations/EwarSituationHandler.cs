using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles EWAR situations with appropriate tactical responses
/// </summary>
public class EwarSituationHandler : IEwarSituationHandler
{
    public SituationResponse HandleEwar(EwarContext context)
    {
        if (context.EwarType == EwarType.None)
        {
            return SituationResponse.NoAction("No EWAR detected");
        }

        var decisions = new List<CombatDecision>();
        var reasoning = new List<string>();

        switch (context.EwarType)
        {
            case EwarType.Jammed:
                HandleJammed(context, decisions, reasoning);
                break;

            case EwarType.Dampened:
                HandleDampened(context, decisions, reasoning);
                break;

            case EwarType.TrackingDisrupted:
                HandleTrackingDisrupted(context, decisions, reasoning);
                break;

            case EwarType.Webbed:
            case EwarType.Scrambled:
                HandleTackled(context, decisions, reasoning);
                break;

            case EwarType.Neutralized:
                HandleNeutralized(context, decisions, reasoning);
                break;
        }

        return new SituationResponse(
            SituationPriority.Ewar,
            $"EWAR: {context.EwarType}",
            string.Join(". ", reasoning),
            decisions
        );
    }

    private void HandleJammed(EwarContext context, List<CombatDecision> decisions, List<string> reasoning)
    {
        reasoning.Add("ECM Jammed - cannot lock targets");

        // When jammed, we can't lock anything, so we should either:
        // 1. Retreat if in danger
        // 2. Approach jam source to break lock (if safe)

        if (context.Hitpoints.ShieldPercentage < 50)
        {
            // Retreat if taking damage
            var farthestEnemy = context.AvailableTargets
                .OrderByDescending(t => t.Distance)
                .FirstOrDefault();

            if (farthestEnemy != null)
            {
                decisions.Add(new ManeuverDecision(
                    ShipManeuverType.KeepAtRange,
                    farthestEnemy,
                    30000,
                    "Jammed and taking damage - retreating"
                ));
                reasoning.Add("Retreating to safety while jammed");
            }
        }
        else if (context.EwarSource != null)
        {
            // Approach jam source if we're healthy
            decisions.Add(new ManeuverDecision(
                ShipManeuverType.Approach,
                context.EwarSource,
                5000,
                "Approaching jam source to break lock"
            ));
            reasoning.Add("Approaching jammer to break their lock");
        }
    }

    private void HandleDampened(EwarContext context, List<CombatDecision> decisions, List<string> reasoning)
    {
        reasoning.Add("Sensor dampened - lock range reduced");

        // With dampening, we need to get closer to targets
        var closestEnemy = context.AvailableTargets
            .Where(t => t.IsEnemy)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        if (closestEnemy != null)
        {
            decisions.Add(new ManeuverDecision(
                ShipManeuverType.Approach,
                closestEnemy,
                5000,
                "Dampened - closing range for locks"
            ));
            reasoning.Add("Moving to close range to overcome dampening");
        }
    }

    private void HandleTrackingDisrupted(EwarContext context, List<CombatDecision> decisions, List<string> reasoning)
    {
        reasoning.Add("Tracking disrupted - weapon accuracy reduced");

        // Increase range to improve tracking (angular velocity)
        var currentTarget = context.AvailableTargets
            .Where(t => t.IsTargeted)
            .FirstOrDefault();

        if (currentTarget != null && currentTarget.Distance < 10000)
        {
            decisions.Add(new ManeuverDecision(
                ShipManeuverType.KeepAtRange,
                currentTarget,
                15000,
                "Tracking disrupted - increasing range"
            ));
            reasoning.Add("Increasing range to reduce angular velocity");
        }
    }

    private void HandleTackled(EwarContext context, List<CombatDecision> decisions, List<string> reasoning)
    {
        var ewarType = context.EwarType == EwarType.Webbed ? "Webbed" : "Scrambled";
        reasoning.Add($"{ewarType} - mobility compromised");

        // Priority: Kill the tackle
        if (context.EwarSource != null)
        {
            // Lock the tackle if not already locked
            if (!context.EwarSource.IsTargeted)
            {
                decisions.Add(new LockTargetDecision(
                    context.EwarSource,
                    $"{ewarType} - locking tackle source as priority"
                ));
            }

            reasoning.Add("Prioritizing tackle source for elimination");
        }

        // Overheat hardeners for survival
        decisions.Add(new ModuleDecision(
            ModuleType.Hardener,
            true,
            true,
            $"{ewarType} - overheating hardeners for survival"
        ));
        reasoning.Add("Overheating hardeners to survive while tackled");
    }

    private void HandleNeutralized(EwarContext context, List<CombatDecision> decisions, List<string> reasoning)
    {
        reasoning.Add("Energy neutralized - capacitor being drained");

        // This is similar to low capacitor but caused by enemy action
        // Priority: Kill the neut source
        if (context.EwarSource != null && !context.EwarSource.IsTargeted)
        {
            decisions.Add(new LockTargetDecision(
                context.EwarSource,
                "Neutralized - locking neut source as priority"
            ));
            reasoning.Add("Targeting neutralizer for elimination");
        }

        // Manage capacitor like low cap situation
        if (context.Hitpoints.CapacitorPercentage < 30)
        {
            decisions.Add(new ModuleDecision(
                ModuleType.MWD,
                false,
                false,
                "Being neuted - conserving capacitor"
            ));
            reasoning.Add("Disabled MWD to conserve capacitor under neut pressure");
        }
    }
}
