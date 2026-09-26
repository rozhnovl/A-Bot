using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace AbotEngine;

/// <summary>
/// Caches the discovered UIRoot addresses to disk, keyed by process id, so restarting
/// against the same client instance skips the ~90s memory scan. Tagged with the process
/// start time so a reused pid (different process) invalidates the cache. Callers should
/// still re-validate by reading a tree and rescan on failure (the client can reallocate).
/// </summary>
public static class RootCache
{
    private sealed record Entry(long StartTimeTicks, string[] Roots);

    private static string PathFor(int pid) =>
        Path.Combine(Path.GetTempPath(), $"abot-roots-{pid}.json");

    public static IImmutableList<ulong>? Load(Process process)
    {
        try
        {
            var path = PathFor(process.Id);
            if (!File.Exists(path)) return null;

            var entry = JsonSerializer.Deserialize<Entry>(File.ReadAllText(path));
            if (entry is null) return null;
            if (entry.StartTimeTicks != SafeStartTicks(process)) return null;

            return entry.Roots.Select(ulong.Parse).ToImmutableList();
        }
        catch { return null; }
    }

    public static void Save(Process process, IEnumerable<ulong> roots)
    {
        try
        {
            var entry = new Entry(SafeStartTicks(process), roots.Select(r => r.ToString()).ToArray());
            File.WriteAllText(PathFor(process.Id), JsonSerializer.Serialize(entry));
        }
        catch { /* best-effort */ }
    }

    private static long SafeStartTicks(Process process)
    {
        try { return process.StartTime.Ticks; } catch { return 0; }
    }
}
