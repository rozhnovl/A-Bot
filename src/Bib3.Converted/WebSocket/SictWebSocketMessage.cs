// Decompiled with JetBrains decompiler
// Type: Bib3.WebSocket.SictWebSocketMessage
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System.Collections.Generic;

namespace Bib3.WebSocket
{
  public class SictWebSocketMessage
  {
    public readonly List<SictWebSocketFrameLeeser> ListeFrame = new List<SictWebSocketFrameLeeser>();

    public SictWebSocketMessage()
    {
    }

    public SictWebSocketMessage(SictWebSocketFrameLeeser frame)
      : this(new SictWebSocketFrameLeeser[1]{ frame })
    {
    }

    public SictWebSocketMessage(SictWebSocketFrameLeeser[] listeFrame) => this.ListeFrame.AddRange((IEnumerable<SictWebSocketFrameLeeser>) listeFrame);

    public byte[] PayloadBeginListeElementBerecne()
    {
      byte[] payloadBeginListeElement;
      this.Berecne(true, out SictWebSocketFrameLeeser[] _, out bool _, out payloadBeginListeElement);
      return payloadBeginListeElement;
    }

    public byte[] PayloadVolsctListeElementBerecne()
    {
      bool payloadVolsctändig;
      byte[] payloadBeginListeElement;
      this.Berecne(true, out SictWebSocketFrameLeeser[] _, out payloadVolsctändig, out payloadBeginListeElement);
      return payloadVolsctändig ? payloadBeginListeElement : (byte[]) null;
    }

    public void Berecne(
      bool payloadListeElementBerecne,
      out SictWebSocketFrameLeeser[] listeFrame,
      out bool payloadVolsctändig,
      out byte[] payloadBeginListeElement)
    {
      payloadVolsctändig = false;
      payloadBeginListeElement = (byte[]) null;
      List<byte[]> listeFeld = new List<byte[]>();
      listeFrame = this.ListeFrame.ToArray();
      for (int index = 0; (long) index < (long) listeFrame.Length; ++index)
      {
        SictWebSocketFrameLeeser socketFrameLeeser = listeFrame[index];
        if (socketFrameLeeser != null)
        {
          if (payloadListeElementBerecne)
          {
            byte[] numArray = socketFrameLeeser.PayloadBeginListeElementBerecne();
            listeFeld.Add(numArray);
          }
          if (socketFrameLeeser.Volsctändig)
          {
            if ((long) listeFrame.Length == (long) (index + 1))
              payloadVolsctändig = true;
          }
          else
            break;
        }
        else
          break;
      }
      if (!payloadListeElementBerecne)
        return;
      payloadBeginListeElement = Glob.ArrayAusListeFeldGeflact<byte>((IEnumerable<byte[]>) listeFeld);
    }
  }
}
