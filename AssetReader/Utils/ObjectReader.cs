namespace SoarCraft.QYun.AssetReader.Utils {
    using System;
    using Entities.Enums;
    using Entities.Structs;

    public sealed class ObjectReader : UnityReader {
        public SerializedFile assetsFile;
        public long m_PathID;
        public long byteStart;
        public uint byteSize;
        public ClassIDType type;
        public SerializedType serializedType;
        public BuildTarget platform;
        public SerializedFileFormatVersion m_Version;

        public int[] version => this.assetsFile.version;
        public BuildType buildType => this.assetsFile.buildType;

        public ObjectReader(UnityReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(reader) {
            this.assetsFile = assetsFile;
            this.m_PathID = objectInfo.m_PathID;
            this.byteStart = objectInfo.byteStart;
            this.byteSize = objectInfo.byteSize;
            if (Enum.IsDefined(typeof(ClassIDType), objectInfo.classID)) {
                this.type = (ClassIDType)objectInfo.classID;
            } else {
                this.type = ClassIDType.UnknownType;
            }
            this.serializedType = objectInfo.serializedType;
            this.platform = assetsFile.m_TargetPlatform;
            this.m_Version = assetsFile.header.m_Version;
        }

        public void Reset() => this.Position = this.byteStart;
    }
}
