namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class SkeletonMaskElement {
        public uint m_PathHash;
        public float m_Weight;

        public SkeletonMaskElement(ObjectReader reader) {
            m_PathHash = reader.ReadUInt32();
            m_Weight = reader.ReadSingle();
        }
    }
}
