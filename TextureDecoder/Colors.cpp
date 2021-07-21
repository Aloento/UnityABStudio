#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    unsigned TextureDecoderService::Color(Byte r, Byte g, Byte b, Byte a) {
        if (IsBigEndian)
            return a | r << 8 | g << 16 | b << 24;
        
        return b | g << 8 | r << 16 | a << 24;
    }

    void TextureDecoderService::RGB565LE(UInt16 d, Byte* r, Byte* g, Byte* b) {
        if (IsBigEndian) {
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
