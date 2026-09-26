using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Sanderling.ABot.Bot.Configuration
{
    /// <summary>
    /// Accumulates what shows up in each abyssal room — enemy types seen and loot
    /// containers encountered — and appends one JSON line per room to a file, so runs
    /// can be analyzed later to tune fits and strategy (which spawns hit hardest, what
    /// loot each room type drops). Deliberately file-based (JSONL) and dependency-free;
    /// the disabled EF `AbyssEnemySpawnContext` was the earlier placeholder for this.
    ///
    /// Usage from the fight loop: call <see cref="Observe"/> every step, <see cref="AdvanceRoom"/>
    /// when a room is cleared and the gate is taken, and <see cref="EndRun"/> when leaving.
    /// </summary>
    public sealed class RoomStatsRecorder
    {
        private readonly string filePath;
        private readonly string runId;
        private int roomIndex;
        private DateTime roomStartUtc = DateTime.UtcNow;

        // name -> peak simultaneous count seen this room
        private readonly Dictionary<string, int> enemyPeak = new();
        private readonly HashSet<string> lootSeen = new(StringComparer.OrdinalIgnoreCase);
        private bool roomHasData;

        public RoomStatsRecorder(string profileName, string? filePath = null)
        {
            this.filePath = filePath ?? "roomstats.jsonl";
            runId = $"{profileName}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        }

        public record OverviewSnapshotEntry(string? Name, string? Type, bool IsEnemy);

        /// <summary>Fold the current overview into this room's running tally.</summary>
        public void Observe(IEnumerable<OverviewSnapshotEntry> entries)
        {
            if (entries == null) return;

            var enemiesNow = new Dictionary<string, int>();
            foreach (var e in entries)
            {
                var label = e.Name ?? e.Type;
                if (string.IsNullOrWhiteSpace(label)) continue;

                if (e.IsEnemy)
                {
                    enemiesNow.TryGetValue(label, out var c);
                    enemiesNow[label] = c + 1;
                    roomHasData = true;
                }
                else if (LooksLikeLoot(label))
                {
                    lootSeen.Add(label);
                    roomHasData = true;
                }
            }

            foreach (var (name, count) in enemiesNow)
            {
                enemyPeak.TryGetValue(name, out var peak);
                if (count > peak) enemyPeak[name] = count;
            }
        }

        private Dictionary<string, int>? lastCargo;
        private readonly string ledgerPath = "lootledger.jsonl";

        /// <summary>
        /// Fold a ship-cargo snapshot (item → quantity) into the loot ledger: whenever the hold's
        /// contents CHANGE between two readable snapshots, append one JSONL record with the deltas
        /// (+picked up / −consumed or jettisoned). Null snapshots (inventory closed or unreadable)
        /// are skipped and do NOT reset the baseline, so a window reopened mid-run diffs against the
        /// last real reading. Caveat: only the visible rows of a scrolled list are in memory, so a
        /// long scrolled hold can produce phantom ±pairs — irrelevant for an abyss frigate's short
        /// cargo list. Priced later offline (ESI Jita) to get ISK per run.
        /// </summary>
        public void ObserveCargo(IReadOnlyDictionary<string, int>? items)
        {
            if (items == null) return;

            if (lastCargo != null)
            {
                var changes = new Dictionary<string, int>();
                foreach (var (name, qty) in items)
                {
                    lastCargo.TryGetValue(name, out var prev);
                    if (qty != prev) changes[name] = qty - prev;
                }
                foreach (var (name, prev) in lastCargo)
                    if (!items.ContainsKey(name))
                        changes[name] = -prev;

                if (changes.Count > 0)
                {
                    try
                    {
                        var record = new { runId, room = roomIndex, tUtc = DateTime.UtcNow, changes };
                        File.AppendAllText(ledgerPath, JsonConvert.SerializeObject(record) + Environment.NewLine);
                    }
                    catch
                    {
                        // Never let ledger logging break the run.
                    }
                }
            }

            lastCargo = new Dictionary<string, int>(items.ToDictionary(kv => kv.Key, kv => kv.Value),
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool LooksLikeLoot(string label) =>
            label.Contains("Wreck", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Cache", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Container", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Bioadaptive", StringComparison.OrdinalIgnoreCase) ||
            label.Contains("Biocombinative", StringComparison.OrdinalIgnoreCase);

        /// <summary>Flush the current room and start a new one.</summary>
        public void AdvanceRoom()
        {
            Flush();
            roomIndex++;
            roomStartUtc = DateTime.UtcNow;
            enemyPeak.Clear();
            lootSeen.Clear();
            roomHasData = false;
        }

        /// <summary>Flush the final room at the end of a run.</summary>
        public void EndRun() => Flush();

        private void Flush()
        {
            if (!roomHasData) return;
            try
            {
                var record = new
                {
                    runId,
                    room = roomIndex,
                    startUtc = roomStartUtc,
                    endUtc = DateTime.UtcNow,
                    durationSec = (DateTime.UtcNow - roomStartUtc).TotalSeconds,
                    enemies = enemyPeak.OrderByDescending(kv => kv.Value)
                        .ToDictionary(kv => kv.Key, kv => kv.Value),
                    loot = lootSeen.OrderBy(x => x).ToArray(),
                };
                File.AppendAllText(filePath, JsonConvert.SerializeObject(record) + Environment.NewLine);
            }
            catch
            {
                // Never let stats logging break the run.
            }
        }
    }
}
