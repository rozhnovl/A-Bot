using Microsoft.Extensions.Logging;
using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Strategies;
using WebUI.Models;
using WebUI.Services;

namespace WebUI.BotIntegration;

/// <summary>
/// Adapter that connects the actual AbyssalBot to the WebUI control system
/// This class should be instantiated in your bot application and will handle
/// bidirectional communication between the bot and the web interface
/// </summary>
public class BotControlAdapter
{
    private readonly Bot _bot;
    private readonly IBotStateService _botStateService;
    private readonly ILogger<BotControlAdapter> _logger;

    private bool _isRunning;
    private bool _isPaused;
    private IStragegyState? _currentState;
    private readonly Queue<BotCommandDto> _commandQueue = new();
    private readonly object _commandLock = new();

    public BotControlAdapter(Bot bot, IBotStateService botStateService, ILogger<BotControlAdapter> logger)
    {
        _bot = bot;
        _botStateService = botStateService;
        _logger = logger;

        // Subscribe to command events from the UI
        _botStateService.StateChanged += OnUiCommandReceived;
    }

    /// <summary>
    /// Call this method after each bot step to update the UI
    /// </summary>
    public async Task UpdateUiAfterStepAsync(string currentStateName)
    {
        try
        {
            // Map current bot state to DTO
            var stateDto = BotStateMapper.MapBotState(_bot, currentStateName, _isRunning, _isPaused);

            // Send update to UI
            await _botStateService.UpdateStateAsync(stateDto);

            // Process any queued commands from the UI
            ProcessQueuedCommands();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating UI after bot step");
        }
    }

    /// <summary>
    /// Start the bot (called from UI)
    /// </summary>
    public void Start()
    {
        _isRunning = true;
        _isPaused = false;
        _logger.LogInformation("Bot started via UI command");
    }

    /// <summary>
    /// Stop the bot (called from UI)
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _isPaused = false;
        _logger.LogInformation("Bot stopped via UI command");
    }

    /// <summary>
    /// Pause the bot (called from UI)
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        _logger.LogInformation("Bot paused via UI command");
    }

    /// <summary>
    /// Resume the bot (called from UI)
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
        _logger.LogInformation("Bot resumed via UI command");
    }

    /// <summary>
    /// Check if bot should execute next step
    /// </summary>
    public bool ShouldExecuteStep()
    {
        return _isRunning && !_isPaused;
    }

    /// <summary>
    /// Handle commands received from the UI
    /// </summary>
    private void OnUiCommandReceived(object? sender, BotStateDto state)
    {
        // This is a simplified handler - in practice, you'd have a more sophisticated
        // command pattern where the UI sends explicit command objects
    }

    /// <summary>
    /// Queue a command from the UI for processing
    /// </summary>
    public void QueueCommand(BotCommandDto command)
    {
        lock (_commandLock)
        {
            _commandQueue.Enqueue(command);
            _logger.LogInformation("Command queued: {CommandType}", command.CommandType);
        }
    }

    /// <summary>
    /// Process queued commands (call this in your bot loop)
    /// </summary>
    private void ProcessQueuedCommands()
    {
        lock (_commandLock)
        {
            while (_commandQueue.Count > 0)
            {
                var command = _commandQueue.Dequeue();
                ProcessCommand(command);
            }
        }
    }

    /// <summary>
    /// Process a single command from the UI
    /// </summary>
    private void ProcessCommand(BotCommandDto command)
    {
        try
        {
            switch (command.CommandType)
            {
                case BotCommandType.Start:
                    Start();
                    break;

                case BotCommandType.Stop:
                    Stop();
                    break;

                case BotCommandType.Pause:
                    Pause();
                    break;

                case BotCommandType.Resume:
                    Resume();
                    break;

                case BotCommandType.EmergencyRetreat:
                    HandleEmergencyRetreat();
                    break;

                case BotCommandType.ChangeState:
                    if (command.Parameters.TryGetValue("state", out var state))
                    {
                        HandleStateChange(state.ToString()!);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown command type: {CommandType}", command.CommandType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command: {CommandType}", command.CommandType);
        }
    }

    /// <summary>
    /// Handle emergency retreat command
    /// </summary>
    private void HandleEmergencyRetreat()
    {
        _logger.LogWarning("EMERGENCY RETREAT INITIATED FROM UI");

        // TODO: Implement emergency retreat logic
        // This might involve:
        // - Stopping all offensive actions
        // - Activating defensive modules
        // - Warping out to safety
        // - For now, we just pause the bot

        Pause();
    }

    /// <summary>
    /// Handle manual state change command
    /// </summary>
    private void HandleStateChange(string newStateName)
    {
        _logger.LogInformation("Manual state change requested: {NewState}", newStateName);

        // TODO: Implement state forcing logic
        // This would require access to the strategy instance
        // and the ability to force a state transition

        // For now, just log the request
    }
}

/// <summary>
/// Extension methods for integrating the adapter into your bot
/// </summary>
public static class BotIntegrationExtensions
{
    /// <summary>
    /// Add bot control services to the DI container
    /// Example usage in your bot's startup:
    ///
    /// services.AddBotControlIntegration();
    /// </summary>
    public static IServiceCollection AddBotControlIntegration(this IServiceCollection services)
    {
        services.AddSingleton<BotControlAdapter>();
        return services;
    }
}
