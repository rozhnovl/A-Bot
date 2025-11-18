using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles critical damage situations
/// </summary>
public interface ICriticalDamageHandler
{
    /// <summary>
    /// Handles critical damage situation
    /// </summary>
    /// <param name="status">Current ship status</param>
    /// <returns>Situation response with emergency decisions</returns>
    SituationResponse HandleCriticalDamage(ShipStatus status);
}
