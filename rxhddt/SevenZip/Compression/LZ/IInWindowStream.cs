// Decompiled with JetBrains decompiler
// Type: SevenZip.Compression.LZ.IInWindowStream
// Assembly: osu!ftw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 43BAB63D-DE2A-4E38-86D4-2EF193F865F8
// Assembly location: C:\Users\Niklas\Downloads\aac_Portable\osu!\osu!ftw.exe

using System.IO;

namespace SevenZip.Compression.LZ
{
  internal interface IInWindowStream
  {
    void SetStream(Stream inStream);

    void Init();

    void ReleaseStream();

    byte GetIndexByte(int index);

    uint GetMatchLen(int index, uint distance, uint limit);

    uint GetNumAvailableBytes();
  }
}
