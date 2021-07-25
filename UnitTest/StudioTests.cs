namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using AssetReader.Unity3D.Objects;
    using Core.Models;
    using Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StudioTests {
        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", -230973727348019624)]
        public void MonoBehaviourTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not MonoBehaviour)
                Assert.Fail($"{PathID} 不是 {nameof(MonoBehaviour)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportMonoBehaviour", new AssetItem(obj, out _, out _), @"C:\CaChe\Result");

            Assert.AreEqual(true, res, "转换失败");
        }
    }
}
