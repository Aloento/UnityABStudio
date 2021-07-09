namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Math;
    using Utils;

    public class Blend2dDataConstant {
        public Vector2[] m_ChildPositionArray;
        public float[] m_ChildMagnitudeArray;
        public Vector2[] m_ChildPairVectorArray;
        public float[] m_ChildPairAvgMagInvArray;
        public MotionNeighborList[] m_ChildNeighborListArray;

        public Blend2dDataConstant(ObjectReader reader) {
            m_ChildPositionArray = reader.ReadVector2Array();
            m_ChildMagnitudeArray = reader.ReadSingleArray();
            m_ChildPairVectorArray = reader.ReadVector2Array();
            m_ChildPairAvgMagInvArray = reader.ReadSingleArray();

            var numNeighbours = reader.ReadInt32();
            m_ChildNeighborListArray = new MotionNeighborList[numNeighbours];
            for (var i = 0; i < numNeighbours; i++) {
                m_ChildNeighborListArray[i] = new MotionNeighborList(reader);
            }
        }
    }
}
