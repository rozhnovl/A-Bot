using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUiModule : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		bool? ModuleButtonVisible
		{
			get;
		}

		IObjectIdInMemory ModuleButtonIconTexture
		{
			get;
		}

		string ModuleButtonQuantity
		{
			get;
		}

		bool RampActive
		{
			get;
		}

		int? RampRotationMilli
		{
			get;
		}

		bool? HiliteVisible
		{
			get;
		}

		bool? GlowVisible
		{
			get;
		}

		bool? BusyVisible
		{
			get;
		}

		bool? OverloadOn
		{
			get;
		}
	}
}
