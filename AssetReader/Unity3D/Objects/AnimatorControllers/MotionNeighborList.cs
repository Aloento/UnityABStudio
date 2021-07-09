namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class MotionNeighborList {
        public uint[] m_NeighborArray;

        public MotionNeighborList(ObjectReader reader) => m_NeighborArray = reader.ReadUInt32Array();
    }
}
