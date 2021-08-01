// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.

namespace SoarCraft.QYun.UnityABStudio {
    using System;
    using Activation;
    using AssetReader;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.Services;
    using Core.Contracts.Services;
    using Core.Services;
    using Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Serilog;
    using Serilog.Core;
    using Services;
    using TextureDecoder;
    using ViewModels;
    using Views;
    using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

    public partial class App : Application {
        public static Window MainWindow { get; set; } = new() { Title = "AppDisplayName".GetLocalized() };
        private readonly ILogger logger;

        public App() {
            InitializeComponent();
            this.UnhandledException += this.App_UnhandledException;
            Ioc.Default.ConfigureServices(this.ConfigureServices());

            logger = Ioc.Default.GetRequiredService<ILogger>();
            logger.Information("-------------------------------------");
            logger.Information("Hello, SoarCraft.QYun.UnityABStudio!");
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e) =>
            logger.Error(e.Message);

        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            base.OnLaunched(args);
            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }

        private IServiceProvider ConfigureServices() {
            var services = new ServiceCollection();

            // Base Services
            _ = services.AddMemoryCache().AddSingleton<CacheService>();
            _ = services.AddSingleton<ILogger, Logger>(_ => new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Verbose()
#endif
                .WriteTo.Async(a => a.File(@"C:\CaChe\UnityABStudio.log"))
                .CreateLogger());

            // Default Activation Handler
            _ = services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            _ = services.AddScoped<IThemeSelectorService, ThemeSelectorService>();
            _ = services.AddTransient<INavigationViewService, NavigationViewService>();

            _ = services.AddScoped<IActivationService, ActivationService>();
            _ = services.AddScoped<IPageService, PageService>();
            _ = services.AddScoped<INavigationService, NavigationService>();

            // Core Services
            _ = services.AddSingleton<ISampleDataService, SampleDataService>();
            _ = services.AddSingleton<IAssetDataService, AssetDataService>();
            _ = services.AddSingleton<SettingsService>();
            _ = services.AddSingleton<AssetsManager>();
            _ = services.AddSingleton<TextureDecoderService>();
            _ = services.AddSingleton<FBXHelpService>();

            // ViewModels
            _ = services.AddScoped<ShellViewModel>();
            _ = services.AddSingleton<OverViewModel>();
            _ = services.AddScoped<ListDetailsViewModel>();
            _ = services.AddScoped<DataGridViewModel>();
            _ = services.AddScoped<ContentGridViewModel>();
            _ = services.AddScoped<ContentGridDetailViewModel>();

            // Views
            _ = services.AddScoped<ShellPage>();
            _ = services.AddScoped<OverViewPage>();
            _ = services.AddScoped<ListDetailsPage>();
            _ = services.AddScoped<DataGridPage>();
            _ = services.AddScoped<ContentGridPage>();
            _ = services.AddScoped<ContentGridDetailPage>();
            _ = services.AddScoped<SettingsPage>();

            return services.BuildServiceProvider();
        }
    }
}
