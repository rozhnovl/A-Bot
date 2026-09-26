using System.Diagnostics;
using System.Text.Json;
using AbotEngine;
using Sanderling.ABot.Bot.Configuration;
using System.Text.Json.Serialization;

namespace AbotMcp;

/// <summary>Process-wide registry of attached clients plus the shared emergency-stop latch.</summary>
internal static class Fleet
{
    public static bool Live;
    public static Action<string> Log = _ => { };
    public static RunProfile Profile = null!;
    public static FleetConfig? Config;
    public static readonly List<AgentHost> Agents = new();
    private static readonly object AttachSync = new();

    /// <summary>Create the engine for one client process and register it (not attached yet).</summary>
    public static AgentHost AddClient(Process p)
    {
        var role = Config?.RoleFor(FleetConfig.SafeTitle(p)) ?? "solo";
        var agent = new ClientAgent(p, Profile, Live, role: role, trace: Log,
            stopRequested: () => EmergencyStopped);
        var host = new AgentHost(agent);
        lock (Agents) Agents.Add(host);
        return host;
    }

    /// <summary>Attach every not-yet-attached client sequentially on a background thread (scans can take ~90s).</summary>
    public static void AttachAllInBackground(bool rescan, Action? done = null)
    {
        _ = Task.Run(() =>
        {
            lock (AttachSync)
            {
                AgentHost[] pending;
                lock (Agents) pending = Agents.Where(a => !a.Attached && !a.Attaching).ToArray();
                foreach (var host in pending)
                {
                    host.Attach(rescan);
                    Log($"pid {host.Agent.Pid}: {host.AttachStatus}");
                }
            }
            done?.Invoke();
        });
    }

    /// <summary>Register clients that appeared after startup (or one explicit pid). Returns what was added.</summary>
    public static List<AgentHost> DiscoverNewClients(int? pid)
    {
        var known = Agents.Select(a => a.Agent.Pid).ToHashSet();
        var candidates = pid is int explicitPid
            ? new[] { Process.GetProcessById(explicitPid) }
            : Process.GetProcessesByName("exefile").OrderBy(p => p.Id).ToArray();
        return candidates.Where(p => !known.Contains(p.Id)).Select(AddClient).ToList();
    }

    private static volatile bool stopped;
    public static bool EmergencyStopped => stopped;
    public static string? StopReason { get; private set; }
    public static DateTime? StoppedAt { get; private set; }

    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string ToJson(object o) => JsonSerializer.Serialize(o, Json);

    public static void TriggerEmergencyStop(string reason)
    {
        stopped = true;
        StopReason = reason;
        StoppedAt = DateTime.Now;
        foreach (var a in Agents) a.StopAutopilot();
        Log($"EMERGENCY STOP ({reason}): input halted, autopilots stopped. Use the resume tool to re-arm.");
    }

    public static void Resume()
    {
        stopped = false;
        StopReason = null;
        StoppedAt = null;
        Log("Emergency stop cleared.");
    }

    /// <summary>Pick a client by pid or by a case-insensitive substring of its window title / its role.</summary>
    public static AgentHost Resolve(string? client)
    {
        if (Agents.Count == 0) throw new InvalidOperationException("no clients registered");
        if (string.IsNullOrWhiteSpace(client))
        {
            if (Agents.Count == 1) return Agents[0];
            throw new InvalidOperationException(
                "several clients are attached; pass client=<pid | title substring | role>: " + Describe());
        }
        var key = client.Trim();
        if (int.TryParse(key, out var pid))
        {
            var byPid = Agents.FirstOrDefault(a => a.Agent.Pid == pid);
            if (byPid != null) return byPid;
        }
        var matches = Agents.Where(a =>
                a.Agent.Title.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.Agent.Role, key, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (matches.Count == 1) return matches[0];
        throw new InvalidOperationException(matches.Count == 0
            ? $"no client matches '{client}': " + Describe()
            : $"'{client}' is ambiguous: " + Describe());
    }

    public static string Describe() =>
        string.Join(", ", Agents.Select(a => $"pid {a.Agent.Pid} \"{a.Agent.Title}\" ({a.Agent.Role})"));

    public static object StatusObject() => new
    {
        live = Live,
        emergencyStop = stopped ? new { reason = StopReason, at = StoppedAt } : null,
        clients = Agents.Select(a => a.Describe()).ToArray(),
    };

    public static string StatusJson() => ToJson(StatusObject());

    public static string StatusText() =>
        $"AbotMcp — mode={(Live ? "LIVE" : "dry-run")}, emergencyStop={(stopped ? StopReason : "no")}\n" +
        string.Join("\n", Agents.Select(a =>
            $"pid {a.Agent.Pid} \"{a.Agent.Title}\" role={a.Agent.Role} attach={a.AttachStatus} autopilot={a.AutopilotState}"));
}
