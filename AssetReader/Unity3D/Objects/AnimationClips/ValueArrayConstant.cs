namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;
    public class ValueArrayConstant {
        public ValueConstant[] m_ValueArray;

        public ValueArrayConstant(ObjectReader reader) {
            var numVals = reader.ReadInt32();
            m_ValueArray = new ValueConstant[numVals];
            for (var i = 0; i < numVals; i++) {
                m_ValueArray[i] = new ValueConstant(reader);
            }
        }
    }
}