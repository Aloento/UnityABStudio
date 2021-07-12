namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using AssetReader;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D;

    public class AssetItem {
        public UObject Asset { get; set; }
        public ClassIDType Type { get; set; }
        public SerializedFile SourceFile { get; set; }
        public string Container { get; set; }
        public string InfoText { get; set; }
        public long m_PathID { get; set; }
        public long FullSize { get; set; }

        public AssetItem(UObject asset) {
            this.Asset = asset;
            this.SourceFile = asset.assetsFile;
            this.Type = asset.type;
            this.m_PathID = asset.m_PathID;
            this.FullSize = asset.byteSize;
        }
    }
}
