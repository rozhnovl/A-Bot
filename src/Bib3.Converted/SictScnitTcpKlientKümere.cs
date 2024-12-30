// Decompiled with JetBrains decompiler
// Type: Bib3.SictScnitTcpKlientKümere
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml.Linq;

namespace Bib3
{
  public class SictScnitTcpKlientKümere : SictAufgaabe
  {
    public readonly TcpClient ScnitsteleTcpBisher;
    public readonly bool ScnitsteleTcpErscteltSol;
    public readonly object ServerAdreseIpSolAbbildObject;
    public readonly object ServerAdreseTcpAbbildObject;
    public readonly object TimeoutMiliSolAbbildObject;

    public SictScnitTcpKlientKümere(
      TcpClient scnitsteleTcpBisher,
      bool scnitsteleTcpErscteltSol,
      object serverAdreseIpSolAbbildObject,
      object serverAdreseTcpAbbildObject,
      object timeoutMiliSolAbbildObject)
    {
      this.ScnitsteleTcpBisher = scnitsteleTcpBisher;
      this.ScnitsteleTcpErscteltSol = scnitsteleTcpErscteltSol;
      this.ServerAdreseIpSolAbbildObject = serverAdreseIpSolAbbildObject;
      this.ServerAdreseTcpAbbildObject = serverAdreseTcpAbbildObject;
      this.TimeoutMiliSolAbbildObject = timeoutMiliSolAbbildObject;
    }

    public XObject[] ListeEraignis { private set; get; }

    public DateTime BeginZaitDateTime { private set; get; }

    public long BeginZaitStopwatchMikro { private set; get; }

    public Exception ScnitscteleTcpKümereAusnaame { private set; get; }

    public TcpClient ScnitscteleTcpZuukünftig { private set; get; }

    public bool ScnitscteleTcpBisherErhalte { private set; get; }

    public XObject[] ScnitscteleTcpErscteleListeEraignis { private set; get; }

    protected override void FüüreAusLocked()
    {
      this.BeginZaitStopwatchMikro = Glob.StopwatchZaitMikroSictInt();
      this.BeginZaitDateTime = Glob.SictDateTimeVonStopwatchZaitMikro(this.BeginZaitStopwatchMikro);
      List<XObject> xobjectList1 = new List<XObject>();
      try
      {
        xobjectList1.Add((XObject) new XAttribute((XName) "Begin.Zait", (object) this.BeginZaitDateTime.SictwaiseKalenderString(".")));
        IPAddress abbildIPAddress;
        xobjectList1.Add((XObject) new XElement((XName) "Server.Adrese.Ip.Sol", (object[]) Glob.SictwaiseIPAddress(this.ServerAdreseIpSolAbbildObject, out abbildIPAddress)));
        int? sictInt32Abbild1;
        xobjectList1.Add((XObject) new XElement((XName) "Server.Adrese.Tcp.Sol", (object[]) Glob.SictwaiseInt32(this.ServerAdreseTcpAbbildObject, out sictInt32Abbild1)));
        int? sictInt32Abbild2;
        xobjectList1.Add((XObject) new XElement((XName) "Timeout.Milli.Sol", (object[]) Glob.SictwaiseInt32(this.TimeoutMiliSolAbbildObject, out sictInt32Abbild2)));
        try
        {
          xobjectList1.Add((XObject) new XAttribute((XName) "ScnitsteleTcp.Ersctelt.Soll", (object) this.ScnitsteleTcpErscteltSol));
          xobjectList1.Add((XObject) new XAttribute((XName) "ScnitsteleTcp.Bisher.Ersctelt", (object) (this.ScnitsteleTcpBisher != null)));
          if (this.ScnitsteleTcpErscteltSol)
          {
            if (this.ScnitsteleTcpBisher == null)
            {
              List<XObject> content = new List<XObject>();
              try
              {
                content.Add((XObject) new XAttribute((XName) "ScnitsteleTcp.Server.Adresse.Ip.Soll", abbildIPAddress == null ? (object) "null" : (object) abbildIPAddress.ToString()));
                List<XObject> xobjectList2 = content;
                XName name1 = (XName) "ScnitsteleTcp.Server.Adresse.Tcp.Soll";
                int num;
                string str1;
                if (sictInt32Abbild1.HasValue)
                {
                  num = sictInt32Abbild1.Value;
                  str1 = num.ToString();
                }
                else
                  str1 = "null";
                XAttribute xattribute1 = new XAttribute(name1, (object) str1);
                xobjectList2.Add((XObject) xattribute1);
                List<XObject> xobjectList3 = content;
                XName name2 = (XName) "ScnitsteleTcp.Timeout.Milli";
                string str2;
                if (sictInt32Abbild2.HasValue)
                {
                  num = sictInt32Abbild2.Value;
                  str2 = num.ToString();
                }
                else
                  str2 = "null";
                XAttribute xattribute2 = new XAttribute(name2, (object) str2);
                xobjectList3.Add((XObject) xattribute2);
                if (!sictInt32Abbild1.HasValue)
                  throw new ArgumentNullException("ServerAdresseTcpSoll");
                if (!sictInt32Abbild2.HasValue)
                  throw new ArgumentNullException("TimeoutMilliSoll");
                TcpClient tcpClient = new TcpClient();
                IAsyncResult asyncResult = tcpClient.BeginConnect(abbildIPAddress, sictInt32Abbild1.Value, (AsyncCallback) (t => { }), (object) null);
                DateTime now = DateTime.Now;
                while ((DateTime.Now - now).TotalMilliseconds < (double) sictInt32Abbild2.Value && !asyncResult.IsCompleted)
                  Thread.Sleep(10);
                if (!asyncResult.IsCompleted)
                  throw new TimeoutException("BeginConnect");
                tcpClient.EndConnect(asyncResult);
                content.Add((XObject) new XAttribute((XName) "ScnitsteleTcp.Connected", (object) tcpClient.Connected));
                if (!tcpClient.Connected)
                  return;
                this.ScnitscteleTcpZuukünftig = tcpClient;
              }
              finally
              {
                content.Add((XObject) new XAttribute((XName) "Erfolg", (object) (this.ScnitscteleTcpZuukünftig != null)));
                this.ScnitscteleTcpErscteleListeEraignis = content.ToArray();
                xobjectList1.Add((XObject) new XElement((XName) "ScnitsteleTcpErsctele", (object) content));
              }
            }
            else
            {
              bool connected = this.ScnitsteleTcpBisher.Connected;
              xobjectList1.Add((XObject) new XAttribute((XName) "ScnitscteleTcp.Bisher.Connected", (object) connected));
              if (connected)
              {
                Socket client = this.ScnitsteleTcpBisher.Client;
                if (client != null && client.RemoteEndPoint is IPEndPoint remoteEndPoint)
                {
                  bool flag1 = remoteEndPoint.Address.Equals((object) abbildIPAddress);
                  int port = remoteEndPoint.Port;
                  int? nullable = sictInt32Abbild1;
                  int valueOrDefault = nullable.GetValueOrDefault();
                  bool flag2 = port == valueOrDefault && nullable.HasValue;
                  if (flag1 & flag2)
                  {
                    this.ScnitscteleTcpBisherErhalte = true;
                    this.ScnitscteleTcpZuukünftig = this.ScnitsteleTcpBisher;
                  }
                }
              }
            }
          }
          else if (this.ScnitsteleTcpBisher != null)
            this.ScnitsteleTcpBisher.Close();
        }
        finally
        {
        }
      }
      catch (Exception ex)
      {
        this.ScnitscteleTcpKümereAusnaame = ex;
        xobjectList1.Add((XObject) Glob.SictwaiseXElement(ex));
      }
      finally
      {
        double totalSeconds = (DateTime.Now - this.BeginZaitDateTime).TotalSeconds;
        xobjectList1.Add((XObject) new XAttribute((XName) "Dauer", (object) totalSeconds.ToString((IFormatProvider) Glob.NumberFormat)));
        this.ListeEraignis = xobjectList1.ToArray();
      }
    }
  }
}
