// Decompiled with JetBrains decompiler
// Type: Bib3.SictSctroomZiilPufer`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System.Collections.Generic;

namespace Bib3
{
  public class SictSctroomZiilPufer<ElementTyp> : SictSctroomZiil<ElementTyp>
  {
    private readonly List<ElementTyp[]> ListePuferListeElement = new List<ElementTyp[]>();

    public override void FüügeAin(ElementTyp[] listeElement)
    {
      lock (this.ListePuferListeElement)
        this.ListePuferListeElement.Add(listeElement);
    }

    public ElementTyp[][] ListePuferListeElementNimHerausAle()
    {
      lock (this.ListePuferListeElement)
      {
        try
        {
          return this.ListePuferListeElement.ToArray();
        }
        finally
        {
          this.ListePuferListeElement.Clear();
        }
      }
    }
  }
}
