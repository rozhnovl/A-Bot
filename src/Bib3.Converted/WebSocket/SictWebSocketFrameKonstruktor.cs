// Decompiled with JetBrains decompiler
// Type: Bib3.WebSocket.SictWebSocketFrameKonstruktor
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bib3.WebSocket
{
  public class SictWebSocketFrameKonstruktor
  {
    public byte Oktet0 { private set; get; }

    public byte Oktet1 { private set; get; }

    public byte[] ListeOktet { private set; get; }

    public static SictWebSocketFrameKonstruktor MessageTextErsctele(
      string text,
      byte[] maskListeElement = null)
    {
      return text == null ? (SictWebSocketFrameKonstruktor) null : new SictWebSocketFrameKonstruktor(Encoding.UTF8.GetBytes(text), true, SictWebSocketFrameOpcodeTyp.TextFrame, maskListeElement);
    }

    /*public SictWebSocketFrameKonstruktor(SictWebSocketFrameLeeser zuKopiirende)
      : this(zuKopiirende == null ? (byte[]) null : zuKopiirende.PayloadVolsctListeElementBerecne(), zuKopiirende != null && ((int) zuKopiirende.FlagFin ?? 0) != 0, zuKopiirende == null ? SictWebSocketFrameOpcodeTyp.ContinuationFrame : (SictWebSocketFrameOpcodeTyp) ((int) zuKopiirende.OpcodeSictEnum ?? 0), zuKopiirende == null ? (byte[]) null : zuKopiirende.MaskListeOktet)
    {
    }*/

    public SictWebSocketFrameKonstruktor(
      byte[] payloadListeElement,
      bool flagFin,
      SictWebSocketFrameOpcodeTyp opcode,
      byte[] maskListeElement)
    {
      this.Oktet0 = (byte) ((SictWebSocketFrameOpcodeTyp) ((flagFin ? 1 : 0) << 7) | opcode & (SictWebSocketFrameOpcodeTyp) 15);
      byte[] source = (byte[]) null;
      long length = payloadListeElement == null ? 0L : (long) payloadListeElement.Length;
      if (length <= (long) ushort.MaxValue)
      {
        if (length >= 126L)
          source = BitConverter.GetBytes((ushort) length);
      }
      else
        source = BitConverter.GetBytes((ulong) length);
      if (source != null)
        source = ((IEnumerable<byte>) source).Reverse<byte>().ToArray<byte>();
      int num = source == null ? (int) (byte) length : (source.Length < 3 ? 126 : (int) sbyte.MaxValue);
      byte[] second = SictWebSocketFrameLeeser.WebSocketFrameMaskAppliziire(payloadListeElement, maskListeElement, 0L);
      this.Oktet1 = (byte) ((maskListeElement == null ? 0 : 1) << 7 | num);
      IEnumerable<byte> bytes = ((IEnumerable<byte>) new byte[2]
      {
        this.Oktet0,
        this.Oktet1
      }).Concat<byte>((IEnumerable<byte>) (source ?? new byte[0])).Concat<byte>((IEnumerable<byte>) (maskListeElement ?? new byte[0]));
      if (second != null)
        bytes = bytes.Concat<byte>((IEnumerable<byte>) second);
      this.ListeOktet = bytes.ToArray<byte>();
    }
  }
}
