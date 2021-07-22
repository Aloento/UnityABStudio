#pragma once
#include "PVRTC.h"
#include "stdint.h"
#include "ASTC.h"

#define A2PInsert \
    Byte* d = Array2Ptr(data);\
    UInt32* i = Array2Ptr(image);\
    UInt32 buffer[16];

#define FastInsert \
    long num_blocks_x = (width + 3) / 4; \
    long num_blocks_y = (height + 3) / 4;\
    A2PInsert;

#define BCInsert \
    uint32_t m_block_width = 4; \
    uint32_t m_block_height = 4; \
    uint32_t m_blocks_x = (width + m_block_width - 1) / m_block_width; \
    uint32_t m_blocks_y = (height + m_block_height - 1) / m_block_height; \
    A2PInsert;

#define FastArgs array<Byte>^ data, int width, int height, array<UInt32>^ image

namespace SoarCraft::QYun::TextureDecoder {
    using namespace System;
    using namespace IO;

    public ref class TextureDecoderService
    {
    public:
        TextureDecoderService();
        static bool IsBigEndian;
        static UInt32 TRANSPARENT_MASK;
        array<Byte>^ UnpackCrunch(array<Byte>^ data);
        array<Byte>^ UnpackUnityCrunch(array<Byte>^ data);

        bool DecodeDXT1(FastArgs);
        bool DecodeDXT5(FastArgs);

        bool DecodePVRTC(FastArgs, bool is2bpp);

        bool DecodeETC1(FastArgs);
        bool DecodeETC2(FastArgs);
        bool DecodeETC2A1(FastArgs);
        bool DecodeETC2A8(FastArgs);

        bool DecodeEACR(FastArgs);
        bool DecodeEACRSigned(FastArgs);
        bool DecodeEACRG(FastArgs);
        bool DecodeEACRGSigned(FastArgs);

        bool DecodeBC4(FastArgs);
        bool DecodeBC5(FastArgs);
        bool DecodeBC6(FastArgs);
        bool DecodeBC7(FastArgs);

        bool DecodeATCRGB4(FastArgs);
        bool DecodeATCRGBA8(FastArgs);
        bool DecodeASTC(FastArgs, int32_t blockWidth, int32_t blockHeight);

    private:
        inline Byte* Array2Ptr(array<Byte>^ array);
        inline UInt32* Array2Ptr(array<UInt32>^ array);

        void DisposeBuffer(void** ppBuffer);
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

        inline uint32_t ApplicateColor(uint_fast8_t c[3], int_fast16_t m);
        inline uint32_t ApplicateColorAlpha(uint_fast8_t c[3], int_fast16_t m, int transparent);
        inline uint32_t ApplicateColorRaw(uint_fast8_t c[3]);
        inline void DecodeETC1Block(Byte* data, UInt32* outbuf);
        inline void DecodeETC2Block(Byte* data, UInt32* outbuf);
        inline void DecodeETC2A1Block(Byte* data, UInt32* outbuf);
        inline void DecodeETC2A8Block(Byte* data, UInt32* outbuf);

        inline void DecodeEACBlock(Byte* data, int color, UInt32* outbuf);
        inline void DecodeEACSignedBlock(Byte* data, int color, UInt32* outbuf);

        inline void DecodeBC6Block(const uint8_t* _src, uint32_t* _dst, bool _signed);
        inline void DecodeBC7Block(const uint8_t* _src, uint32_t* _dst);

        inline void DecodeATCBlock(const uint8_t* _src, uint32_t* _dst);
        inline void DecodeBlock(const uint8_t* buf, const int bw, const int bh, uint32_t* outbuf);

        inline void DecodeBlockParams(const uint8_t* buf, BlockData* block_data);
        inline void DecodeEndpoints(const uint8_t* buf, BlockData* data);
        inline void DecodeWeights(const uint8_t* buf, BlockData* data);
        inline void ApplicateColor(const BlockData* data, uint32_t* outbuf);
    };
}
