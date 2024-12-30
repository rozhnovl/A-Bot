// Decompiled with JetBrains decompiler
// Type: Bib3.WebSocket.SictWebSocketFrameLeeser
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bib3.WebSocket
{
  public class SictWebSocketFrameLeeser : SictSctroomZiil<byte>
  {
    private readonly object Lock = new object();
    private readonly List<byte[]> AingefüügtListeListeElement = new List<byte[]>();
    private readonly List<byte[]> PayloadListeListeElement = new List<byte[]>();
    private readonly List<byte[]> ÜberlaufListeListeElement = new List<byte[]>();

    public static byte[] WebSocketFrameMaskAppliziire(
      byte[] payloadAusscnitVorMaskListeElement,
      byte[] maskListeElement,
      long inPayloadPayloadAusscnitBeginIndex)
    {
      if (payloadAusscnitVorMaskListeElement == null)
        return (byte[]) null;
      if (maskListeElement == null || maskListeElement.Length < 0)
        return payloadAusscnitVorMaskListeElement;
      byte[] numArray = new byte[payloadAusscnitVorMaskListeElement.Length];
      for (long index1 = 0; index1 < (long) numArray.Length; ++index1)
      {
        long index2 = (index1 + inPayloadPayloadAusscnitBeginIndex) % (long) maskListeElement.Length;
        byte num = (byte) ((uint) payloadAusscnitVorMaskListeElement[index1] ^ (uint) maskListeElement[index2]);
        numArray[index1] = num;
      }
      return numArray;
    }

    public bool Volsctändig { private set; get; }

    public SictWebSocketFrameLeeser()
    {
    }

    public SictWebSocketFrameLeeser(byte[] puferListeElement)
      : base(puferListeElement)
    {
    }

    public byte[] PayloadBeginListeElementBerecne()
    {
      lock (this.Lock)
        return Glob.ArrayAusListeFeldGeflact<byte>((IEnumerable<byte[]>) this.PayloadListeListeElement);
    }

    public byte[] PayloadVolsctListeElementBerecne()
    {
      lock (this.Lock)
        return !this.Volsctändig ? new byte[0] : this.PayloadBeginListeElementBerecne();
    }

    public byte[] ÜberlaufListeElementBerecne()
    {
      lock (this.Lock)
        return Glob.ArrayAusListeFeldGeflact<byte>((IEnumerable<byte[]>) this.ÜberlaufListeListeElement);
    }

    public void ÜberlaufEntferne()
    {
      lock (this.Lock)
        this.ÜberlaufListeListeElement.Clear();
    }

    public long AingefüügtListeElementAnzaal { private set; get; }

    public override void FüügeAin(byte[] puferAusscnitListeElement)
    {
      if (puferAusscnitListeElement == null || puferAusscnitListeElement.Length < 1)
        return;
      lock (this.Lock)
      {
        long listeElementAnzaal1 = this.AingefüügtListeElementAnzaal;
        try
        {
          if (this.AingefüügtListeElementAnzaal < 1L)
          {
            byte num1 = puferAusscnitListeElement[0];
            this.Oktet0 = new byte?(num1);
            this.FlagFin = new bool?(((uint) num1 & 128U) > 0U);
            byte num2 = (byte) ((uint) num1 & 15U);
            this.Opcode = new byte?(num2);
            this.OpcodeSictEnum = new SictWebSocketFrameOpcodeTyp?((SictWebSocketFrameOpcodeTyp) num2);
          }
          long index = 1L - this.AingefüügtListeElementAnzaal;
          if (0L <= index && index < (long) puferAusscnitListeElement.Length)
          {
            byte num3 = puferAusscnitListeElement[index];
            this.Oktet1 = new byte?(num3);
            this.FlagMask = new bool?(((uint) num3 & 128U) > 0U);
            byte num4 = (byte) ((uint) num3 & (uint) sbyte.MaxValue);
            this.PayloadLengthBegin = new byte?(num4);
            if (num4 < (byte) 126)
            {
              this.PayloadLengthOptionListeOktetAnzaal = new int?(0);
              this.PayloadLength = new long?((long) num4);
            }
            else if ((byte) 126 == num4)
              this.PayloadLengthOptionListeOktetAnzaal = new int?(2);
            else if ((byte) 127 == num4)
              this.PayloadLengthOptionListeOktetAnzaal = new int?(8);
            this.InFrameListeOktetPayloadLengthEndeOfenOktetIndex = new long?((long) (this.PayloadLengthOptionListeOktetAnzaal.Value + 2));
            this.InFramePayloadBeginOktetIndex = new long?(this.InFrameListeOktetPayloadLengthEndeOfenOktetIndex.Value + (this.FlagMask.Value ? 4L : 0L));
          }
        }
        finally
        {
          this.AingefüügtListeListeElement.Add(puferAusscnitListeElement);
          this.AingefüügtListeElementAnzaal += (long) puferAusscnitListeElement.Length;
        }
        long? nullable1;
        if (!this.PayloadLength.HasValue)
        {
          nullable1 = this.InFrameListeOktetPayloadLengthEndeOfenOktetIndex;
          if (!nullable1.HasValue)
            return;
          this.PayloadLengthSictListeOktet = this.AingefüügtListeListeElement.AusListeListeAusscnit<byte>(2L, (long) this.PayloadLengthOptionListeOktetAnzaal.Value);
          if (this.PayloadLengthSictListeOktet == null)
            return;
          if (2 == this.PayloadLengthSictListeOktet.Length)
            this.PayloadLength = new long?((long) BitConverter.ToUInt16(((IEnumerable<byte>) this.PayloadLengthSictListeOktet).Reverse<byte>().ToArray<byte>(), 0));
          else
            this.PayloadLength = 8 == this.PayloadLengthSictListeOktet.Length ? new long?(BitConverter.ToInt64(((IEnumerable<byte>) this.PayloadLengthSictListeOktet).Reverse<byte>().ToArray<byte>(), 0)) : throw new ArgumentException("PayloadLengthSictListeOktet.LongLength");
          nullable1 = this.InFrameListeOktetPayloadLengthEndeOfenOktetIndex;
          long num5 = nullable1.Value;
          nullable1 = this.PayloadLength;
          long num6 = nullable1.Value;
          this.FrameLength = new long?(num5 + num6);
        }
        long num7 = listeElementAnzaal1;
        nullable1 = this.InFramePayloadBeginOktetIndex;
        long valueOrDefault = nullable1.GetValueOrDefault();
        int num8;
        if ((num7 <= valueOrDefault ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
        {
          nullable1 = this.InFramePayloadBeginOktetIndex;
          long listeElementAnzaal2 = this.AingefüügtListeElementAnzaal;
          num8 = nullable1.GetValueOrDefault() <= listeElementAnzaal2 ? (nullable1.HasValue ? 1 : 0) : 0;
        }
        else
          num8 = 0;
        if (num8 != 0 && this.FlagMask.Value)
        {
          List<byte[]> listeListeElement = this.AingefüügtListeListeElement;
          nullable1 = this.InFrameListeOktetPayloadLengthEndeOfenOktetIndex;
          long ausscnitBeginIndex = nullable1.Value;
          this.MaskListeOktet = listeListeElement.AusListeListeAusscnit<byte>(ausscnitBeginIndex, 4L);
        }
        if (this.Volsctändig)
        {
          this.ÜberlaufListeListeElement.Add(puferAusscnitListeElement);
        }
        else
        {
          long num9 = listeElementAnzaal1;
          nullable1 = this.InFramePayloadBeginOktetIndex;
          long num10 = nullable1.Value;
          long val2_1 = num9 - num10;
          long num11 = listeElementAnzaal1;
          nullable1 = this.InFramePayloadEndeOfenOktetIndex;
          long num12 = nullable1.Value;
          long num13 = num11 - num12;
          long num14 = val2_1 + (long) puferAusscnitListeElement.Length;
          long num15 = num13 + (long) puferAusscnitListeElement.Length;
          nullable1 = this.InFramePayloadBeginOktetIndex;
          long num16 = listeElementAnzaal1;
          long ausscnitBeginIndex1 = (nullable1.HasValue ? new long?(nullable1.GetValueOrDefault() - num16) : new long?()).Value;
          long? nullable2 = this.InFramePayloadEndeOfenOktetIndex;
          long num17 = listeElementAnzaal1;
          long? nullable3;
          if (!nullable2.HasValue)
          {
            nullable1 = new long?();
            nullable3 = nullable1;
          }
          else
            nullable3 = new long?(nullable2.GetValueOrDefault() - num17);
          nullable1 = nullable3;
          long num18 = nullable1.Value;
          nullable1 = this.InFramePayloadEndeOfenOktetIndex;
          long num19 = listeElementAnzaal1;
          long? nullable4;
          if (!nullable1.HasValue)
          {
            nullable2 = new long?();
            nullable4 = nullable2;
          }
          else
            nullable4 = new long?(nullable1.GetValueOrDefault() - num19);
          nullable2 = nullable4;
          long ausscnitBeginIndex2 = nullable2.Value;
          long val1_1 = listeElementAnzaal1 + (long) puferAusscnitListeElement.Length;
          nullable2 = this.InFramePayloadEndeOfenOktetIndex;
          long val2_2 = nullable2.Value;
          long num20 = Math.Min(val1_1, val2_2);
          long val1_2 = listeElementAnzaal1;
          nullable2 = this.InFramePayloadBeginOktetIndex;
          long val2_3 = nullable2.Value;
          long num21 = Math.Max(val1_2, val2_3);
          long num22 = num20 - num21;
          if (0L < num22)
            this.PayloadListeListeElement.Add(SictWebSocketFrameLeeser.WebSocketFrameMaskAppliziire(0L > ausscnitBeginIndex1 ? puferAusscnitListeElement.ArrayAusscnit<byte>(0L, num22) : puferAusscnitListeElement.ArrayAusscnit<byte>(ausscnitBeginIndex1, Math.Min(num22, (long) puferAusscnitListeElement.Length - ausscnitBeginIndex1)), this.MaskListeOktet, Math.Max(0L, val2_1)));
          if (0L <= num15)
          {
            this.Volsctändig = true;
            if (0L < num15)
            {
              Encoding.UTF8.GetString(this.PayloadVolsctListeElementBerecne());
              byte[] numArray = puferAusscnitListeElement.ArrayAusscnit<byte>(ausscnitBeginIndex2, (long) puferAusscnitListeElement.Length - ausscnitBeginIndex2);
              if (numArray != null)
                this.ÜberlaufListeListeElement.Add(numArray);
            }
          }
        }
      }
    }

    public byte? Oktet0 { private set; get; }

    public byte? Oktet1 { private set; get; }

    public byte? Opcode { private set; get; }

    public SictWebSocketFrameOpcodeTyp? OpcodeSictEnum { private set; get; }

    public bool? FlagFin { private set; get; }

    public bool? FlagMask { private set; get; }

    public byte[] MaskListeOktet { private set; get; }

    public byte? PayloadLengthBegin { private set; get; }

    public int? PayloadLengthOptionListeOktetAnzaal { private set; get; }

    public byte[] PayloadLengthSictListeOktet { private set; get; }

    public long? PayloadLength { private set; get; }

    public long? FrameLength { private set; get; }

    public long? InFrameListeOktetPayloadLengthEndeOfenOktetIndex { private set; get; }

    public long? InFramePayloadBeginOktetIndex { private set; get; }

    public long? InFramePayloadEndeOfenOktetIndex
    {
      get
      {
        long? payloadBeginOktetIndex = this.InFramePayloadBeginOktetIndex;
        long? payloadLength = this.PayloadLength;
        return payloadBeginOktetIndex.HasValue & payloadLength.HasValue ? new long?(payloadBeginOktetIndex.GetValueOrDefault() + payloadLength.GetValueOrDefault()) : new long?();
      }
    }

    public long? Länge => this.InFramePayloadEndeOfenOktetIndex;
  }
}
