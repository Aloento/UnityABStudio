namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using System;
    using System.IO;
    using AssetReader;
    using AssetReader.Unity3D;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AssetReaderTests {
        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", null)]
        [DataRow("Assets/m_bat_exterminate_intro.ab", null)]
        public void LoadAssetTest(string filePath, out AssetsManager outManager) {
            Console.WriteLine($"{filePath}");
            outManager = new AssetsManager();
            outManager.LoadFilesAsync(Path.GetFullPath(filePath)).Wait();
        }

        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", 2938589673199698669, null)]
        public void TryGetObjectByID(string filePath, long PathID, out UObject outObj) {
            LoadAssetTest(filePath, out var manager);

            foreach (var serializedFile in manager.AssetsFileList) {
                if (serializedFile.ObjectsDic.TryGetValue(PathID, out var obj)) {
                    outObj = obj;
                    Console.WriteLine($"{outObj}");
                    return;
                }
            }

            outObj = null;
            Assert.Fail($"{filePath} 中没有 {PathID}");
        }
    }
}
