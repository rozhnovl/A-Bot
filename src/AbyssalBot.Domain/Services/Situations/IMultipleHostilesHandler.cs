using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles situations with multiple hostile targets
/// </summary>
public interface IMultipleHostilesHandler
{
    /// <summary>
    /// Handles multiple hostiles situation
    /// </summary>
    /// <param name="hostiles">List of hostile targets</param>
    /// <param name="currentIncomingDps">Current incoming DPS</param>
    /// <param name="orbitBeacon">Target to orbit for defensive positioning</param>
    /// <returns>Situation response with tactical decisions</returns>
    SituationResponse HandleMultipleHostiles(
        IReadOnlyList<Target> hostiles,
        double currentIncomingDps,
        Target? orbitBeacon);
}
