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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using TextureDecoder;

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
        public void TextureDecodeTest(string filePath, long PathID) {
            Ioc.Default.ConfigureServices(new ServiceCollection().AddSingleton<TextureDecoderService>().BuildServiceProvider());

            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);

            if (obj is not Texture2D)
                Assert.Fail($"{PathID} 不是 {nameof(Texture2D)}");

            var img = ((Texture2D)obj).ConvertToImage(true);
            if (img == null)
                Assert.Fail("img 是 Null");

            img.SaveAsPng(@"C:\CaChe\UnityABStudio_TextureResult.png");
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
            try {
                Ioc.Default.ConfigureServices(new ServiceCollection().AddMemoryCache().AddSingleton<CacheService>().BuildServiceProvider());
            } catch (Exception) {
                // ignored
            }

            var cache = Ioc.Default.GetRequiredService<CacheService>();
            var a = cache.GetFileMD5Async(filePath);
            a.Wait();
            Console.WriteLine(a.Result);
        }
    }
}
