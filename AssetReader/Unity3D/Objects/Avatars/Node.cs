namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using Utils;

    public class Node {
        public int m_ParentId;
        public int m_AxesId;

        public Node(ObjectReader reader) {
            m_ParentId = reader.ReadInt32();
            m_AxesId = reader.ReadInt32();
        }
    }
}
