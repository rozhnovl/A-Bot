// Decompiled with JetBrains decompiler
// Type: Bib3.WebSocket.SictWebSocketLeeser
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System.Collections.Generic;

namespace Bib3.WebSocket
{
  public class SictWebSocketLeeser : SictSctroomZiil<byte>
  {
    private readonly object Lock = new object();
    private readonly List<SictWebSocketMessage> ListeMessageVolsct = new List<SictWebSocketMessage>();
    private SictWebSocketMessage MessageInArbait = new SictWebSocketMessage();
    private SictWebSocketFrameLeeser FrameInArbait = new SictWebSocketFrameLeeser();

    public SictWebSocketMessage[] ListeMessageVolsctNimHerausAle()
    {
      lock (this.Lock)
      {
        SictWebSocketMessage[] array = this.ListeMessageVolsct.ToArray();
        this.ListeMessageVolsct.Clear();
        return array;
      }
    }

    public SictWebSocketLeeser()
    {
    }

    public SictWebSocketLeeser(byte[] puferListeElement)
      : base(puferListeElement)
    {
    }

    public long AingefüügtListeElementAnzaal { private set; get; }

    public long PayloadListeElementAnzaal { private set; get; }

    public long ListeFrameVolsctändigAnzaal { private set; get; }

    public long ListeMessageVolsctändigAnzaal { private set; get; }

    public override void FüügeAin(byte[] puferAusscnitListeElement)
    {
      if (puferAusscnitListeElement == null || puferAusscnitListeElement.Length < 1)
        return;
      lock (this.Lock)
      {
        try
        {
          this.FrameInArbait.FüügeAin(puferAusscnitListeElement);
          this.AingefüügtListeElementAnzaal += (long) puferAusscnitListeElement.Length;
          byte[] puferListeElement;
          for (; this.FrameInArbait.Volsctändig; this.FrameInArbait = new SictWebSocketFrameLeeser(puferListeElement))
          {
            ++this.ListeFrameVolsctändigAnzaal;
            this.PayloadListeElementAnzaal += this.FrameInArbait.PayloadLength.Value;
            puferListeElement = this.FrameInArbait.ÜberlaufListeElementBerecne();
            this.FrameInArbait.ÜberlaufEntferne();
            if ((byte) 7 < this.FrameInArbait.Opcode.Value)
            {
              ++this.ListeMessageVolsctändigAnzaal;
              this.ListeMessageVolsct.Add(new SictWebSocketMessage(this.FrameInArbait));
            }
            else
            {
              this.MessageInArbait.ListeFrame.Add(this.FrameInArbait);
              if (this.FrameInArbait.FlagFin.Value)
              {
                ++this.ListeMessageVolsctändigAnzaal;
                this.ListeMessageVolsct.Add(this.MessageInArbait);
                this.MessageInArbait = new SictWebSocketMessage();
              }
            }
          }
        }
        finally
        {
        }
      }
    }
  }
}
