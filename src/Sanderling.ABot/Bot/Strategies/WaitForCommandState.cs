using System.Linq;
using SlackAPI;

namespace Sanderling.ABot.Bot.Strategies
{
	class WaitForCommandState : IStragegyState
	{
		private SlackClient slack;

		public WaitForCommandState(string messageToSend)
		{
			slack = new SlackClient("https://hooks.slack.com/services/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");
			slack.GetChannelList(_ => { });
			var channelId = slack.Channels.FirstOrDefault().id;
			slack.PostMessage(pr => { }, channelId, "testMessageV2", "EveBot1", blocks: new[]
			{
				new ActionsBlock()
				{

					block_id = "t1", elements = new[]
					{
						new ButtonElement() {action_id = "hooray", text = new Text(){text = "Hooray"}},
					}
				}
			});
		}

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