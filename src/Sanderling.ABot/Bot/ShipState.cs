using System;
using System.Linq;
using JetBrains.Annotations;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Parse;
using Sanderling.Interface.MemoryStruct;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;

namespace Sanderling.ABot.Bot
{
	public class ShipState
	{
		private readonly Bot bot;
		private readonly IMemoryMeasurement memory;

		public ShipState(ShipFit fit, Bot bot)
		{
			this.bot = bot;
			this.memory = bot.MemoryMeasurementAtTime.Value;
			Maneuver = (memory?.ShipUi?.Indication?.LabelText?.Any(lt => lt.Text == "Keeping at Range") ?? false)
				? ShipManeuverTypeEnum.KeepAtRange
				: memory.ShipUi.Indication.ManeuverType?? ShipManeuverTypeEnum.None;
			ActiveTargets = new ActiveTargetsContoller(bot, memory);
			Fit = fit;
			Drones = new DronesContoller(memory);
		}

		public ShipFit Fit { get; }

		public bool ManeuverStartPossible => memory.ManeuverStartPossible();
		[NotNull] public IShipHitpointsAndEnergy HitpointsAndEnergy => memory.ShipUi.HitpointsAndEnergy;

		public ShipManeuverTypeEnum Maneuver { get; }

		public DronesContoller Drones { get; }

		public ActiveTargetsContoller ActiveTargets { get; }
		public bool IsInAbyss => !memory.InfoPanelCurrentSystem.HeaderText.Contains("Maurasi");

		public ISerializableBotTask GetTurnOnAlwaysActiveModulesTask()
		{
			return Fit.GetAlwaysActiveModules().Select(m => m.EnsureActive(bot, true))
				.FirstOrDefault(t => t != null);
		}

		public ISerializableBotTask GetSetModuleActiveTask(ShipFit.ModuleType type, bool shouldBeActive)
		{
			switch (type)
			{
				case ShipFit.ModuleType.Weapon:
					var weaponGroup = Fit.GetWeapon();
					if (!weaponGroup.UiModule.IsReloading(bot))
					{
						return weaponGroup.EnsureActive(bot, shouldBeActive);
					}

					break;
				case ShipFit.ModuleType.Hardener:
				case ShipFit.ModuleType.ShieldBooster:
				case ShipFit.ModuleType.MWD:
				case ShipFit.ModuleType.Etc:
					return Fit.GetAllByType(type).Select(m => m.EnsureActive(bot, shouldBeActive))
						.FirstOrDefault(t => t != null);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			return null;
		}

		public ISerializableBotTask GetNextTankingModulesTask(double estimatedIncomingDps)
		{
			DynamicTask task = new DynamicTask();
			var shieldBoosters = Fit.GetShieldBoostersModules().ToList();
			if (HitpointsAndEnergy.Shield < 600)
			{
				if (estimatedIncomingDps <= 150)
				{
					task.With($"Incoming DPS is {estimatedIncomingDps}. Turning on single SB");
					return shieldBoosters.FirstOrDefault().EnsureActive(bot, true)
					       ?? shieldBoosters.Skip(1).Select(sb => sb.EnsureActive(bot, false))
						       .FirstOrDefault(t => t != null);
				}
				else
				{
					return shieldBoosters.Select(sb => sb.EnsureActive(bot, true))
						.FirstOrDefault(t => t != null);
				}
			}
			else
			{
				if (HitpointsAndEnergy.Shield > 800 || HitpointsAndEnergy.Capacitor < 400)
				{
					task.With("Shield HP OK, energy low, turning off SB.");
					foreach (var sb in shieldBoosters)
					{
						var t = sb.EnsureActive(bot, false);
						if (t != null) return t;
					}
				}
			}

			return null;
		}

		public ISerializableBotTask GetReloadTask()
		{
			var weaponGroup = Fit.GetWeapon();

			if (!weaponGroup.UiModule.IsReloading(bot))
			{
				bool shouldReload = int.Parse(weaponGroup.UiModule.ModuleButtonQuantity) != 20;
				if (shouldReload)
					return weaponGroup.UiModule.ClickMenuEntryByRegexPattern(bot, "Reload all");
			}

			return null;
		}

		public ISerializableBotTask GetPopupButtonTask(string buttonText)
		{
			return memory?.WindowOther?.SelectMany(w => w.ButtonText)
				?.FirstOrDefault(bt => bt != null ? (bt.Text == buttonText) : false).ClickTask();
		}
	}
}