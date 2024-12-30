using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertObjMitAdrese
{
	public readonly long HerkunftAdrese;

	public byte[] ListeOktet { get; private set; }

	public static double? DoubleAusListeOktet(byte[] ListeOktet, int BeginOktetIndex)
	{
		if (ListeOktet == null)
		{
			return null;
		}
		if (ListeOktet.Length <= BeginOktetIndex + 8)
		{
			return null;
		}
		return BitConverter.ToDouble(ListeOktet, BeginOktetIndex);
	}

	public static int? Int32AusListeOktet(byte[] ListeOktet, int BeginOktetIndex)
	{
		if (ListeOktet == null)
		{
			return null;
		}
		if (ListeOktet.Length < BeginOktetIndex + 4)
		{
			return null;
		}
		return BitConverter.ToInt32(ListeOktet, BeginOktetIndex);
	}

	public static uint? UInt32AusListeOktet(byte[] ListeOktet, int BeginOktetIndex)
	{
		if (ListeOktet == null)
		{
			return null;
		}
		if (ListeOktet.Length < BeginOktetIndex + 4)
		{
			return null;
		}
		return BitConverter.ToUInt32(ListeOktet, BeginOktetIndex);
	}

	public int? Int32AusListeOktet(int BeginOktetIndex)
	{
		return Int32AusListeOktet(ListeOktet, BeginOktetIndex);
	}

	public uint? UInt32AusListeOktet(int BeginOktetIndex)
	{
		return UInt32AusListeOktet(ListeOktet, BeginOktetIndex);
	}

	public SictAuswertObjMitAdrese(long HerkunftAdrese, IMemoryReader DaatenKwele = null, byte[] ListeOktet = null)
	{
		this.HerkunftAdrese = HerkunftAdrese;
		this.ListeOktet = ListeOktet;
		Ersctele(DaatenKwele);
	}

	public virtual void Ersctele(IMemoryReader DaatenKwele)
	{
		LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(DaatenKwele);
	}

	public void LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(IMemoryReader DaatenKwele)
	{
		if (DaatenKwele != null)
		{
			int num = Math.Min(65536, ListeOktetAnzaalBerecne());
			bool flag = false;
			if (ListeOktet == null)
			{
				flag = true;
			}
			else if (ListeOktet.Length < num)
			{
				flag = true;
			}
			if (flag)
			{
				ListeOktet = Extension.ListeOktetLeeseVonAdrese(DaatenKwele, HerkunftAdrese, (long)num, false);
			}
		}
	}

	public virtual int ListeOktetAnzaalBerecne()
	{
		return 0;
	}
}
