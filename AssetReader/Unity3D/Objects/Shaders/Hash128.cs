namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;
    public class Hash128 {
        public byte[] bytes;

        public Hash128(UnityReader reader) => bytes = reader.ReadBytes(16);
    }
}
