namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class MatrixParameter {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_RowCount;

        public MatrixParameter(UnityReader reader) {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            m_Type = reader.ReadSByte();
            m_RowCount = reader.ReadSByte();
            reader.AlignStream();
        }
    }
}
