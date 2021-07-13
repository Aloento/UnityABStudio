namespace SoarCraft.QYun.UnityABStudio.Contracts.Services {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityABStudio.ViewModels;

    public interface IAssetDataService {
        Task<IEnumerable<AssetItem>> GetHierarchyDataAsync();

        Task<IEnumerable<AssetItem>> GetAssetListDataAsync();

        Task<IEnumerable<AssetItem>> GetClassListDataAsync();
    }
}
