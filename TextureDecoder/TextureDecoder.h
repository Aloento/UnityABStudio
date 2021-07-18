#pragma once

namespace SoarCraft::QYun::TextureDecoder {
    using namespace System;
    using namespace IO;
    using namespace AssetReader::Utils;

    public ref class TextureDecoderService
    {
    public:
        TextureDecoderService();
        static bool NativeEndianness; // true for Big, false for Little

        bool DecodeDXT1(UnityReader^ data, int width, int height, MemoryStream^ image);

    private:
        static unsigned 

        inline unsigned Color(Byte r, Byte g, Byte b);
        inline void DecodeDXT1Block(UnityReader^ data, MemoryStream^ image);
        inline void RGB565LE(UInt16 q, Byte* r, Byte* g, Byte* b);
    };
}
