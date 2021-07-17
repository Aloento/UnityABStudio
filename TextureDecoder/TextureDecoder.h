#pragma once

namespace SoarCraft::QYun::TextureDecoder {
    using namespace System;
    using namespace IO;
    using namespace AssetReader::Utils;

    public ref class TextureDecoderService
    {
    public:
        bool DecodeDXT1(UnityReader^ data, int width, int height, MemoryStream^ image);
    };
}
