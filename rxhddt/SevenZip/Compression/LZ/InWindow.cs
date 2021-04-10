using System.IO;

namespace SevenZip.Compression.LZ
{
  public class InWindow
  {
    public byte[] _bufferBase;
    private Stream _stream;
    private uint _posLimit;
    private bool _streamEndWasReached;
    private uint _pointerToLastSafePosition;
    public uint _bufferOffset;
    public uint _blockSize;
    public uint _pos;
    private uint _keepSizeBefore;
    private uint _keepSizeAfter;
    public uint _streamPos;

    public void MoveBlock()
    {
      uint num1 = this._bufferOffset + this._pos - this._keepSizeBefore;
      if (num1 > 0U)
        --num1;
      uint num2 = this._bufferOffset + this._streamPos - num1;
      for (uint index = 0; index < num2; ++index)
        this._bufferBase[(int) index] = this._bufferBase[(int) num1 + (int) index];
      this._bufferOffset -= num1;
    }

    public virtual void ReadBlock()
    {
      if (this._streamEndWasReached)
        return;
      while (true)
      {
        do
        {
          int count = -(int) this._bufferOffset + (int) this._blockSize - (int) this._streamPos;
          if (count == 0)
            return;
          int num = this._stream.Read(this._bufferBase, (int) this._bufferOffset + (int) this._streamPos, count);
          if (num == 0)
          {
            this._posLimit = this._streamPos;
            if (this._bufferOffset + this._posLimit > this._pointerToLastSafePosition)
              this._posLimit = this._pointerToLastSafePosition - this._bufferOffset;
            this._streamEndWasReached = true;
            return;
          }
          this._streamPos += (uint) num;
        }
        while (this._streamPos < this._pos + this._keepSizeAfter);
        this._posLimit = this._streamPos - this._keepSizeAfter;
      }
    }

    private void Free()
    {
      this._bufferBase = (byte[]) null;
    }

    public void Create(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
    {
      this._keepSizeBefore = keepSizeBefore;
      this._keepSizeAfter = keepSizeAfter;
      uint num = keepSizeBefore + keepSizeAfter + keepSizeReserv;
      if (this._bufferBase == null || (int) this._blockSize != (int) num)
      {
        this.Free();
        this._blockSize = num;
        this._bufferBase = new byte[(int) this._blockSize];
      }
      this._pointerToLastSafePosition = this._blockSize - keepSizeAfter;
    }

    public void SetStream(Stream stream)
    {
      this._stream = stream;
    }

    public void ReleaseStream()
    {
      this._stream = (Stream) null;
    }

    public void Init()
    {
      this._bufferOffset = 0U;
      this._pos = 0U;
      this._streamPos = 0U;
      this._streamEndWasReached = false;
      this.ReadBlock();
    }

    public void MovePos()
    {
      ++this._pos;
      if (this._pos <= this._posLimit)
        return;
      if (this._bufferOffset + this._pos > this._pointerToLastSafePosition)
        this.MoveBlock();
      this.ReadBlock();
    }

    public byte GetIndexByte(int index)
    {
      return this._bufferBase[(long) (this._bufferOffset + this._pos) + (long) index];
    }

    public uint GetMatchLen(int index, uint distance, uint limit)
    {
      if (this._streamEndWasReached && (long) this._pos + (long) index + (long) limit > (long) this._streamPos)
        limit = this._streamPos - (uint) ((ulong) this._pos + (ulong) index);
      ++distance;
      uint num1 = (uint) ((int) this._bufferOffset + (int) this._pos + index);
      uint num2 = 0;
      while (num2 < limit && (int) this._bufferBase[(int) num1 + (int) num2] == (int) this._bufferBase[(int) num1 + (int) num2 - (int) distance])
        ++num2;
      return num2;
    }

    public uint GetNumAvailableBytes()
    {
      return this._streamPos - this._pos;
    }

    public void ReduceOffsets(int subValue)
    {
      this._bufferOffset += (uint) subValue;
      this._posLimit -= (uint) subValue;
      this._pos -= (uint) subValue;
      this._streamPos -= (uint) subValue;
    }
  }
}
