#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    TextureDecoderService::TextureDecoderService() {
        int i = 0x12345678;
        char* pc = (char*)&i;

        if (*pc == 0x12) {
            NativeEndianness = true;
        }
        else if (*pc == 0x78) {
            NativeEndianness = false;
        }

        NativeEndianness = false;
    }

    inline unsigned TextureDecoderService::Color(Byte r, Byte g, Byte b) {
        if (NativeEndianness) {

        }
    }
}
