namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedShaderDependency {
        public string from;
        public string to;

        public SerializedShaderDependency(UnityReader reader) {
            from = reader.ReadAlignedString();
            to = reader.ReadAlignedString();
        }
    }
}
