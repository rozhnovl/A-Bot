using WebUI.Models;

namespace WebUI.Services;

public interface IBotStateService
{
    /// <summary>
    /// Get the current state of the bot
    /// </summary>
    Task<BotStateDto> GetCurrentStateAsync();

    /// <summary>
    /// Send a command to the bot
    /// </summary>
    Task<bool> SendCommandAsync(BotCommandDto command);

    /// <summary>
    /// Get recent action log
    /// </summary>
    Task<List<string>> GetActionLogAsync(int count = 50);

    /// <summary>
    /// Subscribe to bot state changes
    /// </summary>
    event EventHandler<BotStateDto>? StateChanged;
}
