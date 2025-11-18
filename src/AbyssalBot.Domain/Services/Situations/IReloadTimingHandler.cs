using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles optimal timing for module reloads
/// </summary>
public interface IReloadTimingHandler
{
    /// <summary>
    /// Determines if a module should be reloaded now
    /// </summary>
    /// <param name="context">Current combat context</param>
    /// <param name="moduleType">Type of module to check</param>
    /// <returns>True if module should reload now</returns>
    bool ShouldReloadNow(CombatContext context, ModuleType moduleType);

    /// <summary>
    /// Gets the reasoning for reload decision
    /// </summary>
    /// <param name="context">Current combat context</param>
    /// <param name="moduleType">Type of module</param>
    /// <returns>Reasoning string</returns>
    string GetReloadReasoning(CombatContext context, ModuleType moduleType);
}
