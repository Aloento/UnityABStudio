namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using AnimationClips;
    using Utils;

    public class Collider {
        public xform m_X;
        public uint m_Type;
        public uint m_XMotionType;
        public uint m_YMotionType;
        public uint m_ZMotionType;
        public float m_MinLimitX;
        public float m_MaxLimitX;
        public float m_MaxLimitY;
        public float m_MaxLimitZ;

        public Collider(ObjectReader reader) {
            m_X = new xform(reader);
            m_Type = reader.ReadUInt32();
            m_XMotionType = reader.ReadUInt32();
            m_YMotionType = reader.ReadUInt32();
            m_ZMotionType = reader.ReadUInt32();
            m_MinLimitX = reader.ReadSingle();
            m_MaxLimitX = reader.ReadSingle();
            m_MaxLimitY = reader.ReadSingle();
            m_MaxLimitZ = reader.ReadSingle();
        }
    }
}
