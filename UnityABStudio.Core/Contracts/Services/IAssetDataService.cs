namespace SoarCraft.QYun.UnityABStudio.Core.Contracts.Services {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    public interface IAssetDataService {
        Task<IEnumerable<AssetItem>> GetHierarchyDataAsync();

        Task<IEnumerable<AssetItem>> GetAssetListDataAsync();

        Task<IEnumerable<AssetItem>> GetClassListDataAsync();
    }
}
