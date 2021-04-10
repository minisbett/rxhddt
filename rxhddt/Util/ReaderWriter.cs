using System;
using System.IO;
using System.Text;

namespace RXHDDT.Util
{
  internal class ReplayWriter : BinaryWriter
  {
    public ReplayWriter(Stream fileStream)
      : base(fileStream, Encoding.UTF8)
    {

    }

    public override void Write(string value)
    {
      if (value == null)
        Write((byte)0);
      else
      {
        Write((byte)11);
        base.Write(value);
      }
    }

    public override void Write(byte[] buffer)
    {
      if (buffer == null)
        Write(-1);
      else
      {
        int length = buffer.Length;
        Write(length);
        if (length <= 0)
          return;
        base.Write(buffer);
      }
    }

    public void Write(DateTime dateTime)
    {
      Write(dateTime.ToUniversalTime().Ticks);
    }

    public void NormalWrite(byte[] byte_0)
    {
      base.Write(byte_0);
    }
  }
}