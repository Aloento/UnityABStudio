namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using System.Collections.Generic;
    using Utils;

    public class ResourceManager : UObject {
        public KeyValuePair<string, PPtr<UObject>>[] m_Container;

        public ResourceManager(ObjectReader reader) : base(reader) {
            var m_ContainerSize = reader.ReadInt32();
            m_Container = new KeyValuePair<string, PPtr<UObject>>[m_ContainerSize];
            for (var i = 0; i < m_ContainerSize; i++) {
                m_Container[i] = new KeyValuePair<string, PPtr<UObject>>(reader.ReadAlignedString(), new PPtr<UObject>(reader));
            }
        }
    }
}
