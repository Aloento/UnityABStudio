namespace SoarCraft.QYun.AssetReader.Unity3D {
    using System.Collections.Specialized;
    using Entities.Enums;
    using Entities.Structs;
    using Helpers;
    using Utils;

    public class UObject {
        public SerializedFile assetsFile;
        public ObjectReader reader;
        public long m_PathID;
        public int[] version;
        protected BuildType buildType;
        public BuildTarget platform;
        public ClassIDType type;
        public SerializedType serializedType;
        public uint byteSize;

        public UObject(ObjectReader reader) {
            this.reader = reader;
            reader.Reset();
            assetsFile = reader.assetsFile;
            type = reader.type;
            m_PathID = reader.m_PathID;
            version = reader.version;
            buildType = reader.buildType;
            platform = reader.platform;
            serializedType = reader.serializedType;
            byteSize = reader.byteSize;

            if (platform == BuildTarget.NoTarget) {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }

        public string Dump() => serializedType?.m_Type != null ? TypeTreeHelper.ReadTypeString(serializedType.m_Type, reader) : null;

        public string Dump(TypeTree m_Type) => m_Type != null ? TypeTreeHelper.ReadTypeString(m_Type, reader) : null;

        public OrderedDictionary ToType() => serializedType?.m_Type != null ? TypeTreeHelper.ReadType(serializedType.m_Type, reader) : null;

        public OrderedDictionary ToType(TypeTree m_Type) => m_Type != null ? TypeTreeHelper.ReadType(m_Type, reader) : null;

        public byte[] GetRawData() {
            reader.Reset();
            return reader.ReadBytes((int)byteSize);
        }
    }
}
