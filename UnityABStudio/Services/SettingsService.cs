namespace SoarCraft.QYun.UnityABStudio.Services {
    using System.Windows.Input;
    using Windows.ApplicationModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Services;
    using Core.Entities;
    using Extensions;
    using Microsoft.UI.Xaml;

    public class SettingsService : ObservableRecipient {
        private readonly IThemeSelectorService themeSelectorService;
        private ElementTheme elementTheme;

        public ElementTheme ElementTheme {
            get => this.elementTheme;

            set => _ = this.SetProperty(ref this.elementTheme, value);
        }

        private string versionDescription;

        public string VersionDescription {
            get => this.versionDescription;

            set => _ = this.SetProperty(ref this.versionDescription, value);
        }

        private bool convertTexture;

        public bool ConvertTexture {
            get => this.convertTexture;
            set => _ = SetProperty(ref this.convertTexture, value);
        }

        private ImageFormat convertType;

        public ImageFormat ConvertType {
            get => this.convertType;
            set => _ = SetProperty(ref this.convertType, value);
        }

        private ICommand switchThemeCommand;

        public ICommand SwitchThemeCommand => this.switchThemeCommand ??= new RelayCommand<ElementTheme>(
            async (param) => {
                if (this.ElementTheme != param) {
                    this.ElementTheme = param;
                    await this.themeSelectorService.SetThemeAsync(param);
                }
            });

        public SettingsService(IThemeSelectorService themeSelectorService) {
            this.themeSelectorService = themeSelectorService;
            this.elementTheme = this.themeSelectorService.Theme;
            this.VersionDescription = this.GetVersionDescription();
        }

        private string GetVersionDescription() {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
