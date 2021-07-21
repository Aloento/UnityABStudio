#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeETC2A8(array<Byte>^ data, int w, int h, array<UInt32>^ image) {
        FastInsert;

        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 16) {
                DecodeETC2Block(d + 8, buffer);
                DecodeETC2A8Block(d, buffer);
                CopyBlockBuffer(bx, by, w, h, 4, 4, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeETC2A8Block(Byte* data, UInt32* outbuf) {
        if (data[1] & 0xf0) {
            // multiplier != 0
            const uint_fast8_t multiplier = data[1] >> 4;
            const int_fast8_t* table = Etc2AlphaModTable[data[1] & 0xf];
            uint_fast64_t l = *(uint64_t*)data;
            for (int i = 0; i < 16; i++, l >>= 3)
                ((uint8_t*)(outbuf + WriteOrderTableRev[i]))[3] = Clamp(data[0] + multiplier * table[l & 7]);
        }
        else {
            // multiplier == 0 (always same as base codeword)
            for (int i = 0; i < 16; i++, outbuf++)
                ((uint8_t*)outbuf)[3] = data[0];
        }
    }
}
