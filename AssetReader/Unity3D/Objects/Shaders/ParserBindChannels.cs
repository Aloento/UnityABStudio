namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class ParserBindChannels {
        public ShaderBindChannel[] m_Channels;
        public uint m_SourceMap;

        public ParserBindChannels(UnityReader reader) {
            int numChannels = reader.ReadInt32();
            m_Channels = new ShaderBindChannel[numChannels];
            for (int i = 0; i < numChannels; i++) {
                m_Channels[i] = new ShaderBindChannel(reader);
            }
            reader.AlignStream();

            m_SourceMap = reader.ReadUInt32();
        }
    }
}
