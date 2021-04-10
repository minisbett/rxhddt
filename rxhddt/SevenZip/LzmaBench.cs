using SevenZip.Compression.LZMA;
using System;
using System.IO;

namespace SevenZip
{
  internal abstract class LzmaBench
  {
    private const uint kAdditionalSize = 6291456;
    private const uint kCompressedAdditionalSize = 1024;
    private const uint kMaxLzmaPropSize = 10;
    private const int kSubBits = 8;

    private static uint GetLogSize(uint size)
    {
      for (int index1 = 8; index1 < 32; ++index1)
      {
        for (uint index2 = 0; index2 < 256U; ++index2)
        {
          if (size <= (uint) ((1 << index1) + ((int) index2 << index1 - 8)))
            return (uint) (index1 << 8) + index2;
        }
      }
      return 8192;
    }

    private static ulong MyMultDiv64(ulong value, ulong elapsedTime)
    {
      ulong num1 = 10000000;
      ulong num2 = elapsedTime;
      while (num1 > 1000000UL)
      {
        num1 >>= 1;
        num2 >>= 1;
      }
      if (num2 == 0UL)
        num2 = 1UL;
      return value * num1 / num2;
    }

    private static ulong GetCompressRating(uint dictionarySize, ulong elapsedTime, ulong size)
    {
      long num1 = (long) (LzmaBench.GetLogSize(dictionarySize) - 4608U);
      ulong num2 = (ulong) (1060L + (long) ((ulong) (num1 * num1) * 10UL >> 16));
      return LzmaBench.MyMultDiv64(size * num2, elapsedTime);
    }

    private static ulong GetDecompressRating(ulong elapsedTime, ulong outSize, ulong inSize)
    {
      return LzmaBench.MyMultDiv64((ulong) ((long) inSize * 220L + (long) outSize * 20L), elapsedTime);
    }

    private static ulong GetTotalRating(
      uint dictionarySize,
      ulong elapsedTimeEn,
      ulong sizeEn,
      ulong elapsedTimeDe,
      ulong inSizeDe,
      ulong outSizeDe)
    {
      return (LzmaBench.GetCompressRating(dictionarySize, elapsedTimeEn, sizeEn) + LzmaBench.GetDecompressRating(elapsedTimeDe, inSizeDe, outSizeDe)) / 2UL;
    }

    private static void PrintValue(ulong v)
    {
      string str = v.ToString();
      for (int index = 0; index + str.Length < 6; ++index)
        Console.Write(" ");
      Console.Write(str);
    }

    private static void PrintRating(ulong rating)
    {
      LzmaBench.PrintValue(rating / 1000000UL);
      Console.Write(" MIPS");
    }

    private static void PrintResults(
      uint dictionarySize,
      ulong elapsedTime,
      ulong size,
      bool decompressMode,
      ulong secondSize)
    {
      LzmaBench.PrintValue(LzmaBench.MyMultDiv64(size, elapsedTime) / 1024UL);
      Console.Write(" KB/s  ");
      LzmaBench.PrintRating(!decompressMode ? LzmaBench.GetCompressRating(dictionarySize, elapsedTime, size) : LzmaBench.GetDecompressRating(elapsedTime, size, secondSize));
    }

    public static int LzmaBenchmark(int numIterations, uint dictionarySize)
    {
      if (numIterations <= 0)
        return 0;
      if (dictionarySize < 262144U)
      {
        Console.WriteLine("\nError: dictionary size for benchmark must be >= 19 (512 KB)");
        return 1;
      }
      Console.Write("\n       Compressing                Decompressing\n\n");
      Encoder encoder = new Encoder();
      Decoder decoder = new Decoder();
      CoderPropID[] propIDs = new CoderPropID[1]
      {
        CoderPropID.DictionarySize
      };
      object[] properties = new object[1]
      {
        (object) (int) dictionarySize
      };
      uint bufferSize = dictionarySize + 6291456U;
      int capacity = (int) (bufferSize / 2U) + 1024;
      encoder.SetCoderProperties(propIDs, properties);
      MemoryStream memoryStream1 = new MemoryStream();
      encoder.WriteCoderProperties((Stream) memoryStream1);
      byte[] array = memoryStream1.ToArray();
      LzmaBench.CBenchRandomGenerator cbenchRandomGenerator = new LzmaBench.CBenchRandomGenerator();
      cbenchRandomGenerator.Set(bufferSize);
      cbenchRandomGenerator.Generate();
      CRC crc = new CRC();
      crc.Init();
      crc.Update(cbenchRandomGenerator.Buffer, 0U, cbenchRandomGenerator.BufferSize);
      LzmaBench.CProgressInfo cprogressInfo = new LzmaBench.CProgressInfo();
      cprogressInfo.ApprovedStart = (long) dictionarySize;
      ulong size1 = 0;
      ulong elapsedTime1 = 0;
      ulong elapsedTime2 = 0;
      ulong secondSize = 0;
      MemoryStream memoryStream2 = new MemoryStream(cbenchRandomGenerator.Buffer, 0, (int) cbenchRandomGenerator.BufferSize);
      MemoryStream memoryStream3 = new MemoryStream(capacity);
      LzmaBench.CrcOutStream crcOutStream = new LzmaBench.CrcOutStream();
      for (int index1 = 0; index1 < numIterations; ++index1)
      {
        cprogressInfo.Init();
        memoryStream2.Seek(0L, SeekOrigin.Begin);
        memoryStream3.Seek(0L, SeekOrigin.Begin);
        encoder.Code((Stream) memoryStream2, (Stream) memoryStream3, -1L, -1L, (ICodeProgress) cprogressInfo);
        ulong ticks = (ulong) (DateTime.UtcNow - cprogressInfo.Time).Ticks;
        long position = memoryStream3.Position;
        if (cprogressInfo.InSize == 0L)
          throw new Exception("Internal ERROR 1282");
        ulong elapsedTime3 = 0;
        for (int index2 = 0; index2 < 2; ++index2)
        {
          memoryStream3.Seek(0L, SeekOrigin.Begin);
          crcOutStream.Init();
          decoder.SetDecoderProperties(array);
          ulong num = (ulong) bufferSize;
          DateTime utcNow = DateTime.UtcNow;
          decoder.Code((Stream) memoryStream3, (Stream) crcOutStream, 0L, (long) num, (ICodeProgress) null);
          elapsedTime3 = (ulong) (DateTime.UtcNow - utcNow).Ticks;
          if ((int) crcOutStream.GetDigest() != (int) crc.GetDigest())
            throw new Exception("CRC Error");
        }
        ulong size2 = (ulong) bufferSize - (ulong) cprogressInfo.InSize;
        LzmaBench.PrintResults(dictionarySize, ticks, size2, false, 0UL);
        Console.Write("     ");
        LzmaBench.PrintResults(dictionarySize, elapsedTime3, (ulong) bufferSize, true, (ulong) position);
        Console.WriteLine();
        size1 += size2;
        elapsedTime1 += ticks;
        elapsedTime2 += elapsedTime3;
        secondSize += (ulong) position;
      }
      Console.WriteLine("---------------------------------------------------");
      LzmaBench.PrintResults(dictionarySize, elapsedTime1, size1, false, 0UL);
      Console.Write("     ");
      LzmaBench.PrintResults(dictionarySize, elapsedTime2, (ulong) bufferSize * (ulong) numIterations, true, secondSize);
      Console.WriteLine("    Average");
      return 0;
    }

    private class CRandomGenerator
    {
      private uint A1;
      private uint A2;

      public CRandomGenerator()
      {
        this.Init();
      }

      public void Init()
      {
        this.A1 = 362436069U;
        this.A2 = 521288629U;
      }

      public uint GetRnd()
      {
        return (this.A1 = (uint) (36969 * ((int) this.A1 & (int) ushort.MaxValue)) + (this.A1 >> 16)) << 16 ^ (this.A2 = (uint) (18000 * ((int) this.A2 & (int) ushort.MaxValue)) + (this.A2 >> 16));
      }
    }

    private class CBitRandomGenerator
    {
      private LzmaBench.CRandomGenerator RG = new LzmaBench.CRandomGenerator();
      private uint Value;
      private int NumBits;

      public void Init()
      {
        this.Value = 0U;
        this.NumBits = 0;
      }

      public uint GetRnd(int numBits)
      {
        if (this.NumBits > numBits)
        {
          int num = (int) this.Value & (1 << numBits) - 1;
          this.Value >>= numBits;
          this.NumBits -= numBits;
          return (uint) num;
        }
        numBits -= this.NumBits;
        int num1 = (int) this.Value << numBits;
        this.Value = this.RG.GetRnd();
        int num2 = (int) this.Value & (1 << numBits) - 1;
        int num3 = num1 | num2;
        this.Value >>= numBits;
        this.NumBits = 32 - numBits;
        return (uint) num3;
      }
    }

    private class CBenchRandomGenerator
    {
      private LzmaBench.CBitRandomGenerator RG = new LzmaBench.CBitRandomGenerator();
      private uint Pos;
      private uint Rep0;
      public uint BufferSize;
      public byte[] Buffer;

      public void Set(uint bufferSize)
      {
        this.Buffer = new byte[(int) bufferSize];
        this.Pos = 0U;
        this.BufferSize = bufferSize;
      }

      private uint GetRndBit()
      {
        return this.RG.GetRnd(1);
      }

      private uint GetLogRandBits(int numBits)
      {
        return this.RG.GetRnd((int) this.RG.GetRnd(numBits));
      }

      private uint GetOffset()
      {
        if (this.GetRndBit() == 0U)
          return this.GetLogRandBits(4);
        return this.GetLogRandBits(4) << 10 | this.RG.GetRnd(10);
      }

      private uint GetLen1()
      {
        return this.RG.GetRnd(1 + (int) this.RG.GetRnd(2));
      }

      private uint GetLen2()
      {
        return this.RG.GetRnd(2 + (int) this.RG.GetRnd(2));
      }

      public void Generate()
      {
        this.RG.Init();
        this.Rep0 = 1U;
label_10:
        while (this.Pos < this.BufferSize)
        {
          if (this.GetRndBit() == 0U || this.Pos < 1U)
          {
            this.Buffer[(int) this.Pos++] = (byte) this.RG.GetRnd(8);
          }
          else
          {
            uint num1;
            if (this.RG.GetRnd(3) == 0U)
            {
              num1 = 1U + this.GetLen1();
            }
            else
            {
              do
              {
                this.Rep0 = this.GetOffset();
              }
              while (this.Rep0 >= this.Pos);
              ++this.Rep0;
              num1 = 2U + this.GetLen2();
            }
            uint num2 = 0;
            while (true)
            {
              if (num2 < num1 && this.Pos < this.BufferSize)
              {
                this.Buffer[(int) this.Pos] = this.Buffer[(int) this.Pos - (int) this.Rep0];
                ++num2;
                ++this.Pos;
              }
              else
                goto label_10;
            }
          }
        }
      }
    }

    private class CrcOutStream : Stream
    {
      public CRC CRC = new CRC();

      public void Init()
      {
        this.CRC.Init();
      }

      public uint GetDigest()
      {
        return this.CRC.GetDigest();
      }

      public override bool CanRead
      {
        get
        {
          return false;
        }
      }

      public override bool CanSeek
      {
        get
        {
          return false;
        }
      }

      public override bool CanWrite
      {
        get
        {
          return true;
        }
      }

      public override long Length
      {
        get
        {
          return 0;
        }
      }

      public override long Position
      {
        get
        {
          return 0;
        }
        set
        {
        }
      }

      public override void Flush()
      {
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
        return 0;
      }

      public override void SetLength(long value)
      {
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
        return 0;
      }

      public override void WriteByte(byte b)
      {
        this.CRC.UpdateByte(b);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
        this.CRC.Update(buffer, (uint) offset, (uint) count);
      }
    }

    private class CProgressInfo : ICodeProgress
    {
      public long ApprovedStart;
      public long InSize;
      public DateTime Time;

      public void Init()
      {
        this.InSize = 0L;
      }

      public void SetProgress(long inSize, long outSize)
      {
        if (inSize < this.ApprovedStart || this.InSize != 0L)
          return;
        this.Time = DateTime.UtcNow;
        this.InSize = inSize;
      }
    }
  }
}
