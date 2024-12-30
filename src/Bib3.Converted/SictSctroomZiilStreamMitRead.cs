// Decompiled with JetBrains decompiler
// Type: Bib3.SictSctroomZiilStreamMitRead
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Bib3
{
  public class SictSctroomZiilStreamMitRead : SictSctroomZiilStream
  {
    private readonly object LockRead = new object();
    private readonly Queue<byte[]> FürReadListePufer = new Queue<byte[]>();

    public bool ReadGesclose { private set; get; }

    public override bool CanRead => true;

    public void ReadScliise()
    {
      lock (this.FürReadListePufer)
        this.ReadGesclose = true;
    }

    public void FürReadPuferFüügeAin(byte[] fürReadPufer) => this.FürReadListePuferFüügeAin(new byte[1][]
    {
      fürReadPufer
    });

    public void FürReadListePuferFüügeAin(byte[][] fürReadListePufer)
    {
      if (fürReadListePufer == null)
        return;
      lock (this.FürReadListePufer)
      {
        foreach (byte[] numArray in fürReadListePufer)
        {
          if (numArray != null && numArray.Length >= 1)
            this.FürReadListePufer.Enqueue(numArray);
        }
      }
    }

    public override int Read(
      byte[] ziilPufer,
      int beginInZiilElementIndex,
      int ziilListeElementAnzaal)
    {
      if (ziilPufer == null)
        throw new ArgumentNullException("ZiilPufer");
      if ((long) ziilPufer.Length < (long) (beginInZiilElementIndex + ziilListeElementAnzaal))
        throw new ArgumentNullException("ZiilPufer.LongLength < BeginInZiilElementIndex + ZiilListeElementAnzaal");
      if (ziilListeElementAnzaal < 1)
        throw new ArgumentNullException("ZiilListeElementAnzaal < 1");
      lock (this.LockRead)
      {
        int num = 0;
        while (true)
        {
          if (!this.ReadGesclose)
          {
            int length = ziilListeElementAnzaal - num;
            int destinationIndex = beginInZiilElementIndex + num;
            byte[] sourceArray = (byte[]) null;
            lock (this.FürReadListePufer)
            {
              sourceArray = this.FürReadListePufer.FirstOrDefault<byte[]>();
              if (sourceArray == null)
              {
                if (0 < num)
                  return num;
                Thread.Sleep(10);
                continue;
              }
              this.FürReadListePufer.Dequeue();
            }
            if (length < sourceArray.Length)
            {
              Array.Copy((Array) sourceArray, 0, (Array) ziilPufer, destinationIndex, length);
              num += length;
            }
            else
            {
              Array.Copy((Array) sourceArray, 0, (Array) ziilPufer, destinationIndex, sourceArray.Length);
              num += sourceArray.Length;
            }
          }
          else
            break;
        }
        return num;
      }
    }

    public SictSctroomZiilStreamMitRead(SictSctroomZiil<byte> sctroomLeeser)
      : base(sctroomLeeser)
    {
    }
  }
}
