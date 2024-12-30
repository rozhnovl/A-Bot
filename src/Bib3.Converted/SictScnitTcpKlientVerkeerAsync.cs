// Decompiled with JetBrains decompiler
// Type: Bib3.SictScnitTcpKlientVerkeerAsync
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Bib3
{
  public class SictScnitTcpKlientVerkeerAsync
  {
    private readonly object Lock = new object();
    public readonly TcpClient ScnitTcpKlient;
    public readonly long InstanzBeginZaitMikro = Glob.StopwatchZaitMikroSictInt();
    private readonly List<KeyValuePair<byte[], long>> AingangTailListePuferUndZaitMikro = new List<KeyValuePair<byte[], long>>();
    private readonly List<KeyValuePair<byte[], long>> AusgangListeAusscteehendeWrite = new List<KeyValuePair<byte[], long>>();

    private IAsyncResult ScnitTcpKlientReadAsyncResult { set; get; }

    private IAsyncResult ScnitTcpKlientWriteAsyncResult { set; get; }

    public long AingangListeOktetAnzaal { private set; get; }

    public long? AingangLezteZaitStopwatchMikro { private set; get; }

    public long AusgangListeOktetAnzaal { private set; get; }

    public long AusgangListeAusscteehendeWriteAggregiirtListeOktetAnzaal
    {
      get
      {
        lock (this.AusgangListeAusscteehendeWrite)
          return (long) this.AusgangListeAusscteehendeWrite.Select<KeyValuePair<byte[], long>, int>((Func<KeyValuePair<byte[], long>, int>) (ausscteehendeWrite => ausscteehendeWrite.Key != null ? ausscteehendeWrite.Key.Length : 0)).Sum();
      }
    }

    public KeyValuePair<byte[], long>[] InspektAingangTailListePuferUndZaitMikro
    {
      get
      {
        lock (this.AingangTailListePuferUndZaitMikro)
          return this.AingangTailListePuferUndZaitMikro.ToArray();
      }
    }

    public KeyValuePair<byte[], long>[] AingangTailListePuferUndZaitMikroNimAleHeraus()
    {
      lock (this.AingangTailListePuferUndZaitMikro)
      {
        KeyValuePair<byte[], long>[] array = this.AingangTailListePuferUndZaitMikro.ToArray();
        this.AingangTailListePuferUndZaitMikro.Clear();
        return array;
      }
    }

    public bool GeegensaiteHatVerbindungBeendet { private set; get; }

    private KeyValuePair<byte[], long>? AusgangListeAusscteehendeWriteFrüüheste
    {
      get
      {
        KeyValuePair<byte[], long> keyValuePair;
        lock (this.AusgangListeAusscteehendeWrite)
          keyValuePair = this.AusgangListeAusscteehendeWrite.FirstOrDefault<KeyValuePair<byte[], long>>();
        return keyValuePair.Key == null ? new KeyValuePair<byte[], long>?() : new KeyValuePair<byte[], long>?(keyValuePair);
      }
    }

    public KeyValuePair<byte[], long>? AusgangListeAusscteehendeWriteFrüühesteMitAlterMikro
    {
      get
      {
        long num = Glob.StopwatchZaitMikroSictInt();
        KeyValuePair<byte[], long>? ausscteehendeWriteFrüüheste = this.AusgangListeAusscteehendeWriteFrüüheste;
        return !ausscteehendeWriteFrüüheste.HasValue ? new KeyValuePair<byte[], long>?() : new KeyValuePair<byte[], long>?(new KeyValuePair<byte[], long>(ausscteehendeWriteFrüüheste.Value.Key, num - this.InstanzBeginZaitMikro - ausscteehendeWriteFrüüheste.Value.Value));
      }
    }

    public SictScnitTcpKlientVerkeerAsync(TcpClient scnitTcpKlient) => this.ScnitTcpKlient = scnitTcpKlient;

    public void ScnitTcpKlientVerarbaiteAusgangUndAingang(
      int puferListeOktetAnzaal,
      int aingangListePuferAnzaalScrankeMaximum,
      byte[] zuScraibeListeOktet)
    {
      long num1 = Glob.StopwatchZaitMikroSictInt();
      long num2 = num1 - this.InstanzBeginZaitMikro;
      lock (this.Lock)
      {
        TcpClient scnitTcpKlient = this.ScnitTcpKlient;
        if (scnitTcpKlient == null || !scnitTcpKlient.Connected || scnitTcpKlient.Client == null)
          return;
        NetworkStream networkStream = (NetworkStream) null;
        try
        {
          networkStream = scnitTcpKlient.GetStream();
        }
        catch (ObjectDisposedException ex)
        {
        }
        if (networkStream == null)
          return;
        if (zuScraibeListeOktet != null && 0 < zuScraibeListeOktet.Length)
        {
          lock (this.AusgangListeAusscteehendeWrite)
            this.AusgangListeAusscteehendeWrite.Add(new KeyValuePair<byte[], long>(zuScraibeListeOktet, num2));
        }
        lock (this.AusgangListeAusscteehendeWrite)
        {
          KeyValuePair<byte[], long> keyValuePair1 = this.AusgangListeAusscteehendeWrite.FirstOrDefault<KeyValuePair<byte[], long>>();
          if (keyValuePair1.Key != null)
          {
            if (this.ScnitTcpKlientWriteAsyncResult != null && this.ScnitTcpKlientWriteAsyncResult.IsCompleted)
            {
              networkStream.EndWrite(this.ScnitTcpKlientWriteAsyncResult);
              this.ScnitTcpKlientWriteAsyncResult = (IAsyncResult) null;
              this.AusgangListeAusscteehendeWrite.RemoveAt(0);
              this.AusgangListeOktetAnzaal += (long) keyValuePair1.Key.Length;
            }
            KeyValuePair<byte[], long> keyValuePair2 = this.AusgangListeAusscteehendeWrite.FirstOrDefault<KeyValuePair<byte[], long>>();
            if (this.ScnitTcpKlientWriteAsyncResult == null && keyValuePair2.Key != null && scnitTcpKlient.Connected)
              this.ScnitTcpKlientWriteAsyncResult = networkStream.BeginWrite(keyValuePair2.Key, 0, keyValuePair2.Key.Length, (AsyncCallback) (t => { }), (object) null);
          }
        }
        for (int index = 0; index < aingangListePuferAnzaalScrankeMaximum; ++index)
        {
          if (this.ScnitTcpKlientReadAsyncResult != null)
          {
            if (this.ScnitTcpKlientReadAsyncResult.IsCompleted)
            {
              try
              {
                if (!(this.ScnitTcpKlientReadAsyncResult.AsyncState is byte[] asyncState))
                  throw new NotImplementedException("Pufer == null");
                int count = networkStream.EndRead(this.ScnitTcpKlientReadAsyncResult);
                if (count == 0)
                {
                  this.GeegensaiteHatVerbindungBeendet = true;
                  break;
                }
                KeyValuePair<byte[], long> keyValuePair = new KeyValuePair<byte[], long>(((IEnumerable<byte>) asyncState).Take<byte>(count).ToArray<byte>(), num2);
                lock (this.AingangTailListePuferUndZaitMikro)
                  this.AingangTailListePuferUndZaitMikro.Add(keyValuePair);
                this.AingangListeOktetAnzaal += (long) count;
                this.AingangLezteZaitStopwatchMikro = new long?(num1);
              }
              finally
              {
                this.ScnitTcpKlientReadAsyncResult = (IAsyncResult) null;
              }
            }
          }
          if (this.ScnitTcpKlientReadAsyncResult != null)
            break;
          byte[] numArray = new byte[puferListeOktetAnzaal];
          try
          {
            this.ScnitTcpKlientReadAsyncResult = networkStream.BeginRead(numArray, 0, numArray.Length, (AsyncCallback) (t => { }), (object) numArray);
          }
          catch (IOException ex)
          {
          }
        }
      }
    }
  }
}
