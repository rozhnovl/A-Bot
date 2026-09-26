using Eve64;
using Sanderling.Interface.MemoryStruct;

namespace FleetOrchestrator.Fleet;

/// <summary>Helpers that boil a parsed UI down to the small facts coordination needs.</summary>
public static class UiSummary
{
    public static FleetMemberStatus StatusFrom(FleetMember member, ParsedUserInterface ui, long timeMs)
    {
        // The parser fills HP into the concrete ShipUi.HitpointsPercent, not the interface's HitpointsAndEnergy.
        var hp = (ui.ShipUi as ShipUi)?.HitpointsPercent;
        var overviewEntries = ui.WindowOverview?
            .SelectMany(w => w.Entries ?? new List<IOverviewEntry>())
            .Count() ?? 0;

        return new FleetMemberStatus(
            Pid: member.Pid,
            Role: member.Role,
            SolarSystem: ui.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName,
            ArmorPct: hp?.Armor,
            ShieldPct: hp?.Shield,
            StructPct: hp?.Structure,
            TargetsLocked: ui.Target?.Length ?? 0,
            OverviewEntries: overviewEntries,
            InSpace: ui.ShipUi != null,
            UpdatedAtMs: timeMs);
    }

    /// <summary>
    /// Very rough "first thing worth shooting" heuristic for the skeleton: the first
    /// overview entry that has a name. Real target selection (hostiles, NPC priority,
    /// range) plugs in here later.
    /// </summary>
    public static string? FirstOverviewTargetName(ParsedUserInterface ui) =>
        ui.WindowOverview?
            .SelectMany(w => w.Entries ?? new List<IOverviewEntry>())
            .Select(e => e.ObjectName)
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
}
