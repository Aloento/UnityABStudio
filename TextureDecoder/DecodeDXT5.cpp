#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeDXT5(FastArgs) {
        FastInsert;

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 16) {
                DecodeDXT1Block(d + 8, buffer);
                DecodeDXT5Alpha(d, buffer, 3);
                CopyBlockBuffer(bx, by, width, height, 4, 4, buffer, i);
            }
        }
        return true;
    }

    void TextureDecoderService::DecodeDXT5Alpha(Byte* data, UInt32* outbuf, int channel) {
        Byte a[8] = { data[0], data[1] };
        if (a[0] > a[1]) {
            a[2] = (a[0] * 6 + a[1]) / 7;
            a[3] = (a[0] * 5 + a[1] * 2) / 7;
            a[4] = (a[0] * 4 + a[1] * 3) / 7;
            a[5] = (a[0] * 3 + a[1] * 4) / 7;
            a[6] = (a[0] * 2 + a[1] * 5) / 7;
            a[7] = (a[0] + a[1] * 6) / 7;
        }
        else {
            a[2] = (a[0] * 4 + a[1]) / 5;
            a[3] = (a[0] * 3 + a[1] * 2) / 5;
            a[4] = (a[0] * 2 + a[1] * 3) / 5;
            a[5] = (a[0] + a[1] * 4) / 5;
            a[6] = 0;
            a[7] = 255;
        }
        
        Byte* dst = (Byte*)outbuf;
        Int64 d = *(Int64*)data >> 16;
        for (int i = 0; i < 16; i++, d >>= 3)
            dst[i * 4 + channel] = a[d & 7];
    }
}
