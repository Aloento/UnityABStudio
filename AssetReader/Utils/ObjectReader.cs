namespace SoarCraft.QYun.AssetReader.Unity3D {
    using System;
    using Entities.Enums;
    using Utils;

    public sealed class ObjectReader : UnityReader {
        public SerializedFile AssetsFile;
        public long MPathId;
        public long ByteStart;
        public uint ByteSize;
        public ClassIdType Type;
        public SerializedType SerializedType;
        public BuildTarget Platform;
        public SerializedFileFormatVersion MVersion;

        public int[] Version => this.AssetsFile.Version;
        public BuildType BuildType => this.AssetsFile.BuildType;

        public ObjectReader(EndianBinaryReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(reader.BaseStream, reader.endian) {
            this.AssetsFile = assetsFile;
            this.MPathId = objectInfo.m_PathID;
            this.ByteStart = objectInfo.byteStart;
            this.ByteSize = objectInfo.byteSize;
            if (Enum.IsDefined(typeof(ClassIdType), objectInfo.classID)) {
                this.Type = (ClassIdType)objectInfo.classID;
            } else {
                this.Type = ClassIdType.UnknownType;
            }
            this.SerializedType = objectInfo.serializedType;
            this.Platform = assetsFile.MTargetPlatform;
            this.MVersion = assetsFile.Header.m_Version;
        }

        public void Reset() => Position = this.ByteStart;
    }
}
