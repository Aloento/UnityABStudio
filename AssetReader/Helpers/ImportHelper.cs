namespace SoarCraft.QYun.AssetReader.Helpers {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Entities.Enums;
    using Utils;

    public static class ImportHelper {
        public static void MergeSplitAssets(string path, bool subDirectories = false) {
            var splitFiles = Directory.GetFiles(path, "*.split0", subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var splitFile in splitFiles) {
                var destFile = Path.GetFileNameWithoutExtension(splitFile);
                var destPath = Path.GetDirectoryName(splitFile);
                var destFull = Path.Combine(destPath, destFile);
                if (File.Exists(destFull)) {
                    continue;
                }

                var splitParts = Directory.GetFiles(destPath, destFile + ".split*");
                using var destStream = File.Create(destFull);
                for (var i = 0; i < splitParts.Length; i++) {
                    var splitPart = destFull + ".split" + i;
                    using var sourceStream = File.OpenRead(splitPart);
                    sourceStream.CopyTo(destStream);
                }
            }
        }

        public static string[] ProcessingSplitFiles(List<string> selectFile) {
            var splitFiles = selectFile.Where(x => x.Contains(".split"))
                .Select(x => Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x)))
                .Distinct()
                .ToList();
            _ = selectFile.RemoveAll(x => x.Contains(".split"));
            selectFile.AddRange(splitFiles.Where(File.Exists));
            return selectFile.Distinct().ToArray();
        }

        public static FileType CheckFileType(Stream stream, out UnityReader reader) {
            reader = new UnityReader(stream);
            return CheckFileType(reader);
        }

        public static FileType CheckFileType(string fileName, out UnityReader reader) {
            reader = new UnityReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            return CheckFileType(reader);
        }

        private static FileType CheckFileType(UnityReader reader) {
            var signature = reader.ReadStringToNull(20);
            reader.Position = 0;
            switch (signature) {
                case "UnityWeb":
                case "UnityRaw":
                case "UnityArchive":
                case "UnityFS":
                    return FileType.BundleFile;
                case "UnityWebData1.0":
                    return FileType.WebFile;
                default: {
                    var magic = reader.ReadBytes(2);
                    reader.Position = 0;
                    if (WebFile.GzipMagic.SequenceEqual(magic)) {
                        return FileType.WebFile;
                    }
                    reader.Position = 0x20;
                    magic = reader.ReadBytes(6);
                    reader.Position = 0;
                    if (WebFile.BrotliMagic.SequenceEqual(magic)) {
                        return FileType.WebFile;
                    }
                    return SerializedFile.IsSerializedFile(reader) ? FileType.AssetsFile : FileType.ResourceFile;
                }
            }
        }
    }
}
