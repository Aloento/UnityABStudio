namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using Helpers;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using System.IO;
    using Core.Entities;

    public static class Texture2DExtension {
        public static Image ConvertToImage(this Texture2D m_Texture2D, bool flip) {
            var converter = new Texture2DConverter(m_Texture2D);
            var bytes = converter.DecodeTexture2D();
            if (bytes is { Length: > 0 }) {
                var image = Image.LoadPixelData<Bgra32>(bytes, m_Texture2D.m_Width, m_Texture2D.m_Height);
                if (flip) {
                    image.Mutate(x => x.Flip(FlipMode.Vertical));
                }
                return image;
            }
            return null;
        }

        public static MemoryStream ConvertToStream(this Texture2D m_Texture2D, ImageFormat imageFormat, bool flip) {
            var image = ConvertToImage(m_Texture2D, flip);
            if (image != null) {
                using (image) {
                    return image.ConvertToStream(imageFormat);
                }
            }
            return null;
        }
    }
}
