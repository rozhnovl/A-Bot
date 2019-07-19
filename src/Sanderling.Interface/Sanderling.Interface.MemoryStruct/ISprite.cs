using BotEngine;
using BotEngine.Interface;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISprite : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		ColorORGB Color
		{
			get;
		}

		string Name
		{
			get;
		}

		IObjectIdInMemory Texture0Id
		{
			get;
		}

		string TexturePath
		{
			get;
		}

		string HintText
		{
			get;
		}
	}
}
