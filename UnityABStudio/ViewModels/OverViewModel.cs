namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.Storage;
    using AssetReader;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Serilog;
    using Services;

    public class OverViewModel : ObservableRecipient {
        private readonly AssetsManager manager = Ioc.Default.GetRequiredService<AssetsManager>();
        private readonly ILogger logger = Ioc.Default.GetRequiredService<LoggerService>().Logger;

        public async Task LoadAssetsDataAsync(IEnumerable<StorageFile> files) {
            this.logger.Information("Start to load Asset Files...");
            await manager.LoadFilesAsync(files.Select(x => x.Path).ToArray());
            this.logger.Information("AB Files loaded... Start to read contents...");

            var objectCount = this.manager.AssetsFileList.Sum(x => x.Objects.Count);
            var objectAssetsList = new List<AssetItem>(objectCount);



        }
    }
}
