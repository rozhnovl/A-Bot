using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using BotEngine;
using BotEngine.Interface;
using BotEngine.Interface.Process.Snapshot;
using BotEngine.Windows;
using Extension = BotEngine.Windows.Extension;

namespace Optimat.EveOnline;

public class MemoryAuswertWurzelSuuce : SictProcessAuswertWurzelSuuce
{
	private readonly IMemoryReader MemoryReader;

	private static long DebugAdrese = -1L;

	public MemoryAuswertWurzelSuuce(IMemoryReader memoryReader)
	{
		MemoryReader = memoryReader;
	}

	public static KeyValuePair<long, long>[] ListeScpaicerberaicGültigeZiil(IMemoryReader memoryReader)
	{
		ProcessMemoryReader val = (ProcessMemoryReader)(object)((memoryReader is ProcessMemoryReader) ? memoryReader : null);
		SnapshotReader val2 = (SnapshotReader)(object)((memoryReader is SnapshotReader) ? memoryReader : null);
		if (val != null)
		{
			RangeOfPages[] source = Extension.ListRangeOfPagesFromProcessWithId((uint)val.ProcessId, false);
			return source.Where((RangeOfPages rangeOfPages) => Extension.GültigeZaigerZiil(rangeOfPages.BasicInfo))?.Select((RangeOfPages rangeOfPages) => new KeyValuePair<long, long>(rangeOfPages.BasicInfo.BaseAddress.ToInt64(), rangeOfPages.BasicInfo.RegionSize.ToInt64()))?.ToArray();
		}
		if (val2 != null)
		{
			return val2?.MemoryBaseAddressAndListOctet?.Select(delegate(KeyValuePair<long, byte[]> memoryBaseAddressAndListOctet)
			{
				long key = memoryBaseAddressAndListOctet.Key;
				byte[] value = memoryBaseAddressAndListOctet.Value;
				return new KeyValuePair<long, long>(key, (value != null) ? value.Length : 0);
			})?.ToArray();
		}
		return null;
	}

	public override void Berecne()
	{
		InternDauer.BeginSezeJezt();
		try
		{
			KeyValuePair<long, long>[] array = ListeScpaicerberaicGültigeZiil(MemoryReader);
			if (array.IsNullOrEmpty())
			{
				return;
			}
			List<KeyValuePair<long, long>> list = new List<KeyValuePair<long, long>>();
			List<long> source = new List<long>();
			list.AddRange(array);
			IMemoryReader ProcessLeeser = MemoryReader;
			if (ProcessLeeser == null)
			{
				return;
			}
			List<KeyValuePair<long, long>> list2 = new List<KeyValuePair<long, long>>();
			List<long> list3 = new List<long>();
			List<SictAuswertPythonObjType> list4 = new List<SictAuswertPythonObjType>();
			foreach (KeyValuePair<long, long> item in list)
			{
				long key = item.Key;
				long value = item.Value;
				uint[] array2 = MemoryReaderExtension.ReadArray<uint>(ProcessLeeser, key, (int)value);
				if (array2 == null)
				{
					continue;
				}
				long num = 1L;
				long num2 = key;
				while (num < array2.Length)
				{
					uint num3 = array2[num];
					if (num2 == num3)
					{
						uint num4 = num3;
						list3.Add(num4);
						long num5 = Math.Min(num4 + 256, key + value);
						long num6 = num5 - num4;
						if (num6 >= 16)
						{
							byte[] array3 = new byte[num6];
							Buffer.BlockCopy(array2, (int)(num4 - key), array3, 0, array3.Length);
							SictAuswertPythonObjType sictAuswertPythonObjType = new SictAuswertPythonObjType(num4, array3);
							if (65536 >= sictAuswertPythonObjType.tp_basicsize && 4096 >= sictAuswertPythonObjType.tp_itemsize)
							{
								sictAuswertPythonObjType.LaadeReferenziirte(ProcessLeeser);
								if (string.Equals("type", sictAuswertPythonObjType.tp_name))
								{
									list4.Add(sictAuswertPythonObjType);
								}
							}
						}
					}
					num++;
					num2 += 4;
				}
			}
			long num7 = source.Sum();
			if (1 != list4.Count)
			{
			}
			base.PyObjTypType = list4.FirstOrDefault();
			if (base.PyObjTypType == null)
			{
				return;
			}
			long PyObjTypTypeAdrese = base.PyObjTypType.HerkunftAdrese;
			int value2 = Math.Max(1, (int)Math.Log(list2.Count, 2.0) - 4);
			SictAstBinär13<long> sictAstBinär = SictAstBinär13<long>.ErscteleBaumAusListe(list2, value2);
			List<SictAuswertPythonObjType> list5 = new List<SictAuswertPythonObjType>();
			long[] array4 = list?.Select((KeyValuePair<long, long> region) => MemoryReaderExtension.AddressesHoldingValue32Aligned32(ProcessLeeser, (uint)PyObjTypTypeAdrese, region.Key, region.Value + region.Key))?.ConcatNullable()?.ToArray();
			for (int i = 0; i < array4.Length; i++)
			{
				long herkunftAdrese = array4[i] - 4;
				SictAuswertPythonObjType sictAuswertPythonObjType2 = new SictAuswertPythonObjType(herkunftAdrese, null, ProcessLeeser);
				sictAuswertPythonObjType2.LaadeReferenziirte(ProcessLeeser);
				if (sictAuswertPythonObjType2.tp_name != null && sictAuswertPythonObjType2.tp_name.Length >= 1)
				{
					list5.Add(sictAuswertPythonObjType2);
				}
			}
			foreach (SictAuswertPythonObjType item2 in list5)
			{
				PyObjSezeNaacScpaicer(item2);
			}
			MengePyObjTypSezeAusMengeFürHerkunftAdrPyObj();
			FüleRefTypScpezVonMengePyObjType();
			if (base.PyObjTypUIRoot == null)
			{
				return;
			}
			List<SictAuswertPyObjGbsAst> list6 = new List<SictAuswertPyObjGbsAst>();
			long[] array5 = list?.Select((KeyValuePair<long, long> region) => MemoryReaderExtension.AddressesHoldingValue32Aligned32(ProcessLeeser, (uint)base.PyObjTypUIRoot.HerkunftAdrese, region.Key, region.Value + region.Key))?.ConcatNullable()?.ToArray();
			for (int j = 0; j < array5.Length; j++)
			{
				long herkunftAdrese2 = array5[j] - 4;
				SictAuswertPyObjGbsAst sictAuswertPyObjGbsAst = new SictAuswertPyObjGbsAst(herkunftAdrese2, null, ProcessLeeser);
				LaadeReferenziirte(sictAuswertPyObjGbsAst, ProcessLeeser, ObjSezeNaacScpaicer: true, ErmitleTypNurAusScpaicer: true, 1024);
				if (sictAuswertPyObjGbsAst.Dict != null)
				{
					long[] ausChildrenListRef = sictAuswertPyObjGbsAst.AusChildrenListRef;
					if (ausChildrenListRef != null && ausChildrenListRef.Length >= 1)
					{
						list6.Add(sictAuswertPyObjGbsAst);
					}
				}
			}
			base.GbsMengeWurzelObj = list6.ToArray();
		}
		catch
		{
		}
		finally
		{
			InternDauer.EndeSezeJezt();
		}
	}
}
