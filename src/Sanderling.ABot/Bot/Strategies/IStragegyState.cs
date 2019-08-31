namespace Sanderling.ABot.Bot.Strategies
{
	internal interface IStragegyState
	{
		IBotTask GetStateActions(Bot bot);
		IBotTask GetStateExitActions(Bot bot);
		bool MoveToNext { get; }
	}
}