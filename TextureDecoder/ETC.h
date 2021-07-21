#pragma once
#include "pch.h"
#include "stdint.h"

namespace SoarCraft::QYun::TextureDecoder {
    const uint_fast8_t WriteOrderTable[16] = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
    const uint_fast8_t WriteOrderTableRev[16] = { 15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0 };
    const uint_fast8_t Etc1ModifierTable[8][2] = { {2, 8},   {5, 17},  {9, 29},   {13, 42},
                                                  {18, 60}, {24, 80}, {33, 106}, {47, 183} };
    const uint_fast8_t Etc2aModifierTable[2][8][2] = {
      {{0, 8}, {0, 17}, {0, 29}, {0, 42}, {0, 60}, {0, 80}, {0, 106}, {0, 183}},
      {{2, 8}, {5, 17}, {9, 29}, {13, 42}, {18, 60}, {24, 80}, {33, 106}, {47, 183}} };
    const uint_fast8_t Etc1SubblockTable[2][16] = { {0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
                                                   {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1} };
    const uint_fast8_t Etc2DistanceTable[8] = { 3, 6, 11, 16, 23, 32, 41, 64 };
    const int_fast8_t Etc2AlphaModTable[16][8] = {
      {-3, -6, -9, -15, 2, 5, 8, 14}, {-3, -7, -10, -13, 2, 6, 9, 12}, {-2, -5, -8, -13, 1, 4, 7, 12},
      {-2, -4, -6, -13, 1, 3, 5, 12}, {-3, -6, -8, -12, 2, 5, 7, 11},  {-3, -7, -9, -11, 2, 6, 8, 10},
      {-4, -7, -8, -11, 3, 6, 7, 10}, {-3, -5, -8, -11, 2, 4, 7, 10},  {-2, -6, -8, -10, 1, 5, 7, 9},
      {-2, -5, -8, -10, 1, 4, 7, 9},  {-2, -4, -8, -10, 1, 3, 7, 9},   {-2, -5, -7, -10, 1, 4, 6, 9},
      {-3, -4, -7, -10, 2, 3, 6, 9},  {-1, -2, -3, -10, 0, 1, 2, 9},   {-4, -6, -8, -9, 3, 5, 7, 8},
      {-3, -5, -7, -9, 2, 4, 6, 8} };


    uint_fast8_t TextureDecoderService::Clamp(const int n) {
        return n < 0 ? 0 : n > 255 ? 255 : n;
    }

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
