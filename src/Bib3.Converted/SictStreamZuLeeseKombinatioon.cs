// Decompiled with JetBrains decompiler
// Type: Bib3.SictStreamZuLeeseKombinatioon
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bib3
{
  public class SictStreamZuLeeseKombinatioon : Stream
  {
    private readonly object Lock = new object();
    private readonly Stream[] ListeStream;
    private long InternPosition = 0;

    public SictStreamZuLeeseKombinatioon(Stream[] listeStream)
    {
      if (listeStream == null)
        throw new ArgumentNullException(nameof (ListeStream));
      for (int index = 0; index < listeStream.Length; ++index)
      {
        try
        {
          Stream stream = listeStream[index];
          if (stream != null)
          {
            if (!stream.CanRead)
              throw new ArgumentException("!Stream.CanRead");
            if (!stream.CanSeek)
              throw new ArgumentException("!Stream.CanSeek");
          }
        }
        catch (Exception ex)
        {
          throw new ArgumentException("Stream[" + index.ToString() + "]", ex);
        }
      }
      this.ListeStream = listeStream;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override bool CanTimeout => false;

    public override long Length
    {
      get
      {
        long length = 0;
        foreach (Stream stream in this.ListeStream)
        {
          if (stream != null)
            length += stream.Length;
        }
        return length;
      }
    }

    public override long Position
    {
      set
      {
        lock (this.Lock)
          this.InternPosition = value;
      }
      get => this.InternPosition;
    }

    public override void Flush() => throw new NotSupportedException();

    public override void SetLength(long t) => throw new NotSupportedException();

    public override void Write(byte[] t0, int t1, int t2) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin)
    {
      lock (this.Lock)
      {
        switch (origin)
        {
          case SeekOrigin.Begin:
            this.Position = offset;
            break;
          case SeekOrigin.Current:
            this.Position += offset;
            break;
          case SeekOrigin.End:
            this.Position = this.Length + offset;
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof (origin));
        }
        return this.Position;
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      lock (this.Lock)
      {
        if (buffer == null)
          throw new ArgumentNullException(nameof (buffer));
        if (offset < 0)
          throw new ArgumentOutOfRangeException("offset < 0");
        if (count < 0)
          throw new ArgumentOutOfRangeException("count < 0");
        if (buffer.Length < offset + count)
          throw new ArgumentException("buffer.Length < offset + count");
        List<byte[]> source = new List<byte[]>();
        long num1 = 0;
        int num2 = 0;
        for (int index = 0; index < this.ListeStream.Length; ++index)
        {
          int num3 = source.Select<byte[], int>((Func<byte[], int>) (puffer => puffer.Length)).Sum();
          int val2 = count - num3;
          if (val2 > 0)
          {
            Stream stream = this.ListeStream[index];
            if (stream != null)
            {
              long offset1 = this.Position - num1;
              if (2147482624L >= this.Position)
                ;
              if (offset1 < 0L)
                throw new NotImplementedException();
              long val1 = stream.Length - offset1;
              if (0L < val1)
              {
                byte[] buffer1 = new byte[Math.Min(val1, (long) val2)];
                stream.Seek(offset1, SeekOrigin.Begin);
                int num4 = stream.Read(buffer1, 0, buffer1.Length);
                if (num4 < buffer1.Length)
                  throw new NotImplementedException("GeleeseAnzaal < Puffer.Length");
                source.Add(buffer1);
                this.Position += (long) num4;
              }
              num1 += stream.Length;
            }
          }
          else
            break;
        }
        num2 = source.Select<byte[], int>((Func<byte[], int>) (puffer => puffer.Length)).Sum();
        int index1 = 0;
        for (int index2 = 0; index2 < source.Count; ++index2)
        {
          byte[] numArray = source[index2];
          numArray.CopyTo((Array) buffer, index1);
          index1 += numArray.Length;
        }
        return index1;
      }
    }
  }
}
