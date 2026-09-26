using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.ABot.Bot
{
	/// <summary>
	/// End-of-room blackboard shared by the ships running in this process (same pattern as
	/// <see cref="FleetFireBoard"/>). Each bot reports how far it is from the exit conduit and whether
	/// it is ready to jump; the fleet takes the gate together once every fresh report says ready.
	/// A member that stops reporting — it jumped, died, or its autopilot is off — drops out after
	/// <see cref="StaleMs"/>, so nobody can hold the others forever; the fight state adds a wait
	/// timeout on top for the case where a member keeps reporting but never arrives.
	/// </summary>
	public static class FleetGateBoard
	{
		/// <summary>A report older than this no longer counts as a fleet member at the conduit.</summary>
		public const int StaleMs = 20000;

		private sealed record Entry(double DistanceM, bool Ready, long Tick);

		private static readonly object Gate = new();
		private static readonly Dictionary<int, Entry> ByPid = new();

		/// <summary>This client's distance to the conduit and whether it is ready to take the gate.</summary>
		public static void Report(int pid, double distanceM, bool ready)
		{
			lock (Gate)
				ByPid[pid] = new Entry(distanceM, ready, Environment.TickCount64);
		}

		/// <summary>Drop this client (it is taking the gate / left the room).</summary>
		public static void Forget(int pid)
		{
			lock (Gate)
				ByPid.Remove(pid);
		}

		/// <summary>True when at least one fresh report exists and every fresh report is ready.</summary>
		public static bool AllReady(out string detail)
		{
			lock (Gate)
			{
				var now = Environment.TickCount64;
				var fresh = ByPid.Where(kv => now - kv.Value.Tick <= StaleMs).ToList();
				detail = fresh.Count == 0
					? "no fleet reports"
					: string.Join(", ", fresh.Select(kv =>
						$"{kv.Key}: {(kv.Value.Ready ? "ready" : $"{kv.Value.DistanceM:F0} m out")}"));
				return fresh.Count > 0 && fresh.All(kv => kv.Value.Ready);
			}
		}

		public static void Reset()
		{
			lock (Gate)
				ByPid.Clear();
		}
	}
}
