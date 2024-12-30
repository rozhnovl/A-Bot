using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Sanderling.ABot.Bot.Task;
using Sanderling.Parse;
using IShipUiTarget = Sanderling.Parse.IShipUiTarget;

namespace Sanderling.ABot.Bot
{
	public class SimpleTargetInfo : ITarget
	{
		private readonly Bot bot;
		[NotNull] private readonly IShipUiTarget memoryTarget;
		[NotNull] private static readonly Regex DistanceRegex = new Regex(" (\\d+\\,)?(\\d)+ (m|km)", RegexOptions.Compiled);
		public SimpleTargetInfo(Bot bot, [NotNull] IShipUiTarget memoryTarget)
		{
			this.bot = bot;
			this.memoryTarget = memoryTarget;
			Distance = (int) memoryTarget.DistanceMax;
			AssignedEffectsCount = memoryTarget.Assigned?.Length ?? 0;
			DroneAssigned = memoryTarget.Assigned?.Any(a => a.IconTexture == null) ??false;
			WeaponAssigned = memoryTarget.Assigned?.Any(a => a.IconTexture != null) ?? false;
			if (memoryTarget.LabelText == null)
				Name = string.Empty;
			else
			{
				var splittedName = memoryTarget.LabelText.Select(lt => lt.Text.Replace("<center>", string.Empty));
				Name = string.Join(" ", splittedName);
				if (Name.Contains("["))
					Name = Name.Substring(0, Name.IndexOf("["));
				Name = DistanceRegex.Replace(Name, string.Empty);

			}
		}

		public bool WeaponAssigned { get; }

		public bool DroneAssigned { get; }

		public int Distance { get; }
		public int AssignedEffectsCount { get; }
		public string Name { get; }

		public ISerializableBotTask GetUnlockTask()
		{
			return memoryTarget.ClickWithModifier(bot, HotkeyRegistry.UnlockTargetModifier);
		}
	}
}