namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using AnimationClips;
    using Utils;

    public class SkeletonPose {
        public xform[] m_X;

        public SkeletonPose(ObjectReader reader) {
            var numXforms = reader.ReadInt32();
            m_X = new xform[numXforms];
            for (var i = 0; i < numXforms; i++) {
                m_X[i] = new xform(reader);
            }
        }
    }
}
