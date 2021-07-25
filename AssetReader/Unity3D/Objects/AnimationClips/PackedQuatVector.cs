namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using System;
    using Math;
    using Utils;

    public class PackedQuatVector {
        public uint m_NumItems;
        public byte[] m_Data;

        public PackedQuatVector(ObjectReader reader) {
            m_NumItems = reader.ReadUInt32();

            var numData = reader.ReadInt32();
            m_Data = reader.ReadBytes(numData);

            reader.AlignStream();
        }

        public Quaternion[] UnpackQuats() {
            var data = new Quaternion[m_NumItems];
            var indexPos = 0;
            var bitPos = 0;

            for (var i = 0; i < m_NumItems; i++) {
                uint flags = 0;

                var bits = 0;
                while (bits < 3) {
                    flags |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                    var num = Math.Min(3 - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8) {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                flags &= 7;

                var q = new Quaternion();
                float sum = 0;
                for (var j = 0; j < 4; j++) {
                    if ((flags & 3) != j) {
                        var bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
                        uint x = 0;

                        bits = 0;
                        while (bits < bitSize) {
                            x |= (uint)((m_Data[indexPos] >> bitPos) << bits);
                            var num = Math.Min(bitSize - bits, 8 - bitPos);
                            bitPos += num;
                            bits += num;
                            if (bitPos == 8) {
                                indexPos++;
                                bitPos = 0;
                            }
                        }
                        x &= (uint)((1 << bitSize) - 1);
                        q[j] = x / (0.5f * ((1 << bitSize) - 1)) - 1;
                        sum += q[j] * q[j];
                    }
                }

                var lastComponent = (int)(flags & 3);
                q[lastComponent] = (float)Math.Sqrt(1 - sum);
                if ((flags & 4) != 0u)
                    q[lastComponent] = -q[lastComponent];
                data[i] = q;
            }

            return data;
        }
    }
}
