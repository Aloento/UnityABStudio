namespace SoarCraft.QYun.UnityABStudio.Views {
    using System;
    using System.Collections.Generic;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;
    using ViewModels;
    using WinRT.Interop;
    using System.Threading.Tasks;

    public sealed partial class OverViewPage : Page {
        public OverViewModel ViewModel { get; }

        private readonly ILogger logger = Ioc.Default.GetRequiredService<ILogger>();

        public OverViewPage() {
            this.ViewModel = Ioc.Default.GetService<OverViewModel>();
            this.InitializeComponent();
            logger.Debug($"Loading {nameof(OverViewPage)}");
        }

        private async void FileButton_OnClick(object sender, RoutedEventArgs e) {
            ((Button)sender).IsEnabled = false;

            var picker = new FileOpenPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".ab");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var abFile = await picker.PickSingleFileAsync();
            ((Button)sender).IsEnabled = true;

            if (abFile == null)
                return;

            logger.Information($"AB File chosen: {abFile.Path}");
            this.ImageText.Text = abFile.Path;
            _ = await ViewModel.LoadAssetsDataAsync(new List<StorageFile> { abFile });
        }

        private async void LoadPanel_OnLoadedAsync(object sender, RoutedEventArgs e) {
#if DEBUG
            await ViewModel.BuildTestBundleListAsync();
#endif
        }
    }
}
