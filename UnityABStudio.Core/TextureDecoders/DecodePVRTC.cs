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

            if ((num_blocks_x & (num_blocks_x - 1)) == 1 || (num_blocks_y & (num_blocks_y - 1)) == 1) {
                //extern const char* error_msg;
                //error_msg = "the number of blocks of each side must be a power of 2";
                return false;
            }

            var texel_info = new List<PVRTCTexelInfo>(num_blocks);

            for (var i = 0; i < num_blocks; i++) { // Once Reader -> 8

            }


            return true;
        }

        private void GetTexelColors(UnityReader data, PVRTCTexelInfo info) {

        }

    }
}
