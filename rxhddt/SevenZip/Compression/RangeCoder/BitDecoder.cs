namespace SevenZip.Compression.RangeCoder
{
  internal struct BitDecoder
  {
    public const int kNumBitModelTotalBits = 11;
    public const uint kBitModelTotal = 2048;
    private const int kNumMoveBits = 5;
    private uint Prob;

    public void UpdateModel(int numMoveBits, uint symbol)
    {
      if (symbol == 0U)
        this.Prob += 2048U - this.Prob >> numMoveBits;
      else
        this.Prob -= this.Prob >> numMoveBits;
    }

    public void Init()
    {
      this.Prob = 1024U;
    }

    public uint Decode(Decoder rangeDecoder)
    {
      uint num = (rangeDecoder.Range >> 11) * this.Prob;
      if (rangeDecoder.Code < num)
      {
        rangeDecoder.Range = num;
        this.Prob += 2048U - this.Prob >> 5;
        if (rangeDecoder.Range < 16777216U)
        {
          Decoder decoder = rangeDecoder;
          decoder.Code = decoder.Code << 8 | (uint) (byte) rangeDecoder.Stream.ReadByte();
          rangeDecoder.Range <<= 8;
        }
        return 0;
      }
      rangeDecoder.Range -= num;
      rangeDecoder.Code -= num;
      this.Prob -= this.Prob >> 5;
      if (rangeDecoder.Range < 16777216U)
      {
        Decoder decoder = rangeDecoder;
        decoder.Code = decoder.Code << 8 | (uint) (byte) rangeDecoder.Stream.ReadByte();
        rangeDecoder.Range <<= 8;
      }
      return 1;
    }
  }
}
