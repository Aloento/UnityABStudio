namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.VideoClips {
    using Utils;

    public class StreamedResource {
        public string m_Source;
        public ulong m_Offset;
        public ulong m_Size;

        public StreamedResource(UnityReader reader) {
            m_Source = reader.ReadAlignedString();
            m_Offset = reader.ReadUInt64();
            m_Size = reader.ReadUInt64();
        }
    }
}