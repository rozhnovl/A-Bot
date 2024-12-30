// Decompiled with JetBrains decompiler
// Type: Bib3.WebSocket.SictWebSocketFrameOpcodeTyp
// Assembly: Bib3, Version=1606.2109.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 85A2D630-9346-4542-9F17-28F4E4384BAA
// Assembly location: C:\Src\A-Bot\lib\Bib3.dll

namespace Bib3.WebSocket
{
  public enum SictWebSocketFrameOpcodeTyp
  {
    ContinuationFrame = 0,
    TextFrame = 1,
    BinaryFrame = 2,
    ConnectionClose = 8,
    Ping = 9,
    Pong = 10, // 0x0000000A
  }
}
