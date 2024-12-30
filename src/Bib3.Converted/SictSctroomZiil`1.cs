// Decompiled with JetBrains decompiler
// Type: Bib3.SictSctroomZiil`1
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3
{
  public abstract class SictSctroomZiil<ElementTyp>
  {
    public void FüügeAin(
      ElementTyp[] puferListeElement,
      long beginInPuferElementIndex,
      long elementAnzaal)
    {
      if (puferListeElement == null)
        return;
      long num = beginInPuferElementIndex + elementAnzaal;
      if ((long) puferListeElement.Length < num || beginInPuferElementIndex < 0L || elementAnzaal < 1L)
        return;
      this.FüügeAin(puferListeElement.ArrayAusscnit<ElementTyp>(beginInPuferElementIndex, elementAnzaal));
    }

    public abstract void FüügeAin(ElementTyp[] puferAusscnitListeElement);

    public SictSctroomZiil()
    {
    }

    public SictSctroomZiil(ElementTyp[] puferListeElement)
    {
      if (puferListeElement == null)
        return;
      this.FüügeAin(puferListeElement);
    }
  }
}
