namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Windows.Input;
    using Windows.ApplicationModel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.Services;
    using Helpers;
    using Microsoft.UI.Xaml;

    public class SettingsViewModel : ObservableRecipient {
        private readonly IThemeSelectorService _themeSelectorService;
        private ElementTheme _elementTheme;

        public ElementTheme ElementTheme {
            get { return this._elementTheme; }

            set { _ = this.SetProperty(ref this._elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription {
            get { return this._versionDescription; }

            set { _ = this.SetProperty(ref this._versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand {
            get {
                if (this._switchThemeCommand == null) {
                    this._switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) => {
                            if (this.ElementTheme != param) {
                                this.ElementTheme = param;
                                await this._themeSelectorService.SetThemeAsync(param);
                            }
                        });
                }

                return this._switchThemeCommand;
            }
        }

        public SettingsViewModel(IThemeSelectorService themeSelectorService) {
            this._themeSelectorService = themeSelectorService;
            this._elementTheme = this._themeSelectorService.Theme;
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
