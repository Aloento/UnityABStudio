namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    public class SerializedProperty {
        public string m_Name;
        public string m_Description;
        public string[] m_Attributes;
        public SerializedPropertyType m_Type;
        public uint m_Flags;
        public float[] m_DefValue;
        public SerializedTextureProperty m_DefTexture;

        public SerializedProperty(UnityReader reader) {
            m_Name = reader.ReadAlignedString();
            m_Description = reader.ReadAlignedString();
            m_Attributes = reader.ReadStringArray();
            m_Type = (SerializedPropertyType)reader.ReadInt32();
            m_Flags = reader.ReadUInt32();
            m_DefValue = reader.ReadSingleArray(4);
            m_DefTexture = new SerializedTextureProperty(reader);
        }
    }
}