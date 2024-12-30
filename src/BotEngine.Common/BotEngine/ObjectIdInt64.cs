namespace BotEngine;

public class ObjectIdInt64 : IObjectIdInt64
{
	public long Id { get; set; }

	public ObjectIdInt64()
	{
	}

	public ObjectIdInt64(long id)
	{
		Id = id;
	}

	public ObjectIdInt64(IObjectIdInt64 @base)
		: this(@base?.Id ?? 0)
	{
	}
}
