using Sanderling.ABot.Bot;
using Sanderling.Parse;
using WebUI.Models;

namespace WebUI.BotIntegration;

/// <summary>
/// Maps the actual bot state to DTOs for the WebUI
/// This class serves as an adapter between the bot engine and the web interface
/// </summary>
public class BotStateMapper
{
    /// <summary>
    /// Map the bot's internal state to a DTO suitable for the WebUI
    /// </summary>
    public static BotStateDto MapBotState(Bot bot, string currentStateName, bool isRunning, bool isPaused)
    {
        var memory = bot.MemoryMeasurementAtTime?.Value;

        return new BotStateDto
        {
            CurrentState = currentStateName,
            IsRunning = isRunning,
            IsPaused = isPaused,
            ShipStatus = MapShipStatus(memory, bot),
            ActiveTargets = MapTargets(memory),
            RecentActions = new List<string>(), // This would be populated from a logging system
            AbyssProgress = MapAbyssProgress(memory),
            LastUpdate = DateTime.UtcNow,
            StepIndex = bot.stepIndex
        };
    }

    /// <summary>
    /// Map ship status from memory measurement
    /// </summary>
    private static ShipStatusDto? MapShipStatus(IMemoryMeasurement? memory, Bot bot)
    {
        if (memory?.ShipUi?.HitpointsAndEnergy == null)
            return null;

        var hp = memory.ShipUi.HitpointsAndEnergy;

        return new ShipStatusDto
        {
            ShieldHp = hp.Shield ?? 0,
            ShieldMax = 1000, // TODO: Get actual max values from ship fit
            ArmorHp = hp.Armor ?? 0,
            ArmorMax = 500,
            StructureHp = hp.Struct ?? 0,
            StructureMax = 300,
            Capacitor = hp.Capacitor ?? 0,
            CapacitorMax = 500,
            CurrentManeuver = memory.ShipUi?.Indication?.ManeuverType.ToString() ?? "None",
            Location = memory.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName ?? "Unknown",
            IsInAbyss = !memory.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName?.Contains("Maurasi") ?? true,
            ActiveModules = memory.ShipUi?.Module?.Count(m => m.RampActive == true) ?? 0,
            DronesInSpace = memory.ShipUi?.Indication?.LabelText?
                .FirstOrDefault(l => l.Text?.Contains("Drones") == true)?.Text?
                .Split(' ').FirstOrDefault()?.Let(int.TryParse, out var count) == true ? count : 0
        };
    }

    /// <summary>
    /// Map overview entries to target DTOs
    /// </summary>
    private static List<TargetDto> MapTargets(IMemoryMeasurement? memory)
    {
        if (memory?.Target == null)
            return new List<TargetDto>();

        var targets = new List<TargetDto>();

        foreach (var target in memory.Target)
        {
            if (target == null) continue;

            targets.Add(new TargetDto
            {
                Name = target.Name ?? "Unknown",
                Type = DetermineTargetType(target),
                Distance = target.Distance ?? 0,
                ShieldPercent = CalculateHpPercent(target.ShieldPercent),
                ArmorPercent = CalculateHpPercent(target.ArmorPercent),
                StructurePercent = CalculateHpPercent(target.StructPercent),
                IsActiveTarget = target.IsActiveTarget == true,
                Priority = target.Priority ?? 0
            });
        }

        return targets.OrderByDescending(t => t.Priority).ToList();
    }

    /// <summary>
    /// Determine target type from overview entry
    /// </summary>
    private static string DetermineTargetType(Sanderling.Interface.MemoryStruct.ITarget target)
    {
        // This would need to be enhanced based on actual game data
        // Could check icon, name patterns, etc.
        return "NPC"; // Simplified for now
    }

    /// <summary>
    /// Calculate HP percentage from game data
    /// </summary>
    private static double CalculateHpPercent(double? gamePercent)
    {
        if (gamePercent == null) return 0;

        // Game provides percentages in different formats, normalize to 0-100
        if (gamePercent > 1)
            return gamePercent.Value;

        return gamePercent.Value * 100;
    }

    /// <summary>
    /// Map abyss progress information
    /// </summary>
    private static AbyssProgressDto? MapAbyssProgress(IMemoryMeasurement? memory)
    {
        if (memory?.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName?.Contains("Maurasi") == true)
            return null; // Not in abyss

        // TODO: Parse abyss room number from system name or other indicators
        // For now, return basic structure
        return new AbyssProgressDto
        {
            CurrentRoom = 1,
            TotalRooms = 3,
            Weather = "Unknown",
            TimeElapsed = TimeSpan.Zero,
            TimeRemaining = TimeSpan.FromMinutes(20),
            LootCollected = 0
        };
    }
}

/// <summary>
/// Extension methods for cleaner mapping code
/// </summary>
public static class MappingExtensions
{
    public static TResult? Let<T, TResult>(this T obj, Func<T, TResult, bool> func, out TResult result)
    {
        result = default!;
        return func(obj, result) ? result : default;
    }
}
