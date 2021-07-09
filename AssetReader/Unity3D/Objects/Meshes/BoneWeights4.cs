namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Meshes {
    using Utils;

    public class BoneWeights4 {
        public float[] weight;
        public int[] boneIndex;

        public BoneWeights4() {
            weight = new float[4];
            boneIndex = new int[4];
        }

        public BoneWeights4(ObjectReader reader) {
            weight = reader.ReadSingleArray(4);
            boneIndex = reader.ReadInt32Array(4);
        }
    }
}
