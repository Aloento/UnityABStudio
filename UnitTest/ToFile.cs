namespace SoarCraft.QYun.UnityABStudio.UnitTest {
    using System.IO;

    public static class ToFile {
        public static bool ByteArrayToFile(string fileName, byte[] byteArray) {
            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            fs.Write(byteArray, 0, byteArray.Length);
            return true;
        }
    }
}
