using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WindowsInput.Native;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class TakeMissionsState : IStragegyState
	{
		private ISet<string> AcceptedMissionLevels = new HashSet<string>() { "Level 4", "Level 3" };

		private ISet<string> IgnoredMissions = new HashSet<string>()
		{
			"Gate Blitz", "Uproot", "Morale and Morality", "Roidier Rage", "Roidiest Rage", "Tightening the Noose",
			"Cutting the Net", "Shades of Grey", "Chain Reaction", "Supply Interdiction"
		};
		private ISet<string> IgnoredSystems = new HashSet<string>()
		{
			"Harroule", "Jovainnon", "Muetralle", "Loes", "Indregulle"
		};

		private ISet<string> CheckedAgents = new HashSet<string>();
		private Regex MissionNameRegex = new Regex("<span id=subheader>([a-zA-Z0-9\\s]*)</span>");
		private Regex MissionLocationRegex = new Regex(">([a-zA-Z\\s]*)</a> <font color=#E3170D>\\(Low Sec Warning!\\)</font>");
		private Regex AgentStandingRegex = new Regex("Effective Standing: ([0-9]?.?[0-9]?)");
		private bool allMissionsProcessed;
		private bool scrolledToBottom;

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

				var agentNameWithLevel = agentDialogue.Caption.Substring(agentDialogue.Caption.IndexOf('-'));
				var agentName = agentNameWithLevel.Substring(2, agentNameWithLevel.IndexOf('<') - 3);

				var acceptButton = agentDialogue?.ButtonText?.FirstOrDefault(b => b.Text == "Accept");
				if (acceptButton != null)
				{
					var missionName = MissionNameRegex.Match(missionDescription).Groups[1].Value;
					var missionLocation = MissionLocationRegex.Match(missionTarget).Groups[1].Value;
					if (!IgnoredMissions.Contains(missionName) && !IgnoredSystems.Contains(missionLocation))
					{
						AcceptedMissions.Add(new CorporationMissionTaker.MissionDestination()
						{
							AgentName = agentName,
							MissionName = missionName,
						});
						return acceptButton.ClickTask();
					}
					else
					{
						var effectiveStanding = double.Parse(AgentStandingRegex.Match(missionDescription).Groups[1].Value);
						if (!missionDescription.Contains("Declining a mission from this agent within the next") ||
						    effectiveStanding > 8)
							return agentDialogue.ButtonText
								.First(bt => bt.Text == "Decline")
								.ClickTask();

					}

				}

				CheckedAgents.Add(agentName);
				return agentDialogue.ClickMenuEntryByRegexPattern(bot, ".*Close");
			}

			if (!scrolledToBottom && memory.WindowStation.Single().AgentEntry.Length > 6)
			{
				scrolledToBottom = true;
				return new BotTask("Scroll agents list")
				{
					ClientActions = new List<MotionRecommendation>()
					{
						memory.WindowStation.Single().AgentEntry.FirstOrDefault().MouseClick(MouseButtonIdEnum.Left).AsRecommendation(),
						VirtualKeyCode.END.KeyboardPress().AsRecommendation(),
					}
				};
			}

			//check if can speak to another
			var agentsToCheck = memory.WindowStation.Single().AgentEntry
				.Where(a => a.LabelText.Any(lt => AcceptedMissionLevels.Any(aml => lt.Text.Contains(aml))))
				.Where(a => a.LabelText.Any(t => t.Text == "Security"))
				.Where(a => !a.LabelText.Any(t => t.Text.Contains("Accepted")));
			var notCheckedYetAgents =
				agentsToCheck
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
				return bot.MemoryMeasurementAtTime?.Value?.Neocom?.PeopleAndPlacesButton?.ClickTask();
			return null;
		}

		public bool MoveToNext => allMissionsProcessed;

		public IList<CorporationMissionTaker.MissionDestination> AcceptedMissions =
			new List<CorporationMissionTaker.MissionDestination>();
	}
}