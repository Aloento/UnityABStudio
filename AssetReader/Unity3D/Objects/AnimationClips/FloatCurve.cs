namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Entities.Enums;
    using Utils;

    public class FloatCurve {
        public AnimationCurve<float> curve;
        public string attribute;
        public string path;
        public ClassIDType classID;
        public PPtr<MonoScript> script;

        public FloatCurve(ObjectReader reader) {
            curve = new AnimationCurve<float>(reader, reader.ReadSingle);
            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = (ClassIDType)reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
        }
    }
}
