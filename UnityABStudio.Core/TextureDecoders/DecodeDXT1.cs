namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AssetReader.Utils;

    public partial class TextureDecoder {
        public bool DecodeDXT1(UnityReader data, int width, int height, MemoryStream image) {
            var blocks_x = (width + 3) / 4;
            var blocks_y = (height + 3) / 4;

            for (var by = 0; by < blocks_x; by++) {
                for (var bx = 0; bx < blocks_y; bx++) {
                    DecodeDXT1BlockAsync(data, image);
                }
            }

            return true;
        }

        private async Task DecodeDXT1BlockAsync(UnityReader data, MemoryStream image) {
            var q0 = data.ReadUInt16();
            var q1 = data.ReadUInt16();

            RGB565LE(q0, out var r0, out var g0, out var b0);
            RGB565LE(q1, out var r1, out var g1, out var b1);

            var c = new uint[] { Color(r0, g0, b0, 255), Color(r1, g1, b1, 255), 0, 0 };
            if (q0 > q1) {
                c[2] = Color((byte)(((r0 * 2) + r1) / 3), (byte)(((g0 * 2) + g1) / 3), (byte)(((b0 * 2) + b1) / 3), 255);
                c[3] = Color((byte)((r0 + (r1 * 2)) / 3), (byte)((g0 + (g1 * 2)) / 3), (byte)((b0 + (b1 * 2)) / 3), 255);
            } else {
                c[2] = Color((byte)((r0 + r1) / 2), (byte)((g0 + g1) / 2), (byte)((b0 + b1) / 2), 255);
                c[3] = Color(0, 0, 0, 255);
            }

            var d = data.ReadUInt32();
            for (var i = 0; i < 16; i++, d >>= 2) {
                await image.WriteAsync(BitConverter.GetBytes(c[d & 3]));
            }
        }
    }
}
