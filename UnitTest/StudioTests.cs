namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.Meshes;
    using AssetReader.Unity3D.Objects.Sprites;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Core.Models;
    using Core.Services;
    using Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Services;
    using TextureDecoder;
    using static Helpers;

    [TestClass]
    public class StudioTests {
        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", -230973727348019624)]
        public void MonoBehaviourExpTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not MonoBehaviour)
                Assert.Fail($"{PathID} 不是 {nameof(MonoBehaviour)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportMonoBehaviour", new AssetItem(obj, out _, out _), @"C:\CaChe\Result");

            Assert.AreEqual(true, res, "导出失败");
        }

        [DataTestMethod]
        [DataRow("Assets/s_background_chernobog_b.ab", 5421374963156575731)]
        public void MeshExpTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not Mesh)
                Assert.Fail($"{PathID} 不是 {nameof(Mesh)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportMesh", new AssetItem(obj, out _, out _), @"C:\CaChe\Result");

            Assert.AreEqual(true, res, "导出失败");
        }

        [DataTestMethod]
        [DataRow("Assets/[pack]common.ab", 3280994511229409322)]
        public void FontExpTest(string filePath, long PathID) {
            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not Font)
                Assert.Fail($"{PathID} 不是 {nameof(Font)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportFont", new AssetItem(obj, out _, out _), @"C:\CaChe\Result");

            Assert.AreEqual(true, res, "导出失败");
        }

        [DataTestMethod]
        [DataRow("Assets/gacha_phase_0.ab", 133153641501856485)]
        public void AnimatorExpTest(string filePath, long PathID) {
            Ioc.Default.ConfigureServices(new ServiceCollection().
                AddSingleton<FBXHelpService>().AddSingleton<SettingsService>().BuildServiceProvider());

            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not Animator)
                Assert.Fail($"{PathID} 不是 {nameof(Animator)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportAnimator", new AssetItem(obj, out _, out _), @"C:\CaChe\Result", null);

            Assert.AreEqual(true, res, "导出失败");
            Assert.Fail("已知问题：导出内容不全，暂无修复头绪");
        }

        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", 3197167805266407399)]
        public void SpriteExpTest(string filePath, long PathID) {
            Ioc.Default.ConfigureServices(new ServiceCollection().
                AddSingleton<FBXHelpService>().AddSingleton<TextureDecoderService>().
                AddSingleton<SettingsService>().BuildServiceProvider());

            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not Sprite)
                Assert.Fail($"{PathID} 不是 {nameof(Sprite)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportSprite", new AssetItem(obj, out _, out _), @"C:\CaChe\Result");

            Assert.AreEqual(true, res, "导出失败");
        }

        [DataTestMethod]
        [DataRow("Assets/char_1012_skadi2.ab", 2927026290139671665)]
        public void TextExpTest(string filePath, long PathID) {
            Ioc.Default.ConfigureServices(new ServiceCollection().
                AddSingleton<FBXHelpService>().
                AddSingleton<SettingsService>().BuildServiceProvider());

            new AssetReaderTests().TryGetObjectByID(filePath, PathID, out var obj);
            if (obj is not TextAsset)
                Assert.Fail($"{PathID} 不是 {nameof(TextAsset)}");

            var target = new PrivateObject(typeof(ExportExtension));
            var res = target.Invoke("ExportTextAsset", new AssetItem(obj, out _, out _), @"C:\CaChe\Result");

            Assert.AreEqual(true, res, "导出失败");
        }
    }
}
