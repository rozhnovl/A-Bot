using Microsoft.AspNetCore.SignalR;
using WebUI.Hubs;
using WebUI.Models;
using System.Collections.Concurrent;

namespace WebUI.Services;

public class BotStateService : IBotStateService
{
    private readonly IHubContext<BotHub> _hubContext;
    private readonly ILogger<BotStateService> _logger;
    private BotStateDto _currentState = new();
    private readonly ConcurrentQueue<string> _actionLog = new();
    private readonly int _maxLogEntries = 500;

    public event EventHandler<BotStateDto>? StateChanged;

    public BotStateService(IHubContext<BotHub> hubContext, ILogger<BotStateService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        InitializeMockState();
    }

    private void InitializeMockState()
    {
        // Initialize with mock data for demonstration
        _currentState = new BotStateDto
        {
            CurrentState = "AbyssalFightState",
            IsRunning = false,
            IsPaused = false,
            ShipStatus = new ShipStatusDto
            {
                ShieldHp = 850,
                ShieldMax = 1000,
                ArmorHp = 500,
                ArmorMax = 500,
                StructureHp = 300,
                StructureMax = 300,
                Capacitor = 450,
                CapacitorMax = 500,
                CurrentManeuver = "Orbit",
                Location = "Abyss - Room 2",
                IsInAbyss = true,
                ActiveModules = 5,
                DronesInSpace = 3
            },
            ActiveTargets = new List<TargetDto>(),
            RecentActions = new List<string> { "Bot initialized - waiting for commands" },
            StepIndex = 0
        };

        LogAction("Bot state service initialized");
    }

    public Task<BotStateDto> GetCurrentStateAsync()
    {
        return Task.FromResult(_currentState);
    }

    public async Task<bool> SendCommandAsync(BotCommandDto command)
    {
        try
        {
            _logger.LogInformation("Received command: {CommandType}", command.CommandType);

            // Process command
            switch (command.CommandType)
            {
                case BotCommandType.Start:
                    _currentState.IsRunning = true;
                    _currentState.IsPaused = false;
                    LogAction("Bot started");
                    break;

                case BotCommandType.Stop:
                    _currentState.IsRunning = false;
                    LogAction("Bot stopped");
                    break;

                case BotCommandType.Pause:
                    _currentState.IsPaused = true;
                    LogAction("Bot paused");
                    break;

                case BotCommandType.Resume:
                    _currentState.IsPaused = false;
                    LogAction("Bot resumed");
                    break;

                case BotCommandType.EmergencyRetreat:
                    LogAction("EMERGENCY RETREAT INITIATED");
                    _currentState.CurrentState = "RetreatState";
                    break;

                case BotCommandType.ChangeState:
                    if (command.Parameters.TryGetValue("state", out var state))
                    {
                        _currentState.CurrentState = state.ToString() ?? "Unknown";
                        LogAction($"State changed to: {_currentState.CurrentState}");
                    }
                    break;
            }

            // Broadcast state change
            await BroadcastStateAsync();
            StateChanged?.Invoke(this, _currentState);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command: {CommandType}", command.CommandType);
            return false;
        }
    }

    public Task<List<string>> GetActionLogAsync(int count = 50)
    {
        var log = _actionLog.Reverse().Take(count).Reverse().ToList();
        return Task.FromResult(log);
    }

    private void LogAction(string action)
    {
        var timestamp = DateTime.UtcNow.ToString("HH:mm:ss");
        var logEntry = $"[{timestamp}] {action}";

        _actionLog.Enqueue(logEntry);
        _currentState.RecentActions.Insert(0, logEntry);

        // Keep only recent entries
        if (_currentState.RecentActions.Count > 20)
        {
            _currentState.RecentActions = _currentState.RecentActions.Take(20).ToList();
        }

        while (_actionLog.Count > _maxLogEntries)
        {
            _actionLog.TryDequeue(out _);
        }
    }

    private async Task BroadcastStateAsync()
    {
        _currentState.LastUpdate = DateTime.UtcNow;
        await _hubContext.Clients.All.SendAsync("ReceiveBotState", _currentState);
    }

    /// <summary>
    /// Update bot state from external source (e.g., actual bot)
    /// </summary>
    public async Task UpdateStateAsync(BotStateDto newState)
    {
        _currentState = newState;
        await BroadcastStateAsync();
        StateChanged?.Invoke(this, _currentState);
    }
}
