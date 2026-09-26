using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.ABot.Bot
{
	/// <summary>
	/// End-of-room blackboard shared by the ships running in this process (same pattern as
	/// <see cref="FleetFireBoard"/>). Every bot inside the abyss heartbeats each tick, so the board
	/// knows who is alive; at the end of a room each ship reports its distance to the conduit and
	/// whether it is ready to jump, and the fleet takes the gate together once EVERY live member is
	/// ready. A member that has not reached the end-of-room flow yet (still fighting, still
	/// reloading) counts as not ready — on 2026-09-27 the board only knew about ships that had
	/// already reported, so each ship saw itself alone and jumped alone. A member that stops
	/// heartbeating (autopilot off, dead) drops out after <see cref="StaleMs"/>; one that has just
	/// taken the gate is marked jumped and ignored until it heartbeats from a room with a spawn.
	/// </summary>
	public static class FleetGateBoard
	{
		/// <summary>A heartbeat or report older than this no longer counts.</summary>
		public const int StaleMs = 20000;

		private sealed class Member
		{
			public long HeartbeatTick;
			public bool Jumped;
			public double DistanceM;
			public bool Ready;
			public long ReportTick;
		}

		private static readonly object Gate = new();
		private static readonly Dictionary<int, Member> ByPid = new();

		private static Member Get(int pid)
		{
			if (!ByPid.TryGetValue(pid, out var member))
				ByPid[pid] = member = new Member();
			return member;
		}

		/// <summary>
		/// Called every tick by a bot that is inside the abyss. A spawn on grid (<paramref name="roomClear"/>
		/// false) clears the "jumped" mark: the ship has arrived in the next room.
		/// </summary>
		public static void Heartbeat(int pid, bool roomClear)
		{
			lock (Gate)
			{
				var member = Get(pid);
				member.HeartbeatTick = Environment.TickCount64;
				if (!roomClear)
					member.Jumped = false;
			}
		}

		/// <summary>This client's distance to the conduit and whether it is ready to take the gate.</summary>
		public static void Report(int pid, double distanceM, bool ready)
		{
			lock (Gate)
			{
				var member = Get(pid);
				var now = Environment.TickCount64;
				member.DistanceM = distanceM;
				member.Ready = ready;
				member.ReportTick = now;
				member.HeartbeatTick = now;
			}
		}

		/// <summary>This client is taking the gate: nobody waits for it until it shows up in the next room.</summary>
		public static void MarkJumped(int pid)
		{
			lock (Gate)
			{
				var member = Get(pid);
				member.Jumped = true;
				member.Ready = false;
				member.ReportTick = 0;
			}
		}

		/// <summary>Drop this client entirely (client detached / run over).</summary>
		public static void Forget(int pid)
		{
			lock (Gate)
				ByPid.Remove(pid);
		}

		/// <summary>
		/// True when at least one live member exists and every live member (fresh heartbeat, not
		/// jumped) has a fresh report saying ready.
		/// </summary>
		public static bool AllReady(out string detail)
		{
			lock (Gate)
			{
				var now = Environment.TickCount64;
				var live = ByPid
					.Where(kv => now - kv.Value.HeartbeatTick <= StaleMs && !kv.Value.Jumped)
					.ToList();
				detail = live.Count == 0
					? "no live fleet members"
					: string.Join(", ", live.Select(kv =>
					{
						var m = kv.Value;
						var fresh = now - m.ReportTick <= StaleMs;
						return $"{kv.Key}: {(fresh && m.Ready ? "ready" : fresh ? $"{m.DistanceM:F0} m out" : "not at the gate yet")}";
					}));
				return live.Count > 0 && live.All(kv => now - kv.Value.ReportTick <= StaleMs && kv.Value.Ready);
			}
		}

		public static void Reset()
		{
			lock (Gate)
				ByPid.Clear();
		}
	}
}
