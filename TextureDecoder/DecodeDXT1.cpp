#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeDXT1(FastArgs) {
        FastInsert;

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 8) {
                DecodeDXT1Block(d, buffer);
                CopyBlockBuffer(bx, by, width, height, 4, 4, buffer, i);
            }
        }

        return true;
    }

    void TextureDecoderService::DecodeDXT1Block(Byte* data, UInt32* outbuf) {
        Byte r0, g0, b0, r1, g1, b1;
        int q0 = *(UInt16*)(data);
        int q1 = *(UInt16*)(data + 2);

        RGB565LE(q0, &r0, &g0, &b0);
        RGB565LE(q1, &r1, &g1, &b1);
        UInt32 c[4] = { Color(r0, g0, b0, 255), Color(r1, g1, b1, 255) };

        if (q0 > q1) {
            c[2] = Color((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3, 255);
            c[3] = Color((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3, 255);
        }
        else {
            c[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 255);
            c[3] = Color(0, 0, 0, 255);
        }

        UInt32 d = *(UInt32*)(data + 4);
        for (int i = 0; i < 16; i++, d >>= 2)
            outbuf[i] = c[d & 3];
    }
}
