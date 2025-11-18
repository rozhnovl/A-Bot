using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles low capacitor situations
/// </summary>
public interface ILowCapacitorHandler
{
    /// <summary>
    /// Handles low capacitor situation
    /// </summary>
    /// <param name="context">Combat context</param>
    /// <returns>Situation response with decisions</returns>
    SituationResponse HandleLowCapacitor(CombatContext context);
}
