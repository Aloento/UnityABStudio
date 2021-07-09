namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.VideoClips {
    using Utils;

    public class StreamedResource {
        public string m_Source;
        public long m_Offset;
        public long m_Size;

        public StreamedResource(UnityReader reader) {
            m_Source = reader.ReadAlignedString();
            m_Offset = reader.ReadInt64();
            m_Size = reader.ReadInt64();
        }
    }
}
