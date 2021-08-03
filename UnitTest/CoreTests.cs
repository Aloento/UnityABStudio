namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using System;
    using System.IO;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.Shaders;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Converters;
    using Converters.ShaderConverters;
    using Core.Services;
    using Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TextureDecoderNET;

    [TestClass]
    public class CoreTests {
        [DataTestMethod]
        [DataRow("Assets/m_bat_exterminate_intro.ab", 4816768734469872508)]
        public void FMODDecodeTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not AudioClip)
                Assert.Fail($"{PathID} 不是 {nameof(AudioClip)}");

            if (!Helpers.ByteArrayToFile(@"C:\CaChe\UnityABStudio_AudioResult.wav",
                new AudioClipConverter((AudioClip)obj).ConvertToWav())) {
                Assert.Fail("导出失败");
            }
        }

        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", 2938589673199698669)]
        [DataRow("Assets/char_1012_skadi2.ab", -832438985796037128)]
        [DataRow("Assets/char_1012_skadi2.ab", -403375242656156078)]
        [DataRow("Assets/char_1012_skadi2.ab", 7717416592048335994)]
        [DataRow("Assets/char_1012_skadi2.ab", 7422149870479952578)]
        [DataRow("Assets/char_1012_skadi2.ab", -5194634970442144186)]
        [DataRow("Assets/char_1012_skadi2.ab", -2020027452845813661)]
        [DataRow("Assets/char_1012_skadi2.ab", -4093579471333325247)]
        [DataRow("Assets/char_1012_skadi2.ab", 1035854462024827039)]
        [DataRow("Assets/char_1012_skadi2.ab", 6003267157781619085)]
        [DataRow("Assets/char_1012_skadi2.ab", 2222060065091517879)]
        [DataRow("Assets/char_1012_skadi2.ab", -5045650368308443878)]
        public void TextureDecodeTest(string filePath, long PathID) {
            StudioTests.TryInitIoc();

            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);

            if (obj is not Texture2D)
                Assert.Fail($"{PathID} 不是 {nameof(Texture2D)}");

            var img = ((Texture2D)obj).ConvertToImage(true);
            if (img == null)
                Assert.Fail("img 是 Null");

            img.SaveAsPng($@"C:\CaChe\UnityABStudio_TextureResult{DateTime.Now.Ticks}.png");
        }

        [DataTestMethod]
        [DataRow("Assets/s_background_chernobog_b.ab", 952725256833404699)]
        public void ShaderConvertTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);

            if (obj is not Shader)
                Assert.Fail($"{PathID} 不是 {nameof(Shader)}");

            var str = ((Shader)obj).Convert();
            File.WriteAllText(@"C:\CaChe\UnityABStudio_ShaderResult.txt", str);
        }

        [DataTestMethod]
        [DataRow("Assets/[pack]common.ab")]
        [DataRow("Assets/char_1012_skadi2.ab")]
        [DataRow("Assets/gacha_phase_0.ab")]
        [DataRow("Assets/m_bat_exterminate_intro.ab")]
        [DataRow("Assets/s_background_chernobog_b.ab")]
        public void MD5Test(string filePath) {
            StudioTests.TryInitIoc();

            var cache = Ioc.Default.GetRequiredService<CacheService>();
            var a = cache.GetFileMD5Async(filePath);
            a.Wait();
            Console.WriteLine(a.Result);
        }

        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", 2938589673199698669)]
        public void ETC1Test(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);

            if (obj is not Texture2D)
                Assert.Fail($"{PathID} 不是 {nameof(Texture2D)}");

            var texture = (Texture2D)obj;
            _ = new ETC1Decoder(texture.image_data.GetData(), (ulong)texture.m_Width, (ulong)texture.m_Height, out var data);

            if (data is { Length: > 0 }) {
                var image = Image.LoadPixelData<Bgra32>(data, texture.m_Width, texture.m_Height);
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                image.SaveAsPng($@"C:\CaChe\UnityABStudio_TextureResult{DateTime.Now.Ticks}.png");
            } else {
                Assert.Fail("数组为空");
            }
        }
    }
}
