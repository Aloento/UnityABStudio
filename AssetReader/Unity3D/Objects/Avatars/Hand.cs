namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using Utils;

    public class Hand {
        public int[] m_HandBoneIndex;

        public Hand(ObjectReader reader) {
            m_HandBoneIndex = reader.ReadInt32Array();
        }
    }
}
