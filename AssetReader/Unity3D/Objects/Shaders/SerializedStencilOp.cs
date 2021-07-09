namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedStencilOp {
        public SerializedShaderFloatValue pass;
        public SerializedShaderFloatValue fail;
        public SerializedShaderFloatValue zFail;
        public SerializedShaderFloatValue comp;

        public SerializedStencilOp(UnityReader reader) {
            pass = new SerializedShaderFloatValue(reader);
            fail = new SerializedShaderFloatValue(reader);
            zFail = new SerializedShaderFloatValue(reader);
            comp = new SerializedShaderFloatValue(reader);
        }
    }
}
