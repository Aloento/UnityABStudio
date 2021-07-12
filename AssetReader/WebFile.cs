namespace SoarCraft.QYun.AssetReader {
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Brotli;
    using Entities.Structs;
    using Utils;

    public class WebFile {
        public static byte[] gzipMagic = { 0x1f, 0x8b };
        public static byte[] brotliMagic = { 0x62, 0x72, 0x6F, 0x74, 0x6C, 0x69 };
        public StreamFile[] fileList;

        private class WebData {
            public int dataOffset;
            public int dataLength;
            public string path;
        }

        public WebFile(UnityReader reader) {
            var magic = reader.ReadBytes(2);
            reader.Position = 0;
            if (gzipMagic.SequenceEqual(magic)) {
                var stream = new MemoryStream();
                using (var gs = new GZipStream(reader.BaseStream, CompressionMode.Decompress)) {
                    gs.CopyTo(stream);
                }
                stream.Position = 0;
                using var binaryReader = new UnityReader(stream);
                ReadWebData(binaryReader);
            } else {
                reader.Position = 0x20;
                magic = reader.ReadBytes(6);
                reader.Position = 0;
                if (brotliMagic.SequenceEqual(magic)) {
                    var brotliStream = new BrotliInputStream(reader.BaseStream);
                    var stream = new MemoryStream();
                    brotliStream.CopyTo(stream);
                    stream.Position = 0;
                    using var binaryReader = new UnityReader(stream);
                    ReadWebData(binaryReader);
                } else {
                    reader.IsBigEndian = false;
                    ReadWebData(reader);
                }
            }
        }

        private void ReadWebData(UnityReader reader) {
            var signature = reader.ReadStringToNull();
            if (signature != "UnityWebData1.0")
                return;
            var headLength = reader.ReadInt32();
            var dataList = new List<WebData>();
            while (reader.BaseStream.Position < headLength) {
                var data = new WebData {
                    dataOffset = reader.ReadInt32(),
                    dataLength = reader.ReadInt32()
                };
                var pathLength = reader.ReadInt32();
                data.path = Encoding.UTF8.GetString(reader.ReadBytes(pathLength));
                dataList.Add(data);
            }
            fileList = new StreamFile[dataList.Count];
            for (var i = 0; i < dataList.Count; i++) {
                var data = dataList[i];
                var file = new StreamFile {
                    path = data.path,
                    fileName = Path.GetFileName(data.path)
                };
                reader.BaseStream.Position = data.dataOffset;
                file.stream = new MemoryStream(reader.ReadBytes(data.dataLength));
                fileList[i] = file;
            }
        }
    }
}
