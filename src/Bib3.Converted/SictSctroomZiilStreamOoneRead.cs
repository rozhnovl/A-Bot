// Decompiled with JetBrains decompiler
// Type: Bib3.SictSctroomZiilStreamOoneRead
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

using System;

namespace Bib3
{
  public class SictSctroomZiilStreamOoneRead : SictSctroomZiilStream
  {
    public override bool CanRead => false;

    public override int Read(byte[] t0, int t1, int t2) => throw new NotSupportedException();

    public SictSctroomZiilStreamOoneRead(SictSctroomZiil<byte> sctroomLeeser)
      : base(sctroomLeeser)
    {
    }
  }
}
