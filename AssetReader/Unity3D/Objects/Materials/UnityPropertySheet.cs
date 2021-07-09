namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Materials {
    using System.Collections.Generic;
    using Math;
    using Utils;

    public class UnityPropertySheet {
        public KeyValuePair<string, UnityTexEnv>[] m_TexEnvs;
        public KeyValuePair<string, int>[] m_Ints;
        public KeyValuePair<string, float>[] m_Floats;
        public KeyValuePair<string, Color>[] m_Colors;

        public UnityPropertySheet(ObjectReader reader) {
            var version = reader.version;

            var m_TexEnvsSize = reader.ReadInt32();
            m_TexEnvs = new KeyValuePair<string, UnityTexEnv>[m_TexEnvsSize];
            for (var i = 0; i < m_TexEnvsSize; i++) {
                m_TexEnvs[i] = new KeyValuePair<string, UnityTexEnv>(reader.ReadAlignedString(), new UnityTexEnv(reader));
            }

            if (version[0] >= 2021) { //2021.1 and up
                var m_IntsSize = reader.ReadInt32();
                m_Ints = new KeyValuePair<string, int>[m_IntsSize];
                for (var i = 0; i < m_IntsSize; i++) {
                    m_Ints[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
                }
            }

            var m_FloatsSize = reader.ReadInt32();
            m_Floats = new KeyValuePair<string, float>[m_FloatsSize];
            for (var i = 0; i < m_FloatsSize; i++) {
                m_Floats[i] = new KeyValuePair<string, float>(reader.ReadAlignedString(), reader.ReadSingle());
            }

            var m_ColorsSize = reader.ReadInt32();
            m_Colors = new KeyValuePair<string, Color>[m_ColorsSize];
            for (var i = 0; i < m_ColorsSize; i++) {
                m_Colors[i] = new KeyValuePair<string, Color>(reader.ReadAlignedString(), reader.ReadColor4());
            }
        }
    }
}
