namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;
    using Math;

    public class Vector3Curve {
        public AnimationCurve<Vector3> curve;
        public string path;

        public Vector3Curve(ObjectReader reader) {
            curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3);
            path = reader.ReadAlignedString();
        }
    }
}
