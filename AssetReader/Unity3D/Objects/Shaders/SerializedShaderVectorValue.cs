namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedShaderVectorValue {
        public SerializedShaderFloatValue x;
        public SerializedShaderFloatValue y;
        public SerializedShaderFloatValue z;
        public SerializedShaderFloatValue w;
        public string name;

        public SerializedShaderVectorValue(UnityReader reader) {
            x = new SerializedShaderFloatValue(reader);
            y = new SerializedShaderFloatValue(reader);
            z = new SerializedShaderFloatValue(reader);
            w = new SerializedShaderFloatValue(reader);
            name = reader.ReadAlignedString();
        }
    }
}
