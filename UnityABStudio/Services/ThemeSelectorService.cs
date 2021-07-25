namespace SoarCraft.QYun.UnityABStudio.Services {
    using System;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Contracts.Services;
    using Extensions;
    using Microsoft.UI.Xaml;

    public class ThemeSelectorService : IThemeSelectorService {
        private const string SettingsKey = "AppBackgroundRequestedTheme";

        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        public async Task InitializeAsync() {
            this.Theme = await this.LoadThemeFromSettingsAsync();
            await Task.CompletedTask;
        }

        public async Task SetThemeAsync(ElementTheme theme) {
            this.Theme = theme;

            await this.SetRequestedThemeAsync();
            await this.SaveThemeInSettingsAsync(this.Theme);
        }

        public async Task SetRequestedThemeAsync() {
            if (App.MainWindow.Content is FrameworkElement rootElement) {
                rootElement.RequestedTheme = this.Theme;
            }

            await Task.CompletedTask;
        }

        private async Task<ElementTheme> LoadThemeFromSettingsAsync() {
            ElementTheme cacheTheme = ElementTheme.Default;
            string themeName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(themeName)) {
                _ = Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private async Task SaveThemeInSettingsAsync(ElementTheme theme) => await ApplicationData.Current.LocalSettings.SaveAsync(SettingsKey, theme.ToString());
    }
}
