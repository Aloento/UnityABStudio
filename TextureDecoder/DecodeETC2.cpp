#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeETC1(FastArgs) {
        FastInsert;

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 8) {
                DecodeETC2Block(d, buffer);
                CopyBlockBuffer(bx, by, width, height, 4, 4, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeETC2Block(Byte* data, UInt32* outbuf) {
        uint_fast16_t j = data[6] << 8 | data[7];  // 15 -> 0
        uint_fast32_t k = data[4] << 8 | data[5];  // 31 -> 16
        uint_fast8_t c[3][3] = {};

        if (data[3] & 2) {
            // diff bit == 1
            uint_fast8_t r = data[0] & 0xf8;
            int_fast16_t dr = (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20);
            uint_fast8_t g = data[1] & 0xf8;
            int_fast16_t dg = (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20);
            uint_fast8_t b = data[2] & 0xf8;
            int_fast16_t db = (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20);
            if (r + dr < 0 || r + dr > 255) {
                // T
                c[0][0] = (data[0] << 3 & 0xc0) | (data[0] << 4 & 0x30) | (data[0] >> 1 & 0xc) | (data[0] & 3);
                c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
                c[0][2] = (data[1] & 0x0f) | data[1] << 4;
                c[1][0] = (data[2] & 0xf0) | data[2] >> 4;
                c[1][1] = (data[2] & 0x0f) | data[2] << 4;
                c[1][2] = (data[3] & 0xf0) | data[3] >> 4;
                const uint_fast8_t d = Etc2DistanceTable[(data[3] >> 1 & 6) | (data[3] & 1)];
                uint_fast32_t color_set[4] = { ApplicateColorRaw(c[0]), ApplicateColor(c[1], d),
                                              ApplicateColorRaw(c[1]), ApplicateColor(c[1], -d) };
                k <<= 1;
                for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                    outbuf[WriteOrderTable[i]] = color_set[(k & 2) | (j & 1)];
            }
            else if (g + dg < 0 || g + dg > 255) {
                // H
                c[0][0] = (data[0] << 1 & 0xf0) | (data[0] >> 3 & 0xf);
                c[0][1] = (data[0] << 5 & 0xe0) | (data[1] & 0x10);
                c[0][1] |= c[0][1] >> 4;
                c[0][2] = (data[1] & 8) | (data[1] << 1 & 6) | data[2] >> 7;
                c[0][2] |= c[0][2] << 4;
                c[1][0] = (data[2] << 1 & 0xf0) | (data[2] >> 3 & 0xf);
                c[1][1] = (data[2] << 5 & 0xe0) | (data[3] >> 3 & 0x10);
                c[1][1] |= c[1][1] >> 4;
                c[1][2] = (data[3] << 1 & 0xf0) | (data[3] >> 3 & 0xf);
                uint_fast8_t d = (data[3] & 4) | (data[3] << 1 & 2);
                if (c[0][0] > c[1][0] ||
                    (c[0][0] == c[1][0] && (c[0][1] > c[1][1] || (c[0][1] == c[1][1] && c[0][2] >= c[1][2]))))
                    ++d;
                d = Etc2DistanceTable[d];
                uint_fast32_t color_set[4] = { ApplicateColor(c[0], d), ApplicateColor(c[0], -d), ApplicateColor(c[1], d),
                                              ApplicateColor(c[1], -d) };
                k <<= 1;
                for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                    outbuf[WriteOrderTable[i]] = color_set[(k & 2) | (j & 1)];
            }
            else if (b + db < 0 || b + db > 255) {
                // planar
                c[0][0] = (data[0] << 1 & 0xfc) | (data[0] >> 5 & 3);
                c[0][1] = (data[0] << 7 & 0x80) | (data[1] & 0x7e) | (data[0] & 1);
                c[0][2] = (data[1] << 7 & 0x80) | (data[2] << 2 & 0x60) | (data[2] << 3 & 0x18) | (data[3] >> 5 & 4);
                c[0][2] |= c[0][2] >> 6;
                c[1][0] = (data[3] << 1 & 0xf8) | (data[3] << 2 & 4) | (data[3] >> 5 & 3);
                c[1][1] = (data[4] & 0xfe) | data[4] >> 7;
                c[1][2] = (data[4] << 7 & 0x80) | (data[5] >> 1 & 0x7c);
                c[1][2] |= c[1][2] >> 6;
                c[2][0] = (data[5] << 5 & 0xe0) | (data[6] >> 3 & 0x1c) | (data[5] >> 1 & 3);
                c[2][1] = (data[6] << 3 & 0xf8) | (data[7] >> 5 & 0x6) | (data[6] >> 4 & 1);
                c[2][2] = data[7] << 2 | (data[7] >> 4 & 3);
                for (int y = 0, i = 0; y < 4; y++) {
                    for (int x = 0; x < 4; x++, i++) {
                        uint8_t r = Clamp((x * (c[1][0] - c[0][0]) + y * (c[2][0] - c[0][0]) + 4 * c[0][0] + 2) >> 2);
                        uint8_t g = Clamp((x * (c[1][1] - c[0][1]) + y * (c[2][1] - c[0][1]) + 4 * c[0][1] + 2) >> 2);
                        uint8_t b = Clamp((x * (c[1][2] - c[0][2]) + y * (c[2][2] - c[0][2]) + 4 * c[0][2] + 2) >> 2);
                        outbuf[i] = Color(r, g, b, 255);
                    }
                }
            }
            else {
                // differential
                const uint_fast8_t code[2] = { data[3] >> 5, data[3] >> 2 & 7 };
                const uint_fast8_t* table = Etc1SubblockTable[data[3] & 1];
                c[0][0] = r | r >> 5;
                c[0][1] = g | g >> 5;
                c[0][2] = b | b >> 5;
                c[1][0] = r + dr;
                c[1][1] = g + dg;
                c[1][2] = b + db;
                c[1][0] |= c[1][0] >> 5;
                c[1][1] |= c[1][1] >> 5;
                c[1][2] |= c[1][2] >> 5;
                for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
                    uint_fast8_t s = table[i];
                    uint_fast8_t m = Etc1ModifierTable[code[s]][j & 1];
                    outbuf[WriteOrderTable[i]] = ApplicateColor(c[s], k & 1 ? -m : m);
                }
            }
        }
        else {
            // individual (diff bit == 0)
            const uint_fast8_t code[2] = { data[3] >> 5, data[3] >> 2 & 7 };
            const uint_fast8_t* table = Etc1SubblockTable[data[3] & 1];
            c[0][0] = (data[0] & 0xf0) | data[0] >> 4;
            c[1][0] = (data[0] & 0x0f) | data[0] << 4;
            c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
            c[1][1] = (data[1] & 0x0f) | data[1] << 4;
            c[0][2] = (data[2] & 0xf0) | data[2] >> 4;
            c[1][2] = (data[2] & 0x0f) | data[2] << 4;
            for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
                uint_fast8_t s = table[i];
                uint_fast8_t m = Etc1ModifierTable[code[s]][j & 1];
                outbuf[WriteOrderTable[i]] = ApplicateColor(c[s], k & 1 ? -m : m);
            }
        }
    }
}
