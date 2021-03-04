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

    public override void Write(char[] chars)
    {
      if (chars == null)
      {
        this.Write(-1);
      }
      else
      {
        int length = chars.Length;
        this.Write(length);
        if (length <= 0)
          return;
        base.Write(chars);
      }
    }

    public void Write(DateTime dateTime)
    {
      this.Write(dateTime.ToUniversalTime().Ticks);
    }

    public void method_0(object object_0)
    {
      if (object_0 == null)
      {
        this.Write((byte)0);
      }
      else
      {
        switch (object_0.GetType().Name)
        {
          case "Boolean":
            this.Write((byte)1);
            this.Write((bool)object_0);
            break;

          case "Byte":
            this.Write((byte)2);
            this.Write((byte)object_0);
            break;

          case "Byte[]":
            this.Write((byte)16);
            base.Write((byte[])object_0);
            break;

          case "Char":
            this.Write((byte)10);
            this.Write((char)object_0);
            break;

          case "Char[]":
            this.Write((byte)17);
            base.Write((char[])object_0);
            break;

          case "DateTime":
            this.Write((byte)15);
            this.Write((DateTime)object_0);
            break;

          case "Decimal":
            this.Write((byte)14);
            this.Write((Decimal)object_0);
            break;

          case "Double":
            this.Write((byte)13);
            this.Write((double)object_0);
            break;

          case "Int16":
            this.Write((byte)7);
            this.Write((short)object_0);
            break;

          case "Int32":
            this.Write((byte)8);
            this.Write((int)object_0);
            break;

          case "Int64":
            this.Write((byte)9);
            this.Write((long)object_0);
            break;

          case "SByte":
            this.Write((byte)6);
            this.Write((sbyte)object_0);
            break;

          case "Single":
            this.Write((byte)12);
            this.Write((float)object_0);
            break;

          case "String":
            this.Write((byte)11);
            base.Write((string)object_0);
            break;

          case "UInt16":
            this.Write((byte)3);
            this.Write((ushort)object_0);
            break;

          case "UInt32":
            this.Write((byte)4);
            this.Write((uint)object_0);
            break;

          case "UInt64":
            this.Write((byte)5);
            this.Write((ulong)object_0);
            break;

          default:
            this.Write((byte)18);
            new BinaryFormatter()
            {
              AssemblyFormat = FormatterAssemblyStyle.Simple,
              TypeFormat = FormatterTypeStyle.TypesWhenNeeded
            }.Serialize(this.BaseStream, object_0);
            break;
        }
      }
    }

    public void NormalWrite(byte[] byte_0)
    {
      base.Write(byte_0);
    }

    public void writeSpecial(byte[] byte_0)
    {
      if (byte_0 == null)
      {
        this.Write(-1);
      }
      else
      {
        int length = byte_0.Length;
        this.Write(length);
        if (length <= 0)
          return;
        base.Write(byte_0);
      }
    }

    internal void writeStringBytes(string string_0)
    {
      this.NormalWrite(Encoding.UTF8.GetBytes(string_0));
    }
  }
}