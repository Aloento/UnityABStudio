namespace SoarCraft.QYun.UnityABStudio.Converters {
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Texture2Ds;

    public partial class Texture2DConverter {
        private readonly int width;
        private readonly int height;
        private readonly TextureFormat textureFormat;
        private int image_data_size;
        private byte[] image_data;
        private readonly int[] version;
        private readonly BuildTarget platform;

        public Texture2DConverter(Texture2D texture) {
            this.image_data = texture.image_data.GetData();
            this.image_data_size = this.image_data.Length;
            this.width = texture.m_Width;
            this.height = texture.m_Height;
            this.textureFormat = texture.m_TextureFormat;
            this.version = texture.version;
            this.platform = texture.platform;
        }

        public byte[] DecodeTexture2D() {
            byte[] bytes = null;
            switch (this.textureFormat) {
                case TextureFormat.Alpha8: //test pass
                    bytes = this.DecodeAlpha8();
                    break;
                case TextureFormat.ARGB4444: //test pass
                    this.SwapBytesForXbox();
                    bytes = this.DecodeARGB4444();
                    break;
                case TextureFormat.RGB24: //test pass
                    bytes = this.DecodeRGB24();
                    break;
                case TextureFormat.RGBA32: //test pass
                    bytes = this.DecodeRGBA32();
                    break;
                case TextureFormat.ARGB32: //test pass
                    bytes = this.DecodeARGB32();
                    break;
                case TextureFormat.RGB565: //test pass
                    this.SwapBytesForXbox();
                    bytes = this.DecodeRGB565();
                    break;
                case TextureFormat.R16: //test pass
                    bytes = this.DecodeR16();
                    break;
                case TextureFormat.DXT1: //test pass
                    this.SwapBytesForXbox();
                    bytes = this.DecodeDXT1();
                    break;
                case TextureFormat.DXT5: //test pass
                    this.SwapBytesForXbox();
                    bytes = this.DecodeDXT5();
                    break;
                case TextureFormat.RGBA4444: //test pass
                    bytes = this.DecodeRGBA4444();
                    break;
                case TextureFormat.BGRA32: //test pass
                    bytes = this.DecodeBGRA32();
                    break;
                case TextureFormat.RHalf:
                    bytes = this.DecodeRHalf();
                    break;
                case TextureFormat.RGHalf:
                    bytes = this.DecodeRGHalf();
                    break;
                case TextureFormat.RGBAHalf: //test pass
                    bytes = this.DecodeRGBAHalf();
                    break;
                case TextureFormat.RFloat:
                    bytes = this.DecodeRFloat();
                    break;
                case TextureFormat.RGFloat:
                    bytes = this.DecodeRGFloat();
                    break;
                case TextureFormat.RGBAFloat:
                    bytes = this.DecodeRGBAFloat();
                    break;
                case TextureFormat.YUY2: //test pass
                    bytes = this.DecodeYUY2();
                    break;
                case TextureFormat.RGB9e5Float: //test pass
                    bytes = this.DecodeRGB9e5Float();
                    break;
                case TextureFormat.BC4: //test pass
                    bytes = this.DecodeBC4();
                    break;
                case TextureFormat.BC5: //test pass
                    bytes = this.DecodeBC5();
                    break;
                case TextureFormat.BC6H: //test pass
                    bytes = this.DecodeBC6H();
                    break;
                case TextureFormat.BC7: //test pass
                    bytes = this.DecodeBC7();
                    break;
                case TextureFormat.DXT1Crunched: //test pass
                    if (this.UnpackCrunch()) {
                        bytes = this.DecodeDXT1();
                    }

                    break;
                case TextureFormat.DXT5Crunched: //test pass
                    if (this.UnpackCrunch()) {
                        bytes = this.DecodeDXT5();
                    }

                    break;
                case TextureFormat.PVRTC_RGB2: //test pass
                case TextureFormat.PVRTC_RGBA2: //test pass
                    bytes = this.DecodePVRTC(true);
                    break;
                case TextureFormat.PVRTC_RGB4: //test pass
                case TextureFormat.PVRTC_RGBA4: //test pass
                    bytes = this.DecodePVRTC(false);
                    break;
                case TextureFormat.ETC_RGB4: //test pass
                case TextureFormat.ETC_RGB4_3DS:
                    bytes = this.DecodeETC1();
                    break;
                case TextureFormat.ATC_RGB4: //test pass
                    bytes = this.DecodeATCRGB4();
                    break;
                case TextureFormat.ATC_RGBA8: //test pass
                    bytes = this.DecodeATCRGBA8();
                    break;
                case TextureFormat.EAC_R: //test pass
                    bytes = this.DecodeEACR();
                    break;
                case TextureFormat.EAC_R_SIGNED:
                    bytes = this.DecodeEACRSigned();
                    break;
                case TextureFormat.EAC_RG: //test pass
                    bytes = this.DecodeEACRG();
                    break;
                case TextureFormat.EAC_RG_SIGNED:
                    bytes = this.DecodeEACRGSigned();
                    break;
                case TextureFormat.ETC2_RGB: //test pass
                    bytes = this.DecodeETC2();
                    break;
                case TextureFormat.ETC2_RGBA1: //test pass
                    bytes = this.DecodeETC2A1();
                    break;
                case TextureFormat.ETC2_RGBA8: //test pass
                case TextureFormat.ETC_RGBA8_3DS:
                    bytes = this.DecodeETC2A8();
                    break;
                case TextureFormat.ASTC_RGB_4x4: //test pass
                case TextureFormat.ASTC_RGBA_4x4: //test pass
                case TextureFormat.ASTC_HDR_4x4: //test pass
                    bytes = this.DecodeASTC(4);
                    break;
                case TextureFormat.ASTC_RGB_5x5: //test pass
                case TextureFormat.ASTC_RGBA_5x5: //test pass
                case TextureFormat.ASTC_HDR_5x5: //test pass
                    bytes = this.DecodeASTC(5);
                    break;
                case TextureFormat.ASTC_RGB_6x6: //test pass
                case TextureFormat.ASTC_RGBA_6x6: //test pass
                case TextureFormat.ASTC_HDR_6x6: //test pass
                    bytes = this.DecodeASTC(6);
                    break;
                case TextureFormat.ASTC_RGB_8x8: //test pass
                case TextureFormat.ASTC_RGBA_8x8: //test pass
                case TextureFormat.ASTC_HDR_8x8: //test pass
                    bytes = this.DecodeASTC(8);
                    break;
                case TextureFormat.ASTC_RGB_10x10: //test pass
                case TextureFormat.ASTC_RGBA_10x10: //test pass
                case TextureFormat.ASTC_HDR_10x10: //test pass
                    bytes = this.DecodeASTC(10);
                    break;
                case TextureFormat.ASTC_RGB_12x12: //test pass
                case TextureFormat.ASTC_RGBA_12x12: //test pass
                case TextureFormat.ASTC_HDR_12x12: //test pass
                    bytes = this.DecodeASTC(12);
                    break;
                case TextureFormat.RG16: //test pass
                    bytes = this.DecodeRG16();
                    break;
                case TextureFormat.R8: //test pass
                    bytes = this.DecodeR8();
                    break;
                case TextureFormat.ETC_RGB4Crunched: //test pass
                    if (this.UnpackCrunch()) {
                        bytes = this.DecodeETC1();
                    }

                    break;
                case TextureFormat.ETC2_RGBA8Crunched: //test pass
                    if (this.UnpackCrunch()) {
                        bytes = this.DecodeETC2A8();
                    }

                    break;
            }

            return bytes;
        }
    }
}
