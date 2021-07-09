namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SamplerParameter {
        public uint sampler;
        public int bindPoint;

        public SamplerParameter(UnityReader reader) {
            sampler = reader.ReadUInt32();
            bindPoint = reader.ReadInt32();
        }
    }
}
