namespace SoarCraft.QYun.AssetReader {
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using Utils;

    public class WebFile {
        public static byte[] GzipMagic = { 0x1f, 0x8b };
        public static byte[] BrotliMagic = { 0x62, 0x72, 0x6F, 0x74, 0x6C, 0x69 };
        public StreamFile[] FileList;

        private class WebData
        {
            public int DataOffset;
            public int DataLength;
            public string Path;
        }

        public WebFile(EndianBinaryReader reader)
        {
            var magic = reader.ReadBytes(2);
            reader.Position = 0;
            if (GzipMagic.SequenceEqual(magic))
            {
                var stream = new MemoryStream();
                using (var gs = new GZipStream(reader.BaseStream, CompressionMode.Decompress))
                {
                    gs.CopyTo(stream);
                }
                stream.Position = 0;
                using (var binaryReader = new BinaryReader(stream))
                {
                    ReadWebData(binaryReader);
                }
            }
            else
            {
                reader.Position = 0x20;
                magic = reader.ReadBytes(6);
                reader.Position = 0;
                if (BrotliMagic.SequenceEqual(magic))
                {
                    var brotliStream = new BrotliInputStream(reader.BaseStream);
                    var stream = new MemoryStream();
                    brotliStream.CopyTo(stream);
                    stream.Position = 0;
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        ReadWebData(binaryReader);
                    }
                }
                else
                {
                    reader.endian = EndianType.LittleEndian;
                    ReadWebData(reader);
                }
            }
        }

        private void ReadWebData(BinaryReader reader)
        {
            var signature = reader.ReadStringToNull();
            if (signature != "UnityWebData1.0")
                return;
            var headLength = reader.ReadInt32();
            var dataList = new List<WebData>();
            while (reader.BaseStream.Position < headLength)
            {
                var data = new WebData();
                data.DataOffset = reader.ReadInt32();
                data.DataLength = reader.ReadInt32();
                var pathLength = reader.ReadInt32();
                data.Path = Encoding.UTF8.GetString(reader.ReadBytes(pathLength));
                dataList.Add(data);
            }
            this.FileList = new StreamFile[dataList.Count];
            for (var i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                var file = new StreamFile();
                file.Path = data.path;
                file.FileName = Path.GetFileName(data.path);
                reader.BaseStream.Position = data.dataOffset;
                file.Stream = new MemoryStream(reader.ReadBytes(data.dataLength));
                this.FileList[i] = file;
            }
        }
    }
}
