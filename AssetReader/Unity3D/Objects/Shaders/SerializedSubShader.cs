namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedSubShader {
        public SerializedPass[] m_Passes;
        public SerializedTagMap m_Tags;
        public int m_LOD;

        public SerializedSubShader(ObjectReader reader) {
            var numPasses = reader.ReadInt32();
            m_Passes = new SerializedPass[numPasses];
            for (var i = 0; i < numPasses; i++) {
                m_Passes[i] = new SerializedPass(reader);
            }

            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
        }
    }
}
