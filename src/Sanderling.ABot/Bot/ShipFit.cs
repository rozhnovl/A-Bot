using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using Microsoft.EntityFrameworkCore;
using Sanderling.ABot.Bot.Task;
using Sanderling.Parse;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{

	public class AbyssEnemySpawnContext: DbContext
	{
		/*
		public System.Data.Entity.DbSet<AbyssEnemySpawn> Spawns { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(@"Server=.\RESTO;Database=EveBot;Trusted_Connection=True;");
		}*/
	}

	public class AbyssEnemySpawn
	{
		public Guid Id { get; set; }
		public DateTime Time { get; set; }
		public string[] Enemies { get; set; }

	}

	/// <summary>
	/// Capacitor model of a fit, in GJ / seconds, taken from the fit's real stats (Pyfa). With this
	/// present the tank logic PROJECTS net capacitor drain — running shield boosters + prop mod +
	/// enemy neut pressure − natural regen — and injects a cap-booster charge BEFORE the capacitor
	/// would hit the reserve floor, instead of reacting only after a low-cap threshold is crossed.
	/// </summary>
	public sealed record CapacitorProfile
	{
		/// <summary>Full capacitor including battery and skills.</summary>
		public required double TotalGj { get; init; }
		/// <summary>Capacitor recharge time constant (tau), seconds.</summary>
		public required double RechargeTimeSec { get; init; }
		/// <summary>Prop-mod (AB/MWD) cap use per second while running.</summary>
		public double PropGjPerSec { get; init; }
		/// <summary>ONE shield booster's cap use per second while running.</summary>
		public double BoosterGjPerSec { get; init; }
		/// <summary>GJ restored by one cap-booster charge (e.g. Navy Cap Booster 400 = 400).</summary>
		public double InjectionGj { get; init; }
		/// <summary>Reserve fraction the planner never allows the projection to cross (escape/AB budget).</summary>
		public double FloorFraction { get; init; } = 0.15;
		/// <summary>Inject when projected time-to-floor drops under this — covers the injector's reload.</summary>
		public double LeadTimeSec { get; init; } = 15;
		/// <summary>
		/// Fraction of enemy neut pressure that actually lands on us (cap batteries resist energy
		/// warfare — e.g. a Pb-Acid battery's −22% makes this 0.78). 1 = no resistance.
		/// </summary>
		public double NeutResistanceFactor { get; init; } = 1.0;

		/// <summary>EVE capacitor regen at a given charge fraction: 10*Total/tau*(sqrt(x)-x), peaking near 25%.</summary>
		public double RegenGjPerSec(double fraction)
		{
			var x = Math.Clamp(fraction, 0.001, 1.0);
			return 10.0 * TotalGj / RechargeTimeSec * (Math.Sqrt(x) - x);
		}
	}

	public class ShipFit
	{
		/// <summary>
		/// One charge the fit can carry. All numbers are the EFFECTIVE ones this pilot sees in the item's
		/// info window (skills and fitting already applied) except <see cref="Drf"/>, which is a fixed
		/// property of the charge type. <see cref="AmmoController"/> feeds them into EVE's missile
		/// application formula.
		/// </summary>
		public sealed record AmmoCharge
		{
			/// <summary>SDE type id — what the HUD reports as the loaded charge (<c>ModuleInfo.ChargeTypeId</c>).</summary>
			public required int TypeId { get; init; }
			/// <summary>Exact in-game name; the module's context menu lists charges under it.</summary>
			public required string Name { get; init; }
			/// <summary>Damage one missile deals on a full hit (against our damage type).</summary>
			public required double DamagePerMissile { get; init; }
			public required double ExplosionRadiusM { get; init; }
			public required double ExplosionVelocityMs { get; init; }
			/// <summary>Damage reduction factor — the exponent in the application formula; lower is better.</summary>
			public required double Drf { get; init; }
			/// <summary>Flight range = max velocity × flight time. 0 = unlimited (don't range-gate).</summary>
			public double MaxRangeM { get; init; }
			/// <summary>
			/// Damage type the charge deals — em / th / kin / exp, the NPC DB's resonance keys. Missiles
			/// are single-typed, so this picks which EHP column of the target the charge chews through.
			/// </summary>
			public string DamageType { get; init; } = "kin";
			/// <summary>
			/// Carried only when there is room for it: not part of the readiness gate, and the planner
			/// loads it only after it has SEEN it in the hold — never a blind click on a charge that may
			/// not be aboard.
			/// </summary>
			public bool Optional { get; init; }
		}

		/// <summary>
		/// Which charges a fit carries and the launcher numbers needed to turn them into DPS.
		/// One charge = no switching; the bot then never touches ammo.
		/// </summary>
		public sealed record AmmoPlan
		{
			public required IReadOnlyList<AmmoCharge> Charges { get; init; }
			/// <summary>Charge loaded whenever the room is clear (the one we enter every room with).</summary>
			public required int DefaultChargeTypeId { get; init; }
			/// <summary>Launchers firing as one group.</summary>
			public required int LauncherCount { get; init; }
			/// <summary>Effective cycle time of one launcher, seconds (skills and ship bonuses applied).</summary>
			public required double CycleTimeSec { get; init; }
			/// <summary>
			/// Abyssal weather multiplier on NPC velocity — Dark rooms run rats at ×1.3, which is exactly
			/// what shifts application toward the tighter charge.
			/// </summary>
			public double NpcVelocityMultiplier { get; init; } = 1.0;
			/// <summary>
			/// Ships firing on the same target as us. Kill times shrink with the fleet's DPS, but the 10 s
			/// reload does not, so a swap that pays for a lone Hawk often does not for the trio. 1 = solo.
			/// </summary>
			public double SharedShooters { get; init; } = 1;
			/// <summary>Rounds ONE launcher holds (LML II: 0.795 m³ / 0.015 m³ = 53). Diagnostics only.</summary>
			public int MagazineRounds { get; init; }

			public AmmoCharge Default => Charges.FirstOrDefault(c => c.TypeId == DefaultChargeTypeId) ?? Charges[0];

			/// <summary>
			/// Default first, then the declared order: the queue we fall back along when charges run out.
			/// </summary>
			public IEnumerable<AmmoCharge> FallbackOrder =>
				new[] { Default }.Concat(Charges.Where(c => c.TypeId != DefaultChargeTypeId));
		}

		public int MaxTargetingRange { get; init; }
		public int MaxTargets { get; init; }
		public int MaxDronesInSpace { get; init; }
		/// <summary>
		/// If &gt; 0, the combat core holds a FIXED orbit at this range on the primary for every target
		/// (brawl-in-the-pack, no kite/standoff archetype logic) — for boats that want to sit on the spawn,
		/// e.g. a chain-damage Vorton battlecruiser. 0 = use the per-enemy range archetype.
		/// </summary>
		public int PreferredOrbitMeters { get; init; }
		/// <summary>
		/// Raw incoming DPS the fit shrugs off passively (shield recharge + buffer). The adaptive self-state
		/// turns the resist Hardeners ON only when estimated incoming DPS exceeds this, so trivial spawns are
		/// tanked passively and hardeners fire only when a spawn actually threatens the passive regen.
		/// 0 = hardeners come on as soon as any enemy is present.
		/// </summary>
		public int PassiveRegenDps { get; init; }
		/// <summary>Whether this fit carries a Mobile Tractor Unit and should loot with it.</summary>
		public bool UsesTractor { get; init; }
		/// <summary>
		/// Range within which the weapon is kept firing on the active target. 0 = legacy default (11 km).
		/// Set it to the fit's real applied weapon range (e.g. light-missile flight range on a Hawk).
		/// </summary>
		public int AttackRange { get; init; }
		/// <summary>
		/// Estimated HP one weapon volley removes (grouped launchers count as one volley). 0 = unknown,
		/// so fire-control will not apply the one-ship-per-one-hit rule.
		/// </summary>
		public int VolleyDamage { get; init; }
		/// <summary>
		/// Capacitor model for predictive cap management (null = fall back to a reactive low-cap threshold).
		/// </summary>
		public CapacitorProfile CapProfile { get; init; }
		/// <summary>
		/// Which charges this fit carries and when to swap them (null = the bot never touches ammo).
		/// See <see cref="AmmoController"/>.
		/// </summary>
		public AmmoPlan Ammo { get; init; }
		/// <summary>
		/// Incoming raw DPS this fit's ACTIVE tank sustains indefinitely. Below it the room is not a
		/// threat, so the bot may break off and go collect loot while the guns keep working.
		/// 0 = unknown, and the bot then never treats a room as safe.
		/// </summary>
		public int SustainableIncomingDps { get; init; }
		private ModuleInfo[] High { get; }
		private ModuleInfo[] Mid { get; }
		private ModuleInfo[] Low { get; }

		public ShipFit(Interface.MemoryStruct.IShipUi memoryModules, ModuleInfo[][] fitInfo)
		{
			var modulesByY = memoryModules.ModuleButtons.GroupBy(m => m.UINode.Region?.Min1).OrderBy(g => g.Key).ToArray();
			if (modulesByY.Count() != 3)
				throw new ArgumentException("Couldn't determine 3 module groups");
			High = modulesByY[0].OrderBy(m=>m.UINode.Region.Value.Min0).Select((m, i) =>
			{
				if (fitInfo[0].Length <= i)
					return new ModuleInfo(ModuleType.Etc)
					{
						UiModule = m
					};
				fitInfo[0][i].UiModule = m;
				return fitInfo[0][i];

			}).ToArray();
			Mid = modulesByY[1].OrderBy(m => m.UINode.Region.Value.Min0).Select((m, i) =>
			{
				if (fitInfo[1].Length <= i)
					return new ModuleInfo(ModuleType.Etc)
					{
						UiModule = m
					};
				fitInfo[1][i].UiModule = m;
				return fitInfo[1][i];
			}).ToArray();
			Low = modulesByY[2].OrderBy(m => m.UINode.Region.Value.Min0).Select((m, i) =>
			{
				if (fitInfo[2].Length <= i)
					return new ModuleInfo(ModuleType.Etc)
					{
						UiModule = m
					};
				fitInfo[2][i].UiModule = m;
				return fitInfo[2][i];
			}).ToArray();
		}

		public IEnumerable<ModuleInfo> GetAlwaysActiveModules()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.Hardener || moduleInfo.Type == ModuleType.AlwaysOn)
					yield return moduleInfo;
			}
		}

		public IEnumerable<ModuleInfo> GetShieldBoostersModules()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.ShieldBooster)
					yield return moduleInfo;
			}
		}

		public ModuleInfo GetWeapon()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.Weapon)
					return moduleInfo;
			}

			return null;
		}

		public ModuleInfo GetMWD()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.MWD)
					return moduleInfo;
			}

			return null;
		}

		public IEnumerable<ModuleInfo> GetAllByType(ModuleType type)
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == type)
					yield return moduleInfo;
			}
		}

		public class ModuleInfo
		{
			public ModuleInfo(ModuleType type, params VirtualKeyCode[] hotKey)
			{
				Type = type;
				HotKey = hotKey;
			}

			public ModuleType Type { get; }
			public VirtualKeyCode[] HotKey { get; }

			public ShipUIModuleButton UiModule { get; set; }
			public int OptimalRange = 4000;

			public ISerializableBotTask? EnsureActive(Bot bot, bool shouldBeActive, bool shouldBeOverloaded)
			{
				if (UiModule is null) return null;
				if (shouldBeActive)
				{
					//TODO
					//if (shouldBeOverloaded && !(UiModule.OverloadOn ?? false))
					//	return new ModuleToggleTask(this, VirtualKeyCode.SHIFT);
					// Winding down but wanted on: one click cancels the pending stop.
					if (UiModule.IsDeactivating == true) return new ModuleToggleTask(this, null);
					if (UiModule.AppearsActive) return null;
					// Unknown ramp_active: do not guess-click (would toggle a running module off).
					if (UiModule.IsActive is null) return null;
					return new ModuleToggleTask(this, null);
				}

				// Already winding down: a second click would CANCEL the stop (the stuck booster of
				// 2026-09-26). Let the cycle finish.
				if (UiModule.IsDeactivating == true) return null;
				if (UiModule.AppearsActive)
					return new ModuleToggleTask(this, null);
				//if (!shouldBeOverloaded && (UiModule.OverloadOn ?? false))
				//	return new ModuleToggleTask(this, VirtualKeyCode.SHIFT);

				return null;
			}
		}

		public enum ModuleType
		{
			Hardener,
			Weapon,
			ShieldBooster,
			MWD,
			Etc,
			/// <summary>An active utility module the bot keeps running unconditionally (e.g. a sensor booster or an abyss afterburner).</summary>
			AlwaysOn,
			/// <summary>A charge-fed capacitor booster: the tank logic injects when capacitor runs low.</summary>
			CapBooster,
			/// <summary>
			/// A second, lesser gun kept firing at whatever is locked (the Hawk's 75 mm rail). It costs
			/// nothing to run, so it stays on while we fly to loot instead of idling.
			/// </summary>
			SecondaryWeapon,
		}
	}
}