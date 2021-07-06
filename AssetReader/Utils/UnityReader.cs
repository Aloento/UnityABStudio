namespace SoarCraft.QYun.AssetReader.Utils {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Entities.Enums;
    using Math;

    public class UnityReader : EndianBinaryReader {
        public string FullPath;
        public string FileName;
        public FileType FileType;

        public UnityReader(string path) : this(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public UnityReader(string path, Stream stream) : base(stream) {
            this.FullPath = Path.GetFullPath(path);
            this.FileName = Path.GetFileName(path);
            this.FileType = CheckFileType();
        }

        public UnityReader(UnityReader reader) : base(reader.BaseStream, reader.IsBigEndian) {
            this.FullPath = reader.FullPath;
            this.FileName = reader.FileName;
            this.FileType = reader.FileType;
        }

        public UnityReader(Stream stream, bool isBigEndian = false) : base(stream, isBigEndian) {
        }

        public void AlignStream() => AlignStream(4);

        public void AlignStream(int alignment) {
            var pos = this.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0) {
                this.BaseStream.Position += alignment - mod;
            }
        }

        public string ReadAlignedString() {
            var length = ReadInt32();
            if (length <= 0 || length > this.BaseStream.Length - this.BaseStream.Position) {
                return "";
            }

            var stringData = ReadBytes(length);
            var result = Encoding.UTF8.GetString(stringData);
            AlignStream(4);
            return result;
        }

        public string ReadStringToNull(int maxLength = 32767) {
            var bytes = new List<byte>();
            var count = 0;
            while (this.BaseStream.Position != this.BaseStream.Length && count < maxLength) {
                var b = ReadByte();
                if (b == 0) {
                    break;
                }
                bytes.Add(b);
                count++;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public Quaternion ReadQuaternion() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

        public Vector2 ReadVector2() => new(ReadSingle(), ReadSingle());

        public Vector3 ReadVector3() => new(ReadSingle(), ReadSingle(), ReadSingle());

        public Vector4 ReadVector4() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

        public Color ReadColor4() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

        public Matrix4X4 ReadMatrix() => new(ReadSingleArray(16));

        private static T[] ReadArray<T>(Func<T> del, int length) {
            var array = new T[length];
            for (var i = 0; i < length; i++) {
                array[i] = del();
            }
            return array;
        }

        public bool[] ReadBooleanArray() => ReadArray(this.ReadBoolean, ReadInt32());

        public byte[] ReadUInt8Array() => ReadBytes(ReadInt32());

        public ushort[] ReadUInt16Array() => ReadArray(this.ReadUInt16, ReadInt32());

        public int[] ReadInt32Array() => ReadArray(this.ReadInt32, ReadInt32());

        public int[] ReadInt32Array(int length) => ReadArray(this.ReadInt32, length);

        public uint[] ReadUInt32Array() => ReadArray(this.ReadUInt32, ReadInt32());

        public uint[][] ReadUInt32ArrayArray() => ReadArray(this.ReadUInt32Array, ReadInt32());

        public uint[] ReadUInt32Array(int length) => ReadArray(this.ReadUInt32, length);

        public float[] ReadSingleArray() => ReadArray(this.ReadSingle, ReadInt32());

        public float[] ReadSingleArray(int length) => ReadArray(this.ReadSingle, length);

        public string[] ReadStringArray() => ReadArray(this.ReadAlignedString, ReadInt32());

        public Vector2[] ReadVector2Array() => ReadArray(this.ReadVector2, ReadInt32());

        public Vector4[] ReadVector4Array() => ReadArray(this.ReadVector4, ReadInt32());

        public Matrix4X4[] ReadMatrixArray() => ReadArray(this.ReadMatrix, ReadInt32());

        private FileType CheckFileType() {
            var signature = ReadStringToNull(20);
            Position = 0;
            switch (signature) {
                case "UnityWeb":
                case "UnityRaw":
                case "UnityArchive":
                case "UnityFS":
                    return FileType.BundleFile;
                case "UnityWebData1.0":
                    return FileType.WebFile;
                default: {
                    var magic = ReadBytes(2);
                    Position = 0;
                    if (WebFile.gzipMagic.SequenceEqual(magic)) {
                        return FileType.WebFile;
                    }
                    Position = 0x20;
                    magic = ReadBytes(6);
                    Position = 0;
                    if (WebFile.brotliMagic.SequenceEqual(magic)) {
                        return FileType.WebFile;
                    }
                    return IsSerializedFile() ? FileType.AssetsFile : FileType.ResourceFile;
                }
            }
        }

        private bool IsSerializedFile() {
            var fileSize = BaseStream.Length;
            if (fileSize < 20) {
                return false;
            }
            var m_MetadataSize = ReadUInt32();
            long m_FileSize = ReadUInt32();
            var m_Version = ReadUInt32();
            long m_DataOffset = ReadUInt32();
            var m_Endianess = ReadByte();
            var m_Reserved = ReadBytes(3);
            if (m_Version >= 22) {
                if (fileSize < 48) {
                    Position = 0;
                    return false;
                }
                m_MetadataSize = ReadUInt32();
                m_FileSize = ReadInt64();
                m_DataOffset = ReadInt64();
            }
            Position = 0;
            if (m_FileSize != fileSize) {
                return false;
            }
            return m_DataOffset <= fileSize;
        }
    }
}
