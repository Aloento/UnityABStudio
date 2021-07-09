namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class SelectorStateConstant {
        public SelectorTransitionConstant[] m_TransitionConstantArray;
        public uint m_FullPathID;
        public bool m_isEntry;

        public SelectorStateConstant(ObjectReader reader) {
            var numTransitions = reader.ReadInt32();
            m_TransitionConstantArray = new SelectorTransitionConstant[numTransitions];
            for (var i = 0; i < numTransitions; i++) {
                m_TransitionConstantArray[i] = new SelectorTransitionConstant(reader);
            }

            m_FullPathID = reader.ReadUInt32();
            m_isEntry = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
