namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AssetBundles {
    using Utils;

    public class AssetInfo {
        public int preloadIndex;
        public int preloadSize;
        public PPtr<UObject> asset;

        public AssetInfo(ObjectReader reader) {
            preloadIndex = reader.ReadInt32();
            preloadSize = reader.ReadInt32();
            asset = new PPtr<UObject>(reader);
        }
    }
}
