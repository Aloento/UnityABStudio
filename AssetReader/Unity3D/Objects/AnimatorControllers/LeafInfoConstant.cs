namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class LeafInfoConstant {
        public uint[] m_IDArray;
        public uint m_IndexOffset;

        public LeafInfoConstant(ObjectReader reader) {
            m_IDArray = reader.ReadUInt32Array();
            m_IndexOffset = reader.ReadUInt32();
        }
    }
}
