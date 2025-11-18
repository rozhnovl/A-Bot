using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles Abyssal room strategy determination
/// </summary>
public interface IAbyssRoomStrategyHandler
{
    /// <summary>
    /// Determines the best strategy for the current room
    /// </summary>
    /// <param name="room">Current Abyss room information</param>
    /// <returns>Room strategy with priority targets</returns>
    RoomStrategy DetermineStrategy(AbyssRoom room);
}
