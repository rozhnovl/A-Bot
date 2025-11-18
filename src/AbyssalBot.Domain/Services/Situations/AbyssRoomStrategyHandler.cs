using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Determines optimal strategies for different Abyssal room types
/// </summary>
public class AbyssRoomStrategyHandler : IAbyssRoomStrategyHandler
{
    public RoomStrategy DetermineStrategy(AbyssRoom room)
    {
        // Cache room strategy
        if (room.IsCacheRoom)
        {
            return HandleCacheRoom(room);
        }

        // Determine room type by enemy composition
        var roomType = DetermineRoomType(room.Enemies);

        return roomType switch
        {
            "Triglavian" => HandleTriglavianRoom(room),
            "Drone" => HandleDroneRoom(room),
            "Sleeper" => HandleSleeperRoom(room),
            "EWAR" => HandleEwarRoom(room),
            _ => HandleGenericRoom(room)
        };
    }

    private string DetermineRoomType(IReadOnlyList<Target> enemies)
    {
        if (!enemies.Any())
        {
            return "Empty";
        }

        var enemyNames = enemies.Select(e => e.Name.ToLower()).ToList();

        // Triglavian rooms
        if (enemyNames.Any(n => n.Contains("damavik") || n.Contains("vedmak") || n.Contains("leshak")))
        {
            return "Triglavian";
        }

        // Drone rooms
        if (enemyNames.Any(n => n.Contains("drone") || n.Contains("swarm")))
        {
            return "Drone";
        }

        // Sleeper rooms
        if (enemyNames.Any(n => n.Contains("sleeper")))
        {
            return "Sleeper";
        }

        // EWAR rooms (neutralizers, jammers, etc.)
        if (enemyNames.Any(n => n.Contains("nullifier") || n.Contains("dissipator") || n.Contains("renewing")))
        {
            return "EWAR";
        }

        return "Generic";
    }

    private RoomStrategy HandleTriglavianRoom(AbyssRoom room)
    {
        // Triglavian strategy: Kill logistics ships first (Renewing Logi)
        var priorityTargets = new List<Target>();

        // Priority 1: Logi ships
        var logiShips = room.Enemies
            .Where(e => e.Name.Contains("Renewing", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(logiShips);

        // Priority 2: High DPS frigates (Damavik)
        var frigates = room.Enemies
            .Where(e => e.Name.Contains("Damavik", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(frigates);

        // Priority 3: Cruisers (Vedmak)
        var cruisers = room.Enemies
            .Where(e => e.Name.Contains("Vedmak", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(cruisers);

        // Priority 4: Everything else
        var remaining = room.Enemies
            .Where(e => !priorityTargets.Contains(e))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(remaining);

        return new RoomStrategy(
            "Triglavian Room",
            priorityTargets,
            logiShips.Any()
                ? "Kill Renewing Logi first to prevent enemy reps, then frigates for DPS reduction"
                : "Kill frigates first for DPS reduction, then larger ships"
        );
    }

    private RoomStrategy HandleDroneRoom(AbyssRoom room)
    {
        // Drone strategy: Focus on AOE if available, prioritize closest
        var priorityTargets = room.Enemies
            .OrderBy(e => e.Distance) // Closest first for AOE effectiveness
            .ToList();

        return new RoomStrategy(
            "Drone Room",
            priorityTargets,
            "Focus fire on closest drones - AOE damage optimal for swarms"
        );
    }

    private RoomStrategy HandleSleeperRoom(AbyssRoom room)
    {
        // Sleeper strategy: Kill frigates first (high DPS)
        var priorityTargets = new List<Target>();

        // Priority 1: Frigates (highest DPS for their size)
        var frigates = room.Enemies
            .Where(e => e.Name.Contains("Sleeper", StringComparison.OrdinalIgnoreCase)
                        && e.Distance < 10000)
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(frigates);

        // Priority 2: Everything else by distance
        var remaining = room.Enemies
            .Where(e => !priorityTargets.Contains(e))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(remaining);

        return new RoomStrategy(
            "Sleeper Room",
            priorityTargets,
            "Kill close frigates first for DPS reduction"
        );
    }

    private RoomStrategy HandleEwarRoom(AbyssRoom room)
    {
        // EWAR strategy: Kill neutralizers and jammers first
        var priorityTargets = new List<Target>();

        // Priority 1: Energy neutralizers (Nullifier, Dissipator)
        var neuts = room.Enemies
            .Where(e => e.Name.Contains("Nullifier", StringComparison.OrdinalIgnoreCase)
                        || e.Name.Contains("Dissipator", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(neuts);

        // Priority 2: ECM/Jammers
        var jammers = room.Enemies
            .Where(e => e.Name.Contains("Jammer", StringComparison.OrdinalIgnoreCase)
                        || e.Name.Contains("Damping", StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(jammers);

        // Priority 3: Everything else
        var remaining = room.Enemies
            .Where(e => !priorityTargets.Contains(e))
            .OrderBy(e => e.Distance)
            .ToList();
        priorityTargets.AddRange(remaining);

        return new RoomStrategy(
            "EWAR Room",
            priorityTargets,
            neuts.Any()
                ? "Kill energy neutralizers first to protect capacitor, then jammers"
                : "Kill EWAR ships first to restore combat effectiveness"
        );
    }

    private RoomStrategy HandleCacheRoom(AbyssRoom room)
    {
        // Cache room: Kill defenders, loot cache
        var priorityTargets = room.Enemies
            .OrderBy(e => e.Distance)
            .ToList();

        return new RoomStrategy(
            "Cache Room",
            priorityTargets,
            "Clear defenders quickly, then loot cache before taking gate",
            ShouldLootCache: true
        );
    }

    private RoomStrategy HandleGenericRoom(AbyssRoom room)
    {
        // Generic strategy: Kill closest first
        var priorityTargets = room.Enemies
            .OrderBy(e => e.Distance)
            .ToList();

        return new RoomStrategy(
            "Generic Room",
            priorityTargets,
            "Kill closest enemies first to reduce immediate threat"
        );
    }
}
