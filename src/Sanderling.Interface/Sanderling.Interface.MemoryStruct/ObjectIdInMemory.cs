using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class ObjectIdInMemory : ObjectIdInt64, IObjectIdInMemory, IObjectIdInt64
	{
		private ObjectIdInMemory()
		{
		}

		public ObjectIdInMemory(IObjectIdInt64 @base)
			: base(@base)
		{
		}

		public ObjectIdInMemory(long id)
			: base(id)
		{
		}
	}
}
