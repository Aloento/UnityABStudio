namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AssetReader;
    using Contracts.Services;
    using Models;

    public class AssetDataService : IAssetDataService {
        public static readonly AssetsManager AssetsManager = new();

        public Task<IEnumerable<AssetItem>> GetHierarchyDataAsync() => throw new NotImplementedException();

        public Task<IEnumerable<AssetItem>> GetAssetListDataAsync() => throw new NotImplementedException();

        public Task<IEnumerable<AssetItem>> GetClassListDataAsync() => throw new NotImplementedException();
    }
}
