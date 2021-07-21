#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeBC7(FastArgs) {
        uint32_t m_block_width = 4;
        uint32_t m_block_height = 4;
        uint32_t m_blocks_x = (width + m_block_width - 1) / m_block_width;
        uint32_t m_blocks_y = (height + m_block_height - 1) / m_block_height;

        Byte* d = Array2Ptr(data);
        UInt32* i = Array2Ptr(image);
        uint32_t buffer[16];

        for (uint32_t by = 0; by < m_blocks_y; by++) {
            for (uint32_t bx = 0; bx < m_blocks_x; bx++, d += 16) {
                DecodeBC7Block(d, buffer);
                CopyBlockBuffer(bx, by, width, height, m_block_width, m_block_height, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeBC7Block(const uint8_t* _src, uint32_t* _dst) {


    }
}
