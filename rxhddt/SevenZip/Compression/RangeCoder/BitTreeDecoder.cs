namespace SevenZip.Compression.RangeCoder
{
  internal struct BitTreeDecoder
  {
    private BitDecoder[] Models;
    private int NumBitLevels;

    public BitTreeDecoder(int numBitLevels)
    {
      this.NumBitLevels = numBitLevels;
      this.Models = new BitDecoder[1 << numBitLevels];
    }

    public void Init()
    {
      for (uint index = 1; (long) index < (long) (1 << this.NumBitLevels); ++index)
        this.Models[(int) index].Init();
    }

    public uint Decode(Decoder rangeDecoder)
    {
      uint num = 1;
      for (int numBitLevels = this.NumBitLevels; numBitLevels > 0; --numBitLevels)
        num = (num << 1) + this.Models[(int) num].Decode(rangeDecoder);
      return num - (uint) (1 << this.NumBitLevels);
    }

    public uint ReverseDecode(Decoder rangeDecoder)
    {
      uint num1 = 1;
      uint num2 = 0;
      for (int index = 0; index < this.NumBitLevels; ++index)
      {
        uint num3 = this.Models[(int) num1].Decode(rangeDecoder);
        num1 = (num1 << 1) + num3;
        num2 |= num3 << index;
      }
      return num2;
    }

    public static uint ReverseDecode(
      BitDecoder[] Models,
      uint startIndex,
      Decoder rangeDecoder,
      int NumBitLevels)
    {
      uint num1 = 1;
      uint num2 = 0;
      for (int index = 0; index < NumBitLevels; ++index)
      {
        uint num3 = Models[(int) startIndex + (int) num1].Decode(rangeDecoder);
        num1 = (num1 << 1) + num3;
        num2 |= num3 << index;
      }
      return num2;
    }
  }
}
