namespace SoarCraft.QYun.UnityABStudio.Core.Helpers {
    using System.IO;
    using AssetReader.Utils;
    using TextureDecoder;

    public class TextureDecoderHelper {
        public void Test() {
            var data = new UnityReader("123");
            var width = 10;
            var height = 20;
            var buff = new MemoryStream(width * height * 4);

            var decoder = new TextureDecoderService();
            var result = decoder.DecodeDXT1(data, width, height, buff);
        }
    }
}
