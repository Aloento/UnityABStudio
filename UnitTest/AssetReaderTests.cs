namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using System.IO;
    using AssetReader;
    using AssetReader.Unity3D;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AssetReaderTests {
        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab")]
        public void LoadAssetTest(string filePath) {
            var manager = new AssetsManager();
            manager.LoadFilesAsync(Path.GetFullPath(filePath)).Wait();
        }

        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", 2938589673199698669, null)]
        public void TryGetObjectByID(string filePath, long PathID, out UObject outObj) {
            var manager = new AssetsManager();
            manager.LoadFilesAsync(Path.GetFullPath(filePath)).Wait();

            foreach (var serializedFile in manager.AssetsFileList) {
                if (serializedFile.ObjectsDic.TryGetValue(PathID, out var obj)) {
                    outObj = obj;
                }
            }

            outObj = null;
            Assert.Fail($"{filePath} 中没有 {PathID}");
        }
    }
}
