#include "pch.h"
#include "TextureDecoder.h"
#include <cstring>

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeEACRSigned(FastArgs) {
        FastInsert;

        uint32_t base_buffer[16];
        for (int i = 0; i < 16; i++)
            base_buffer[i] = Color(0, 0, 0, 255);
        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 8) {
                memcpy(buffer, base_buffer, sizeof(buffer));
                DecodeEACSignedBlock(d, 2, buffer);
                CopyBlockBuffer(bx, by, width, height, 4, 4, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeEACSignedBlock(Byte* data, int color, UInt32* outbuf) {
        int8_t base = (int8_t)data[0];
        uint_fast8_t multiplier = data[1] >> 1 & 0x78;
        if (multiplier == 0)
            multiplier = 1;
        const int_fast8_t* table = Etc2AlphaModTable[data[1] & 0xf];
        uint_fast64_t l = *(uint64_t*)data;
        for (int i = 0; i < 16; i++, l >>= 3) {
            int_fast16_t val = base * 8 + multiplier * table[l & 7] + 1023;
            ((uint8_t*)(outbuf + WriteOrderTableRev[i]))[color] = val < 0 ? 0 : val >= 2048 ? 0xff : val >> 3;
        }
    }
}
