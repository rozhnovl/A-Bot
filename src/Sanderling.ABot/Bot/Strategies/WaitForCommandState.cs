namespace Sanderling.ABot.Bot.Strategies
{
	class WaitForCommandState : IStragegyState
	{
		public IBotTask GetStateActions(Bot bot)
		{
			//TODO Check command server if anything required
			return null;
		}

		public IBotTask GetStateExitActions(Bot bot) => null;

		public IStragegyState NextState { get; private set; }
		public bool MoveToNext => NextState != null;
	}
}