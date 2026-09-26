using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Sanderling.ABot.Bot.Task;
using IShipUiTarget = Sanderling.Parse.IShipUiTarget;

namespace Sanderling.ABot.Bot
{
	public class SimpleTargetInfo : ITarget
	{
		private readonly Bot bot;
		[NotNull] private readonly IShipUiTarget memoryTarget;

		[NotNull] private static readonly Regex DistanceRegex =
			new Regex(" (\\d+\\,)?(\\d)+ (m|km)", RegexOptions.Compiled);

		public SimpleTargetInfo(Bot bot, [NotNull] IShipUiTarget memoryTarget)
		{
			this.bot = bot;
			this.memoryTarget = memoryTarget;
			// A just-locked target can briefly have no parsed distance — treat it as "far" for one
			// tick instead of throwing and killing the whole bot step.
			Distance = memoryTarget.Distance ?? int.MaxValue;
			// Assigned icons are not parsed by Eve64 yet — these stay 0/false until the parser
			// learns them; nothing may rely on them to gate combat behaviour.
#pragma warning disable CS0618
			AssignedEffectsCount = memoryTarget.Assigned?.Length ?? 0;
			DroneAssigned = memoryTarget.Assigned?.Any(a => a.IconTexture == null) ?? false;
			WeaponAssigned = memoryTarget.Assigned?.Any(a => a.IconTexture != null) ?? false;
#pragma warning restore CS0618
			if (memoryTarget.LabelText == null)
				Name = string.Empty;
			else
			{
				var splittedName = memoryTarget.LabelText.Select(lt => lt.Replace("<center>", string.Empty));
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

		public double? RemainingHitpointsFraction
		{
			get
			{
				var hp = memoryTarget.Hitpoints;
				if (hp == null) return null;
				var layers = new[] { hp.Shield, hp.Armor, hp.Struct }.Where(v => v.HasValue).Select(v => v!.Value).ToArray();
				if (layers.Length == 0) return null;
				return Math.Clamp(layers.Average() / 1000.0, 0, 1);
			}
		}

		public ISerializableBotTask GetUnlockTask()
		{
			return memoryTarget.ClickWithModifier(bot, HotkeyRegistry.UnlockTargetModifier);
		}

		public ISerializableBotTask GetOrbitTask()
		{
			return memoryTarget.ClickWithModifier(bot, HotkeyRegistry.OrbitModifier);
		}
	}
}