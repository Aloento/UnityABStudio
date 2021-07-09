namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class StructParameter {
        public MatrixParameter[] m_MatrixParams;
        public VectorParameter[] m_VectorParams;

        public StructParameter(UnityReader reader) {
            var m_NameIndex = reader.ReadInt32();
            var m_Index = reader.ReadInt32();
            var m_ArraySize = reader.ReadInt32();
            var m_StructSize = reader.ReadInt32();

            var numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (var i = 0; i < numVectorParams; i++) {
                m_VectorParams[i] = new VectorParameter(reader);
            }

            var numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (var i = 0; i < numMatrixParams; i++) {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }
        }
    }
}
