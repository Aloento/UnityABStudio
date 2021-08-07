namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class ShaderBindChannel {
        public sbyte source;
        public sbyte target;

        public ShaderBindChannel(UnityReader reader) {
            source = reader.ReadSByte();
            target = reader.ReadSByte();
        }
    }
}
