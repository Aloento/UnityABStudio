namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using System;
    using Utils;

    public class PackedIntVector {
        public uint m_NumItems;
        public byte[] m_Data;
        public byte m_BitSize;

        public PackedIntVector(ObjectReader reader) {
            m_NumItems = reader.ReadUInt32();

            var numData = reader.ReadInt32();
            m_Data = reader.ReadBytes(numData);
            reader.AlignStream();

            m_BitSize = reader.ReadByte();
            reader.AlignStream();
        }

        public int[] UnpackInts() {
            var data = new int[m_NumItems];
            var indexPos = 0;
            var bitPos = 0;
            for (var i = 0; i < m_NumItems; i++) {
                var bits = 0;
                data[i] = 0;
                while (bits < m_BitSize) {
                    data[i] |= (m_Data[indexPos] >> bitPos) << bits;
                    var num = Math.Min(m_BitSize - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8) {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                data[i] &= (1 << m_BitSize) - 1;
            }
            return data;
        }
    }
}
