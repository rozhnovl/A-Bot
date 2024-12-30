using System;

namespace Optimat.EveOnline;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class SictInPyDictEntryKeyAttribut : Attribute
{
	public readonly string DictEntryKeyString;

	public SictInPyDictEntryKeyAttribut(string DictEntryKeyString)
	{
		this.DictEntryKeyString = DictEntryKeyString;
	}
}
