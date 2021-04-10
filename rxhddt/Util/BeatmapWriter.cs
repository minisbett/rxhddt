// Decompiled with JetBrains decompiler
// Type: osu_ftw.BeatmapWriter
// Assembly: osu!ftw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 43BAB63D-DE2A-4E38-86D4-2EF193F865F8
// Assembly location: C:\Users\Niklas\Downloads\aac_Portable\osu!\osu!ftw.exe

using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RXHDDT.Util
{
  internal class BeatmapWriter : BinaryWriter
  {
    public BeatmapWriter(Stream fileStream)
      : base(fileStream, Encoding.UTF8)
    {

    }

    public override void Write(string value)
    {
      if (value == null)
      {
        this.Write((byte)0);
      }
      else
      {
        this.Write((byte)11);
        base.Write(value);
      }
    }

    public override void Write(byte[] buffer)
    {
      if (buffer == null)
      {
        this.Write(-1);
      }
      else
      {
        int length = buffer.Length;
        this.Write(length);
        if (length <= 0)
          return;
        base.Write(buffer);
      }
    }

    public void Write(DateTime dateTime)
    {
      this.Write(dateTime.ToUniversalTime().Ticks);
    }

    public void NormalWrite(byte[] byte_0)
    {
      base.Write(byte_0);
    }
  }
}