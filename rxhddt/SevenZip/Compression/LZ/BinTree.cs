using System;
using System.IO;

namespace SevenZip.Compression.LZ
{
  public class BinTree : InWindow, IMatchFinder, IInWindowStream
  {
    private uint _cutValue = (uint) byte.MaxValue;
    private bool HASH_ARRAY = true;
    private uint kMinMatchCheck = 4;
    private uint kFixHashSize = 66560;
    private uint _cyclicBufferPos;
    private uint _cyclicBufferSize;
    private uint _matchMaxLen;
    private uint[] _son;
    private uint[] _hash;
    private uint _hashMask;
    private uint _hashSizeSum;
    private const uint kHash2Size = 1024;
    private const uint kHash3Size = 65536;
    private const uint kBT2HashSize = 65536;
    private const uint kStartMaxLen = 1;
    private const uint kHash3Offset = 1024;
    private const uint kEmptyHashValue = 0;
    private const uint kMaxValForNormalize = 2147483647;
    private uint kNumHashDirectBytes;

    public void SetType(int numHashBytes)
    {
      this.HASH_ARRAY = numHashBytes > 2;
      if (this.HASH_ARRAY)
      {
        this.kNumHashDirectBytes = 0U;
        this.kMinMatchCheck = 4U;
        this.kFixHashSize = 66560U;
      }
      else
      {
        this.kNumHashDirectBytes = 2U;
        this.kMinMatchCheck = 3U;
        this.kFixHashSize = 0U;
      }
    }

    public new void SetStream(Stream stream)
    {
      base.SetStream(stream);
    }

    public new void ReleaseStream()
    {
      base.ReleaseStream();
    }

    public new void Init()
    {
      base.Init();
      for (uint index = 0; index < this._hashSizeSum; ++index)
        this._hash[(int) index] = 0U;
      this._cyclicBufferPos = 0U;
      this.ReduceOffsets(-1);
    }

    public new void MovePos()
    {
      if (++this._cyclicBufferPos >= this._cyclicBufferSize)
        this._cyclicBufferPos = 0U;
      base.MovePos();
      if (this._pos != (uint) int.MaxValue)
        return;
      this.Normalize();
    }

    public new byte GetIndexByte(int index)
    {
      return base.GetIndexByte(index);
    }

    public new uint GetMatchLen(int index, uint distance, uint limit)
    {
      return base.GetMatchLen(index, distance, limit);
    }

    public new uint GetNumAvailableBytes()
    {
      return base.GetNumAvailableBytes();
    }

    public void Create(
      uint historySize,
      uint keepAddBufferBefore,
      uint matchMaxLen,
      uint keepAddBufferAfter)
    {
      if (historySize > 2147483391U)
        throw new Exception();
      this._cutValue = 16U + (matchMaxLen >> 1);
      uint keepSizeReserv = (historySize + keepAddBufferBefore + matchMaxLen + keepAddBufferAfter) / 2U + 256U;
      this.Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, keepSizeReserv);
      this._matchMaxLen = matchMaxLen;
      uint num1 = historySize + 1U;
      if ((int) this._cyclicBufferSize != (int) num1)
        this._son = new uint[(int) (this._cyclicBufferSize = num1) * 2];
      uint num2 = 65536;
      if (this.HASH_ARRAY)
      {
        int num3 = (int) (historySize - 1U);
        int num4 = (int) ((uint) num3 | (uint) num3 >> 1);
        int num5 = (int) ((uint) num4 | (uint) num4 >> 2);
        int num6 = (int) ((uint) num5 | (uint) num5 >> 4);
        uint num7 = ((uint) num6 | (uint) num6 >> 8) >> 1 | (uint) ushort.MaxValue;
        if (num7 > 16777216U)
          num7 >>= 1;
        this._hashMask = num7;
        num2 = num7 + 1U + this.kFixHashSize;
      }
      if ((int) num2 == (int) this._hashSizeSum)
        return;
      this._hash = new uint[(int) (this._hashSizeSum = num2)];
    }

    public uint GetMatches(uint[] distances)
    {
      uint num1;
      if (this._pos + this._matchMaxLen <= this._streamPos)
      {
        num1 = this._matchMaxLen;
      }
      else
      {
        num1 = this._streamPos - this._pos;
        if (num1 < this.kMinMatchCheck)
        {
          this.MovePos();
          return 0;
        }
      }
      uint num2 = 0;
      uint num3 = this._pos > this._cyclicBufferSize ? this._pos - this._cyclicBufferSize : 0U;
      uint num4 = this._bufferOffset + this._pos;
      uint num5 = 1;
      uint num6 = 0;
      uint num7 = 0;
      uint num8;
      if (this.HASH_ARRAY)
      {
        int num9 = (int) CRC.Table[(int) this._bufferBase[(int) num4]] ^ (int) this._bufferBase[(int) num4 + 1];
        num6 = (uint) (num9 & 1023);
        int num10 = num9 ^ (int) this._bufferBase[(int) num4 + 2] << 8;
        num7 = (uint) (num10 & (int) ushort.MaxValue);
        num8 = (uint) (num10 ^ (int) CRC.Table[(int) this._bufferBase[(int) num4 + 3]] << 5) & this._hashMask;
      }
      else
        num8 = (uint) this._bufferBase[(int) num4] ^ (uint) this._bufferBase[(int) num4 + 1] << 8;
      uint num11 = this._hash[(int) this.kFixHashSize + (int) num8];
      if (this.HASH_ARRAY)
      {
        uint num9 = this._hash[(int) num6];
        uint num10 = this._hash[1024 + (int) num7];
        this._hash[(int) num6] = this._pos;
        this._hash[1024 + (int) num7] = this._pos;
        if (num9 > num3 && (int) this._bufferBase[(int) this._bufferOffset + (int) num9] == (int) this._bufferBase[(int) num4])
        {
          uint[] numArray1 = distances;
          int index1 = (int) num2;
          uint num12 = (uint) (index1 + 1);
          int num13;
          num5 = (uint) (num13 = 2);
          numArray1[index1] = (uint) num13;
          uint[] numArray2 = distances;
          int index2 = (int) num12;
          num2 = (uint) (index2 + 1);
          int num14 = (int) this._pos - (int) num9 - 1;
          numArray2[index2] = (uint) num14;
        }
        if (num10 > num3 && (int) this._bufferBase[(int) this._bufferOffset + (int) num10] == (int) this._bufferBase[(int) num4])
        {
          if ((int) num10 == (int) num9)
            num2 -= 2U;
          uint[] numArray1 = distances;
          int index1 = (int) num2;
          uint num12 = (uint) (index1 + 1);
          int num13;
          num5 = (uint) (num13 = 3);
          numArray1[index1] = (uint) num13;
          uint[] numArray2 = distances;
          int index2 = (int) num12;
          num2 = (uint) (index2 + 1);
          int num14 = (int) this._pos - (int) num10 - 1;
          numArray2[index2] = (uint) num14;
          num9 = num10;
        }
        if (num2 != 0U && (int) num9 == (int) num11)
        {
          num2 -= 2U;
          num5 = 1U;
        }
      }
      this._hash[(int) this.kFixHashSize + (int) num8] = this._pos;
      uint num15 = (uint) (((int) this._cyclicBufferPos << 1) + 1);
      uint num16 = this._cyclicBufferPos << 1;
      uint val2;
      uint val1 = val2 = this.kNumHashDirectBytes;
      if (this.kNumHashDirectBytes != 0U && num11 > num3 && (int) this._bufferBase[(int) this._bufferOffset + (int) num11 + (int) this.kNumHashDirectBytes] != (int) this._bufferBase[(int) num4 + (int) this.kNumHashDirectBytes])
      {
        uint[] numArray1 = distances;
        int index1 = (int) num2;
        uint num9 = (uint) (index1 + 1);
        int numHashDirectBytes;
        num5 = (uint) (numHashDirectBytes = (int) this.kNumHashDirectBytes);
        numArray1[index1] = (uint) numHashDirectBytes;
        uint[] numArray2 = distances;
        int index2 = (int) num9;
        num2 = (uint) (index2 + 1);
        int num10 = (int) this._pos - (int) num11 - 1;
        numArray2[index2] = (uint) num10;
      }
      uint cutValue = this._cutValue;
      while (num11 > num3 && cutValue-- != 0U)
      {
        uint num9 = this._pos - num11;
        uint num10 = (uint) ((num9 <= this._cyclicBufferPos ? (int) this._cyclicBufferPos - (int) num9 : (int) this._cyclicBufferPos - (int) num9 + (int) this._cyclicBufferSize) << 1);
        uint num12 = this._bufferOffset + num11;
        uint num13 = Math.Min(val1, val2);
        if ((int) this._bufferBase[(int) num12 + (int) num13] == (int) this._bufferBase[(int) num4 + (int) num13])
        {
          do
            ;
          while ((int) ++num13 != (int) num1 && (int) this._bufferBase[(int) num12 + (int) num13] == (int) this._bufferBase[(int) num4 + (int) num13]);
          if (num5 < num13)
          {
            uint[] numArray1 = distances;
            int index1 = (int) num2;
            uint num14 = (uint) (index1 + 1);
            int num17;
            num5 = (uint) (num17 = (int) num13);
            numArray1[index1] = (uint) num17;
            uint[] numArray2 = distances;
            int index2 = (int) num14;
            num2 = (uint) (index2 + 1);
            int num18 = (int) num9 - 1;
            numArray2[index2] = (uint) num18;
            if ((int) num13 == (int) num1)
            {
              this._son[(int) num16] = this._son[(int) num10];
              this._son[(int) num15] = this._son[(int) num10 + 1];
              goto label_29;
            }
          }
        }
        if ((int) this._bufferBase[(int) num12 + (int) num13] < (int) this._bufferBase[(int) num4 + (int) num13])
        {
          this._son[(int) num16] = num11;
          num16 = num10 + 1U;
          num11 = this._son[(int) num16];
          val2 = num13;
        }
        else
        {
          this._son[(int) num15] = num11;
          num15 = num10;
          num11 = this._son[(int) num15];
          val1 = num13;
        }
      }
      this._son[(int) num15] = this._son[(int) num16] = 0U;
label_29:
      this.MovePos();
      return num2;
    }

    public void Skip(uint num)
    {
      do
      {
        uint num1;
        if (this._pos + this._matchMaxLen <= this._streamPos)
        {
          num1 = this._matchMaxLen;
        }
        else
        {
          num1 = this._streamPos - this._pos;
          if (num1 < this.kMinMatchCheck)
          {
            this.MovePos();
            goto label_19;
          }
        }
        uint num2 = this._pos > this._cyclicBufferSize ? this._pos - this._cyclicBufferSize : 0U;
        uint num3 = this._bufferOffset + this._pos;
        uint num4;
        if (this.HASH_ARRAY)
        {
          int num5 = (int) CRC.Table[(int) this._bufferBase[(int) num3]] ^ (int) this._bufferBase[(int) num3 + 1];
          this._hash[num5 & 1023] = this._pos;
          int num6 = num5 ^ (int) this._bufferBase[(int) num3 + 2] << 8;
          this._hash[1024 + (num6 & (int) ushort.MaxValue)] = this._pos;
          num4 = (uint) (num6 ^ (int) CRC.Table[(int) this._bufferBase[(int) num3 + 3]] << 5) & this._hashMask;
        }
        else
          num4 = (uint) this._bufferBase[(int) num3] ^ (uint) this._bufferBase[(int) num3 + 1] << 8;
        uint num7 = this._hash[(int) this.kFixHashSize + (int) num4];
        this._hash[(int) this.kFixHashSize + (int) num4] = this._pos;
        uint num8 = (uint) (((int) this._cyclicBufferPos << 1) + 1);
        uint num9 = this._cyclicBufferPos << 1;
        uint val2;
        uint val1 = val2 = this.kNumHashDirectBytes;
        uint cutValue = this._cutValue;
        while (num7 > num2 && cutValue-- != 0U)
        {
          uint num5 = this._pos - num7;
          uint num6 = (uint) ((num5 <= this._cyclicBufferPos ? (int) this._cyclicBufferPos - (int) num5 : (int) this._cyclicBufferPos - (int) num5 + (int) this._cyclicBufferSize) << 1);
          uint num10 = this._bufferOffset + num7;
          uint num11 = Math.Min(val1, val2);
          if ((int) this._bufferBase[(int) num10 + (int) num11] == (int) this._bufferBase[(int) num3 + (int) num11])
          {
            do
              ;
            while ((int) ++num11 != (int) num1 && (int) this._bufferBase[(int) num10 + (int) num11] == (int) this._bufferBase[(int) num3 + (int) num11]);
            if ((int) num11 == (int) num1)
            {
              this._son[(int) num9] = this._son[(int) num6];
              this._son[(int) num8] = this._son[(int) num6 + 1];
              goto label_18;
            }
          }
          if ((int) this._bufferBase[(int) num10 + (int) num11] < (int) this._bufferBase[(int) num3 + (int) num11])
          {
            this._son[(int) num9] = num7;
            num9 = num6 + 1U;
            num7 = this._son[(int) num9];
            val2 = num11;
          }
          else
          {
            this._son[(int) num8] = num7;
            num8 = num6;
            num7 = this._son[(int) num8];
            val1 = num11;
          }
        }
        this._son[(int) num8] = this._son[(int) num9] = 0U;
label_18:
        this.MovePos();
label_19:;
      }
      while (--num != 0U);
    }

    private void NormalizeLinks(uint[] items, uint numItems, uint subValue)
    {
      for (uint index = 0; index < numItems; ++index)
      {
        uint num1 = items[(int) index];
        uint num2 = num1 > subValue ? num1 - subValue : 0U;
        items[(int) index] = num2;
      }
    }

    private void Normalize()
    {
      uint subValue = this._pos - this._cyclicBufferSize;
      this.NormalizeLinks(this._son, this._cyclicBufferSize * 2U, subValue);
      this.NormalizeLinks(this._hash, this._hashSizeSum, subValue);
      this.ReduceOffsets((int) subValue);
    }

    public void SetCutValue(uint cutValue)
    {
      this._cutValue = cutValue;
    }
  }
}
