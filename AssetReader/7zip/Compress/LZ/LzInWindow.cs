// LzInWindow.cs

namespace SoarCraft.QYun.AssetReader._7zip.Compress.LZ {
    using System;

    public class InWindow {
        public Byte[] _bufferBase = null; // pointer to buffer with data
        System.IO.Stream _stream;
        UInt32 _posLimit; // offset (from _buffer) of first byte when new block reading must be done
        bool _streamEndWasReached; // if (true) then _streamPos shows real end of stream

        UInt32 _pointerToLastSafePosition;

        public UInt32 _bufferOffset;

        public UInt32 _blockSize; // Size of Allocated memory block
        public UInt32 _pos; // offset (from _buffer) of curent byte
        UInt32 _keepSizeBefore; // how many BYTEs must be kept in buffer before _pos
        UInt32 _keepSizeAfter; // how many BYTEs must be kept buffer after _pos
        public UInt32 _streamPos; // offset (from _buffer) of first not read byte from Stream

        public void MoveBlock() {
            var offset = (UInt32)this._bufferOffset + this._pos - this._keepSizeBefore;
            // we need one additional byte, since MovePos moves on 1 byte.
            if (offset > 0)
                offset--;

            var numBytes = (UInt32)this._bufferOffset + this._streamPos - offset;

            // check negative offset ????
            for (UInt32 i = 0; i < numBytes; i++)
                this._bufferBase[i] = this._bufferBase[offset + i];
            this._bufferOffset -= offset;
        }

        public virtual void ReadBlock() {
            if (this._streamEndWasReached)
                return;
            while (true) {
                var size = (int)(0 - this._bufferOffset + this._blockSize - this._streamPos);
                if (size == 0)
                    return;
                var numReadBytes = this._stream.Read(this._bufferBase, (int)(this._bufferOffset + this._streamPos), size);
                if (numReadBytes == 0) {
                    this._posLimit = this._streamPos;
                    var pointerToPostion = this._bufferOffset + this._posLimit;
                    if (pointerToPostion > this._pointerToLastSafePosition)
                        this._posLimit = (UInt32)(this._pointerToLastSafePosition - this._bufferOffset);

                    this._streamEndWasReached = true;
                    return;
                }
                this._streamPos += (UInt32)numReadBytes;
                if (this._streamPos >= this._pos + this._keepSizeAfter)
                    this._posLimit = this._streamPos - this._keepSizeAfter;
            }
        }

        void Free() { this._bufferBase = null; }

        public void Create(UInt32 keepSizeBefore, UInt32 keepSizeAfter, UInt32 keepSizeReserv) {
            this._keepSizeBefore = keepSizeBefore;
            this._keepSizeAfter = keepSizeAfter;
            var blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;
            if (this._bufferBase == null || this._blockSize != blockSize) {
                this.Free();
                this._blockSize = blockSize;
                this._bufferBase = new Byte[this._blockSize];
            }
            this._pointerToLastSafePosition = this._blockSize - keepSizeAfter;
        }

        public void SetStream(System.IO.Stream stream) { this._stream = stream; }
        public void ReleaseStream() { this._stream = null; }

        public void Init() {
            this._bufferOffset = 0;
            this._pos = 0;
            this._streamPos = 0;
            this._streamEndWasReached = false;
            this.ReadBlock();
        }

        public void MovePos() {
            this._pos++;
            if (this._pos > this._posLimit) {
                var pointerToPostion = this._bufferOffset + this._pos;
                if (pointerToPostion > this._pointerToLastSafePosition)
                    this.MoveBlock();
                this.ReadBlock();
            }
        }

        public Byte GetIndexByte(Int32 index) { return this._bufferBase[this._bufferOffset + this._pos + index]; }

        // index + limit have not to exceed _keepSizeAfter;
        public UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit) {
            if (this._streamEndWasReached)
                if (this._pos + index + limit > this._streamPos)
                    limit = this._streamPos - (UInt32)(this._pos + index);
            distance++;
            // Byte *pby = _buffer + (size_t)_pos + index;
            var pby = this._bufferOffset + this._pos + (UInt32)index;

            UInt32 i;
            for (i = 0; i < limit && this._bufferBase[pby + i] == this._bufferBase[pby + i - distance]; i++)
                ;
            return i;
        }

        public UInt32 GetNumAvailableBytes() { return this._streamPos - this._pos; }

        public void ReduceOffsets(Int32 subValue) {
            this._bufferOffset += (UInt32)subValue;
            this._posLimit -= (UInt32)subValue;
            this._pos -= (UInt32)subValue;
            this._streamPos -= (UInt32)subValue;
        }
    }
}
