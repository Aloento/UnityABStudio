// InBuffer.cs

namespace SoarCraft.QYun.AssetReader._7zip.Common {
    public class InBuffer {
        byte[] m_Buffer;
        uint m_Pos;
        uint m_Limit;
        uint m_BufferSize;
        System.IO.Stream m_Stream;
        bool m_StreamWasExhausted;
        ulong m_ProcessedSize;

        public InBuffer(uint bufferSize) {
            this.m_Buffer = new byte[bufferSize];
            this.m_BufferSize = bufferSize;
        }

        public void Init(System.IO.Stream stream) {
            this.m_Stream = stream;
            this.m_ProcessedSize = 0;
            this.m_Limit = 0;
            this.m_Pos = 0;
            this.m_StreamWasExhausted = false;
        }

        public bool ReadBlock() {
            if (this.m_StreamWasExhausted)
                return false;
            this.m_ProcessedSize += this.m_Pos;
            var aNumProcessedBytes = this.m_Stream.Read(this.m_Buffer, 0, (int)this.m_BufferSize);
            this.m_Pos = 0;
            this.m_Limit = (uint)aNumProcessedBytes;
            this.m_StreamWasExhausted = aNumProcessedBytes == 0;
            return !this.m_StreamWasExhausted;
        }


        public void ReleaseStream() {
            // m_Stream.Close(); 
            this.m_Stream = null;
        }

        public bool ReadByte(byte b) // check it
        {
            if (this.m_Pos >= this.m_Limit)
                if (!this.ReadBlock())
                    return false;
            b = this.m_Buffer[this.m_Pos++];
            return true;
        }

        public byte ReadByte() {
            // return (byte)m_Stream.ReadByte();
            if (this.m_Pos >= this.m_Limit)
                if (!this.ReadBlock())
                    return 0xFF;
            return this.m_Buffer[this.m_Pos++];
        }

        public ulong GetProcessedSize() {
            return this.m_ProcessedSize + this.m_Pos;
        }
    }
}
