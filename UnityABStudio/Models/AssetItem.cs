namespace SoarCraft.QYun.UnityABStudio.Models {
    using Microsoft.UI.Xaml.Controls;
    using SoarCraft.QYun.AssetReader;
    using SoarCraft.QYun.AssetReader.Entities.Enums;
    using SoarCraft.QYun.AssetReader.Unity3D;

    public class AssetItem : ListViewItem {
        public UObject Asset;
        public ClassIDType Type;
        public SerializedFile SourceFile;
        public string Container = string.Empty;
        public string InfoText;
        public long m_PathID;
        public long FullSize;

        public AssetItem(UObject asset) {
            Asset = asset;
            SourceFile = asset.assetsFile;
            Type = asset.type;
            m_PathID = asset.m_PathID;
            FullSize = asset.byteSize;
        }
    }
}
