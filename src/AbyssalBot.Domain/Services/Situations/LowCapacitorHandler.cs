using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles low capacitor situations by managing module activation
/// and capacitor restoration
/// </summary>
public class LowCapacitorHandler : ILowCapacitorHandler
{
    private const double EmergencyCapThreshold = 10.0;
    private const double LowCapThreshold = 30.0;
    private const double SafeCapThreshold = 50.0;

    public SituationResponse HandleLowCapacitor(CombatContext context)
    {
        var capPercentage = context.Hitpoints.CapacitorPercentage;

        // No issue if cap is above 30%
        if (capPercentage > LowCapThreshold)
        {
            return SituationResponse.NoAction("Capacitor levels normal");
        }

        var decisions = new List<CombatDecision>();
        var priority = capPercentage < EmergencyCapThreshold
            ? SituationPriority.CapacitorEmergency
            : SituationPriority.LowCapacitor;

        var reasoning = new List<string>();

        // Emergency: Turn off everything except hardeners
        if (capPercentage < EmergencyCapThreshold)
        {
            reasoning.Add($"EMERGENCY: Capacitor at {capPercentage:F1}%");

            // Turn off MWD
            var mwd = context.Fitting.GetMWD();
            if (mwd?.IsActive == true)
            {
                decisions.Add(new ModuleDecision(
                    ModuleType.MWD,
                    false,
                    false,
                    "Emergency cap - disabling MWD"
                ));
                reasoning.Add("Disabled MWD to conserve capacitor");
            }

            // Turn off weapons if cap is critical
            var weapon = context.Fitting.GetWeapon();
            if (weapon?.IsActive == true && capPercentage < 5.0)
            {
                decisions.Add(new ModuleDecision(
                    ModuleType.Weapon,
                    false,
                    false,
                    "Critical cap - disabling weapons temporarily"
                ));
                reasoning.Add("Disabled weapons temporarily");
            }

            // Kite away to reduce incoming damage
            if (context.ActiveCombatTarget != null && context.ActiveCombatTarget.Distance < 15000)
            {
                decisions.Add(new ManeuverDecision(
                    ShipManeuverType.KeepAtRange,
                    context.ActiveCombatTarget,
                    20000,
                    "Emergency cap - kiting to reduce damage"
                ));
                reasoning.Add("Kiting away to reduce incoming damage");
            }
        }
        // Low cap: Reduce non-essential module usage
        else if (capPercentage < LowCapThreshold)
        {
            reasoning.Add($"Low capacitor at {capPercentage:F1}%");

            // Turn off MWD if we're in orbit range
            var mwd = context.Fitting.GetMWD();
            if (mwd?.IsActive == true && context.ActiveCombatTarget != null)
            {
                var distanceToTarget = context.ActiveCombatTarget.Distance;
                if (distanceToTarget < 8000)
                {
                    decisions.Add(new ModuleDecision(
                        ModuleType.MWD,
                        false,
                        false,
                        "Low cap - disabling MWD at close range"
                    ));
                    reasoning.Add("Disabled MWD to conserve capacitor");
                }
            }

            // Only run one shield booster if multiple are active
            var activeBoosters = context.Fitting.GetShieldBoosters()
                .Where(b => b.IsActive)
                .ToList();

            if (activeBoosters.Count > 1 && context.Hitpoints.ShieldPercentage > 30)
            {
                // Turn off all but one booster
                foreach (var booster in activeBoosters.Skip(1))
                {
                    decisions.Add(new ModuleDecision(
                        ModuleType.ShieldBooster,
                        false,
                        false,
                        "Low cap - running single booster only"
                    ));
                }
                reasoning.Add("Reduced shield boosters to conserve capacitor");
            }
        }

        var finalReasoning = string.Join(". ", reasoning);

        return new SituationResponse(
            priority,
            "Low Capacitor",
            finalReasoning,
            decisions
        );
    }
}
