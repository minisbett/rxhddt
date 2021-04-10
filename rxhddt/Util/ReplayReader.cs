using System;
using System.IO;
using System.Text;
using System.Threading;

namespace RXHDDT.Util
{
  internal class ReplayReader : BinaryReader
  {
    public ReplayReader(Stream stream_0)
      : base(stream_0, Encoding.UTF8)
    {
    }

    public override string ReadString()
    {
      if (ReadByte() == 0)
        return null;
      return base.ReadString();
    }

    public byte[] ReadByteArray()
    {
      int count = ReadInt32();
      if (count > 0)
        return ReadBytes(count);
      if (count < 0)
        return null;
      return new byte[0];
    }

    public char[] ReadCharArray()
    {
      int count = ReadInt32();
      if (count > 0)
        return ReadChars(count);
      if (count < 0)
        return null;
      return new char[0];
    }

    public DateTime ReadDateTime()
    {
      long ticks = ReadInt64();
      if (ticks < 0L)
        throw new AbandonedMutexException("oops");
      return new DateTime(ticks, DateTimeKind.Utc);
    }
  }
}
