#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeATCRGBA8(FastArgs) {
        BCInsert;

        for (uint32_t by = 0; by < m_blocks_y; by++) {
            for (uint32_t bx = 0; bx < m_blocks_x; bx++, d += 16) {
                DecodeATCBlock(d + 8, buffer);
                DecodeDXT5Alpha(d, buffer, 3);
                CopyBlockBuffer(bx, by, width, height, m_block_width, m_block_height, buffer, i);
            }
        }

        return 1;
    }
}
