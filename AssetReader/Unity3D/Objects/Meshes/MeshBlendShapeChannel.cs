namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Meshes {
    using Utils;

    public class MeshBlendShapeChannel {
        public string name;
        public uint nameHash;
        public int frameIndex;
        public int frameCount;

        public MeshBlendShapeChannel(ObjectReader reader) {
            name = reader.ReadAlignedString();
            nameHash = reader.ReadUInt32();
            frameIndex = reader.ReadInt32();
            frameCount = reader.ReadInt32();
        }
    }
}
