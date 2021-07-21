#pragma once

namespace SoarCraft::QYun::TextureDecoder {
    using namespace System;
    using namespace IO;

    public ref class TextureDecoderService
    {
    public:
        TextureDecoderService();
        static bool IsBigEndian;

        bool DecodeDXT1(array<Byte>^ data, int width, int height, array<UInt32>^ image);

    private:
        Byte* ArrayToPointer(array<Byte>^ array);
        UInt32* ArrayToPointer(array<UInt32>^ array);

        void CopyBlockBuffer(long bx, long by, long w, long h, long bw,
                             long bh, UInt32* buffer, UInt32* image);

        inline unsigned Color(Byte r, Byte g, Byte b, Byte a);
        inline void RGB565LE(UInt16 q, Byte* r, Byte* g, Byte* b);

        inline void DecodeDXT1Block(Byte* data, UInt32* outbuf);
    };
}
