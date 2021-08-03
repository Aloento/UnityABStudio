namespace SoarCraft.QYun.TextureDecoderNET {
    public class ETC1Decoder {

        private readonly byte[] writeOrderTable = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };

        private readonly byte[,] etc1SubBlockTable = {
            {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
            {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1}
        };

        private readonly byte[,] etc1ModifierTable = {
            {2, 8}, {5, 17}, {9, 29}, {13, 42},
            {18, 60}, {24, 80}, {33, 106}, {47, 183}
        };

        public ETC1Decoder(byte[] data, ulong width, ulong height, out byte[] image) =>
            DecodeETC1(data, width, height, out image);

        private unsafe void DecodeETC1(byte[] data, ulong width, ulong height, out byte[] image) {
            var blocksX = (width + 3) / 4;
            var blocksY = (height + 3) / 4;
            image = new byte[width * height * 4];

            fixed (byte* pData = data, pImage = image) {
                var d = pData;
                for (ulong y = 0; y < blocksY; y++) {
                    for (ulong x = 0; x < blocksX; x++, d += 8) {
                        this.DecodeETC1Block(d, out var block);
                        Helpers.CopyBlockBuffer(width, height, x, y, block, pImage);
                    }
                }
            }
        }

        private unsafe void DecodeETC1Block(byte* data, out uint* block) {
            var code = new[] { (byte)(data[3] >> 5), (byte)((data[3] >> 2) & 7) };
            var dimension = data[3] & 1;
            var c = new byte[2, 3];

            fixed (uint* outBuf = new uint[16]) {
                block = outBuf;
                if ((data[3] & 2) != 0) {
                    // diff bit == 1
                    c[0, 0] = (byte)(data[0] & 0xf8);
                    c[0, 1] = (byte)(data[1] & 0xf8);
                    c[0, 2] = (byte)(data[2] & 0xf8);
                    c[1, 0] = (byte)(c[0, 0] + ((data[0] << 3) & 0x18) - ((data[0] << 3) & 0x20));
                    c[1, 1] = (byte)(c[0, 1] + ((data[1] << 3) & 0x18) - ((data[1] << 3) & 0x20));
                    c[1, 2] = (byte)(c[0, 2] + ((data[2] << 3) & 0x18) - ((data[2] << 3) & 0x20));
                    c[0, 0] |= (byte)(c[0, 0] >> 5);
                    c[0, 1] |= (byte)(c[0, 1] >> 5);
                    c[0, 2] |= (byte)(c[0, 2] >> 5);
                    c[1, 0] |= (byte)(c[1, 0] >> 5);
                    c[1, 1] |= (byte)(c[1, 1] >> 5);
                    c[1, 2] |= (byte)(c[1, 2] >> 5);
                } else {
                    // diff bit == 0
                    c[0, 0] = (byte)((data[0] & 0xf0) | (data[0] >> 4));
                    c[1, 0] = (byte)((data[0] & 0x0f) | (data[0] << 4));
                    c[0, 1] = (byte)((data[1] & 0xf0) | (data[1] >> 4));
                    c[1, 1] = (byte)((data[1] & 0x0f) | (data[1] << 4));
                    c[0, 2] = (byte)((data[2] & 0xf0) | (data[2] >> 4));
                    c[1, 2] = (byte)((data[2] & 0x0f) | (data[2] << 4));
                }

                var j = (uint)((data[6] << 8) | data[7]);
                var k = (uint)((data[4] << 8) | data[5]);

                for (var i = 0; i < 16; i++, j >>= 1, k >>= 1) {
                    var s = this.etc1SubBlockTable[dimension, i];
                    var m = this.etc1ModifierTable[code[s], j & 1];

                    block[writeOrderTable[i]] = Helpers.ApplicateColor(c, (k & 1) != 0 ? -m : m, s);
                }
            }
        }
    }
}
