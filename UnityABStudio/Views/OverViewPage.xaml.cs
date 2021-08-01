namespace SoarCraft.QYun.UnityABStudio.Views {
    using System;
    using Windows.Storage.Pickers;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;
    using ViewModels;
    using WinRT.Interop;
    using Microsoft.UI.Xaml.Input;

    public sealed partial class OverViewPage : Page {
        public OverViewModel ViewModel { get; }

        private readonly ILogger logger = Ioc.Default.GetRequiredService<ILogger>();

        public OverViewPage() {
            this.ViewModel = Ioc.Default.GetService<OverViewModel>();
            this.InitializeComponent();

#if DEBUG
            logger.Debug($"Loading {nameof(OverViewPage)}");
#endif
        }

        private void LoadPanel_OnLoaded(object sender, RoutedEventArgs e) {
            //#if DEBUG
            //_ = ViewModel.BuildTestBundleListAsync().ConfigureAwait(false);
            //#endif

            #region CommandBar

            var openFileCommand = new StandardUICommand(StandardUICommandKind.Open);
            openFileCommand.ExecuteRequested += OpenFileCommandOnExecuteRequested;
            this.OpenFiles.Command = openFileCommand;

            var openFolderCommand = new StandardUICommand(StandardUICommandKind.Open);
            openFolderCommand.ExecuteRequested += OpenFolderCommandOnExecuteRequested;
            this.OpenFolder.Command = openFolderCommand;

            var ejectCommand = new StandardUICommand(StandardUICommandKind.Delete);
            ejectCommand.ExecuteRequested += EjectCommandOnExecuteRequested;
            this.EjectItems.Command = ejectCommand;

            #endregion
        }

        private void EjectCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            this.ViewModel.EjectFiles(this.LoadedList.SelectedItems);

        private async void OpenFolderCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) {
            this.OpenFolder.IsEnabled = false;

            var picker = new FolderPicker() {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var folder = await picker.PickSingleFolderAsync();
            this.OpenFolder.IsEnabled = true;

            if (folder == null)
                return;

            _ = ViewModel.LoadAssetFolderAsync(folder).ConfigureAwait(false);
        }

        private async void OpenFileCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) {
            this.OpenFiles.IsEnabled = false;

            var picker = new FileOpenPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".ab");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var abFile = await picker.PickMultipleFilesAsync();
            this.OpenFiles.IsEnabled = true;

            if (abFile == null)
                return;

            _ = ViewModel.LoadAssetFilesAsync(abFile).ConfigureAwait(false);
        }

        private void AnimatorBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void AnimatorBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void AudioClipBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void AudioClipBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void FontBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void FontBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void MeshBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void MeshBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void MonoBehaviourBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void MonoBehaviourBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void MovieTextureBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void MovieTextureBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ShaderBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ShaderBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void SpriteBox_OnClick(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void SpriteBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Texture2DBox_OnClick(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Texture2DBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void TextAssetBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void TextAssetBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void VideoClipBox_OnChecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void VideoClipBox_OnUnchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
