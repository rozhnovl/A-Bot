using FleetOrchestrator.Fleet.Activities;

namespace FleetOrchestrator.Fleet;

/// <summary>
/// The catalog of switchable fleet activities. Add a new activity by registering it
/// here; it then becomes selectable by name at startup (and, later, at runtime).
/// </summary>
public sealed class FleetActivityRegistry
{
    private readonly Dictionary<string, IFleetActivity> byName;

    public FleetActivityRegistry(IEnumerable<IFleetActivity> activities)
    {
        byName = activities.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
    }

    public static FleetActivityRegistry Default() => new(new IFleetActivity[]
    {
        new IdleActivity(),
        new AnomalyRattingActivity(),
        new BeltRattingActivity(),
        new AbyssalActivity(),
    });

    public IReadOnlyCollection<IFleetActivity> All => byName.Values;

    public bool TryGet(string name, out IFleetActivity activity) =>
        byName.TryGetValue(name, out activity!);

    public IFleetActivity GetOrIdle(string name) =>
        TryGet(name, out var a) ? a : byName["Idle"];
}
