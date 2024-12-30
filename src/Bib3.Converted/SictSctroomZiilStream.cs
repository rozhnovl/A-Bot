// Decompiled with JetBrains decompiler
// Type: Bib3.SictSctroomZiilStream
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;
using System.IO;

namespace Bib3
{
  public abstract class SictSctroomZiilStream : Stream
  {
    public readonly SictSctroomZiil<byte> SctroomLeeser;

    public SictSctroomZiilStream(SictSctroomZiil<byte> sctroomLeeser) => this.SctroomLeeser = sctroomLeeser;

    public override void Write(
      byte[] puferListeElement,
      int beginInPuferElementIndex,
      int elementAnzaal)
    {
      this.SctroomLeeser?.FüügeAin(puferListeElement, (long) beginInPuferElementIndex, (long) elementAnzaal);
    }

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override long Seek(long t, SeekOrigin t1) => throw new NotSupportedException();

    public override void SetLength(long t) => throw new NotSupportedException();
  }
}
