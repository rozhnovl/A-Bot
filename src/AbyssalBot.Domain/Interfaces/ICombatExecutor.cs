using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Interfaces;

/// <summary>
/// Executes combat decisions (commands the ship/modules)
/// </summary>
public interface ICombatExecutor
{
    /// <summary>
    /// Executes a combat decision
    /// </summary>
    /// <param name="decision">The decision to execute</param>
    /// <returns>True if execution was successful</returns>
    Task<bool> ExecuteDecisionAsync(CombatDecision decision);

    /// <summary>
    /// Executes multiple combat decisions in order
    /// </summary>
    Task<IReadOnlyList<bool>> ExecuteDecisionsAsync(IEnumerable<CombatDecision> decisions);

    /// <summary>
    /// Checks if a decision can be executed currently
    /// </summary>
    bool CanExecute(CombatDecision decision);
}
