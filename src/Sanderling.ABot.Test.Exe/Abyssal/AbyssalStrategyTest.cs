using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sanderling.ABot.Bot.Strategies;

namespace Sanderling.ABot.Test.Exe.Abyssal
{
	[TestFixture]
	class AbyssalStrategyTest
	{
		private AbyssalRunner runner;

		[SetUp]
		private void Setup()
		{
			runner = new AbyssalRunner();
		}
	}
}
