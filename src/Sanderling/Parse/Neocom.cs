using System;
using System.Linq;
using Bib3.Geometrik;
using BotEngine.Common;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;

namespace Sanderling.Parse
{
	public interface INeocom : MemoryStruct.INeocom
	{
		MemoryStruct.IUIElement PeopleAndPlacesButton { get; }

		MemoryStruct.IUIElement ChatButton { get; }

		MemoryStruct.IUIElement MailButton { get; }

		MemoryStruct.IUIElement FittingButton { get; }

		MemoryStruct.IUIElement InventoryButton { get; }

		MemoryStruct.IUIElement MarketButton { get; }

	}

	public class Neocom : INeocom
	{
		public MemoryStruct.INeocom Raw { private set; get; }

		public MemoryStruct.IUIElement PeopleAndPlacesButton => Raw?.PeopleAndPlacesButton;

		public MemoryStruct.IUIElement ChatButton => Raw?.ChatButton;

		public MemoryStruct.IUIElement MailButton => Raw?.MailButton;

		public MemoryStruct.IUIElement FittingButton => Raw?.FittingButton;

		public MemoryStruct.IUIElement MarketButton => Raw?.MarketButton;

		public MemoryStruct.IUIElement InventoryButton => Raw?.InventoryButton;

		Neocom()
		{ }

		public Neocom(MemoryStruct.INeocom raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			//var ButtonWithTexturePathMatch = new Func<string, MemoryStruct.IUIElement>(texturePathRegexPattern =>
			//	raw?.Button?.FirstOrDefault(candidate => candidate?.TexturePath?.RegexMatchSuccess(texturePathRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false));

			//PeopleAndPlacesButton = ButtonWithTexturePathMatch("peopleandplaces");

			//ChatButton = ButtonWithTexturePathMatch("chat");

			//MailButton = ButtonWithTexturePathMatch("mail");

			//FittingButton = ButtonWithTexturePathMatch("fitting");

			////InventoryButton = ButtonWithTexturePathMatch("items");

			//MarketButton = ButtonWithTexturePathMatch("market");
		}
	}
}
