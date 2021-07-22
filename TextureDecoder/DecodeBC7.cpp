#include "pch.h"
#include "TextureDecoder.h"
#include "BCN.h"
#include <utility>

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
        BitReader bit(_src);

        uint8_t mode = 0;
        for (; mode < 8 && 0 == bit.read(1); ++mode)
        {
        }

        if (mode == 8)
        {
            memset(_dst, 0, 16 * 4);
            return;
        }

        const Bc7ModeInfo& mi = s_bp7ModeInfo[mode];
        const uint8_t modePBits = 0 != mi.endpointPBits
            ? mi.endpointPBits
            : mi.sharedPBits
            ;

        const uint8_t partitionSetIdx = uint8_t(bit.read(mi.partitionBits));
        const uint8_t rotationMode = uint8_t(bit.read(mi.rotationBits));
        const uint8_t indexSelectionMode = uint8_t(bit.read(mi.indexSelectionBits));

        uint8_t epR[6];
        uint8_t epG[6];
        uint8_t epB[6];
        uint8_t epA[6];

        for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
        {
            epR[ii * 2 + 0] = uint8_t(bit.read(mi.colorBits) << modePBits);
            epR[ii * 2 + 1] = uint8_t(bit.read(mi.colorBits) << modePBits);
        }

        for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
        {
            epG[ii * 2 + 0] = uint8_t(bit.read(mi.colorBits) << modePBits);
            epG[ii * 2 + 1] = uint8_t(bit.read(mi.colorBits) << modePBits);
        }

        for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
        {
            epB[ii * 2 + 0] = uint8_t(bit.read(mi.colorBits) << modePBits);
            epB[ii * 2 + 1] = uint8_t(bit.read(mi.colorBits) << modePBits);
        }

        if (mi.alphaBits)
        {
            for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
            {
                epA[ii * 2 + 0] = uint8_t(bit.read(mi.alphaBits) << modePBits);
                epA[ii * 2 + 1] = uint8_t(bit.read(mi.alphaBits) << modePBits);
            }
        }
        else
        {
            memset(epA, 0xff, 6);
        }

        if (0 != modePBits)
        {
            for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
            {
                const uint8_t pda = uint8_t(bit.read(modePBits));
                const uint8_t pdb = uint8_t(0 == mi.sharedPBits ? bit.read(modePBits) : pda);

                epR[ii * 2 + 0] |= pda;
                epR[ii * 2 + 1] |= pdb;
                epG[ii * 2 + 0] |= pda;
                epG[ii * 2 + 1] |= pdb;
                epB[ii * 2 + 0] |= pda;
                epB[ii * 2 + 1] |= pdb;
                epA[ii * 2 + 0] |= pda;
                epA[ii * 2 + 1] |= pdb;
            }
        }

        const uint8_t colorBits = mi.colorBits + modePBits;

        for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
        {
            epR[ii * 2 + 0] = expand_quantized(epR[ii * 2 + 0], colorBits);
            epR[ii * 2 + 1] = expand_quantized(epR[ii * 2 + 1], colorBits);
            epG[ii * 2 + 0] = expand_quantized(epG[ii * 2 + 0], colorBits);
            epG[ii * 2 + 1] = expand_quantized(epG[ii * 2 + 1], colorBits);
            epB[ii * 2 + 0] = expand_quantized(epB[ii * 2 + 0], colorBits);
            epB[ii * 2 + 1] = expand_quantized(epB[ii * 2 + 1], colorBits);
        }

        if (mi.alphaBits)
        {
            const uint8_t alphaBits = mi.alphaBits + modePBits;

            for (uint8_t ii = 0; ii < mi.numSubsets; ++ii)
            {
                epA[ii * 2 + 0] = expand_quantized(epA[ii * 2 + 0], alphaBits);
                epA[ii * 2 + 1] = expand_quantized(epA[ii * 2 + 1], alphaBits);
            }
        }

        const bool hasIndexBits1 = 0 != mi.indexBits[1];

        const uint8_t* factors[] =
        {
                            s_bptcFactors[mi.indexBits[0] - 2],
            hasIndexBits1 ? s_bptcFactors[mi.indexBits[1] - 2] : factors[0],
        };

        uint16_t offset[2] =
        {
            0,
            uint16_t(mi.numSubsets * (16 * mi.indexBits[0] - 1)),
        };

        for (uint8_t yy = 0; yy < 4; ++yy)
        {
            for (uint8_t xx = 0; xx < 4; ++xx)
            {
                const uint8_t idx = yy * 4 + xx;

                uint8_t subsetIndex = 0;
                uint8_t indexAnchor = 0;
                switch (mi.numSubsets)
                {
                case 2:
                    subsetIndex = (s_bptcP2[partitionSetIdx] >> idx) & 1;
                    indexAnchor = 0 != subsetIndex ? s_bptcA2[partitionSetIdx] : 0;
                    break;

                case 3:
                    subsetIndex = (s_bptcP3[partitionSetIdx] >> (2 * idx)) & 3;
                    indexAnchor = 0 != subsetIndex ? s_bptcA3[subsetIndex - 1][partitionSetIdx] : 0;
                    break;

                default:
                    break;
                }

                const uint8_t anchor = idx == indexAnchor;
                const uint8_t num[2] =
                {
                    uint8_t(mi.indexBits[0] - anchor),
                    uint8_t(hasIndexBits1 ? mi.indexBits[1] - anchor : 0),
                };

                const uint8_t index[2] =
                {
                                    (uint8_t)bit.peek(offset[0], num[0]),
                    hasIndexBits1 ? (uint8_t)bit.peek(offset[1], num[1]) : index[0],
                };

                offset[0] += num[0];
                offset[1] += num[1];

                const uint8_t fc = factors[indexSelectionMode][index[indexSelectionMode]];
                const uint8_t fa = factors[!indexSelectionMode][index[!indexSelectionMode]];

                const uint8_t fca = 64 - fc;
                const uint8_t fcb = fc;
                const uint8_t faa = 64 - fa;
                const uint8_t fab = fa;

                subsetIndex *= 2;
                uint8_t rr = uint8_t(uint16_t(epR[subsetIndex] * fca + epR[subsetIndex + 1] * fcb + 32) >> 6);
                uint8_t gg = uint8_t(uint16_t(epG[subsetIndex] * fca + epG[subsetIndex + 1] * fcb + 32) >> 6);
                uint8_t bb = uint8_t(uint16_t(epB[subsetIndex] * fca + epB[subsetIndex + 1] * fcb + 32) >> 6);
                uint8_t aa = uint8_t(uint16_t(epA[subsetIndex] * faa + epA[subsetIndex + 1] * fab + 32) >> 6);

                switch (rotationMode)
                {
                case 1: std::swap(aa, rr); break;
                case 2: std::swap(aa, gg); break;
                case 3: std::swap(aa, bb); break;
                default:                  break;
                };

                _dst[idx] = Color(rr, gg, bb, aa);
            }
        }
    }
}
