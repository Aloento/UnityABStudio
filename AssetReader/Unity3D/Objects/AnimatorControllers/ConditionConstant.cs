namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class ConditionConstant {
        public uint m_ConditionMode;
        public uint m_EventID;
        public float m_EventThreshold;
        public float m_ExitTime;

        public ConditionConstant(ObjectReader reader) {
            m_ConditionMode = reader.ReadUInt32();
            m_EventID = reader.ReadUInt32();
            m_EventThreshold = reader.ReadSingle();
            m_ExitTime = reader.ReadSingle();
        }
    }
}
