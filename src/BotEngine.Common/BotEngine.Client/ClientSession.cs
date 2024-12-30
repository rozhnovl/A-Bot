using Bib3;

namespace BotEngine.Client;

public class ClientSession
{
	public static bool SessionIdEqual(byte[] id0, byte[] id1)
	{
		return Glob.SequenceEqual(id0, id1);
	}
}
