#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeDXT1(UnityReader^ data, int width, int height, MemoryStream^ image) {
        auto blocks_x = (width + 3) / 4;
        auto blocks_y = (height + 3) / 4;

        for (auto y = 0; y < blocks_y; y++) {
            for (auto x = 0; x < blocks_x; x++) {
                DecodeDXT1Block(data, image);
            }
        }

        return true;
    }

    inline void TextureDecoderService::DecodeDXT1Block(UnityReader^ data, MemoryStream^ image) {
        Byte r0, g0, b0, r1, g1, b1;
        UInt16 q0 = data->ReadUInt16();
        UInt16 q1 = data->ReadUInt16();

        RGB565LE(q0, &r0, &g0, &b0);
        RGB565LE(q1, &r1, &g1, &b1);

        unsigned c[4] = 
    }

    inline void TextureDecoderService::RGB565LE(UInt16 d, Byte* r, Byte* g, Byte* b) {
        if (NativeEndianness) {
            *r = (d & 0xf8) | (d >> 5 & 7);
            *g = (d << 5 & 0xe0) | (d >> 11 & 0x1c) | (d >> 1 & 3);
            *b = (d >> 5 & 0xf8) | (d >> 10 & 0x7);
        }
        else {
            *r = (d >> 8 & 0xf8) | (d >> 13);
            *g = (d >> 3 & 0xfc) | (d >> 9 & 3);
            *b = (d << 3) | (d >> 2 & 7);
        }
    }
}
