namespace SoarCraft.QYun.AssetReader.Utils {
    using System.IO;

    public class ResourceReader {
        private bool needSearch;
        private string path;
        private SerializedFile assetsFile;
        private long offset;
        private long size;
        private UnityReader reader;

        public ResourceReader(string path, SerializedFile assetsFile, long offset, long size) {
            this.needSearch = true;
            this.path = path;
            this.assetsFile = assetsFile;
            this.offset = offset;
            this.size = size;
        }

        public ResourceReader(UnityReader reader, long offset, long size) {
            this.reader = reader;
            this.offset = offset;
            this.size = size;
        }

        private UnityReader GetReader() {
            if (this.needSearch) {
                var resourceFileName = Path.GetFileName(this.path);
                if (this.assetsFile.assetsManager.resourceFileReaders.TryGetValue(resourceFileName, out this.reader)) {
                    this.needSearch = false;
                    return this.reader;
                }
                var assetsFileDirectory = Path.GetDirectoryName(this.assetsFile.fullName);
                var resourceFilePath = Path.Combine(assetsFileDirectory, resourceFileName);
                if (!File.Exists(resourceFilePath)) {
                    var findFiles = Directory.GetFiles(assetsFileDirectory, resourceFileName, SearchOption.AllDirectories);
                    if (findFiles.Length > 0) {
                        resourceFilePath = findFiles[0];
                    }
                }
                if (File.Exists(resourceFilePath)) {
                    this.needSearch = false;
                    this.reader = new UnityReader(File.OpenRead(resourceFilePath));
                    this.assetsFile.assetsManager.resourceFileReaders.Add(resourceFileName, this.reader);
                    return this.reader;
                }
                throw new FileNotFoundException($"Can't find the resource file {resourceFileName}");
            }

            return this.reader;
        }

        public byte[] GetData() {
            var binaryReader = this.GetReader();
            binaryReader.BaseStream.Position = this.offset;
            return binaryReader.ReadBytes((int)this.size);
        }

        public void WriteData(string path) {
            var binaryReader = this.GetReader();
            binaryReader.BaseStream.Position = this.offset;
            using var writer = File.OpenWrite(path);
            binaryReader.BaseStream.CopyTo(writer, this.size);
        }
    }
}
