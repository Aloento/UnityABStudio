#include "pch.h"
#include "TextureDecoder.h"
#include <cstring>

namespace SoarCraft::QYun::TextureDecoder {
    TextureDecoderService::TextureDecoderService() {
        int i = 0x12345678;
        char* pc = (char*)&i;

        if (*pc == 0x12) {
            IsBigEndian = true;
            TRANSPARENT_MASK = 0xffffff00;
        }
        else if (*pc == 0x78) {
            IsBigEndian = false;
            TRANSPARENT_MASK = 0x00ffffff;
        }

        IsBigEndian = false;
    }

    Byte* TextureDecoderService::Array2Ptr(array<Byte>^ array) {
        pin_ptr<Byte> pin = &array[0];
        Byte* ptr = pin;
        return ptr;
    }

    UInt32* TextureDecoderService::Array2Ptr(array<UInt32>^ array) {
        pin_ptr<UInt32> pin = &array[0];
        UInt32* ptr = pin;
        return ptr;
    }

    void TextureDecoderService::CopyBlockBuffer(long bx, long by, long w, long h, long bw,
                                                long bh, UInt32* buffer, UInt32* image) {
        long x = bw * bx;
        long xl = (bw * (bx + 1) > w ? w - bw * bx : bw) * 4;
        const UInt32* buffer_end = buffer + bw * bh;
        for (long y = by * bh; buffer < buffer_end && y < h; buffer += bw, y++)
            memcpy(image + y * w + x, buffer, xl);
    }
}
