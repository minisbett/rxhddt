using System;
using System.IO;
using System.Text;
using System.Threading;

namespace RXHDDT.Util
{
  internal class BeatmapReader : BinaryReader
  {
    public BeatmapReader(Stream stream_0)
      : base(stream_0, Encoding.UTF8)
    {
    }

    public override string ReadString()
    {
      if (this.ReadByte() == (byte)0)
        return (string)null;
      return base.ReadString();
    }

    public byte[] ReadByteArray()
    {
      int count = this.ReadInt32();
      if (count > 0)
        return this.ReadBytes(count);
      if (count < 0)
        return (byte[])null;
      return new byte[0];
    }

    public char[] ReadCharArray()
    {
      int count = this.ReadInt32();
      if (count > 0)
        return this.ReadChars(count);
      if (count < 0)
        return (char[])null;
      return new char[0];
    }

    public DateTime ReadDateTime()
    {
      long ticks = this.ReadInt64();
      if (ticks < 0L)
        throw new AbandonedMutexException("oops");
      return new DateTime(ticks, DateTimeKind.Utc);
    }

    public object method_4()
    {
      switch (this.ReadByte())
      {
        case 1:
          return (object)this.ReadBoolean();
        case 2:
          return (object)this.ReadByte();
        case 3:
          return (object)this.ReadUInt16();
        case 4:
          return (object)this.ReadUInt32();
        case 5:
          return (object)this.ReadUInt64();
        case 6:
          return (object)this.ReadSByte();
        case 7:
          return (object)this.ReadInt16();
        case 8:
          return (object)this.ReadInt32();
        case 9:
          return (object)this.ReadInt64();
        case 10:
          return (object)this.ReadChar();
        case 11:
          return (object)base.ReadString();
        case 12:
          return (object)this.ReadSingle();
        case 13:
          return (object)this.ReadDouble();
        case 14:
          return (object)this.ReadDecimal();
        case 15:
          return (object)this.ReadDateTime();
        case 16:
          return (object)this.ReadByteArray();
        case 17:
          return (object)this.ReadCharArray();
        default:
          return (object)null;
      }
    }
  }
}
