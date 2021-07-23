namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using AssetReader.Unity3D.Objects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Helpers;

    [TestClass]
    public class CoreTests {
        [DataTestMethod]
        [DataRow("Assets/m_bat_exterminate_intro.ab", 4816768734469872508)]
        public void FMODDecodeTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not AudioClip)
                Assert.Fail($"{PathID} 不是 {nameof(AudioClip)}");

            if (!ToFile.ByteArrayToFile(@"C:\CaChe\UnityABStudio_AudioResult.wav",
                new AudioClipConverter((AudioClip)obj).ConvertToWav())) {
                Assert.Fail("导出失败");
            }
        }

        [TestMethod]
        public void TextureDecodeTest() {
        }
    }
}
