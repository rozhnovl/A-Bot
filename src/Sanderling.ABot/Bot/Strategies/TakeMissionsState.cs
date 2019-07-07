using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	class TakeMissionsState : IStragegyState
	{
		private ISet<string> AcceptedMissionLevels = new HashSet<string>() { "Level 3" , "Level 4" };
		private ISet<string> IgnoredMissions = new HashSet<string>(){ "Gate Blitz", "Uproot" };
		private ISet<string> CheckedAgents = new HashSet<string>();
		private Regex MissionNameRegex = new Regex("<span id=subheader>([a-zA-Z0-9\\s]*)</span>");
		private bool allMissionsProcessed;
		public IBotTask GetStateActions(Bot bot)
		{
			var memory = bot?.MemoryMeasurementAtTime?.Value;

			if (memory.WindowAgentDialogue?.Any() ?? false)
			{
				var agentDialogue = memory.WindowAgentDialogue.FirstOrDefault();
				if (agentDialogue.ButtonText.Any(bt => bt.Text == "Request Mission" || bt.Text == "View Mission"))
					return agentDialogue.ButtonText
						.First(bt => bt.Text == "Request Mission" || bt.Text == "View Mission")
						.ClickTask();
				var missionDescription = ((WindowAgentDialogue) agentDialogue.RegionInteraction).LeftPane.Html;
				var missionTarget = ((WindowAgentDialogue) agentDialogue.RegionInteraction).RightPane.Html;
				if (!MissionNameRegex.IsMatch(missionDescription))
				{
					throw new InvalidOperationException("Failed to parse mission name");
				}

				var acceptButton = agentDialogue?.ButtonText?.FirstOrDefault(b => b.Text == "Accept");
				if (acceptButton != null)
				{
					var missionName = MissionNameRegex.Match(missionDescription).Groups[1].Value;
					if (!IgnoredMissions.Contains(missionName))
					{
						return acceptButton.ClickTask();
					}
					else if (!missionDescription.Contains("Declining a mission from this agent within the next"))
						return agentDialogue.ButtonText
							.First(bt => bt.Text == "Decline")
							.ClickTask();
				}

				var agentNameWithLevel = agentDialogue.Caption.Substring(agentDialogue.Caption.IndexOf('-'));
				CheckedAgents.Add(agentNameWithLevel.Substring(2, agentNameWithLevel.IndexOf('<') - 3));
				return agentDialogue.ClickMenuEntryByRegexPattern(bot, ".*Close");
			}

			//check if can speak to another
			var agentsToCheck = memory.WindowStation.Single().AgentEntry.Where(a =>
				a.LabelText.Any(lt => AcceptedMissionLevels.Any(aml => lt.Text.Contains(aml))));
			var notCheckedYetAgents =
				agentsToCheck
					.Where(a => a.LabelText.Any(t => t.Text == "Security"))
					.Where(a => !a.LabelText.Any(t => CheckedAgents.Any(ca => t.Text.Contains(ca))));
			var agentToTalk = notCheckedYetAgents.FirstOrDefault();
			if (agentToTalk != null)
				return agentToTalk.DoubleClickTask();
			allMissionsProcessed = true;
			return null;
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var fittingWindow = bot.MemoryMeasurementAtTime?.Value?.WindowPeopleAndPlaces?.FirstOrDefault();
			if (fittingWindow != null)
				return new BotTask()
				{
					ClientActions = new[]
					{
						bot.MemoryMeasurementAtTime?.Value?.Neocom?.PeopleAndPlacesButton?.MouseClick(
							MouseButtonIdEnum
								.Left)
					}
				};
			return null;
		}

		public bool MoveToNext => allMissionsProcessed;
	}
}