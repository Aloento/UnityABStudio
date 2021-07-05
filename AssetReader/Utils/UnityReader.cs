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
            this.FileType = this.CheckFileType();
        }

        public UnityReader(UnityReader reader) : base(reader.BaseStream, reader.IsBigEndian) {
            this.FullPath = reader.FullPath;
            this.FileName = reader.FileName;
            this.FileType = reader.FileType;
        }

        public void AlignStream() => this.AlignStream(4);

        public void AlignStream(int alignment) {
            var pos = this.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0) {
                this.BaseStream.Position += alignment - mod;
            }
        }

        public string ReadAlignedString() {
            var length = this.ReadInt32();
            if (length <= 0 || length > this.BaseStream.Length - this.BaseStream.Position) {
                return "";
            }

            var stringData = this.ReadBytes(length);
            var result = Encoding.UTF8.GetString(stringData);
            this.AlignStream(4);
            return result;
        }

        public string ReadStringToNull(int maxLength = 32767) {
            var bytes = new List<byte>();
            var count = 0;
            while (this.BaseStream.Position != this.BaseStream.Length && count < maxLength) {
                var b = this.ReadByte();
                if (b == 0) {
                    break;
                }
                bytes.Add(b);
                count++;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public Quaternion ReadQuaternion() => new(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());

        public Vector2 ReadVector2() => new(this.ReadSingle(), this.ReadSingle());

        public Vector3 ReadVector3() => new(this.ReadSingle(), this.ReadSingle(), this.ReadSingle());

        public Vector4 ReadVector4() => new(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());

        public Color ReadColor4() => new(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());

        public Matrix4X4 ReadMatrix() => new(this.ReadSingleArray(16));

        private static T[] ReadArray<T>(Func<T> del, int length) {
            var array = new T[length];
            for (var i = 0; i < length; i++) {
                array[i] = del();
            }
            return array;
        }

        public bool[] ReadBooleanArray() => ReadArray(this.ReadBoolean, this.ReadInt32());

        public byte[] ReadUInt8Array() => this.ReadBytes(this.ReadInt32());

        public ushort[] ReadUInt16Array() => ReadArray(this.ReadUInt16, this.ReadInt32());

        public int[] ReadInt32Array() => ReadArray(this.ReadInt32, this.ReadInt32());

        public int[] ReadInt32Array(int length) => ReadArray(this.ReadInt32, length);

        public uint[] ReadUInt32Array() => ReadArray(this.ReadUInt32, this.ReadInt32());

        public uint[][] ReadUInt32ArrayArray() => ReadArray(this.ReadUInt32Array, this.ReadInt32());

        public uint[] ReadUInt32Array(int length) => ReadArray(this.ReadUInt32, length);

        public float[] ReadSingleArray() => ReadArray(this.ReadSingle, this.ReadInt32());

        public float[] ReadSingleArray(int length) => ReadArray(this.ReadSingle, length);

        public string[] ReadStringArray() => ReadArray(this.ReadAlignedString, this.ReadInt32());

        public Vector2[] ReadVector2Array() => ReadArray(this.ReadVector2, this.ReadInt32());

        public Vector4[] ReadVector4Array() => ReadArray(this.ReadVector4, this.ReadInt32());

        public Matrix4X4[] ReadMatrixArray() => ReadArray(this.ReadMatrix, this.ReadInt32());

        private FileType CheckFileType() {
            var signature = this.ReadStringToNull(20);
            Position = 0;
            switch (signature) {
                case "UnityWeb":
                case "UnityRaw":
                case "UnityArchive":
                case "UnityFS":
                    return this.FileType.BundleFile;
                case "UnityWebData1.0":
                    return this.FileType.WebFile;
                default: {
                    var magic = ReadBytes(2);
                    Position = 0;
                    if (WebFile.GzipMagic.SequenceEqual(magic)) {
                        return this.FileType.WebFile;
                    }
                    Position = 0x20;
                    magic = ReadBytes(6);
                    Position = 0;
                    if (WebFile.BrotliMagic.SequenceEqual(magic)) {
                        return this.FileType.WebFile;
                    }
                    return this.IsSerializedFile() ? this.FileType.AssetsFile : this.FileType.ResourceFile;
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
