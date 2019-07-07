using System.Collections.Generic;
using BotEngine.Motor;
using JetBrains.Annotations;
using Sanderling.Accumulation;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class ModuleToggleTask : SimpleBotTask
	{
		public readonly IShipUiModule module;
		
		public override IEnumerable<MotionParam> ClientActions
		{
			get
			{
				var toggleKey = module?.TooltipLast?.Value?.ToggleKey;

				if (0 < toggleKey?.Length)
					yield return toggleKey?.KeyboardPressCombined();

				yield return module?.MouseClick(MouseButtonIdEnum.Left);
			}
		}

		public ModuleToggleTask(Bot bot, [NotNull] IShipUiModule module) : base(bot)
		{
			this.module = module;
		}
	}
}