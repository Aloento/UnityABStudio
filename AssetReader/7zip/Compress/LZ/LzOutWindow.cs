// LzOutWindow.cs

namespace SoarCraft.QYun.AssetReader._7zip.Compress.LZ {
    public class OutWindow {
        byte[] _buffer = null;
        uint _pos;
        uint _windowSize = 0;
        uint _streamPos;
        System.IO.Stream _stream;

        public uint TrainSize = 0;

        public void Create(uint windowSize) {
            if (this._windowSize != windowSize) {
                // System.GC.Collect();
                this._buffer = new byte[windowSize];
            }
            this._windowSize = windowSize;
            this._pos = 0;
            this._streamPos = 0;
        }

        public void Init(System.IO.Stream stream, bool solid) {
            this.ReleaseStream();
            this._stream = stream;
            if (!solid) {
                this._streamPos = 0;
                this._pos = 0;
                this.TrainSize = 0;
            }
        }

        public bool Train(System.IO.Stream stream) {
            var len = stream.Length;
            var size = len < this._windowSize ? (uint)len : this._windowSize;
            this.TrainSize = size;
            stream.Position = len - size;
            this._streamPos = this._pos = 0;
            while (size > 0) {
                var curSize = this._windowSize - this._pos;
                if (size < curSize)
                    curSize = size;
                var numReadBytes = stream.Read(this._buffer, (int)this._pos, (int)curSize);
                if (numReadBytes == 0)
                    return false;
                size -= (uint)numReadBytes;
                this._pos += (uint)numReadBytes;
                this._streamPos += (uint)numReadBytes;
                if (this._pos == this._windowSize)
                    this._streamPos = this._pos = 0;
            }
            return true;
        }

        public void ReleaseStream() {
            this.Flush();
            this._stream = null;
        }

        public void Flush() {
            var size = this._pos - this._streamPos;
            if (size == 0)
                return;
            this._stream.Write(this._buffer, (int)this._streamPos, (int)size);
            if (this._pos >= this._windowSize)
                this._pos = 0;
            this._streamPos = this._pos;
        }

        public void CopyBlock(uint distance, uint len) {
            var pos = this._pos - distance - 1;
            if (pos >= this._windowSize)
                pos += this._windowSize;
            for (; len > 0; len--) {
                if (pos >= this._windowSize)
                    pos = 0;
                this._buffer[this._pos++] = this._buffer[pos++];
                if (this._pos >= this._windowSize)
                    this.Flush();
            }
        }

        public void PutByte(byte b) {
            this._buffer[this._pos++] = b;
            if (this._pos >= this._windowSize)
                this.Flush();
        }

        public byte GetByte(uint distance) {
            var pos = this._pos - distance - 1;
            if (pos >= this._windowSize)
                pos += this._windowSize;
            return this._buffer[pos];
        }
    }
}
