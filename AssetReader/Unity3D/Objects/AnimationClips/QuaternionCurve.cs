namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Math;
    using Utils;

    public class QuaternionCurve {
        public AnimationCurve<Quaternion> curve;
        public string path;

        public QuaternionCurve(ObjectReader reader) {
            curve = new AnimationCurve<Quaternion>(reader, reader.ReadQuaternion);
            path = reader.ReadAlignedString();
        }
    }
}
