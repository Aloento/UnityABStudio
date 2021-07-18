namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AssetReader.Utils;

    public partial class TextureDecoder {
        public async Task<bool> DecodeDXT5Async(UnityReader data, int width, int height, MemoryStream image) {
            var blocks_x = (width + 3) / 4;
            var blocks_y = (height + 3) / 4;

            for (var by = 0; by < blocks_x; by++) {
                for (var bx = 0; bx < blocks_y; bx++) {
                    data.Position += 8;

                    var pos = image.Position;
                    await DecodeDXT1BlockAsync(data, image);
                    image.Position = pos;

                    data.Position -= 16;
                    DecodeDXT5Alpha(data, image);
                }
            }

            return true;
        }

        private void DecodeDXT5Alpha(UnityReader data, MemoryStream image, int channel = 3) {
            var a = new List<byte>(8) { data.ReadByte(), data.ReadByte() };
            if (a[0] > a[1]) {
                a[2] = (byte)(((a[0] * 6) + a[1]) / 7);
                a[3] = (byte)(((a[0] * 5) + (a[1] * 2)) / 7);
                a[4] = (byte)(((a[0] * 4) + (a[1] * 3)) / 7);
                a[5] = (byte)(((a[0] * 3) + (a[1] * 4)) / 7);
                a[6] = (byte)(((a[0] * 2) + (a[1] * 5)) / 7);
                a[7] = (byte)((a[0] + (a[1] * 6)) / 7);
            } else {
                a[2] = (byte)(((a[0] * 4) + a[1]) / 5);
                a[3] = (byte)(((a[0] * 3) + (a[1] * 2)) / 5);
                a[4] = (byte)(((a[0] * 2) + (a[1] * 3)) / 5);
                a[5] = (byte)((a[0] + (a[1] * 4)) / 5);
                a[6] = 0;
                a[7] = 255;
            }

            data.Position -= 2;
            var d = data.ReadUInt64();
            for (var i = 0; i < 16; i++, d >>= 3) {
                image.Position += (i * 4) + channel;
                image.WriteByte(a[(int)(d & 7)]);
            }
        }
    }
}
