namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AssetBundles {
    using System.Collections.Generic;
    using Contracts;
    using Utils;

    public sealed class AssetBundle : NamedObject {
        public PPtr<UObject>[] m_PreloadTable;
        public KeyValuePair<string, AssetInfo>[] m_Container;

        public AssetBundle(ObjectReader reader) : base(reader) {
            var m_PreloadTableSize = reader.ReadInt32();
            m_PreloadTable = new PPtr<UObject>[m_PreloadTableSize];
            for (int i = 0; i < m_PreloadTableSize; i++) {
                m_PreloadTable[i] = new PPtr<UObject>(reader);
            }

            var m_ContainerSize = reader.ReadInt32();
            m_Container = new KeyValuePair<string, AssetInfo>[m_ContainerSize];
            for (int i = 0; i < m_ContainerSize; i++) {
                m_Container[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
            }
        }
    }
}
