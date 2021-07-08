namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;

    public class ConstantClip {
        public float[] data;

        public ConstantClip(ObjectReader reader) => data = reader.ReadSingleArray();
    }
}
