namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class SkeletonMask {
        public SkeletonMaskElement[] m_Data;

        public SkeletonMask(ObjectReader reader) {
            var numElements = reader.ReadInt32();
            m_Data = new SkeletonMaskElement[numElements];
            for (var i = 0; i < numElements; i++) {
                m_Data[i] = new SkeletonMaskElement(reader);
            }
        }
    }
}
