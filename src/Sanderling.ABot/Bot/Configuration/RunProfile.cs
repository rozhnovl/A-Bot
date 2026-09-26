using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Configuration
{
    /// <summary>
    /// An item + quantity the ship should carry to run a profile. <paramref name="InDroneBay"/>
    /// items (drones) live in the drone bay and are verified via the drone window, not the cargo hold.
    /// </summary>
    public sealed record CargoRequirement(string Item, int Quantity, bool InDroneBay = false);

    /// <summary>
    /// A named, self-contained description of "which ship runs which abyss": the fit to
    /// build, the filament to activate to enter, the cargo it should carry, and the slot
    /// layout used to sanity-check that the detected ship matches. Switching profiles is
    /// how you retask the bot between ships and abyss tiers without touching strategy code.
    /// </summary>
    public sealed record RunProfile
    {
        public required string Name { get; init; }

        /// <summary>Exact overview/inventory name of the filament that opens this abyss (e.g. "Calm Exotic Filament" = T1).</summary>
        public required string FilamentName { get; init; }

        /// <summary>Builds the fit against the live ship UI.</summary>
        public required System.Func<Bot, ShipFit> BuildFit { get; init; }

        public required IReadOnlyList<CargoRequirement> RequiredCargo { get; init; }

        public int ExpectedHighSlots { get; init; }
        public int ExpectedMidSlots { get; init; }
        public int ExpectedLowSlots { get; init; }
        public int DronesNeeded { get; init; }

        public int ExpectedModules => ExpectedHighSlots + ExpectedMidSlots + ExpectedLowSlots;
    }

    /// <summary>The catalog of run profiles. Add a ship/tier here to make it selectable.</summary>
    public static class ProfilesRegistry
    {
        /// <summary>Worm (drone/missile shield buffer) running a T1 ("Calm") exotic abyss.</summary>
        public static readonly RunProfile Worm_T1 = new()
        {
            Name = "Worm_T1",
            FilamentName = "Calm Exotic Filament",
            BuildFit = FitsRegistry.Worm,
            ExpectedHighSlots = 1,
            ExpectedMidSlots = 4,
            ExpectedLowSlots = 2,
            DronesNeeded = 5,
            RequiredCargo = new[]
            {
                new CargoRequirement("Calm Exotic Filament", 1),
                // Per-run minimum, not the full stock: a 1-launcher Worm won't burn more than this
                // in a single T1 run, and the gate should pass a partially-used ammo bay.
                new CargoRequirement("Caldari Navy Scourge Light Missile", 400),
                new CargoRequirement("Nanite Repair Paste", 10),
                new CargoRequirement("Hornet II", 5, InDroneBay: true),
            },
        };

        /// <summary>Stormbringer (Vorton battlecruiser) roaming Guristas combat anomalies (Refuge/Den) — no abyss, no filament.</summary>
        public static readonly RunProfile Stormbringer = new()
        {
            Name = "Stormbringer",
            FilamentName = "(none — roaming ratter)",
            BuildFit = FitsRegistry.Stormbringer,
            ExpectedHighSlots = 1,
            ExpectedMidSlots = 6,
            ExpectedLowSlots = 3,
            DronesNeeded = 0,
            RequiredCargo = new[]
            {
                // Vorton charges are the ship's ammo; BlastShot is the close/DPS pack. Keep the gate light —
                // this is untimed roaming, not an abyss run.
                new CargoRequirement("BlastShot Condenser Pack M", 200),
                new CargoRequirement("Nanite Repair Paste", 50),
            },
        };

        /// <summary>Throwaway "ttt" Hawk (one of three) running a T4 ("Raging") dark abyss as a spider fleet.</summary>
        public static readonly RunProfile Hawk_T4 = new()
        {
            Name = "Hawk_T4",
            FilamentName = "Raging Dark Filament",
            // Dark Matter Field speeds the rats up by tier: ≈1.3 at T4, ≈1.5 at T5. Feeds the ammo model.
            BuildFit = bot => FitsRegistry.HawkTTT(bot, 1.3),
            // 2 high BUTTONS: the 4 launchers are grouped into one, plus the rail (see FitsRegistry.HawkTTT).
            ExpectedHighSlots = 2,
            ExpectedMidSlots = 5,
            ExpectedLowSlots = 2,
            DronesNeeded = 0,
            RequiredCargo = new[]
            {
                new CargoRequirement("Raging Dark Filament", 1),
                // Per-run minimum for a 3-room T4, not the full stock. Since patch 2026-09-22 the Hawk's
                // damage bonus covers every damage type and Triglavian armor is weakest to explosive:
                // the bot flies on Navy Nova and the planner loads Fury Nova for battleship-heavy
                // stretches of the kill queue (AmmoController). Navy Inferno / Navy Scourge are optional
                // extras — carried if there is room, used only once seen in the hold.
                new CargoRequirement("Caldari Navy Nova Light Missile", 600),
                new CargoRequirement("Nova Fury Light Missile", 200),
                new CargoRequirement("Navy Cap Booster 400", 5),
                new CargoRequirement("Nanite Repair Paste", 5),
            },
        };

        /// <summary>Same "ttt" Hawk trio one tier down — T3 ("Fierce") dark abyss, the shake-down run.</summary>
        public static readonly RunProfile Hawk_T3 = new()
        {
            Name = "Hawk_T3",
            FilamentName = "Fierce Dark Filament",
            // T3 Dark Matter Field speeds the rats up less than T4/T5; the ammo model is the only
            // consumer and its charge decisions do not flip anywhere in the 1.2–1.5 range.
            BuildFit = bot => FitsRegistry.HawkTTT(bot, 1.2),
            ExpectedHighSlots = 2,
            ExpectedMidSlots = 5,
            ExpectedLowSlots = 2,
            DronesNeeded = 0,
            RequiredCargo = new[]
            {
                new CargoRequirement("Fierce Dark Filament", 1),
                new CargoRequirement("Caldari Navy Nova Light Missile", 600),
                new CargoRequirement("Nova Fury Light Missile", 200),
                new CargoRequirement("Navy Cap Booster 400", 5),
                new CargoRequirement("Nanite Repair Paste", 5),
            },
        };

        public static readonly IReadOnlyList<RunProfile> All = new[] { Worm_T1, Stormbringer, Hawk_T4, Hawk_T3 };

        public static RunProfile Default => Worm_T1;

        public static RunProfile GetByName(string? name) =>
            All.FirstOrDefault(p => string.Equals(p.Name, name, System.StringComparison.OrdinalIgnoreCase))
            ?? Default;
    }

    /// <summary>One readiness check outcome.</summary>
    public sealed record ReadinessItem(string Name, bool Ok, string Detail);

    public sealed record ReadinessReport(IReadOnlyList<ReadinessItem> Items)
    {
        public bool AllOk => Items.All(i => i.Ok);
        public IEnumerable<string> Problems => Items.Where(i => !i.Ok).Select(i => i.Name);
    }

    /// <summary>
    /// Checks whether the detected client is compatible with, and prepared for, a profile —
    /// before you commit to a run. Structural checks (ship in space, slot layout, drones,
    /// windows) are read straight from the parsed UI. Cargo checks (filament/ammo) need the
    /// inventory window open, and degrade to "unknown" when it isn't.
    /// </summary>
    public static class ReadinessCheck
    {
        public static ReadinessReport Evaluate(IMemoryMeasurement mem, RunProfile profile)
        {
            var items = new List<ReadinessItem>();

            var shipUi = mem?.ShipUi;
            var inSpace = shipUi != null;
            items.Add(new ReadinessItem("Ship in space", inSpace,
                inSpace ? "ship UI present" : "no ship UI — docked or on a non-space screen"));

            if (inSpace)
            {
                var rows = shipUi!.ModuleButtons
                    .GroupBy(m => m.UINode.Region?.Min1)
                    .OrderBy(g => g.Key)
                    .ToArray();
                var moduleCount = shipUi.ModuleButtons.Count;

                items.Add(new ReadinessItem("Module rows", rows.Length == 3,
                    $"{rows.Length} row(s) detected (fit mapping needs exactly 3)"));
                items.Add(new ReadinessItem("Module count", moduleCount == profile.ExpectedModules,
                    $"{moduleCount} modules (profile expects {profile.ExpectedModules})"));
            }

            var droneWindow = mem?.WindowDroneView;
            if (profile.DronesNeeded > 0)
            {
                items.Add(new ReadinessItem("Drone window", droneWindow != null,
                    droneWindow != null ? "open" : "not open — needed for drone control"));


                var inBay = droneWindow?.DroneGroupInBay?.Children?.Count
                            ?? droneWindow?.DroneGroups?.SelectMany(g => g.Children ?? new List<IDronesWindowEntryDrone>()).Count()
                            ?? 0;
                items.Add(new ReadinessItem("Drones in bay", inBay >= profile.DronesNeeded,
                    $"{inBay} in bay (need {profile.DronesNeeded})"));
            }

            var overviewOpen = (mem?.WindowOverview?.Length ?? 0) > 0;
            items.Add(new ReadinessItem("Overview window", overviewOpen,
                overviewOpen ? "open" : "not open — needed to see enemies/objects"));

            // Cargo checks require the inventory window; report honestly when we cannot see it.
            var inventoryOpen = (mem?.WindowInventory?.Length ?? 0) > 0;
            items.Add(new ReadinessItem($"Filament ({profile.FilamentName})", inventoryOpen,
                inventoryOpen
                    ? "inventory open — verify filament/ammo counts on the dashboard/overview"
                    : "unknown — open the inventory to verify cargo"));

            return new ReadinessReport(items);
        }
    }
}
