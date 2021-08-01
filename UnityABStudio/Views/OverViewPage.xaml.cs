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

        private void QuickExportButton_OnClick(object sender, RoutedEventArgs e) {
#if DEBUG
            this.logger.Debug($"{this.ViewModel.ExpAnimator}");
            this.logger.Debug($"{this.ViewModel.ExpAudioClip}");
            this.logger.Debug($"{this.ViewModel.ExpFont}");
            this.logger.Debug($"{this.ViewModel.ExpMesh}");
            this.logger.Debug($"{this.ViewModel.ExpMonoBehaviour}");
            this.logger.Debug($"{this.ViewModel.ExpMovieTexture}");
            this.logger.Debug($"{this.ViewModel.ExpShader}");
            this.logger.Debug($"{this.ViewModel.ExpSprite}");
            this.logger.Debug($"{this.ViewModel.ExpTexture2D}");
            this.logger.Debug($"{this.ViewModel.ExpTextAsset}");
            this.logger.Debug($"{this.ViewModel.ExpVideoClip}");
#endif
        }

        #region QuickBoxSet

        private void AnimatorBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpAnimator = true;

        private void AnimatorBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpAnimator = false;

        private void AudioClipBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpAudioClip = true;

        private void AudioClipBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpAudioClip = false;

        private void FontBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpFont = true;

        private void FontBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpFont = false;

        private void MeshBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpMesh = true;

        private void MeshBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpMesh = false;

        private void MonoBehaviourBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpMonoBehaviour = true;

        private void MonoBehaviourBox_OnUnchecked(object sender, RoutedEventArgs e) =>
            ViewModel.ExpMonoBehaviour = false;

        private void MovieTextureBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpMovieTexture = true;

        private void MovieTextureBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpMovieTexture = false;

        private void ShaderBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpShader = true;

        private void ShaderBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpShader = false;

        private void SpriteBox_OnClick(object sender, RoutedEventArgs e) => ViewModel.ExpSprite = true;

        private void SpriteBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpSprite = false;

        private void Texture2DBox_OnClick(object sender, RoutedEventArgs e) => ViewModel.ExpTexture2D = true;

        private void Texture2DBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpTexture2D = false;

        private void TextAssetBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpTextAsset = true;

        private void TextAssetBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpTextAsset = false;

        private void VideoClipBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpVideoClip = true;

        private void VideoClipBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpVideoClip = false;

        #endregion
    }
}
