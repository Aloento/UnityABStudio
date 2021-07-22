#include "pch.h"
#include "TextureDecoder.h"
#include "ETC.h"

namespace SoarCraft::QYun::TextureDecoder {
    uint32_t TextureDecoderService::ApplicateColor(uint_fast8_t c[3], int_fast16_t m) {
        return Color(Clamp(c[0] + m), Clamp(c[1] + m), Clamp(c[2] + m), 255);
    }

    uint32_t TextureDecoderService::ApplicateColorAlpha(uint_fast8_t c[3], int_fast16_t m, int transparent) {
        return Color(Clamp(c[0] + m), Clamp(c[1] + m), Clamp(c[2] + m), transparent ? 0 : 255);
    }

    uint32_t TextureDecoderService::ApplicateColorRaw(uint_fast8_t c[3]) {
        return Color(c[0], c[1], c[2], 255);
    }
}
