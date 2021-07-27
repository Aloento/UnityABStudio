// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace SoarCraft.QYun.UnityABStudio {
    using System;
    using Activation;
    using AssetReader;
    using AutoDeskFBX;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.Services;
    using Core.Contracts.Services;
    using Core.Services;
    using Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Services;
    using TextureDecoder;
    using ViewModels;
    using Views;
    using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

    public partial class App : Application {
        public static Window MainWindow { get; set; } = new() { Title = "AppDisplayName".GetLocalized() };

        public App() {
            InitializeComponent();
            this.UnhandledException += this.App_UnhandledException;
            Ioc.Default.ConfigureServices(this.ConfigureServices());
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e) => Ioc.Default.GetService<LoggerService>()?.Logger.Error(e.Message);

        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            base.OnLaunched(args);
            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }

        private IServiceProvider ConfigureServices() {
            var services = new ServiceCollection();
            _ = services.AddSingleton<LoggerService>();

            // Default Activation Handler
            _ = services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            _ = services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            _ = services.AddTransient<INavigationViewService, NavigationViewService>();

            _ = services.AddSingleton<IActivationService, ActivationService>();
            _ = services.AddSingleton<IPageService, PageService>();
            _ = services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            _ = services.AddSingleton<ISampleDataService, SampleDataService>();
            _ = services.AddSingleton<IAssetDataService, AssetDataService>();
            _ = services.AddSingleton<SettingsService>();
            _ = services.AddSingleton<AssetsManager>();
            _ = services.AddSingleton<TextureDecoderService>();
            _ = services.AddSingleton<FBXService>();

            // Views and ViewModels
            _ = services.AddTransient<ShellPage>();
            _ = services.AddTransient<ShellViewModel>();
            _ = services.AddTransient<OverViewModel>();
            _ = services.AddTransient<OverViewPage>();
            _ = services.AddTransient<ListDetailsViewModel>();
            _ = services.AddTransient<ListDetailsPage>();
            _ = services.AddTransient<DataGridViewModel>();
            _ = services.AddTransient<DataGridPage>();
            _ = services.AddTransient<ContentGridViewModel>();
            _ = services.AddTransient<ContentGridPage>();
            _ = services.AddTransient<ContentGridDetailViewModel>();
            _ = services.AddTransient<ContentGridDetailPage>();
            _ = services.AddTransient<SettingsPage>();
            return services.BuildServiceProvider();
        }
    }
}
