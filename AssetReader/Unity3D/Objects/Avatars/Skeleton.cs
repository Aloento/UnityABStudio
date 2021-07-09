namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using Utils;

    public class Skeleton {
        public Node[] m_Node;
        public uint[] m_ID;
        public Axes[] m_AxesArray;

        public Skeleton(ObjectReader reader) {
            var numNodes = reader.ReadInt32();
            m_Node = new Node[numNodes];
            for (var i = 0; i < numNodes; i++) {
                m_Node[i] = new Node(reader);
            }

            m_ID = reader.ReadUInt32Array();

            var numAxes = reader.ReadInt32();
            m_AxesArray = new Axes[numAxes];
            for (var i = 0; i < numAxes; i++) {
                m_AxesArray[i] = new Axes(reader);
            }
        }
    }
}
