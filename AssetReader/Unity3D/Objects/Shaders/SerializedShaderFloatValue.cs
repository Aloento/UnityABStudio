namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedShaderFloatValue {
        public float val;
        public string name;

        public SerializedShaderFloatValue(UnityReader reader) {
            val = reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }
}
