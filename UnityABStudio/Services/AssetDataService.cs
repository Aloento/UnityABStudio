namespace SoarCraft.QYun.UnityABStudio.Services {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AssetReader;
    using Contracts.Services;
    using ViewModels;

    public class AssetDataService : IAssetDataService {
        public static readonly AssetsManager AssetsManager = new();

        public Task<IEnumerable<AssetItem>> GetHierarchyDataAsync() => throw new System.NotImplementedException();

        public Task<IEnumerable<AssetItem>> GetAssetListDataAsync() => throw new System.NotImplementedException();

        public Task<IEnumerable<AssetItem>> GetClassListDataAsync() => throw new System.NotImplementedException();
    }
}
