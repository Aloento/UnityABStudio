namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Meshes {
    using Math;
    using Utils;

    public class MinMaxAABB {
        public Vector3 m_Min;
        public Vector3 m_Max;

        public MinMaxAABB(UnityReader reader) {
            m_Min = reader.ReadVector3();
            m_Max = reader.ReadVector3();
        }
    }
}
