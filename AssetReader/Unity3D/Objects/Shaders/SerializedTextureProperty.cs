namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    public class SerializedTextureProperty {
        public string m_DefaultName;
        public TextureDimension m_TexDim;

        public SerializedTextureProperty(UnityReader reader) {
            m_DefaultName = reader.ReadAlignedString();
            m_TexDim = (TextureDimension)reader.ReadInt32();
        }
    }
}