#include "pch.h"
#include "TextureDecoder.h"
#include <cstring>

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeEACRG(array<Byte>^ data, int w, int h, array<UInt32>^ image) {
        FastInsert;

        uint32_t base_buffer[16];
        for (int i = 0; i < 16; i++)
            base_buffer[i] = Color(0, 0, 0, 255);
        for (long by = 0; by < num_blocks_y; by++) {
            for (long bx = 0; bx < num_blocks_x; bx++, d += 16) {
                memcpy(buffer, base_buffer, sizeof(buffer));
                DecodeEACBlock(d, 2, buffer);
                DecodeEACBlock(d + 8, 1, buffer);
                CopyBlockBuffer(bx, by, w, h, 4, 4, buffer, i);
            }
        }
        return 1;
    }
}
