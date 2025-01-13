using System;
using System.Linq;
using Bib3.Geometrik;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using Bib3;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	public interface IShipUiTarget : MemoryStruct.IShipUiTarget
	{
		string[] TextRow { get; }
	}

	public class ShipUiTarget : IShipUiTarget
	{
		MemoryStruct.IShipUiTarget Raw;

		public int? Distance { set; get; }

		public string[] TextRow { set; get; }

		public MemoryStruct.ShipUiTargetAssignedGroup[] Assigned => Raw?.Assigned;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public MemoryStruct.IShipHitpointsAndEnergy Hitpoints => Raw?.Hitpoints;

		public long Id => Raw?.Id ?? 0;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? IsSelected => Raw?.IsSelected;

		public string[] LabelText => Raw?.LabelText;

		public RectInt? Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		ShipUiTarget()
		{
		}

		public ShipUiTarget(MemoryStruct.IShipUiTarget raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			var TextRow =
				raw?.LabelText
				?.Select(labelText => labelText?.RemoveXmlTag())
				?.ToArray();

			Distance = raw.Distance;

			this.TextRow = TextRow?.Reverse()?.Skip(1)?.Reverse()?.ToArray();
		}
	}

	public interface IShipUi : MemoryStruct.IShipUi
	{
		new IShipUIIndication? Indication { get; }
	}

	public interface IShipUiIndication : IContainer
	{
		ShipManeuverTypeEnum? ManeuverType { get; }
	}

	public class ShipUiIndication : Container, IShipUiIndication
	{
		public ShipManeuverTypeEnum? ManeuverType { private set; get; }

		ShipUiIndication()
		{
		}

		public ShipUiIndication(IUIElement raw)
			:
			base(raw)
		{
			ManeuverType =
				LabelText?.OrderBy(label => label.RegionCenter()?.B)?.FirstOrDefault()?.Text?.ManeuverTypeFromShipUiIndicationText();
		}
	}

	public partial class ShipUi : IShipUi
	{
		public MemoryStruct.IShipUi Raw { private set; get; }

		public IShipUIIndication? Indication { private set; get; }

		Int64? SpeedMilliParsed { set; get; }

		ShipUi()
		{
		}

		public ShipUi(MemoryStruct.IShipUi raw)
		{
			Raw = raw;

			Indication = Raw?.Indication;

			SpeedMilliParsed = Raw?.SpeedLabel?.Text?.RegexMatchIfSuccess("(" + Number.DefaultNumberFormatRegexPatternAllowLeadingAndTrailingChars + @")\s*m/s")?.Groups[1]?.Value?.NumberParseDecimalMilli();
		}
	}

	public partial class ShipUi
	{
		public IEnumerable<IUIElementText> ButtonText => Raw?.ButtonText;

		public IEnumerable<IUIElementInputText> InputText => Raw?.InputText;

		public IEnumerable<IUIElementText> LabelText => Raw?.LabelText;

		public IEnumerable<ISprite> Sprite => Raw?.Sprite;

		public RectInt? Region => Raw?.Region ?? default(RectInt);

		public int? InTreeIndex => Raw?.InTreeIndex;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public IUIElement RegionInteraction => Raw?.RegionInteraction ?? null;

		public long Id => Raw?.Id ?? 0;

		public IUIElement Center => Raw?.Center;

		public IShipHitpointsAndEnergy HitpointsAndEnergy => Raw?.HitpointsAndEnergy;

		public IUIElementText SpeedLabel => Raw?.SpeedLabel;

		public ShipUiEWarElement[] EWarElement => Raw?.EWarElement;

		public IUIElement ButtonSpeed0 => Raw?.ButtonSpeed0;

		public IUIElement ButtonSpeedMax => Raw?.ButtonSpeedMax;
		public List<ShipUIModuleButton> ModuleButtons => Raw.ModuleButtons;
		public ModuleButtonsRows ModuleButtonsRows => Raw.ModuleButtonsRows;

		public IUIElementText[] Readout => Raw?.Readout;

		public long? SpeedMilli => Raw?.SpeedMilli ?? SpeedMilliParsed;

		public IShipUiTimer[] Timer => Raw?.Timer;

		public ISquadronsUI SquadronsUI => Raw?.SquadronsUI;
	}

	static public class ShipUiExtension
	{
		static public IShipUiTarget Parse(this MemoryStruct.IShipUiTarget shipUiTarget) =>
			null == shipUiTarget ? null : new ShipUiTarget(shipUiTarget);

		static public IShipUi Parse(this MemoryStruct.IShipUi raw) =>
			null == raw ? null : new ShipUi(raw);

		static public string ShipManeuverTypeIndicationRegexPattern(this ShipManeuverTypeEnum type) =>
			Regex.Replace(type.ToString(), "[A-Z]", new MatchEvaluator(match => @"\s*" + match.Value));

		static readonly KeyValuePair<string, ShipManeuverTypeEnum>[] ShipManeuverTypeFromIndicationTextRegexPattern =
			Bib3.Extension.EnumGetValues<ShipManeuverTypeEnum>().Except(new[] { ShipManeuverTypeEnum.None })
			.Select(typeEnum => new KeyValuePair<string, ShipManeuverTypeEnum>(
				typeEnum.ShipManeuverTypeIndicationRegexPattern(), typeEnum)).ToArray();

		static public IShipUiIndication ParseAsShipUiIndication(this IContainer container) =>
			null == container ? null :
			new ShipUiIndication(container);

		static public ShipManeuverTypeEnum? ManeuverTypeFromShipUiIndicationText(this string indicationText) =>
			ShipManeuverTypeFromIndicationTextRegexPattern.CastToNullable()
			.FirstOrDefault(patternAndType => indicationText?.RegexMatchSuccessIgnoreCase(patternAndType?.Key) ?? false)?.Value;
	}
}
