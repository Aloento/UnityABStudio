namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using AnimationClips;
    using Utils;

    public class Handle {
        public xform m_X;
        public uint m_ParentHumanIndex;
        public uint m_ID;

        public Handle(ObjectReader reader) {
            m_X = new xform(reader);
            m_ParentHumanIndex = reader.ReadUInt32();
            m_ID = reader.ReadUInt32();
        }
    }
}
