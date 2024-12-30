// Decompiled with JetBrains decompiler
// Type: Bib3.SictScnitTcpKlient
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Bib3
{
  public class SictScnitTcpKlient
  {
    private readonly object Lock = new object();
    public object ServerAdresseIpSolAbbildObject;
    public object ServerAdresseTcpSolAbbildObject;
    public object TimeoutMilliSolAbbildObject;
    public bool ErscteltSol;

    public long AufgaabeKümereLezteBeginZaitStopwatchMikro
    {
      get
      {
        SictScnitTcpKlientKümere aufgaabeKümereLezte = this.AufgaabeKümereLezte;
        return aufgaabeKümereLezte == null ? -1L : aufgaabeKümereLezte.BeginZaitStopwatchMikro;
      }
    }

    public DateTime AufgaabeKümereLezteBeginZaitDateTime => Glob.SictDateTimeVonStopwatchZaitMikro(this.AufgaabeKümereLezteBeginZaitStopwatchMikro);

    public KeyValuePair<TcpClient, long> TcpClientMitBeginZaitStopwatchMikro
    {
      get
      {
        lock (this.Lock)
        {
          SictScnitTcpKlientKümere verbindungErscteltLezte = this.AufgaabeKümereVerbindungErscteltLezte;
          return verbindungErscteltLezte == null ? new KeyValuePair<TcpClient, long>() : new KeyValuePair<TcpClient, long>(verbindungErscteltLezte.ScnitscteleTcpZuukünftig, verbindungErscteltLezte.BeginZaitStopwatchMikro);
        }
      }
    }

    public KeyValuePair<TcpClient, DateTime> TcpClientMitBeginZaitDateTime
    {
      get
      {
        lock (this.Lock)
        {
          KeyValuePair<TcpClient, long> zaitStopwatchMikro = this.TcpClientMitBeginZaitStopwatchMikro;
          return new KeyValuePair<TcpClient, DateTime>(zaitStopwatchMikro.Key, Glob.SictDateTimeVonStopwatchZaitMikro(zaitStopwatchMikro.Value));
        }
      }
    }

    public SictScnitTcpKlientKümere AufgaabeKümereLezte { private set; get; }

    public SictScnitTcpKlientKümere AufgaabeKümereVerbindungErscteltLezte { private set; get; }

    public void Kümere()
    {
      lock (this.Lock)
      {
        KeyValuePair<TcpClient, DateTime> beginZaitDateTime = this.TcpClientMitBeginZaitDateTime;
        SictScnitTcpKlientKümere scnitTcpKlientKümere = new SictScnitTcpKlientKümere(beginZaitDateTime.Key, this.ErscteltSol, this.ServerAdresseIpSolAbbildObject, this.ServerAdresseTcpSolAbbildObject, this.TimeoutMilliSolAbbildObject);
        scnitTcpKlientKümere.ScteleSicerAusfüürungFertig();
        this.AufgaabeKümereLezte = scnitTcpKlientKümere;
        if (scnitTcpKlientKümere.ScnitscteleTcpZuukünftig == beginZaitDateTime.Key)
          return;
        this.AufgaabeKümereVerbindungErscteltLezte = scnitTcpKlientKümere;
      }
    }
  }
}
