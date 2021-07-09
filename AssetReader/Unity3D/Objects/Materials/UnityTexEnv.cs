namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Materials {
    using Contracts;
    using Math;
    using Utils;

    public class UnityTexEnv {
        public PPtr<Texture> m_Texture;
        public Vector2 m_Scale;
        public Vector2 m_Offset;

        public UnityTexEnv(ObjectReader reader) {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = reader.ReadVector2();
            m_Offset = reader.ReadVector2();
        }
    }
}
