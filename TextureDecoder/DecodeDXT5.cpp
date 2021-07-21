#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeDXT5(array<Byte>^ data, int w, int h, array<UInt32>^ image) {
        long num_blocks_x = (w + 3) / 4;
        long num_blocks_y = (h + 3) / 4;

        Byte* d = Array2Ptr(data);
        UInt32* i = Array2Ptr(image);
        UInt32 buffer[16];

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 16) {
                DecodeDXT1Block(d + 8, buffer);
                DecodeDXT5Alpha(d, buffer, 3);
                CopyBlockBuffer(bx, by, w, h, 4, 4, buffer, i);
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
