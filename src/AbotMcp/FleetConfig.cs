using System.Diagnostics;
using System.Text.Json;

namespace AbotMcp;

/// <summary>The subset of fleet.config.json needed to label clients with their doctrine role.</summary>
internal sealed class FleetConfig
{
    public string? Activity { get; set; }
    public string? FleetCommander { get; set; }
    public List<RoleAssignment>? Assignments { get; set; }

    public sealed class RoleAssignment
    {
        public string? TitleContains { get; set; }
        public string Role { get; set; } = "dps";
    }

    public string? RoleFor(string title) =>
        Assignments?.FirstOrDefault(a =>
                !string.IsNullOrEmpty(a.TitleContains) &&
                title.Contains(a.TitleContains, StringComparison.OrdinalIgnoreCase))
            ?.Role.ToLowerInvariant();

    public static FleetConfig? Load(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<FleetConfig>(File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return null; }
    }

    public static string SafeTitle(Process p)
    {
        try { return p.MainWindowTitle; } catch { return ""; }
    }
}
