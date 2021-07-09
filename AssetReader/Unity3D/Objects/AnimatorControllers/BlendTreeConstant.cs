namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using AnimationClips;
    using Utils;

    public class BlendTreeConstant {
        public BlendTreeNodeConstant[] m_NodeArray;
        public ValueArrayConstant m_BlendEventArrayConstant;

        public BlendTreeConstant(ObjectReader reader) {
            var version = reader.version;

            var numNodes = reader.ReadInt32();
            m_NodeArray = new BlendTreeNodeConstant[numNodes];
            for (var i = 0; i < numNodes; i++) {
                m_NodeArray[i] = new BlendTreeNodeConstant(reader);
            }

            if (version[0] < 4 || (version[0] == 4 && version[1] < 5)) { //4.5 down
                m_BlendEventArrayConstant = new ValueArrayConstant(reader);
            }
        }
    }
}
