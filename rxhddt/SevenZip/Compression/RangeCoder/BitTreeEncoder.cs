namespace SevenZip.Compression.RangeCoder
{
  internal struct BitTreeEncoder
  {
    private BitEncoder[] Models;
    private int NumBitLevels;

    public BitTreeEncoder(int numBitLevels)
    {
      this.NumBitLevels = numBitLevels;
      this.Models = new BitEncoder[1 << numBitLevels];
    }

    public void Init()
    {
      for (uint index = 1; (long) index < (long) (1 << this.NumBitLevels); ++index)
        this.Models[(int) index].Init();
    }

    public void Encode(Encoder rangeEncoder, uint symbol)
    {
      uint num = 1;
      int numBitLevels = this.NumBitLevels;
      while (numBitLevels > 0)
      {
        --numBitLevels;
        uint symbol1 = symbol >> numBitLevels & 1U;
        this.Models[(int) num].Encode(rangeEncoder, symbol1);
        num = num << 1 | symbol1;
      }
    }

    public void ReverseEncode(Encoder rangeEncoder, uint symbol)
    {
      uint num = 1;
      for (uint index = 0; (long) index < (long) this.NumBitLevels; ++index)
      {
        uint symbol1 = symbol & 1U;
        this.Models[(int) num].Encode(rangeEncoder, symbol1);
        num = num << 1 | symbol1;
        symbol >>= 1;
      }
    }

    public uint GetPrice(uint symbol)
    {
      uint num1 = 0;
      uint num2 = 1;
      int numBitLevels = this.NumBitLevels;
      while (numBitLevels > 0)
      {
        --numBitLevels;
        uint symbol1 = symbol >> numBitLevels & 1U;
        num1 += this.Models[(int) num2].GetPrice(symbol1);
        num2 = (num2 << 1) + symbol1;
      }
      return num1;
    }

    public uint ReverseGetPrice(uint symbol)
    {
      uint num1 = 0;
      uint num2 = 1;
      for (int numBitLevels = this.NumBitLevels; numBitLevels > 0; --numBitLevels)
      {
        uint symbol1 = symbol & 1U;
        symbol >>= 1;
        num1 += this.Models[(int) num2].GetPrice(symbol1);
        num2 = num2 << 1 | symbol1;
      }
      return num1;
    }

    public static uint ReverseGetPrice(
      BitEncoder[] Models,
      uint startIndex,
      int NumBitLevels,
      uint symbol)
    {
      uint num1 = 0;
      uint num2 = 1;
      for (int index = NumBitLevels; index > 0; --index)
      {
        uint symbol1 = symbol & 1U;
        symbol >>= 1;
        num1 += Models[(int) startIndex + (int) num2].GetPrice(symbol1);
        num2 = num2 << 1 | symbol1;
      }
      return num1;
    }

    public static void ReverseEncode(
      BitEncoder[] Models,
      uint startIndex,
      Encoder rangeEncoder,
      int NumBitLevels,
      uint symbol)
    {
      uint num = 1;
      for (int index = 0; index < NumBitLevels; ++index)
      {
        uint symbol1 = symbol & 1U;
        Models[(int) startIndex + (int) num].Encode(rangeEncoder, symbol1);
        num = num << 1 | symbol1;
        symbol >>= 1;
      }
    }
  }
}
