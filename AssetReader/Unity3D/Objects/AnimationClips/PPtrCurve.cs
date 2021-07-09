namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;
    public class PPtrCurve {
        public PPtrKeyframe[] curve;
        public string attribute;
        public string path;
        public int classID;
        public PPtr<MonoScript> script;

        public PPtrCurve(ObjectReader reader) {
            var numCurves = reader.ReadInt32();
            curve = new PPtrKeyframe[numCurves];
            for (var i = 0; i < numCurves; i++) {
                curve[i] = new PPtrKeyframe(reader);
            }

            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
        }
    }
}
