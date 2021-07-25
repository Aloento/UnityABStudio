namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using System;
    using System.Collections.Generic;
    using Utils;

    public class PackedFloatVector {
        public uint m_NumItems;
        public float m_Range;
        public float m_Start;
        public byte[] m_Data;
        public byte m_BitSize;

        public PackedFloatVector(ObjectReader reader) {
            m_NumItems = reader.ReadUInt32();
            m_Range = reader.ReadSingle();
            m_Start = reader.ReadSingle();

            var numData = reader.ReadInt32();
            m_Data = reader.ReadBytes(numData);
            reader.AlignStream();

            m_BitSize = reader.ReadByte();
            reader.AlignStream();
        }

        public float[] UnpackFloats(int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1) {
            var bitPos = m_BitSize * start;
            var indexPos = bitPos / 8;
            bitPos %= 8;

            var scale = 1.0f / m_Range;
            if (numChunks == -1)
                numChunks = (int)m_NumItems / itemCountInChunk;
            var end = chunkStride * numChunks / 4;
            var data = new List<float>();
            for (var index = 0; index != end; index += chunkStride / 4) {
                for (var i = 0; i < itemCountInChunk; ++i) {
                    uint x = 0;

                    var bits = 0;
                    while (bits < m_BitSize) {
                        x |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                        var num = Math.Min(m_BitSize - bits, 8 - bitPos);
                        bitPos += num;
                        bits += num;
                        if (bitPos == 8) {
                            indexPos++;
                            bitPos = 0;
                        }
                    }
                    x &= (uint)(1 << m_BitSize) - 1u;
                    data.Add(x / (scale * ((1 << m_BitSize) - 1)) + m_Start);
                }
            }

            return data.ToArray();
        }
    }
}
