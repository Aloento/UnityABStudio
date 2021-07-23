namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Core.FMOD;
    using Helpers;

    [TestClass]
    public class CoreTests {
        [TestMethod]
        public void FMODDecodeTest() {
            var a = new AudioClipConverter(null);

            var m_AudioData = File.ReadAllBytes("Assets/TestAudio.bin");
            if (m_AudioData.Length == 0)
                Assert.Fail("文件读取失败");

            var exinfo = new CREATESOUNDEXINFO();
            var result = Factory.System_Create(out var system);
            if (result != RESULT.OK)
                Assert.Fail("创建实例失败");

            result = system.init(1, INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != RESULT.OK)
                Assert.Fail("初始化失败");

            Assert.AreEqual(8353696, m_AudioData.Length, $"数组大小不正常: {m_AudioData.Length}");
            exinfo.cbsize = Marshal.SizeOf(exinfo);
            exinfo.length = (uint)m_AudioData.Length;

            result = system.createSound(m_AudioData, MODE.OPENMEMORY, ref exinfo, out var sound);
            if (result != RESULT.OK)
                Assert.Fail("创建声音失败");

            result = sound.getNumSubSounds(out var numsubsounds);
            if (result != RESULT.OK)
                Assert.Fail("获得子声音数量失败");

            byte[] buff;
            if (numsubsounds > 0) {
                result = sound.getSubSound(0, out var subsound);
                if (result != RESULT.OK)
                    Assert.Fail("获得子声音失败");

                _ = buff = a.SoundToWav(subsound);
                _ = subsound.release();
            } else {
                _ = buff = a.SoundToWav(sound);
            }

            _ = sound.release();
            _ = system.release();

            Assert.AreEqual(true, ToFile.ByteArrayToFile(@"C:\CaChe\UnityABStudio_AudioResult.wav", buff), "导出失败");
        }

        [TestMethod]
        public void TextureDecodeTest() {

        }
    }
}
