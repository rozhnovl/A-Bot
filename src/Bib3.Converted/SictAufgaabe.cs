// Decompiled with JetBrains decompiler
// Type: Bib3.SictAufgaabe
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Threading;

namespace Bib3
{
  public class SictAufgaabe
  {
    private readonly object InternAufgaabeAlsObjekt;
    private readonly object Lock = new object();
    private bool InternAusfüürungBegone = false;
    private bool InternAusfüürungFertig = false;
    private Exception InternAusnaameWäärendAusfüürung = (Exception) null;

    public object AufgaabeAlsObjekt => this.InternAufgaabeAlsObjekt;

    public bool Fertig => this.InternAusfüürungFertig;

    private Exception AusnaameWäärendAusfüürung => this.InternAusnaameWäärendAusfüürung;

    protected SictAufgaabe()
      : this((object) null)
    {
    }

    protected SictAufgaabe(object aufgaabe) => this.InternAufgaabeAlsObjekt = aufgaabe;

    public void ScteleSicerAusfüürungFertig()
    {
      lock (this.Lock)
      {
        if (this.InternAusfüürungBegone)
        {
          while (!this.Fertig)
            Thread.Sleep(1);
        }
        else
        {
          this.InternAusfüürungBegone = true;
          try
          {
            this.FüüreAusLocked();
          }
          catch (Exception ex)
          {
            this.InternAusnaameWäärendAusfüürung = ex;
          }
          finally
          {
            this.InternAusfüürungFertig = true;
          }
        }
      }
    }

    protected virtual void FüüreAusLocked()
    {
    }
  }
}
