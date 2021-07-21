#pragma once
#include "PVRTC.h"

namespace SoarCraft::QYun::TextureDecoder {
    using namespace System;
    using namespace IO;

    public ref class TextureDecoderService
    {
    public:
        TextureDecoderService();
        static bool IsBigEndian;
        static UInt32 TRANSPARENT_MASK;

        bool DecodeDXT1(array<Byte>^ data, int width, int height, array<UInt32>^ image);
        bool DecodeDXT5(array<Byte>^ data, int width, int height, array<UInt32>^ image);

        bool DecodePVRTC(array<Byte>^ data, int width, int height, array<UInt32>^ image, bool is2bpp);

    private:
        inline Byte* Array2Ptr(array<Byte>^ array);
        inline UInt32* Array2Ptr(array<UInt32>^ array);

        inline void CopyBlockBuffer(long bx, long by, long w, long h, long bw,
                                    long bh, UInt32* buffer, UInt32* image);

        inline unsigned Color(Byte r, Byte g, Byte b, Byte a);
        inline void RGB565LE(UInt16 q, Byte* r, Byte* g, Byte* b);

        inline void DecodeDXT1Block(Byte* data, UInt32* outbuf);
        inline void DecodeDXT5Alpha(Byte* data, UInt32* outbuf, int channel);

        inline long MortonIndex(const long x, const long y, const long min_dim);
        inline void GetTexelColors(Byte* data, PVRTCTexelInfo* info);
        inline void GetTexelWeights2Bpp(Byte* data, PVRTCTexelInfo* info);
        inline void GetTexelWeights4Bpp(Byte* data, PVRTCTexelInfo* info);
        inline void ApplicateColor2Bpp(Byte* data, PVRTCTexelInfo* info[9], UInt32 buf[32]);
        inline void ApplicateColor4Bpp(Byte* data, PVRTCTexelInfo* info[9], UInt32 buf[32]);
    };
}
