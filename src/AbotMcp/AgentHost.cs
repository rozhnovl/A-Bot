using AbotEngine;
using Eve64;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace AbotMcp;

/// <summary>
/// One attached client: serializes every engine operation (tool calls arrive concurrently and the
/// autopilot loop runs on its own thread), keeps the latest perception plus an id -> element index
/// so a tool can click what a previous tool listed, and owns the optional autopilot loop.
/// </summary>
internal sealed class AgentHost : IDisposable
{
    public ClientAgent Agent { get; }
    public readonly object Sync = new();

    public string AttachStatus { get; private set; } = "pending";
    public bool Attached { get; private set; }
    public bool Attaching { get; private set; }

    public ParsedUserInterface? LastParsed { get; private set; }
    public long LastParsedAtMs { get; private set; }
    public AgentSnapshot? LastStep { get; private set; }
    public long LastStepAtMs { get; private set; }

    private readonly Dictionary<long, IUIElement> parsedElements = new();
    private readonly Dictionary<long, IUIElement> rawElements = new();

    private Thread? loop;
    private volatile bool running;
    private int intervalMs = 1500;
    private long autopilotSteps;
    private string? autopilotError;

    public AgentHost(ClientAgent agent) { Agent = agent; }

    public void Attach(bool rescan)
    {
        lock (Sync)
        {
            AttachStatus = "attaching (cached UIRoot if available, else ~10-90s scan)";
            Attaching = true;
            try
            {
                Attached = Agent.Attach(rescan);
                AttachStatus = Attached ? "attached" : "failed (no UIRoot — login screen? 32-bit client?)";
            }
            catch (Exception e)
            {
                Attached = false;
                AttachStatus = $"failed: {e.Message}";
            }
            finally { Attaching = false; }
        }
    }

    public T WithLock<T>(Func<T> f) { lock (Sync) return f(); }

    /// <summary>Fresh read + parse (no bot brain). Rebuilds the parsed-element index.</summary>
    public ParsedUserInterface? Perceive()
    {
        lock (Sync)
        {
            if (!Attached) return null;
            var p = Agent.Perceive();
            if (p is null) return null;
            LastParsed = p;
            LastParsedAtMs = Environment.TickCount64;
            parsedElements.Clear();
            UiSummary.IndexElements(p, Register);
            return p;
        }
    }

    public void Register(IUIElement? el)
    {
        if (el is null) return;
        try { parsedElements[el.Id] = el; } catch { /* element without id */ }
    }

    public void RegisterRaw(IUIElement el) => rawElements[el.Id] = el;

    public IUIElement? Find(long id) =>
        parsedElements.TryGetValue(id, out var e) ? e :
        rawElements.TryGetValue(id, out var r) ? r : null;

    /// <summary>Execute operator motions against the latest perception (perceives first if none).</summary>
    public string Execute(IEnumerable<MotionParam> motions, string action)
    {
        lock (Sync)
        {
            var parsed = LastParsed ?? Perceive();
            if (parsed is null) return $"{action}: UI unreadable (not attached or read failed)";
            if (Fleet.EmergencyStopped)
                return $"{action}: emergency stop active ({Fleet.StopReason}) — call resume first";
            return Agent.ExecuteManual(motions, parsed, action);
        }
    }

    /// <summary>One perceive -> bot decide -> (live: act) iteration of the configured strategy.</summary>
    public AgentSnapshot Step()
    {
        lock (Sync)
        {
            var s = Agent.Step();
            LastStep = s;
            LastStepAtMs = Environment.TickCount64;
            return s;
        }
    }

    // --- autopilot -------------------------------------------------------------

    public string AutopilotState => running ? $"running ({intervalMs} ms, {autopilotSteps} steps)" : "stopped";

    public object AutopilotStatus() => new
    {
        running,
        intervalMs,
        steps = autopilotSteps,
        lastError = autopilotError,
        lastStepAgeMs = LastStep is null ? (long?)null : Environment.TickCount64 - LastStepAtMs,
    };

    public string StartAutopilot(int interval)
    {
        if (Fleet.EmergencyStopped) return "emergency stop active — call resume first";
        if (!Attached) return "not attached";
        intervalMs = Math.Max(200, interval);
        if (running) return $"already running ({intervalMs} ms)";
        running = true;
        autopilotError = null;
        loop = new Thread(() =>
        {
            while (running && !Fleet.EmergencyStopped)
            {
                try
                {
                    Step();
                    Interlocked.Increment(ref autopilotSteps);
                }
                catch (Exception e) { autopilotError = e.Message; }
                for (var waited = 0; waited < intervalMs && running; waited += 50) Thread.Sleep(50);
            }
            running = false;
        })
        { IsBackground = true, Name = $"autopilot-{Agent.Pid}" };
        loop.Start();
        return $"autopilot started ({intervalMs} ms)" +
               (Agent.Live ? " — LIVE: the bot's own motions will execute" : " — dry-run: decisions only");
    }

    public string StopAutopilot()
    {
        if (!running) return "autopilot not running";
        running = false;
        return "autopilot stopped";
    }

    public object Describe() => new
    {
        pid = Agent.Pid,
        title = Agent.Title,
        role = Agent.Role,
        live = Agent.Live,
        attach = AttachStatus,
        autopilot = AutopilotState,
        lastPerceiveAgeMs = LastParsed is null ? (long?)null : Environment.TickCount64 - LastParsedAtMs,
        lastBotStep = LastStep?.StepIndex,
    };

    public void Dispose()
    {
        running = false;
        Agent.Dispose();
    }
}
