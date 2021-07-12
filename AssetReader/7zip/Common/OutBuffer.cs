// OutBuffer.cs

namespace SoarCraft.QYun.AssetReader._7zip.Common {
    public class OutBuffer {
        byte[] m_Buffer;
        uint m_Pos;
        uint m_BufferSize;
        System.IO.Stream m_Stream;
        ulong m_ProcessedSize;

        public OutBuffer(uint bufferSize) {
            this.m_Buffer = new byte[bufferSize];
            this.m_BufferSize = bufferSize;
        }

        public void SetStream(System.IO.Stream stream) { this.m_Stream = stream; }
        public void FlushStream() { this.m_Stream.Flush(); }
        public void CloseStream() { this.m_Stream.Close(); }
        public void ReleaseStream() { this.m_Stream = null; }

        public void Init() {
            this.m_ProcessedSize = 0;
            this.m_Pos = 0;
        }

        public void WriteByte(byte b) {
            this.m_Buffer[this.m_Pos++] = b;
            if (this.m_Pos >= this.m_BufferSize)
                this.FlushData();
        }

        public void FlushData() {
            if (this.m_Pos == 0)
                return;
            this.m_Stream.Write(this.m_Buffer, 0, (int)this.m_Pos);
            this.m_Pos = 0;
        }

        public ulong GetProcessedSize() { return this.m_ProcessedSize + this.m_Pos; }
    }
}
