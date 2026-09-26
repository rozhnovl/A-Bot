using System.ComponentModel;
using System.Text.RegularExpressions;
using AbotEngine;
using BotEngine.Motor;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using WindowsInput.Native;

namespace AbotMcp;

/// <summary>The MCP tool surface. Every tool returns text (JSON or a short status line).</summary>
[McpServerToolType]
public sealed class AbotTools
{
    public const string Instructions = """
        A-Bot MCP: perceive and control one or more running EVE Online clients through the bot engine.

        Workflow: list_clients -> status/get_ui (find element ids) -> click / context_menu / press_keys ->
        watch or status to confirm the effect. Element ids come from get_ui/find_ui and are only valid
        for the latest perception; every action tool re-reads the UI before acting.

        Modes: the server runs dry-run unless started with --live. In dry-run every input tool only
        reports what it would do. In LIVE mode input goes to the real client — act deliberately, one
        step at a time, and verify. emergency_stop (or Ctrl+Alt+K on the PC) halts all input until resume.

        bot_step runs the bot's own strategy once; autopilot loops it. Manual tools and the autopilot
        share one lock per client, so they never interleave inside one action.
        """;

    private static readonly string ShotDir = Path.Combine(Path.GetTempPath(), "abot-mcp");

    private static string Guard(Func<string> f)
    {
        try { return f(); }
        catch (Exception e) { return $"error: {e.Message}"; }
    }

    private static string Json(object o) => Fleet.ToJson(o);

    // --- fleet ---------------------------------------------------------------

    [McpServerTool(Name = "list_clients", ReadOnly = true, Idempotent = true),
     Description("List attached EVE clients (pid, title, role, attach state, autopilot state) and the server mode (live/dry-run, emergency stop).")]
    public static string ListClients() => Guard(() => Json(Fleet.StatusObject()));

    [McpServerTool(Name = "attach_clients"),
     Description("Discover EVE clients (exefile.exe) that appeared after the server started — or one explicit pid — register them and attach in the background (a first-ever UIRoot scan can take ~90 s). Poll list_clients for the attach state. Also retries clients whose attach failed.")]
    public static string AttachClients(
        [Description("specific process id; omit to discover all exefile.exe processes")] int? pid = null,
        [Description("ignore the cached UIRoot addresses and rescan")] bool rescan = false)
        => Guard(() =>
        {
            var added = Fleet.DiscoverNewClients(pid);
            Fleet.AttachAllInBackground(rescan);
            return Json(new
            {
                added = added.Select(a => a.Describe()).ToArray(),
                clients = Fleet.Agents.Select(a => a.Describe()).ToArray(),
                note = "attaching in background; call list_clients to see progress",
            });
        });

    [McpServerTool(Name = "status", ReadOnly = true),
     Description("Fresh compact state of one client: ship HP/cap/speed/maneuver, locks, enemies on grid, active modules, open menus, plus the last bot decision (if any) and autopilot state. No input is sent.")]
    public static string Status(
        [Description("pid, window-title substring or role; optional when only one client is attached")] string? client = null)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var parsed = host.Perceive();
            var last = host.LastStep;
            return Json(new
            {
                client = host.Describe(),
                emergencyStop = Fleet.EmergencyStopped ? Fleet.StopReason : null,
                ui = parsed is null ? null : UiSummary.Quick(parsed),
                uiError = parsed is null ? (host.Attached ? "UI read/parse returned nothing" : host.AttachStatus) : null,
                lastBot = last is null ? null : new
                {
                    ageMs = Environment.TickCount64 - host.LastStepAtMs,
                    last.StepIndex,
                    last.ParseOk,
                    strategy = last.StrategyStatus,
                    last.Intents,
                    last.MotionCount,
                    last.Executed,
                    last.MotionProblems,
                    last.LastError,
                    readiness = last.Readiness.Where(r => !r.Ok).ToArray(),
                },
                autopilot = host.AutopilotStatus(),
            });
        });

    // --- perception ----------------------------------------------------------

    [McpServerTool(Name = "get_ui", ReadOnly = true),
     Description("Read the client UI now and return structured sections with element ids for clicking. " +
                 "sections: comma list of ship,targets,overview,menu,windows,drones,inventory,messages,probe,chat or 'all' " +
                 "(default: ship,targets,overview,menu,windows). Overview is sorted by distance; 'enemy' is the red-icon heuristic.")]
    public static string GetUi(
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("comma-separated section names or 'all'")] string? sections = null,
        [Description("max overview rows (default 60)")] int maxOverview = 60)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var parsed = host.Perceive();
            if (parsed is null) return $"error: UI unreadable ({host.AttachStatus})";
            var list = string.IsNullOrWhiteSpace(sections) ? UiSummary.DefaultSections
                : sections.Trim().Equals("all", StringComparison.OrdinalIgnoreCase) ? UiSummary.AllSections
                : sections.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return Json(UiSummary.Summarize(parsed, list, Math.Max(1, maxOverview)));
        });

    [McpServerTool(Name = "find_ui", ReadOnly = true),
     Description("Search the RAW UI tree (everything the client draws, including windows the parser does not model) for nodes whose python type, name or text contains the query. Returns ids clickable with click(). Use when get_ui lacks what you need.")]
    public static string FindUi(
        [Description("case-insensitive substring matched against type name, _name and text")] string query,
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("max hits (default 40)")] int limit = 40)
        => Guard(() =>
        {
            if (string.IsNullOrWhiteSpace(query)) return "error: query is empty";
            var host = Fleet.Resolve(client);
            return host.WithLock(() =>
            {
                var root = host.Agent.PerceiveRawWithRegion();
                if (root is null) return $"error: raw UI unreadable ({host.AttachStatus})";
                var hits = UiSummary.SearchRaw(root, query, Math.Clamp(limit, 1, 500));
                foreach (var h in hits.Where(h => h.W > 0 && h.H > 0))
                    host.RegisterRaw(Input.RegionElement(h.Id,
                        new Bib3.Geometrik.RectInt(h.X, h.Y, h.X + h.W, h.Y + h.H)));
                return Json(new { query, hits = hits.Count, results = hits });
            });
        });

    [McpServerTool(Name = "screenshot", ReadOnly = true),
     Description("Capture the client window as a PNG (returned inline and saved to disk). The window must be visible on screen. Text reports scale and window origin so image pixels map to click_at screen coordinates: screen = origin + image/scale.")]
    public static CallToolResult Screenshot(
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("downscale to this width (default 1600, 0 = original)")] int maxWidth = 1600)
    {
        try
        {
            var host = Fleet.Resolve(client);
            var shot = AbotMcp.Screenshot.Capture(host.Agent.MainWindowHandle, maxWidth, ShotDir);
            var info = Json(new
            {
                path = shot.Path,
                image = new { shot.Width, shot.Height },
                window = new { x = shot.OriginX, y = shot.OriginY, w = shot.WindowWidth, h = shot.WindowHeight },
                shot.Scale,
                hint = "screenX = window.x + imageX / scale; screenY = window.y + imageY / scale",
            });
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock { Text = info },
                    ImageContentBlock.FromBytes(shot.Png, "image/png"),
                ],
            };
        }
        catch (Exception e)
        {
            return new CallToolResult
            {
                IsError = true,
                Content = [new TextContentBlock { Text = $"error: {e.Message}" }],
            };
        }
    }

    // --- input -----------------------------------------------------------------

    [McpServerTool(Name = "click", Destructive = true),
     Description("Click a UI element by id (from get_ui/find_ui). button: left|right|middle|hover. modifiers: e.g. 'ctrl' (ctrl+click locks an overview entry), 'shift', 'ctrl+shift'. Re-reads the UI first; fails if the element vanished. Dry-run mode only reports.")]
    public static string Click(
        [Description("element id from get_ui / find_ui")] long id,
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("left (default) | right | middle | hover")] string button = "left",
        [Description("modifier keys held during the click, e.g. 'ctrl'")] string? modifiers = null,
        [Description("double-click instead of single")] bool doubleClick = false)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var btn = Input.ParseButton(button);
            var mods = Input.ParseModifiers(modifiers);
            return host.WithLock(() =>
            {
                host.Perceive();
                var el = host.Find(id);
                if (el is null) return $"error: element #{id} not present in the current UI (call get_ui/find_ui again)";
                if ((el.RegionInteraction?.Region ?? el.Region) is null) return $"error: element #{id} has no screen region";
                var what = $"{(doubleClick ? "double-" : "")}{btn.ToString().ToLower()}-click" +
                           (mods.Length > 0 ? $" with {modifiers}" : "") + $" on {Input.Describe(el)}";
                return host.Execute(Input.Click(el, btn, mods, doubleClick), what);
            });
        });

    [McpServerTool(Name = "click_at", Destructive = true),
     Description("Click absolute screen coordinates (use screenshot's mapping). Bypasses the occlusion check — prefer click(id) when an element id exists.")]
    public static string ClickAt(
        [Description("screen x")] int x,
        [Description("screen y")] int y,
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("left (default) | right | middle | hover")] string button = "left",
        [Description("modifier keys, e.g. 'ctrl'")] string? modifiers = null,
        [Description("double-click instead of single")] bool doubleClick = false)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var btn = Input.ParseButton(button);
            var mods = Input.ParseModifiers(modifiers);
            var el = Input.PointElement(x, y);
            var what = $"{btn.ToString().ToLower()}-click at ({x},{y})" + (mods.Length > 0 ? $" with {modifiers}" : "");
            return host.WithLock(() =>
            {
                host.Perceive();
                return host.Execute(Input.Click(el, btn, mods, doubleClick), what);
            });
        });

    [McpServerTool(Name = "press_keys", Destructive = true),
     Description("Press keyboard chords in the client, space-separated: 'f1', 'ctrl+f1', 'alt+p', 'esc', 'ctrl+space', 'f1 f2 f3'. Module hotkeys: F1-F8 high, ctrl+F1.. / alt+F1.. per EVE settings.")]
    public static string PressKeys(
        [Description("chords separated by spaces; keys inside a chord joined by '+'")] string keys,
        [Description("pid, title substring or role; optional with a single client")] string? client = null)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var chords = Input.ParseChords(keys);
            if (chords.Length == 0) return "error: no keys given";
            var motions = chords.Select(c => c.KeyboardPressCombined()).ToArray();
            motions[0].WindowToForeground = true;
            return host.WithLock(() =>
            {
                host.Perceive();
                return host.Execute(motions, $"press {keys}");
            });
        });

    [McpServerTool(Name = "type_text", Destructive = true),
     Description("Type literal text into the focused input of the client (click the input first).")]
    public static string TypeText(
        [Description("text to type")] string text,
        [Description("pid, title substring or role; optional with a single client")] string? client = null)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            if (string.IsNullOrEmpty(text)) return "error: text is empty";
            return host.WithLock(() =>
            {
                host.Perceive();
                return host.Execute(new[] { text.TextEntry() }, $"type \"{text}\"");
            });
        });

    [McpServerTool(Name = "context_menu", Destructive = true),
     Description("Right-click an element and follow a context-menu path, e.g. path='Warp to Within > Within 0 m', 'Orbit > 5,000 m', 'Lock Target'. Each segment is a regex matched (ignore-case) against menu entries, deepest open level first; intermediate segments are hovered, the last one is clicked. Returns the trail and the menus seen.")]
    public static string ContextMenu(
        [Description("element id to right-click (overview entry, target, item, ...)")] long id,
        [Description("menu path; segments separated by '>' (or '|')")] string path,
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("ms to wait for each menu level (default 250)")] int settleMs = 250)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var segments = path.Split(new[] { '>', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (segments.Length == 0) return "error: empty path";
            return host.WithLock(() =>
            {
                host.Perceive();
                var root = host.Find(id);
                if (root is null) return $"error: element #{id} not present in the current UI";
                if (!Fleet.Live)
                    return $"dry-run (no input sent): would right-click {Input.Describe(root)} then follow [{string.Join(" > ", segments)}]";

                var trail = new List<string>
                {
                    host.Execute(Input.Click(root, MouseButtonIdEnum.Right, Array.Empty<VirtualKeyCode>(), false),
                        $"right-click {Input.Describe(root)}"),
                };
                if (trail[0].StartsWith("right-click", StringComparison.Ordinal) && trail[0].Contains(": "))
                    return Json(new { ok = false, trail });

                for (var i = 0; i < segments.Length; i++)
                {
                    var last = i == segments.Length - 1;
                    IMenuEntry? found = null;
                    string menusSeen = "";
                    for (var attempt = 0; attempt < 8 && found is null; attempt++)
                    {
                        Thread.Sleep(Math.Clamp(settleMs, 50, 2000));
                        var parsed = host.Perceive();
                        if (parsed is null) continue;
                        var levels = (parsed.Menu ?? Array.Empty<IMenu>()).Where(m => m?.Entry?.Any() == true).ToArray();
                        menusSeen = string.Join("; ", levels.Select((m, l) =>
                            $"L{l}=[{string.Join(" | ", m.Entry.Select(e => UiSummary.Clean(e?.Text)))}]"));
                        for (var level = levels.Length - 1; level >= 0 && found is null; level--)
                            found = levels[level].Entry.FirstOrDefault(e =>
                                Regex.IsMatch(UiSummary.Clean(e?.Text), segments[i], RegexOptions.IgnoreCase));
                    }
                    if (found is null)
                        return Json(new { ok = false, failedSegment = segments[i], trail, menus = menusSeen });

                    var button = last ? MouseButtonIdEnum.Left : MouseButtonIdEnum.None;
                    trail.Add(host.Execute(Input.Click(found, button, Array.Empty<VirtualKeyCode>(), false),
                        $"{(last ? "click" : "hover")} '{UiSummary.Clean(found.Text)}'"));
                }

                Thread.Sleep(150);
                var after = host.Perceive();
                var remaining = after is null ? null : (after.Menu ?? Array.Empty<IMenu>())
                    .Where(m => m?.Entry?.Any() == true)
                    .Select(m => m.Entry.Select(e => UiSummary.Clean(e?.Text)).ToArray()).ToArray();
                return Json(new { ok = true, trail, menusStillOpen = remaining is { Length: > 0 } ? remaining : null });
            });
        });

    // --- observation -----------------------------------------------------------

    [McpServerTool(Name = "watch", ReadOnly = true),
     Description("Sample the client state for a few seconds and return only the changes (HP, cap, speed, maneuver, locks, enemies, active modules, menus). Use right after an action to see its effect. No input is sent.")]
    public static string Watch(
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("how long to observe (default 5, max 30)")] double seconds = 5,
        [Description("sampling period in ms (default 500)")] int sampleMs = 500)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            var total = TimeSpan.FromSeconds(Math.Clamp(seconds, 0.5, 30));
            var period = Math.Clamp(sampleMs, 100, 5000);
            var started = DateTime.UtcNow;
            var lines = new List<string>();
            UiSummary.QuickState? prev = null;
            var samples = 0;
            while (DateTime.UtcNow - started < total)
            {
                var parsed = host.Perceive();
                samples++;
                var t = (DateTime.UtcNow - started).TotalSeconds;
                if (parsed is null)
                {
                    if (prev is not null || lines.Count == 0) lines.Add($"{t,5:F1}s UI unreadable");
                    prev = null;
                }
                else
                {
                    var q = UiSummary.Quick(parsed);
                    if (prev is null || q != prev) lines.Add($"{t,5:F1}s {q.Line()}");
                    prev = q;
                }
                Thread.Sleep(period);
            }
            return Json(new { samples, changes = lines, final = prev });
        });

    [McpServerTool(Name = "wait"),
     Description("Sleep server-side for up to 15000 ms (lets an in-game action settle before the next tool call).")]
    public static string Wait([Description("milliseconds")] int ms)
    {
        Thread.Sleep(Math.Clamp(ms, 0, 15000));
        return $"waited {Math.Clamp(ms, 0, 15000)} ms";
    }

    // --- bot ---------------------------------------------------------------------

    [McpServerTool(Name = "bot_step", Destructive = true),
     Description("Run ONE perceive->decide->act iteration of the bot's configured strategy (run profile) and return its snapshot: strategy status, intents, motions. In LIVE mode the bot's motions execute.")]
    public static string BotStep(
        [Description("pid, title substring or role; optional with a single client")] string? client = null)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            if (!host.Attached) return $"error: {host.AttachStatus}";
            if (Fleet.EmergencyStopped) return $"error: emergency stop active ({Fleet.StopReason}) — call resume first";
            var s = host.Step();
            return Json(new
            {
                s.StepIndex, s.Mode, s.ParseOk, s.System, s.InSpace,
                hp = new { s.Shield, s.Armor, s.Struct, s.Capacitor },
                s.TargetsLocked, s.Attackers, s.IncomingDps,
                strategy = s.StrategyStatus, s.Intents, s.MotionCount, s.Executed, s.MotionProblems, s.LastError,
                readinessFailures = s.Readiness.Where(r => !r.Ok).ToArray(),
            });
        });

    [McpServerTool(Name = "autopilot", Destructive = true),
     Description("Control the background bot loop for a client: action=start|stop|status. While running, bot_step repeats every intervalMs (LIVE: it acts). Manual tools still work between steps.")]
    public static string Autopilot(
        [Description("start | stop | status")] string action,
        [Description("pid, title substring or role; optional with a single client")] string? client = null,
        [Description("step period in ms for start (default 1500)")] int intervalMs = 1500)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            return action.Trim().ToLowerInvariant() switch
            {
                "start" => host.StartAutopilot(intervalMs),
                "stop" => host.StopAutopilot(),
                "status" => Json(host.AutopilotStatus()),
                _ => "error: action must be start | stop | status",
            };
        });

    [McpServerTool(Name = "emergency_stop"),
     Description("Immediately halt ALL input on all clients and stop every autopilot (same as Ctrl+Alt+K). Stays latched until resume.")]
    public static string EmergencyStop([Description("why (for the log)")] string reason = "tool")
    {
        Fleet.TriggerEmergencyStop($"tool: {reason}");
        return $"emergency stop latched ({reason}); autopilots stopped; input refused until resume";
    }

    [McpServerTool(Name = "resume"),
     Description("Clear the emergency-stop latch so input tools and autopilot work again.")]
    public static string Resume()
    {
        if (!Fleet.EmergencyStopped) return "no emergency stop active";
        Fleet.Resume();
        return "resumed";
    }

    [McpServerTool(Name = "dump_diagnostics", ReadOnly = true),
     Description("Write the raw UI tree (rawtree.json) and a parsed summary (parsed.json) to a new folder for offline analysis; returns the paths.")]
    public static string DumpDiagnostics(
        [Description("pid, title substring or role; optional with a single client")] string? client = null)
        => Guard(() =>
        {
            var host = Fleet.Resolve(client);
            if (!host.Attached) return $"error: {host.AttachStatus}";
            var dir = Path.GetFullPath($"mcp-dump-{host.Agent.Pid}-{DateTime.Now:yyyyMMdd-HHmmss}");
            var files = host.WithLock(() => host.Agent.DumpDiagnostics(dir));
            return Json(new { dir, files });
        });
}
