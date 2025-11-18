using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles Electronic Warfare (EWAR) situations
/// </summary>
public interface IEwarSituationHandler
{
    /// <summary>
    /// Handles EWAR situation
    /// </summary>
    /// <param name="context">EWAR context</param>
    /// <returns>Situation response with tactical decisions</returns>
    SituationResponse HandleEwar(EwarContext context);
}
