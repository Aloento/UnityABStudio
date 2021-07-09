namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Texture2Ds {
    using Utils;

    public class StreamingInfo {
        public long offset;
        public uint size;
        public string path;

        public StreamingInfo(ObjectReader reader) {
            var version = reader.version;

            this.offset = version[0] >= 2020 ? reader.ReadInt64() : reader.ReadUInt32();
            size = reader.ReadUInt32();
            path = reader.ReadAlignedString();
        }
    }
}
