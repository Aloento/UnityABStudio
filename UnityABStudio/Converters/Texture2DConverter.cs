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
                case TextureFormat.Alpha8:
                    bytes = this.DecodeAlpha8();
                    break;
                case TextureFormat.ARGB4444:
                    this.SwapBytesForXbox();
                    bytes = this.DecodeARGB4444();
                    break;
                case TextureFormat.RGB24:
                    bytes = this.DecodeRGB24();
                    break;
                case TextureFormat.RGBA32:
                    bytes = this.DecodeRGBA32();
                    break;
                case TextureFormat.ARGB32:
                    bytes = this.DecodeARGB32();
                    break;
                case TextureFormat.RGB565:
                    this.SwapBytesForXbox();
                    bytes = this.DecodeRGB565();
                    break;
                case TextureFormat.R16:
                    bytes = this.DecodeR16();
                    break;
                case TextureFormat.DXT1:
                    this.SwapBytesForXbox();
                    bytes = this.DecodeDXT1();
                    break;
                case TextureFormat.DXT5:
                    this.SwapBytesForXbox();
                    bytes = this.DecodeDXT5();
                    break;
                case TextureFormat.RGBA4444:
                    bytes = this.DecodeRGBA4444();
                    break;
                case TextureFormat.BGRA32:
                    bytes = this.DecodeBGRA32();
                    break;
                case TextureFormat.RHalf:
                    bytes = this.DecodeRHalf();
                    break;
                case TextureFormat.RGHalf:
                    bytes = this.DecodeRGHalf();
                    break;
                case TextureFormat.RGBAHalf:
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
                case TextureFormat.YUY2:
                    bytes = this.DecodeYUY2();
                    break;
                case TextureFormat.RGB9e5Float:
                    bytes = this.DecodeRGB9e5Float();
                    break;
                case TextureFormat.BC4:
                    bytes = this.DecodeBC4();
                    break;
                case TextureFormat.BC5:
                    bytes = this.DecodeBC5();
                    break;
                case TextureFormat.BC6H:
                    bytes = this.DecodeBC6H();
                    break;
                case TextureFormat.BC7:
                    bytes = this.DecodeBC7();
                    break;
                case TextureFormat.DXT1Crunched:
                    if (this.UnpackCrunch())
                        bytes = this.DecodeDXT1();

                    break;
                case TextureFormat.DXT5Crunched:
                    if (this.UnpackCrunch())
                        bytes = this.DecodeDXT5();

                    break;
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                    bytes = this.DecodePVRTC(true);
                    break;
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                    bytes = this.DecodePVRTC(false);
                    break;
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC_RGB4_3DS:
                    bytes = this.DecodeETC1();
                    break;
                case TextureFormat.ATC_RGB4:
                    bytes = this.DecodeATCRGB4();
                    break;
                case TextureFormat.ATC_RGBA8:
                    bytes = this.DecodeATCRGBA8();
                    break;
                case TextureFormat.EAC_R:
                    bytes = this.DecodeEACR();
                    break;
                case TextureFormat.EAC_R_SIGNED:
                    bytes = this.DecodeEACRSigned();
                    break;
                case TextureFormat.EAC_RG:
                    bytes = this.DecodeEACRG();
                    break;
                case TextureFormat.EAC_RG_SIGNED:
                    bytes = this.DecodeEACRGSigned();
                    break;
                case TextureFormat.ETC2_RGB:
                    bytes = this.DecodeETC2();
                    break;
                case TextureFormat.ETC2_RGBA1:
                    bytes = this.DecodeETC2A1();
                    break;
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC_RGBA8_3DS:
                    bytes = this.DecodeETC2A8();
                    break;
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_HDR_4x4:
                    bytes = this.DecodeASTC(4);
                    break;
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_HDR_5x5:
                    bytes = this.DecodeASTC(5);
                    break;
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_HDR_6x6:
                    bytes = this.DecodeASTC(6);
                    break;
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.ASTC_HDR_8x8:
                    bytes = this.DecodeASTC(8);
                    break;
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_HDR_10x10:
                    bytes = this.DecodeASTC(10);
                    break;
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_12x12:
                case TextureFormat.ASTC_HDR_12x12:
                    bytes = this.DecodeASTC(12);
                    break;
                case TextureFormat.RG16:
                    bytes = this.DecodeRG16();
                    break;
                case TextureFormat.R8:
                    bytes = this.DecodeR8();
                    break;
                case TextureFormat.ETC_RGB4Crunched:
                    if (this.UnpackCrunch())
                        bytes = this.DecodeETC1();

                    break;
                case TextureFormat.ETC2_RGBA8Crunched:
                    if (this.UnpackCrunch())
                        bytes = this.DecodeETC2A8();

                    break;
            }

            return bytes;
        }
    }
}
