#include "pch.h"
#include "TextureDecoder.h"
#include "BCN.h"
#include <cstring>
#include <utility>

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeBC6(FastArgs) {
        BCInsert;

        for (uint32_t by = 0; by < m_blocks_y; by++) {
            for (uint32_t bx = 0; bx < m_blocks_x; bx++, d += 16) {
                DecodeBC6Block(d, buffer, false);
                CopyBlockBuffer(bx, by, width, height, m_block_width, m_block_height, buffer, i);
            }
        }
        return 1;
    }

    void TextureDecoderService::DecodeBC6Block(const uint8_t* _src, uint32_t* _dst, bool _signed) {
        BitReader bit(_src);

        uint8_t mode = uint8_t(bit.read(2));

        uint16_t epR[4] = { /* rw, rx, ry, rz */ };
        uint16_t epG[4] = { /* gw, gx, gy, gz */ };
        uint16_t epB[4] = { /* bw, bx, by, bz */ };

        if (mode & 2) {
            // 5-bit mode
            mode |= bit.read(3) << 2;

            if (0 == s_bc6hModeInfo[mode].endpointBits) {
                memset(_dst, 0, 16 * 4);
                return;
            }

            switch (mode) {
            case 2:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(5) << 0;
                epR[0] |= bit.read(1) << 10;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(4) << 0;
                epG[0] |= bit.read(1) << 10;
                epB[3] |= bit.read(1) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(4) << 0;
                epB[0] |= bit.read(1) << 10;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 3;
                break;

            case 3:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(10) << 0;
                epG[1] |= bit.read(10) << 0;
                epB[1] |= bit.read(10) << 0;
                break;

            case 6:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(4) << 0;
                epR[0] |= bit.read(1) << 10;
                epG[3] |= bit.read(1) << 4;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(5) << 0;
                epG[0] |= bit.read(1) << 10;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(4) << 0;
                epB[0] |= bit.read(1) << 10;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(4) << 0;
                epB[3] |= bit.read(1) << 0;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(4) << 0;
                epG[2] |= bit.read(1) << 4;
                epB[3] |= bit.read(1) << 3;
                break;

            case 7:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(9) << 0;
                epR[0] |= bit.read(1) << 10;
                epG[1] |= bit.read(9) << 0;
                epG[0] |= bit.read(1) << 10;
                epB[1] |= bit.read(9) << 0;
                epB[0] |= bit.read(1) << 10;
                break;

            case 10:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(4) << 0;
                epR[0] |= bit.read(1) << 10;
                epB[2] |= bit.read(1) << 4;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(4) << 0;
                epG[0] |= bit.read(1) << 10;
                epB[3] |= bit.read(1) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(5) << 0;
                epB[0] |= bit.read(1) << 10;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(4) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(4) << 0;
                epB[3] |= bit.read(1) << 4;
                epB[3] |= bit.read(1) << 3;
                break;

            case 11:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(8) << 0;
                epR[0] |= bit.read(1) << 11;
                epR[0] |= bit.read(1) << 10;
                epG[1] |= bit.read(8) << 0;
                epG[0] |= bit.read(1) << 11;
                epG[0] |= bit.read(1) << 10;
                epB[1] |= bit.read(8) << 0;
                epB[0] |= bit.read(1) << 11;
                epB[0] |= bit.read(1) << 10;
                break;

            case 14:
                epR[0] |= bit.read(9) << 0;
                epB[2] |= bit.read(1) << 4;
                epG[0] |= bit.read(9) << 0;
                epG[2] |= bit.read(1) << 4;
                epB[0] |= bit.read(9) << 0;
                epB[3] |= bit.read(1) << 4;
                epR[1] |= bit.read(5) << 0;
                epG[3] |= bit.read(1) << 4;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 3;
                break;

            case 15:
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(4) << 0;
                epR[0] |= bit.read(1) << 15;
                epR[0] |= bit.read(1) << 14;
                epR[0] |= bit.read(1) << 13;
                epR[0] |= bit.read(1) << 12;
                epR[0] |= bit.read(1) << 11;
                epR[0] |= bit.read(1) << 10;
                epG[1] |= bit.read(4) << 0;
                epG[0] |= bit.read(1) << 15;
                epG[0] |= bit.read(1) << 14;
                epG[0] |= bit.read(1) << 13;
                epG[0] |= bit.read(1) << 12;
                epG[0] |= bit.read(1) << 11;
                epG[0] |= bit.read(1) << 10;
                epB[1] |= bit.read(4) << 0;
                epB[0] |= bit.read(1) << 15;
                epB[0] |= bit.read(1) << 14;
                epB[0] |= bit.read(1) << 13;
                epB[0] |= bit.read(1) << 12;
                epB[0] |= bit.read(1) << 11;
                epB[0] |= bit.read(1) << 10;
                break;

            case 18:
                epR[0] |= bit.read(8) << 0;
                epG[3] |= bit.read(1) << 4;
                epB[2] |= bit.read(1) << 4;
                epG[0] |= bit.read(8) << 0;
                epB[3] |= bit.read(1) << 2;
                epG[2] |= bit.read(1) << 4;
                epB[0] |= bit.read(8) << 0;
                epB[3] |= bit.read(1) << 3;
                epB[3] |= bit.read(1) << 4;
                epR[1] |= bit.read(6) << 0;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(6) << 0;
                epR[3] |= bit.read(6) << 0;
                break;

            case 22:
                epR[0] |= bit.read(8) << 0;
                epB[3] |= bit.read(1) << 0;
                epB[2] |= bit.read(1) << 4;
                epG[0] |= bit.read(8) << 0;
                epG[2] |= bit.read(1) << 5;
                epG[2] |= bit.read(1) << 4;
                epB[0] |= bit.read(8) << 0;
                epG[3] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 4;
                epR[1] |= bit.read(5) << 0;
                epG[3] |= bit.read(1) << 4;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(6) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 3;
                break;

            case 26:
                epR[0] |= bit.read(8) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(1) << 4;
                epG[0] |= bit.read(8) << 0;
                epB[2] |= bit.read(1) << 5;
                epG[2] |= bit.read(1) << 4;
                epB[0] |= bit.read(8) << 0;
                epB[3] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 4;
                epR[1] |= bit.read(5) << 0;
                epG[3] |= bit.read(1) << 4;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(6) << 0;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 3;
                break;

            case 30:
                epR[0] |= bit.read(6) << 0;
                epG[3] |= bit.read(1) << 4;
                epB[3] |= bit.read(1) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(1) << 4;
                epG[0] |= bit.read(6) << 0;
                epG[2] |= bit.read(1) << 5;
                epB[2] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 2;
                epG[2] |= bit.read(1) << 4;
                epB[0] |= bit.read(6) << 0;
                epG[3] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 3;
                epB[3] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 4;
                epR[1] |= bit.read(6) << 0;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(6) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(6) << 0;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(6) << 0;
                epR[3] |= bit.read(6) << 0;
                break;

            default:
                break;
            }
        } else {
            switch (mode) {
            case 0:
                epG[2] |= bit.read(1) << 4;
                epB[2] |= bit.read(1) << 4;
                epB[3] |= bit.read(1) << 4;
                epR[0] |= bit.read(10) << 0;
                epG[0] |= bit.read(10) << 0;
                epB[0] |= bit.read(10) << 0;
                epR[1] |= bit.read(5) << 0;
                epG[3] |= bit.read(1) << 4;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 2;
                epR[3] |= bit.read(5) << 0;
                epB[3] |= bit.read(1) << 3;
                break;

            case 1:
                epG[2] |= bit.read(1) << 5;
                epG[3] |= bit.read(1) << 4;
                epG[3] |= bit.read(1) << 5;
                epR[0] |= bit.read(7) << 0;
                epB[3] |= bit.read(1) << 0;
                epB[3] |= bit.read(1) << 1;
                epB[2] |= bit.read(1) << 4;
                epG[0] |= bit.read(7) << 0;
                epB[2] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 2;
                epG[2] |= bit.read(1) << 4;
                epB[0] |= bit.read(7) << 0;
                epB[3] |= bit.read(1) << 3;
                epB[3] |= bit.read(1) << 5;
                epB[3] |= bit.read(1) << 4;
                epR[1] |= bit.read(6) << 0;
                epG[2] |= bit.read(4) << 0;
                epG[1] |= bit.read(6) << 0;
                epG[3] |= bit.read(4) << 0;
                epB[1] |= bit.read(6) << 0;
                epB[2] |= bit.read(4) << 0;
                epR[2] |= bit.read(6) << 0;
                epR[3] |= bit.read(6) << 0;
                break;

            default:
                break;
            }
        }

        const Bc6hModeInfo mi = s_bc6hModeInfo[mode];

        if (_signed) {
            epR[0] = sign_extend(epR[0], mi.endpointBits);
            epG[0] = sign_extend(epG[0], mi.endpointBits);
            epB[0] = sign_extend(epB[0], mi.endpointBits);
        }

        const uint8_t numSubsets = !!mi.partitionBits + 1;

        for (uint8_t ii = 1, num = numSubsets * 2; ii < num; ++ii) {
            if (_signed
                || mi.transformed) {
                epR[ii] = sign_extend(epR[ii], mi.deltaBits[0]);
                epG[ii] = sign_extend(epG[ii], mi.deltaBits[1]);
                epB[ii] = sign_extend(epB[ii], mi.deltaBits[2]);
            }

            if (mi.transformed) {
                const uint16_t mask = (1 << mi.endpointBits) - 1;

                epR[ii] = (epR[ii] + epR[0]) & mask;
                epG[ii] = (epG[ii] + epG[0]) & mask;
                epB[ii] = (epB[ii] + epB[0]) & mask;

                if (_signed) {
                    epR[ii] = sign_extend(epR[ii], mi.endpointBits);
                    epG[ii] = sign_extend(epG[ii], mi.endpointBits);
                    epB[ii] = sign_extend(epB[ii], mi.endpointBits);
                }
            }
        }

        for (uint8_t ii = 0, num = numSubsets * 2; ii < num; ++ii) {
            epR[ii] = unquantize(epR[ii], _signed, mi.endpointBits);
            epG[ii] = unquantize(epG[ii], _signed, mi.endpointBits);
            epB[ii] = unquantize(epB[ii], _signed, mi.endpointBits);
        }

        const uint8_t partitionSetIdx = uint8_t(mi.partitionBits ? bit.read(5) : 0);
        const uint8_t indexBits = mi.partitionBits ? 3 : 4;
        const uint8_t* factors = s_bptcFactors[indexBits - 2];

        for (uint8_t yy = 0; yy < 4; ++yy) {
            for (uint8_t xx = 0; xx < 4; ++xx) {
                const uint8_t idx = yy * 4 + xx;

                uint8_t subsetIndex = 0;
                uint8_t indexAnchor = 0;

                if (0 != mi.partitionBits) {
                    subsetIndex = (s_bptcP2[partitionSetIdx] >> idx) & 1;
                    indexAnchor = subsetIndex ? s_bptcA2[partitionSetIdx] : 0;
                }

                const uint8_t anchor = idx == indexAnchor;
                const uint8_t num = indexBits - anchor;
                const uint8_t index = (uint8_t)bit.read(num);

                const uint8_t fc = factors[index];
                const uint8_t fca = 64 - fc;
                const uint8_t fcb = fc;

                subsetIndex *= 2;
                uint16_t rr = finish_unquantize((epR[subsetIndex] * fca + epR[subsetIndex + 1] * fcb + 32) >> 6, _signed);
                uint16_t gg = finish_unquantize((epG[subsetIndex] * fca + epG[subsetIndex + 1] * fcb + 32) >> 6, _signed);
                uint16_t bb = finish_unquantize((epB[subsetIndex] * fca + epB[subsetIndex + 1] * fcb + 32) >> 6, _signed);

                _dst[idx] = Color(half_to_u8(rr), half_to_u8(gg), half_to_u8(bb), 255);
            }
        }
    }
}
