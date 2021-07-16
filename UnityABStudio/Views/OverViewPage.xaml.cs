namespace SoarCraft.QYun.UnityABStudio.Views {
    using System;
    using System.Collections.Generic;
    using Windows.Storage;
    using Windows.Storage.Pickers;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Core.Services;
    using Helpers;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;
    using ViewModels;

    public sealed partial class OverViewPage : Page {
        public OverViewModel ViewModel { get; }

        private readonly ILogger logger = Ioc.Default.GetRequiredService<LoggerService>().Logger;

        public OverViewPage() {
            this.ViewModel = Ioc.Default.GetService<OverViewModel>();
            this.InitializeComponent();
        }

        private async void FileButton_OnClick(object sender, RoutedEventArgs e) {
            ((Button)sender).IsEnabled = false;

            var picker = new FileOpenPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".ab");

            WindowHelper.InitializeWithWindow(picker);
            var abFile = await picker.PickSingleFileAsync();
            ((Button)sender).IsEnabled = true;

            if (abFile == null)
                return;

            logger.Information($"AB File chosen: {abFile.Path}");
            this.ImageText.Text = abFile.Path;
            await ViewModel.LoadAssetsDataAsync(new List<StorageFile> { abFile });
        }
    }
}
