namespace AbotEngine.Fleet;

/// <summary>
/// Offline scenarios that run hand-built <see cref="FleetPerception"/> states through the
/// <see cref="FleetBrain"/> and print the decisions. Doubles as a smoke test and living
/// documentation of the spider-tank decision logic. No live client involved.
/// </summary>
public static class FleetBrainScenarios
{
    public static void Run(Action<string> log)
    {
        Scenario(log, "1) All healthy, a webber + two normal rats",
            new FleetPerception
            {
                Members = new[]
                {
                    Hawk(1, sh: 95, inDps: 0, attackers: 0),
                    Hawk(2, sh: 88, inDps: 40, attackers: 1),
                    Hawk(3, sh: 92, inDps: 0, attackers: 0),
                },
                Enemies = new[]
                {
                    Npc(101, "Tessella Tesseris", dist: 8000, ehp: 3000, dps: 40, web: true),
                    Npc(102, "Vila Damavik", dist: 12000, ehp: 2000, dps: 20),
                    Npc(103, "Vedmak", dist: 20000, ehp: 9000, dps: 60),
                },
            });

        Scenario(log, "2) Ship 2 is being focused, dropping to 30% (overheat reps)",
            new FleetPerception
            {
                Members = new[]
                {
                    Hawk(1, sh: 80, inDps: 0, attackers: 0),
                    Hawk(2, sh: 30, inDps: 180, attackers: 4),
                    Hawk(3, sh: 75, inDps: 0, attackers: 0),
                },
                Enemies = new[]
                {
                    Npc(201, "Deviant Automata Suppressor", dist: 9000, ehp: 4000, dps: 0, suppressor: true),
                    Npc(202, "Vedmak", dist: 6000, ehp: 8000, dps: 90),
                },
            });

        Scenario(log, "3) Overwhelming incoming on ship 3 at 15% — rep-broken + bail",
            new FleetPerception
            {
                Members = new[]
                {
                    Hawk(1, sh: 60, inDps: 0, attackers: 0),
                    Hawk(2, sh: 55, inDps: 0, attackers: 0),
                    Hawk(3, sh: 15, inDps: 500, attackers: 6),
                },
                Enemies = new[] { Npc(301, "Karybdis Tyrannos", dist: 3000, ehp: 40000, dps: 400, karybdis: true) },
            });

        ScenarioMultiTick(log, "4) Karybdis on grid — orbit tightens each tick (angular tank); ship 2 cap-locked",
            new FleetPerception
            {
                Members = new[]
                {
                    Hawk(1, sh: 70, inDps: 50, attackers: 1),
                    Hawk(2, sh: 90, inDps: 0, attackers: 0, cap: 10),   // cap-locked
                    Hawk(3, sh: 85, inDps: 0, attackers: 0),
                },
                Enemies = new[] { Npc(401, "Karybdis Tyrannos", dist: 9000, ehp: 40000, dps: 300, karybdis: true) },
            }, ticks: 4);

        Scenario(log, "5) Room clear (no enemies) — reps warm, hold fire",
            new FleetPerception
            {
                Members = new[] { Hawk(1, sh: 70), Hawk(2, sh: 100), Hawk(3, sh: 100) },
                Enemies = Array.Empty<NpcState>(),
                Room = new RoomState { Index = 1, TimerRemainingSec = 800, GatePresent = true },
            });
    }

    private static void Scenario(Action<string> log, string title, FleetPerception p)
    {
        var brain = new FleetBrain();
        Print(log, title, brain.Decide(p), p);
    }

    private static void ScenarioMultiTick(Action<string> log, string title, FleetPerception p, int ticks)
    {
        var brain = new FleetBrain();
        log("");
        log($"=== {title} ===");
        for (var i = 0; i < ticks; i++)
        {
            var d = brain.Decide(p);
            log($"    -- tick {i}: {d.Summary}");
            foreach (var m in p.Members)
                if (d.Orders.TryGetValue(m.Pid, out var o))
                    log($"        pid {m.Pid} (sh {m.ShieldPct}%): {o.Intent}");
        }
    }

    private static void Print(Action<string> log, string title, FleetDecision d, FleetPerception p)
    {
        log("");
        log($"=== {title} ===");
        log($"    {d.Summary}");
        foreach (var m in p.Members)
            if (d.Orders.TryGetValue(m.Pid, out var o))
                log($"    pid {m.Pid} (sh {m.ShieldPct}%): {o.Intent}");
    }

    private static HawkState Hawk(int pid, int sh = 100, int inDps = 0, int attackers = 0, int cap = -1) =>
        new() { Pid = pid, InSpace = true, ShieldPct = sh, ArmorPct = 100, StructPct = 100, CapPct = cap, IncomingDps = inDps, Attackers = attackers };

    private static NpcState Npc(long id, string name, int dist, int ehp, int dps,
        bool web = false, bool scram = false, bool neut = false, bool suppressor = false, bool karybdis = false) =>
        new() { Id = id, Name = name, Distance = dist, EstimatedEhp = ehp, ApproxDps = dps,
                IsWebbing = web, IsScrambling = scram, IsNeuting = neut, IsSuppressor = suppressor, IsKarybdis = karybdis };
}
