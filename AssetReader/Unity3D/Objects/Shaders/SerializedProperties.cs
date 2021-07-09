namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedProperties {
        public SerializedProperty[] m_Props;

        public SerializedProperties(UnityReader reader) {
            var numProps = reader.ReadInt32();
            m_Props = new SerializedProperty[numProps];
            for (var i = 0; i < numProps; i++) {
                m_Props[i] = new SerializedProperty(reader);
            }
        }
    }
}
