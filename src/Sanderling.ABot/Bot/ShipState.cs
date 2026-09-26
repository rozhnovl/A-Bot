using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Parse;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;

namespace Sanderling.ABot.Bot
{
	public class ShipState : IShipState
	{
		private readonly Bot bot;
		private readonly IMemoryMeasurement memory;

		public ShipState(ShipFit fit, Bot bot)
		{
			this.bot = bot;
			this.memory = bot.MemoryMeasurementAtTime.Value;
			Maneuver = memory?.ShipUi?.Indication?.ManeuverType ?? ShipManeuverType.None;
			//TODO
			/*Maneuver = (memory?.ShipUi?.Indication?.LabelText?.Any(lt => lt.Text == "Keeping at Range") ?? false)
				? ShipManeuverTypeEnum.KeepAtRange
				: memory.ShipUi.Indication.ManeuverType?? ShipManeuverTypeEnum.None;*/
			ActiveTargets = new ActiveTargetsContoller(bot, memory);
			Fit = fit;
			Drones = new DronesContoller(memory, fit);
		}

		public ShipFit Fit { get; }

		public bool ManeuverStartPossible => memory.ManeuverStartPossible();

		public ShipManeuverType Maneuver { get; }

		public DronesContoller Drones { get; }

		public ActiveTargetsContoller ActiveTargets { get; }
		public int AttackRange => Fit.AttackRange > 0 ? Fit.AttackRange : 11000;
		public bool ShouldUseTractorForLooting => Fit.UsesTractor;
		public bool IsInAbyss => !memory.InfoPanelContainer.LocationInfo.CurrentSolarSystemName?.Contains("Maurasi") ?? true;

		public ISerializableBotTask? GetTurnOnAlwaysActiveModulesTask()
		{
			return Fit.GetAlwaysActiveModules().Select(m => m.EnsureActive(bot, true,false))
				.FirstOrDefault(t => t != null);
		}

		public ISerializableBotTask? GetSetModuleActiveTask(ShipFit.ModuleType type, bool shouldBeActive)
		{
			switch (type)
			{
				case ShipFit.ModuleType.Weapon:
					var weaponGroup = Fit.GetWeapon();
					if (!weaponGroup.UiModule.IsBusy)
					{
						return weaponGroup.EnsureActive(bot, shouldBeActive, false);
					}

					break;
				case ShipFit.ModuleType.Hardener:
				case ShipFit.ModuleType.ShieldBooster:
				case ShipFit.ModuleType.MWD:
				case ShipFit.ModuleType.Etc:
				case ShipFit.ModuleType.AlwaysOn:
				case ShipFit.ModuleType.CapBooster:
				case ShipFit.ModuleType.SecondaryWeapon:
					return Fit.GetAllByType(type).Select(m => m.EnsureActive(bot, shouldBeActive, false))
						.FirstOrDefault(t => t != null);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			return null;
		}

		public ISerializableBotTask? GetAttackTasks()
		{
			var focusedTarget = ActiveTargets.ActiveTarget;
			if (focusedTarget == null)
				return null;
			ISerializableBotTask? weaponTask = GetSetModuleActiveTask(ShipFit.ModuleType.Weapon,
				ShouldFireAt(focusedTarget));
			if (weaponTask != null)
				return weaponTask;
			else
			{
				//TODO should be checking if already orbiting speicifc selected target
				if (Fit.GetAllByType(ShipFit.ModuleType.Weapon).Any(w => focusedTarget.Distance > w.OptimalRange) &&
				    Maneuver != ShipManeuverType.Orbit)
					return focusedTarget.GetOrbitTask();
			}

			foreach (var t in Drones.GetDronesAttackTasks(focusedTarget))
				return t;
			return null;
		}

		/// <summary>
		/// Hawk missiles apply to 60 km and F1 starts a repeat — turning them off on a range
		/// threshold just flaps the launcher. Keep them running on any live target.
		/// </summary>
		private bool ShouldFireAt(ITarget focusedTarget)
		{
			if (NpcInfoProvider.IsWreckName(focusedTarget.Name))
				return Fit.GetWeapon()?.UiModule?.AppearsActive == true;
			return true;
		}

		/// <summary>Shield charge 0–100%, from the ship UI gauge the Eve64 parser fills. Null = unreadable.</summary>
		public int? ShieldPercent => memory?.ShipUi?.HitpointsPercent?.Shield;

		/// <summary>Capacitor charge 0–100%, from the pmark gauge. Null = unreadable.</summary>
		public int? CapacitorPercent => memory?.ShipUi?.Capacitor?.LevelFromPmarksPercent;

		// NOTE: the Eve64 parser does NOT populate the legacy HitpointsAndEnergy — shield and cap
		// must be read via HitpointsPercent / Capacitor.LevelFromPmarksPercent (0–100 percent).
		public ISerializableBotTask GetNextTankingModulesTask(double estimatedIncomingDps, double enemyNeutGjPerSec = 0)
		{
			// Cap first: an active tank without capacitor is a dead ship. Inject well before the
			// boosters would stall; the module reloads itself from cargo charges between injections.
			var capBooster = Fit.GetAllByType(ShipFit.ModuleType.CapBooster).FirstOrDefault();
			if (capBooster != null && ShouldInjectCap(enemyNeutGjPerSec))
			{
				var injectTask = capBooster.EnsureActive(bot, true, false);
				if (injectTask != null) return injectTask;
			}

			var shieldBoosters = Fit.GetShieldBoostersModules().ToList();
			if (shieldBoosters.Count == 0)
				return null;
			// Unreadable gauge: do nothing rather than toggle modules on a parser hiccup.
			if (ShieldPercent is not { } shield)
				return null;
			// Boost-on threshold scales with pressure: under heavy incoming DPS one volley carves
			// 50+ shield points between two bot ticks (Lucifer Dramiels took a Hawk 80%→28% in ~7 s),
			// so start boosting at 80% instead of waiting for 60%. Hand-played runs pre-boost the
			// same way. The off threshold rises with it to keep the hysteresis band.
			var heavyPressure = estimatedIncomingDps > 100;
			var boostOnBelow = heavyPressure ? 80 : 60;
			var boostOffAbove = heavyPressure ? 92 : 85;
			if (shield < boostOnBelow)
			{
				if (estimatedIncomingDps <= 150)
				{
					// Light pressure: one booster keeps up; make sure the rest are off to save cap.
					return shieldBoosters.FirstOrDefault().EnsureActive(bot, true, false)
					       ?? shieldBoosters.Skip(1).Select(sb => sb.EnsureActive(bot, false, false))
						       .FirstOrDefault(t => t != null);
				}
				else
				{
					var activateSbTask = shieldBoosters
						.Select(sb => sb.EnsureActive(bot, true, shield < 15))
						.FirstOrDefault(t => t != null);
					if (activateSbTask != null)
						return activateSbTask;
				}
			}
			else
			{
				// Shield comfortable again, or cap critically low even after injections (charges out):
				// shut the boosters down. The on/off gap is hysteresis so boosters don't flap.
				if (shield > boostOffAbove || CapacitorPercent < 20)
				{
					foreach (var sb in shieldBoosters)
					{
						var t = sb.EnsureActive(bot, false, false);
						if (t != null) return t;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Predictive cap-injection decision. With a <see cref="CapacitorProfile"/> on the fit, projects
		/// net drain (running boosters + prop + enemy neuts − regen at the current charge level) and
		/// injects when the projection reaches the reserve floor within the lead time — so the charge
		/// lands BEFORE the tank stalls, not after. Without a profile, falls back to the reactive
		/// threshold observed in hand-played runs (inject around 45%).
		/// </summary>
		private bool ShouldInjectCap(double enemyNeutGjPerSec)
		{
			// Unreadable capacitor => assume full rather than burning charges on a parser hiccup.
			var capPercent = CapacitorPercent ?? 100;
			var profile = Fit.CapProfile;
			if (profile == null)
				return capPercent < 45;

			var fraction = capPercent / 100.0;
			var capGj = fraction * profile.TotalGj;
			enemyNeutGjPerSec *= profile.NeutResistanceFactor;

			// Don't waste a charge topping a near-full capacitor — unless someone is actively neuting us.
			if (profile.TotalGj - capGj < profile.InjectionGj * 0.6 && enemyNeutGjPerSec <= 0)
				return false;

			var floorGj = profile.TotalGj * profile.FloorFraction;
			if (capGj <= floorGj)
				return true;

			// Under ACTIVE neut pressure don't trust the projection alone: neut volleys land in
			// spikes the lead-time model can't see between ticks (a Lucifer Cynabal took the cap
			// 84%→0 in ~30 s while the projection still said "fine"). Keep a half-full buffer.
			if (enemyNeutGjPerSec > 0 && capPercent < 50)
				return true;

			var boostersRunning = Fit.GetShieldBoostersModules().Count(sb => sb.UiModule?.IsActive ?? false);
			var propRunning = Fit.GetAllByType(ShipFit.ModuleType.AlwaysOn)
				.Concat(Fit.GetAllByType(ShipFit.ModuleType.MWD))
				.Any(m => m.UiModule?.IsActive ?? false);
			var drainGjPerSec = boostersRunning * profile.BoosterGjPerSec
			                    + (propRunning ? profile.PropGjPerSec : 0)
			                    + enemyNeutGjPerSec
			                    - profile.RegenGjPerSec(fraction);
			if (drainGjPerSec <= 0)
				return false; // net positive — regen outruns the tank, no charge needed

			var secondsToFloor = (capGj - floorGj) / drainGjPerSec;
			return secondsToFloor < profile.LeadTimeSec;
		}

		/// <summary>
		/// Top the launchers up when they are not carrying a full load. Called in the quiet moment after
		/// a room is clear — the operator's rule: never take the gate on a partial magazine, because the
		/// next room starts with whatever is loaded now.
		/// </summary>
		public ISerializableBotTask? GetReloadTask()
		{
			var weapon = Fit.GetWeapon()?.UiModule;
			var info = weapon?.ModuleInfo;
			if (weapon?.UINode == null || info == null) return null;
			if (weapon.IsBusy) return null;
			if (info.MaxCharges <= 0 || info.ChargeCount >= info.MaxCharges) return null;

			return weapon.UINode.ClickMenuEntryByRegexPattern(bot, "Reload.*");
		}

		/// <summary>
		/// Perform an action through the Selected Item panel instead of the overview's right-click menu
		/// (the operator's preferred path for the gate: left-click the row, then the panel button).
		/// Returns the select-click while the panel still shows something else, then the button click.
		/// Null when the panel has no such button — the caller can fall back to the context menu.
		/// </summary>
		public ISerializableBotTask? GetSelectedItemActionTask(IOverviewEntry entry, string buttonNamePattern)
		{
			if (entry?.SelectElement == null) return null;

			var panel = memory?.WindowSelectedItemView?.FirstOrDefault();
			var shownName = panel?.SelectedItemName;
			var wanted = entry.Name ?? entry.Type ?? "";

			// Panel not on this object yet → select it with a plain left click.
			if (string.IsNullOrEmpty(shownName) || !FleetFireBoard.NamesMatch(shownName, wanted))
				return entry.SelectElement.ClickTask();

			var button = panel?.ActionButtons?
				.FirstOrDefault(kv => Regex.IsMatch(kv.Key, buttonNamePattern, RegexOptions.IgnoreCase)).Value;
			return button?.ClickTask();
		}

		public ISerializableBotTask GetPopupButtonTask(string buttonText)
		{
			// Match by case-insensitive Contains, not exact: the abyss activation popup button reads
			// "Activate for fleet" for frigates (not a bare "Activate"), and casing/spacing varies.
			return memory?.WindowOther?.SelectMany(w => w.ButtonText)
				?.FirstOrDefault(bt => bt?.Text != null &&
				                       bt.Text.Contains(buttonText, System.StringComparison.OrdinalIgnoreCase))
				?.ClickTask();
		}
	}
}