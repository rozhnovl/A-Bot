﻿using Bib3.Geometrik;
using BotEngine.Common;
using System;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using Bib3;

namespace Sanderling.Parse
{
	public interface IOverviewEntry : MemoryStruct.IOverviewEntry, IListEntry
	{
		MemoryStruct.ISprite MainIcon { set; get; }

		bool? MainIconIsRed { get; }

		bool? IsAttackingMe { get; }

		bool? IsHostile { get; }

		bool? MeTargeted { get; }

		bool? MeTargeting { get; }

		bool? MeActiveTarget { get; }

		EWarTypeEnum[] EWarType { get; }
	}

	public interface IWindowOverview : MemoryStruct.IWindowOverview
	{
		new MemoryStruct.IListViewAndControl<IOverviewEntry> ListView { get; }
	}

	public class OverviewEntry : ListEntry, IOverviewEntry
	{
		MemoryStruct.IOverviewEntry Raw;

		public IEnumerable<string> MainIconSetIndicatorName => Raw?.MainIconSetIndicatorName;

		public MemoryStruct.ISprite[] RightIcon => Raw?.RightIcon;

		public MemoryStruct.ISprite MainIcon { set; get; }

		public bool? MainIconIsRed { set; get; }

		public bool? IsAttackingMe { set; get; }

		public bool? IsHostile { set; get; }

		public bool? MeTargeted { set; get; }

		public bool? MeTargeting { set; get; }

		public bool? MeActiveTarget { set; get; }

		public EWarTypeEnum[] EWarType { set; get; }

		public OverviewEntry()
		{
		}

		public OverviewEntry(MemoryStruct.IOverviewEntry raw)
			:
			base(raw)
		{
			this.Raw = raw;

			MainIcon = raw?.SetSprite?.FirstOrDefault(sprite => sprite?.Name == "iconSprite");

			MainIconIsRed = MainIcon?.Color?.IsRed();

			var MainIconContainsIndicatorWithNameMatchingRegexPattern = new Func<string, bool>(regexPattern =>
				raw?.MainIconSetIndicatorName?.Any(indicatorName => indicatorName.RegexMatchSuccessIgnoreCase(regexPattern)) ?? false);

			IsAttackingMe = MainIconContainsIndicatorWithNameMatchingRegexPattern("attacking.*me");
			IsHostile = MainIconContainsIndicatorWithNameMatchingRegexPattern("hostile");
			MeTargeting = MainIconContainsIndicatorWithNameMatchingRegexPattern("targeting");
			MeTargeted = MainIconContainsIndicatorWithNameMatchingRegexPattern("targetedByMe");
			MeActiveTarget = MainIconContainsIndicatorWithNameMatchingRegexPattern("myActiveTarget");

			EWarType = RightIcon?.Select(OverviewExtension.EWarTypeFromOverviewEntryRightIcon)?.WhereNotNullSelectValue()?.ToArrayIfNotEmpty();
		}
	}

	public class WindowOverview : IWindowOverview
	{
		public MemoryStruct.IWindowOverview Raw;

		public MemoryStruct.IListViewAndControl<IOverviewEntry> ListView { set; get; }

		public IEnumerable<MemoryStruct.IUIElementText> ButtonText => Raw?.ButtonText;

		public string Caption => Raw?.Caption;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public MemoryStruct.ISprite[] HeaderButton => Raw?.HeaderButton;

		public bool? HeaderButtonsVisible => Raw?.HeaderButtonsVisible;

		public long Id => Raw?.Id ?? 0;

		public IEnumerable<MemoryStruct.IUIElementInputText> InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? isModal => Raw?.isModal;

		public IEnumerable<MemoryStruct.IUIElementText> LabelText => Raw?.LabelText;

		public MemoryStruct.Tab[] PresetTab => Raw?.PresetTab;

		public RectInt? Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public IEnumerable<MemoryStruct.ISprite> Sprite => Raw?.Sprite;

		public string ViewportOverallLabelString => Raw?.ViewportOverallLabelString;

		MemoryStruct.IListViewAndControl<MemoryStruct.IOverviewEntry> MemoryStruct.IWindowOverview.ListView => ListView;

		WindowOverview()
		{
		}

		public WindowOverview(MemoryStruct.IWindowOverview raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			ListView = raw?.ListView?.Map(OverviewExtension.Parse);
		}
	}

	static public class OverviewExtension
	{
		static public IWindowOverview Parse(this MemoryStruct.IWindowOverview windowOverview) =>
			null == windowOverview ? null : new WindowOverview(windowOverview);

		static public IOverviewEntry Parse(this MemoryStruct.IOverviewEntry overviewEntry) =>
			null == overviewEntry ? null : new OverviewEntry(overviewEntry);

		static public KeyValuePair<string, EWarTypeEnum>[] SetEWarTypeFromOverviewEntryRightIconHint = new[]
		{
			new KeyValuePair<string, EWarTypeEnum>("jamming.*me", EWarTypeEnum.ECM),
			new KeyValuePair<string, EWarTypeEnum>("warp.*disrupt.*me", EWarTypeEnum.WarpDisrupt),
			new KeyValuePair<string, EWarTypeEnum>("warp.*scramble.*me", EWarTypeEnum.WarpScramble),
			new KeyValuePair<string, EWarTypeEnum>("web.*me", EWarTypeEnum.Web),
		};

		static public EWarTypeEnum? EWarTypeFromOverviewEntryRightIcon(this MemoryStruct.ISprite icon) =>
			null == icon ? (EWarTypeEnum?)null :
			SetEWarTypeFromOverviewEntryRightIconHint?.CastToNullable()?.FirstOrDefault(eWarTypeFromOverviewEntryRightIconHint =>
				icon?.HintText?.RegexMatchSuccessIgnoreCase(eWarTypeFromOverviewEntryRightIconHint?.Key) ?? false)?.Value ?? EWarTypeEnum.Other;
	}
}
