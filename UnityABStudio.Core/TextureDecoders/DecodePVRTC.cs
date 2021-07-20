namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System.Collections.Generic;
    using System.IO;
    using AssetReader.Utils;

    public partial class TextureDecoder {
        public bool DecodePVRTC(UnityReader data, int width, int height, MemoryStream image, bool is2bpp) {
            var bw = is2bpp ? 8 : 4;
            var num_blocks_x = is2bpp ? (width + 7) / 8 : (width + 3) / 4;
            var num_blocks_y = (height + 3) / 4;
            var num_blocks = num_blocks_x * num_blocks_y;
            var min_num_blocks = num_blocks_x <= num_blocks_y ? num_blocks_x : num_blocks_y;

            if ((num_blocks_x & (num_blocks_x - 1)) != 0 || (num_blocks_y & (num_blocks_y - 1)) != 0) {
                //extern const char* error_msg;
                //error_msg = "the number of blocks of each side must be a power of 2";
                return false;
            }

            var texel_info = new List<PVRTCTexelInfo>(num_blocks);

            for (var i = 0; i < num_blocks; i++) { // Once Reader -> 8
                GetTexelColors(data, texel_info[i]);
            }

            return true;
        }

        private void GetTexelColors(UnityReader data, PVRTCTexelInfo info) {
            _ = data.Mark();
            var ca = data.Move(4).ReadUInt16(); // Reader -> 6
            var cb = data.ReadUInt16(); // Reader -> 2

            if ((ca & 0x8000) != 0) {
                info.a.r = (byte)((ca >> 10) & 0x1f);
                info.a.g = (byte)((ca >> 5) & 0x1f);
                info.a.b = (byte)((ca & 0x1e) | ((ca >> 4) & 1));
                info.a.a = 0xf;
            } else {
                info.a.r = (byte)(((ca >> 7) & 0x1e) | ((ca >> 11) & 1));
                info.a.g = (byte)(((ca >> 3) & 0x1e) | ((ca >> 7) & 1));
                info.a.b = (byte)(((ca << 1) & 0x1c) | ((ca >> 2) & 3));
                info.a.a = (byte)((ca >> 11) & 0xe);
            }

            if ((cb & 0x8000) != 0) {
                info.b.r = (byte)((cb >> 10) & 0x1f);
                info.b.g = (byte)((cb >> 5) & 0x1f);
                info.b.b = (byte)(cb & 0x1f);
                info.b.a = 0xf;
            } else {
                info.b.r = (byte)(((cb >> 7) & 0x1e) | ((cb >> 11) & 1));
                info.b.g = (byte)(((cb >> 3) & 0x1e) | ((cb >> 7) & 1));
                info.b.b = (byte)(((cb << 1) & 0x1e) | ((cb >> 3) & 1));
                info.b.a = (byte)((cb >> 11) & 0xe);
            }

            _ = data.Back();
        }

        private void GetTexelWeights2Bpp(UnityReader data, PVRTCTexelInfo info) {
            info.punch_through_flag = 0;
            _ = data.Mark();

            var mod_bits = data.ReadUInt32(); // Reader -> 4
            var mod_mode = (data.ReadByte() & 1) != 0; // Reader -> 1

            if (mod_mode) {
                var fillFlag = (data.Back().ReadByte() & 1) != 0
                    ? ((data.Move(2).ReadByte() & 0x10) != 0 ? -1 : -2)
                    : -3;
            }
        }
    }
}
