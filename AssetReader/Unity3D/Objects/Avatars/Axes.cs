namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using Math;
    using Utils;

    public class Axes {
        public Vector4 m_PreQ;
        public Vector4 m_PostQ;
        public object m_Sgn;
        public Limit m_Limit;
        public float m_Length;
        public uint m_Type;

        public Axes(ObjectReader reader) {
            var version = reader.version;
            m_PreQ = reader.ReadVector4();
            m_PostQ = reader.ReadVector4();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) { //5.4 and up
                m_Sgn = reader.ReadVector3();
            } else {
                m_Sgn = reader.ReadVector4();
            }
            m_Limit = new Limit(reader);
            m_Length = reader.ReadSingle();
            m_Type = reader.ReadUInt32();
        }
    }
}
