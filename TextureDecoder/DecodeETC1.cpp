#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeETC1(FastArgs) {
        FastInsert;

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 8) {
                DecodeETC1Block(d, buffer);
                CopyBlockBuffer(bx, by, width, height, 4, 4, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeETC1Block(Byte* data, UInt32* outbuf) {
        const uint_fast8_t code[2] = { data[3] >> 5, data[3] >> 2 & 7 };  // Table codewords
        const uint_fast8_t* table = Etc1SubblockTable[data[3] & 1];
        uint_fast8_t c[2][3];
        if (data[3] & 2) {
            // diff bit == 1
            c[0][0] = data[0] & 0xf8;
            c[0][1] = data[1] & 0xf8;
            c[0][2] = data[2] & 0xf8;
            c[1][0] = c[0][0] + (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20);
            c[1][1] = c[0][1] + (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20);
            c[1][2] = c[0][2] + (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20);
            c[0][0] |= c[0][0] >> 5;
            c[0][1] |= c[0][1] >> 5;
            c[0][2] |= c[0][2] >> 5;
            c[1][0] |= c[1][0] >> 5;
            c[1][1] |= c[1][1] >> 5;
            c[1][2] |= c[1][2] >> 5;
        }
        else {
            // diff bit == 0
            c[0][0] = (data[0] & 0xf0) | data[0] >> 4;
            c[1][0] = (data[0] & 0x0f) | data[0] << 4;
            c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
            c[1][1] = (data[1] & 0x0f) | data[1] << 4;
            c[0][2] = (data[2] & 0xf0) | data[2] >> 4;
            c[1][2] = (data[2] & 0x0f) | data[2] << 4;
        }

        uint_fast16_t j = data[6] << 8 | data[7];  // less significant pixel index bits
        uint_fast16_t k = data[4] << 8 | data[5];  // more significant pixel index bits
        for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
            uint_fast8_t s = table[i];
            uint_fast8_t m = Etc1ModifierTable[code[s]][j & 1];
            outbuf[WriteOrderTable[i]] = ApplicateColor(c[s], k & 1 ? -m : m);
        }
    }
}
