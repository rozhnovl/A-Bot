using System.Collections.Immutable;
using System.Diagnostics;
using Eve64;
using Sanderling.Interface.MemoryStruct;

namespace FleetOrchestrator.Fleet;

/// <summary>
/// One EVE client window under fleet control: its process, its assigned role, and
/// the memory-reading state needed to sample its UI each tick.
///
/// The expensive UIRoot memory scan is done once and the resulting root addresses
/// are cached, exactly like ConsoleRunner does — subsequent ticks only re-read the
/// tree from the known roots, which is fast.
/// </summary>
public sealed class FleetMember
{
    public int Pid { get; }
    public FleetRole Role { get; set; }
    public string WindowTitle { get; }

    private readonly EveOnline64.MemoryReaderFromLiveProcess reader;
    private IImmutableList<ulong>? rootAddresses;

    public FleetMember(Process process, FleetRole role)
    {
        Pid = process.Id;
        Role = role;
        WindowTitle = SafeTitle(process);
        reader = new EveOnline64.MemoryReaderFromLiveProcess(process.Id);
    }

    /// <summary>True once the initial UIRoot scan has located at least one root.</summary>
    public bool IsAttached => rootAddresses is { Count: > 0 };

    /// <summary>
    /// Runs the one-time UIRoot memory scan. Slow (seconds); call once at startup or
    /// after the client reloads. Returns whether any root was found.
    /// </summary>
    public bool Attach()
    {
        rootAddresses = EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(Pid);
        return IsAttached;
    }

    /// <summary>
    /// Reads and parses the current UI for this window. Returns null if nothing could
    /// be read (client busy, reloaded, or on a screen with no UIRoot instance).
    /// </summary>
    public ParsedUserInterface? ReadCurrentUi()
    {
        if (rootAddresses is null)
            return null;

        var largest = rootAddresses
            .Select(addr => EveOnline64.ReadUITreeFromAddress(addr, reader, 99))
            .Where(tree => tree != null)
            .OrderByDescending(tree => tree!.EnumerateSelfAndDescendants().Count())
            .FirstOrDefault();

        if (largest is null)
            return null;

        var withRegion = Parser.ParseUITreeWithDisplayRegionFromUITree(largest);
        return Parser.ParseUserInterfaceFromUITree(withRegion);
    }

    private static string SafeTitle(Process p)
    {
        try { return p.MainWindowTitle; } catch { return "<unavailable>"; }
    }
}
