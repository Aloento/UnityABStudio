namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AssetReader.Utils;

    public partial class TextureDecoder {
        private readonly byte[] PVRTC1_STANDARD_WEIGHT = { 0, 3, 5, 8 };
        private readonly byte[] PVRTC1_PUNCHTHROUGH_WEIGHT = { 0, 4, 4, 8 };

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
            var getTexelWeights = is2bpp ? new Action<UnityReader, PVRTCTexelInfo>(GetTexelWeights2Bpp) : this.GetTexelWeights4Bpp;
            var applicateColor = is2bpp ? new Action<UnityReader, List<PVRTCTexelInfo>, MemoryStream>(ApplicateColor2Bpp) : this.ApplicateColor4Bpp;

            for (var i = 0; i < num_blocks; i++) { // Once Reader -> 8
                GetTexelColors(data, texel_info[i]);
                getTexelWeights(data, texel_info[i]);
            }

            var local_info = new List<PVRTCTexelInfo>(9);
            var pos_x = new int[3];
            var pos_y = new int[3];

            for (var by = 0; by < num_blocks_y; by++) {
                pos_y[0] = by == 0 ? num_blocks_y - 1 : by - 1;
                pos_y[1] = by;
                pos_y[2] = by == num_blocks_y - 1 ? 0 : by + 1;
                for (int bx = 0, x = 0; bx < num_blocks_x; bx++, x += 4) {
                    pos_x[0] = bx == 0 ? num_blocks_x - 1 : bx - 1;
                    pos_x[1] = bx;
                    pos_x[2] = bx == num_blocks_x - 1 ? 0 : bx + 1;
                    for (int cy = 0, c = 0; cy < 3; cy++) {
                        for (var cx = 0; cx < 3; cx++, c++)
                            local_info[c] = texel_info[MortonIndex(pos_x[cx], pos_y[cy], min_num_blocks)];
                    }

                    applicateColor(data.Move(MortonIndex(bx, by, min_num_blocks) * 8), local_info, image);
                }
            }

            return true;
        }

        private int MortonIndex(int x, int y, long min_dim) {
            int offset = 0, shift = 0;
            for (var mask = 1; mask < min_dim; mask <<= 1, shift++)
                offset |= ((y & mask) | ((x & mask) << 1)) << shift;
            offset |= ((x | y) >> shift) << (shift * 2);
            return offset;
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
            _ = data.Mark();
            info.punch_through_flag = 0;

            var mod_bits = data.ReadUInt32(); // Reader -> 4
            var mod_mode = (data.ReadByte() & 1) != 0; // Reader -> 1

            if (mod_mode) {
                var fillFlag = (data.Back().ReadByte() & 1) != 0 ? ((data.Move(2).ReadByte() & 0x10) != 0 ? -1 : -2) : -3;
                for (int y = 0, i = 1; y < 4;) {
                    if ((++y & 1) != 0)
                        --i;
                    else
                        ++i;

                    for (var x = 0; x < 4; x++, i += 2)
                        info.weight[i] = (byte)fillFlag;
                }

                for (int y = 0, i = 0; y < 4;) {
                    if ((++y & 1) != 0)
                        ++i;
                    else
                        --i;

                    for (var x = 0; x < 4; x++, i += 2, mod_bits >>= 2)
                        info.weight[i] = PVRTC1_STANDARD_WEIGHT[mod_bits & 3];
                }

                info.weight[0] = (byte)((info.weight[0] + 3) & 8);
                if ((data.Back().ReadByte() & 1) != 0)
                    info.weight[20] = (byte)((info.weight[20] + 3) & 8);
            } else {
                for (var i = 0; i < 32; i++, mod_bits >>= 1)
                    info.weight[i] = (byte)((mod_bits & 1) != 0 ? 8 : 0);
            }

            _ = data.Back();
        }

        private void GetTexelWeights4Bpp(UnityReader data, PVRTCTexelInfo info) {
            _ = data.Mark();
            info.punch_through_flag = 0;

            var mod_bits = data.ReadUInt32(); // Reader -> 4
            var mod_mode = (data.ReadByte() & 1) != 0; // Reader -> 1

            if (mod_mode) {
                for (var i = 0; i < 16; i++, mod_bits >>= 2) {
                    info.weight[i] = PVRTC1_PUNCHTHROUGH_WEIGHT[mod_bits & 3];
                    if ((mod_bits & 3) == 2)
                        info.punch_through_flag |= 1u << i;
                }
            } else {
                for (var i = 0; i < 16; i++, mod_bits >>= 2)
                    info.weight[i] = PVRTC1_STANDARD_WEIGHT[mod_bits & 3];
            }

            _ = data.Back();
        }

        private void ApplicateColor2Bpp(UnityReader data, List<PVRTCTexelInfo> info, MemoryStream buf) {
        }

        private void ApplicateColor4Bpp(UnityReader data, List<PVRTCTexelInfo> info, MemoryStream buf) {
        }
    }
}
