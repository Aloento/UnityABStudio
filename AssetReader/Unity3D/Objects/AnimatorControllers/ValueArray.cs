namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Math;
    using Utils;

    public class ValueArray {
        public bool[] m_BoolValues;
        public int[] m_IntValues;
        public float[] m_FloatValues;
        public Vector4[] m_VectorValues;
        public Vector3[] m_PositionValues;
        public Vector4[] m_QuaternionValues;
        public Vector3[] m_ScaleValues;

        public ValueArray(ObjectReader reader) {
            var version = reader.version;

            if (version[0] < 5 || (version[0] == 5 && version[1] < 5)) { //5.5 down
                m_BoolValues = reader.ReadBooleanArray();
                reader.AlignStream();
                m_IntValues = reader.ReadInt32Array();
                m_FloatValues = reader.ReadSingleArray();
            }

            if (version[0] < 4 || (version[0] == 4 && version[1] < 3)) { //4.3 down
                m_VectorValues = reader.ReadVector4Array();
            } else {
                int numPosValues = reader.ReadInt32();
                m_PositionValues = new Vector3[numPosValues];
                for (int i = 0; i < numPosValues; i++) {
                    m_PositionValues[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : reader.ReadVector4(); //5.4 and up
                }

                m_QuaternionValues = reader.ReadVector4Array();

                int numScaleValues = reader.ReadInt32();
                m_ScaleValues = new Vector3[numScaleValues];
                for (int i = 0; i < numScaleValues; i++) {
                    m_ScaleValues[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : reader.ReadVector4(); //5.4 and up
                }

                if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) { //5.5 and up
                    m_FloatValues = reader.ReadSingleArray();
                    m_IntValues = reader.ReadInt32Array();
                    m_BoolValues = reader.ReadBooleanArray();
                    reader.AlignStream();
                }
            }
        }
    }
}
