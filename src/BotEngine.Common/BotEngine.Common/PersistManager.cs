using System;
using System.Collections.Generic;
using Bib3;

namespace BotEngine.Common;

public class PersistManager
{
	public string FilePath;

	public Action<Exception> PersistExceptionDelegate;

	public KeyValuePair<string, byte[]>? FileStateLast { get; private set; }

	public byte[] ReadFromFileSerialized()
	{
		string filePath = FilePath;
		byte[] array = Glob.InhaltAusDataiMitPfaad(filePath);
		FileStateLast = new KeyValuePair<string, byte[]>(filePath, array);
		return array;
	}

	public void ValueChanged(byte[] serialized)
	{
		string filePath = FilePath;
		if (!(FileStateLast?.Key == filePath) || !(FileStateLast?.Value).SequenceEqualPerObjectEquals(serialized))
		{
			try
			{
				FileStateLast = new KeyValuePair<string, byte[]>(filePath, serialized);
				Glob.ScraibeInhaltNaacDataiPfaad(filePath, serialized);
			}
			catch (Exception obj)
			{
				PersistExceptionDelegate?.Invoke(obj);
			}
		}
	}
}
