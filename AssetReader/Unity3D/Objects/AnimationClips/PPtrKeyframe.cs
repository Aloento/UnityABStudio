namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;

    public class PPtrKeyframe {
        public float time;
        public PPtr<UObject> value;

        public PPtrKeyframe(ObjectReader reader) {
            time = reader.ReadSingle();
            value = new PPtr<UObject>(reader);
        }
    }
}
