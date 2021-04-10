using System;
using System.IO;

namespace SevenZip.Compression.LZMA
{
  public static class SevenZipHelper
  {
    private static int dictionary = 8388608;
    private static bool eos = false;
    private static CoderPropID[] propIDs = new CoderPropID[8]
    {
      CoderPropID.DictionarySize,
      CoderPropID.PosStateBits,
      CoderPropID.LitContextBits,
      CoderPropID.LitPosBits,
      CoderPropID.Algorithm,
      CoderPropID.NumFastBytes,
      CoderPropID.MatchFinder,
      CoderPropID.EndMarker
    };
    private static object[] properties = new object[8]
    {
      (object) SevenZipHelper.dictionary,
      (object) 2,
      (object) 3,
      (object) 0,
      (object) 2,
      (object) 128,
      (object) "bt4",
      (object) SevenZipHelper.eos
    };

    public static byte[] Compress(byte[] inputBytes)
    {
      MemoryStream memoryStream1 = new MemoryStream(inputBytes);
      MemoryStream memoryStream2 = new MemoryStream();
      Encoder encoder = new Encoder();
      encoder.SetCoderProperties(SevenZipHelper.propIDs, SevenZipHelper.properties);
      encoder.WriteCoderProperties((Stream) memoryStream2);
      long length = memoryStream1.Length;
      for (int index = 0; index < 8; ++index)
        memoryStream2.WriteByte((byte) (length >> 8 * index));
      encoder.Code((Stream) memoryStream1, (Stream) memoryStream2, -1L, -1L, (ICodeProgress) null);
      return memoryStream2.ToArray();
    }

    public static byte[] Decompress(byte[] inputBytes)
    {
      MemoryStream memoryStream1 = new MemoryStream(inputBytes);
      Decoder decoder = new Decoder();
      memoryStream1.Seek(0L, SeekOrigin.Begin);
      MemoryStream memoryStream2 = new MemoryStream();
      byte[] numArray = new byte[5];
      if (memoryStream1.Read(numArray, 0, 5) != 5)
        throw new Exception("input .lzma is too short");
      long outSize = 0;
      for (int index = 0; index < 8; ++index)
      {
        int num = memoryStream1.ReadByte();
        if (num < 0)
          throw new Exception("Can't Read 1");
        outSize |= (long) (byte) num << 8 * index;
      }
      decoder.SetDecoderProperties(numArray);
      long inSize = memoryStream1.Length - memoryStream1.Position;
      decoder.Code((Stream) memoryStream1, (Stream) memoryStream2, inSize, outSize, (ICodeProgress) null);
      return memoryStream2.ToArray();
    }
  }
}
