namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;

    public class AnimationEvent {
        public float time;
        public string functionName;
        public string data;
        public PPtr<UObject> objectReferenceParameter;
        public float floatParameter;
        public int intParameter;
        public int messageOptions;

        public AnimationEvent(ObjectReader reader) {
            var version = reader.version;

            time = reader.ReadSingle();
            functionName = reader.ReadAlignedString();
            data = reader.ReadAlignedString();
            objectReferenceParameter = new PPtr<UObject>(reader);
            floatParameter = reader.ReadSingle();
            if (version[0] >= 3) //3 and up
            {
                intParameter = reader.ReadInt32();
            }
            messageOptions = reader.ReadInt32();
        }
    }
}