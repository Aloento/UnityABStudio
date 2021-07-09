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
                    if (WebFile.gzipMagic.SequenceEqual(magic)) {
                        return FileType.WebFile;
                    }
                    reader.Position = 0x20;
                    magic = reader.ReadBytes(6);
                    reader.Position = 0;
                    if (WebFile.brotliMagic.SequenceEqual(magic)) {
                        return FileType.WebFile;
                    }
                    return IsSerializedFile(reader) ? FileType.AssetsFile : FileType.ResourceFile;
                }
            }
        }

        private static bool IsSerializedFile(UnityReader reader) {
            var fileSize = reader.BaseStream.Length;
            if (fileSize < 20) {
                return false;
            }
            var m_MetadataSize = reader.ReadUInt32();
            long m_FileSize = reader.ReadUInt32();
            var m_Version = reader.ReadUInt32();
            long m_DataOffset = reader.ReadUInt32();
            var m_Endianess = reader.ReadByte();
            var m_Reserved = reader.ReadBytes(3);
            if (m_Version >= 22) {
                if (fileSize < 48) {
                    reader.Position = 0;
                    return false;
                }
                m_MetadataSize = reader.ReadUInt32();
                m_FileSize = reader.ReadInt64();
                m_DataOffset = reader.ReadInt64();
            }
            reader.Position = 0;
            if (m_FileSize != fileSize) {
                return false;
            }
            return m_DataOffset <= fileSize;
        }
    }
}
