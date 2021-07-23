namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System.IO;
    using System.Linq;
    using Core.Models;

    public static partial class ExportExtension {
        public static string FixFileName(string str) => str.Length >= 260 ? Path.GetRandomFileName() : Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));

        private static bool TryExportFile(string dir, AssetItem item, string extension, out string fullPath) {
            var fileName = FixFileName(item.Name);
            fullPath = Path.Combine(dir, fileName + extension);
            if (!File.Exists(fullPath)) {
                _ = Directory.CreateDirectory(dir);
                return true;
            }
            fullPath = Path.Combine(dir, $"{fileName}#{item.BaseID}{extension}");
            if (!File.Exists(fullPath)) {
                _ = Directory.CreateDirectory(dir);
                return true;
            }
            return false;
        }

    }
}
