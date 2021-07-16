// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace SoarCraft.QYun.UnityABStudio {
    using Activation;
    using AssetReader;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.Services;
    using Core.Contracts.Services;
    using Core.Models;
    using Core.Services;
    using Helpers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Services;
    using ViewModels;
    using Views;

    public partial class App : Application {
        public static Window MainWindow { get; set; } = new() { Title = "AppDisplayName".GetLocalized() };

        public App() {
            InitializeComponent();
            this.UnhandledException += this.App_UnhandledException;
            Ioc.Default.ConfigureServices(this.ConfigureServices());
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/windows/winui/api/microsoft.ui.xaml.unhandledexceptioneventargs
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            base.OnLaunched(args);
            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }

        private System.IServiceProvider ConfigureServices() {
            // TODO WTS: Register your services, viewmodels and pages here
            var services = new ServiceCollection();

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
            _ = services.AddTransient<SettingsService>();
            _ = services.AddSingleton<AssetsManager>();
            _ = services.AddSingleton<LoggerService>();

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
