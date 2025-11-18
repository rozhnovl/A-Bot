using System;
using System.Collections.Generic;

namespace WebUI.Models;

public class BotStateDto
{
    public string CurrentState { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }
    public ShipStatusDto? ShipStatus { get; set; }
    public List<TargetDto> ActiveTargets { get; set; } = new();
    public List<string> RecentActions { get; set; } = new();
    public AbyssProgressDto? AbyssProgress { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    public int StepIndex { get; set; }
}

public class ShipStatusDto
{
    public double ShieldHp { get; set; }
    public double ShieldMax { get; set; }
    public double ShieldPercent => ShieldMax > 0 ? (ShieldHp / ShieldMax) * 100 : 0;

    public double ArmorHp { get; set; }
    public double ArmorMax { get; set; }
    public double ArmorPercent => ArmorMax > 0 ? (ArmorHp / ArmorMax) * 100 : 0;

    public double StructureHp { get; set; }
    public double StructureMax { get; set; }
    public double StructurePercent => StructureMax > 0 ? (StructureHp / StructureMax) * 100 : 0;

    public double Capacitor { get; set; }
    public double CapacitorMax { get; set; }
    public double CapacitorPercent => CapacitorMax > 0 ? (Capacitor / CapacitorMax) * 100 : 0;

    public string CurrentManeuver { get; set; } = "None";
    public string Location { get; set; } = "Unknown";
    public bool IsInAbyss { get; set; }
    public int ActiveModules { get; set; }
    public int DronesInSpace { get; set; }
}

public class TargetDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Distance { get; set; }
    public double ShieldPercent { get; set; }
    public double ArmorPercent { get; set; }
    public double StructurePercent { get; set; }
    public bool IsActiveTarget { get; set; }
    public int Priority { get; set; }
}

public class AbyssProgressDto
{
    public int CurrentRoom { get; set; }
    public int TotalRooms { get; set; }
    public string Weather { get; set; } = string.Empty;
    public TimeSpan TimeElapsed { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public int LootCollected { get; set; }
}

public class BotCommandDto
{
    public BotCommandType CommandType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum BotCommandType
{
    Start,
    Stop,
    Pause,
    Resume,
    EmergencyRetreat,
    ChangeState,
    ActivateModule,
    DeactivateModule,
    SetTargetPriority,
    EnableBehavior,
    DisableBehavior
}
