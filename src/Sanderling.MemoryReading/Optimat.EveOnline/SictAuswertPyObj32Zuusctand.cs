using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32Zuusctand
{
	public static readonly int ObjektBeginListeOktetAnzaal = Marshal.SizeOf(typeof(SictPyObjAusrictCPython32Erwaitert1));

	protected int ObjektListeOktetAnzaal = ObjektBeginListeOktetAnzaal;

	private readonly SictPyObjAusrictCPython32Erwaitert1[] InternObjektBegin = new SictPyObjAusrictCPython32Erwaitert1[1];

	public readonly long HerkunftAdrese;

	public readonly long BeginZait;

	public SictAuswertPythonObjType TypeObjektKlas;

	public bool InternAnnaameImmutable { get; protected set; }

	public uint? RefTypeFrühest { get; private set; }

	public bool RefTypeGeändert { get; private set; }

	public SictPyObjAusrictCPython32Erwaitert1 ObjektBegin => InternObjektBegin[0];

	public long RefType => ObjektBegin.ObjBegin.RefType;

	public long AktualisLezteZait { get; private set; }

	public KeyValuePair<byte[], int> AusScpaicerLeeseLezteListeOktetUndAnzaal { get; private set; }

	public SictAuswertPyObj32Zuusctand(long HerkunftAdrese, long BeginZait)
	{
		this.HerkunftAdrese = HerkunftAdrese;
		this.BeginZait = BeginZait;
	}

	protected int ZwiscenscpaicerListeOktetFüleAusProzes(IMemoryReader AusProzesLeeser, int ListeOktetAnzaal)
	{
		long herkunftAdrese = HerkunftAdrese;
		int num = Math.Max(0, ListeOktetAnzaal);
		byte[] key = AusScpaicerLeeseLezteListeOktetUndAnzaal.Key;
		byte[] array = key;
		if (array != null && (array.Length < num || num < array.Length / 2 - 10))
		{
			array = null;
		}
		if (array == null)
		{
			array = new byte[num + 4];
		}
		else
		{
			Array.Clear(array, 0, num);
		}
		int num2 = 0;
		try
		{
			if (AusProzesLeeser == null)
			{
				return num2;
			}
			num2 = (int)Extension.ListeOktetLeeseVonAdrese(AusProzesLeeser, herkunftAdrese, (long)num, array);
		}
		finally
		{
			AusScpaicerLeeseLezteListeOktetUndAnzaal = new KeyValuePair<byte[], int>(array, num2);
		}
		return num2;
	}

	public virtual void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		if (!ZuLeeseListeOktetAnzaal.HasValue)
		{
			ZuLeeseListeOktetAnzaal = Math.Min(ObjektBeginListeOktetAnzaal, ObjektListeOktetAnzaal);
		}
		Geändert = false;
		if (AusProzesLeeser == null)
		{
			return;
		}
		AktualisLezteZait = Zait;
		ZuLeeseListeOktetAnzaal = Math.Max(0, ZuLeeseListeOktetAnzaal.Value);
		KeyValuePair<byte[], int> ausScpaicerLeeseLezteListeOktetUndAnzaal = AusScpaicerLeeseLezteListeOktetUndAnzaal;
		int val = ZwiscenscpaicerListeOktetFüleAusProzes(AusProzesLeeser, ZuLeeseListeOktetAnzaal.Value);
		KeyValuePair<byte[], int> ausScpaicerLeeseLezteListeOktetUndAnzaal2 = AusScpaicerLeeseLezteListeOktetUndAnzaal;
		int length = Math.Min(val, ObjektBeginListeOktetAnzaal);
		if (ausScpaicerLeeseLezteListeOktetUndAnzaal2.Key == null || ausScpaicerLeeseLezteListeOktetUndAnzaal2.Value < ObjektBeginListeOktetAnzaal)
		{
			Array.Clear(InternObjektBegin, 0, 1);
		}
		if (ausScpaicerLeeseLezteListeOktetUndAnzaal2.Key != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(InternObjektBegin, GCHandleType.Pinned);
			try
			{
				Marshal.Copy(ausScpaicerLeeseLezteListeOktetUndAnzaal2.Key, 0, gCHandle.AddrOfPinnedObject(), length);
			}
			finally
			{
				gCHandle.Free();
			}
		}
		SictPyObjAusrictCPython32Erwaitert1 objektBegin = ObjektBegin;
		if (!RefTypeFrühest.HasValue)
		{
			RefTypeFrühest = objektBegin.ObjBegin.RefType;
		}
		if (RefTypeFrühest != objektBegin.ObjBegin.RefType)
		{
			RefTypeGeändert = true;
		}
		AusScpaicerLeeseLezteListeOktetUndAnzaal = ausScpaicerLeeseLezteListeOktetUndAnzaal2;
	}
}
