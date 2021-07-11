// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace UnityABStudio {
    using CommunityToolkit.Mvvm.DependencyInjection;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;

    using UnityABStudio.Activation;
    using UnityABStudio.Contracts.Services;
    using UnityABStudio.Core.Contracts.Services;
    using UnityABStudio.Core.Services;
    using UnityABStudio.Helpers;
    using UnityABStudio.Services;
    using UnityABStudio.ViewModels;
    using UnityABStudio.Views;

    public partial class App : Application {
        public static Window MainWindow { get; set; } = new Window() { Title = "AppDisplayName".GetLocalized() };

        public App() {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            Ioc.Default.ConfigureServices(ConfigureServices());
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

            // Views and ViewModels
            _ = services.AddTransient<ShellPage>();
            _ = services.AddTransient<ShellViewModel>();
            _ = services.AddTransient<MainViewModel>();
            _ = services.AddTransient<MainPage>();
            _ = services.AddTransient<ListDetailsViewModel>();
            _ = services.AddTransient<ListDetailsPage>();
            _ = services.AddTransient<DataGridViewModel>();
            _ = services.AddTransient<DataGridPage>();
            _ = services.AddTransient<ContentGridViewModel>();
            _ = services.AddTransient<ContentGridPage>();
            _ = services.AddTransient<ContentGridDetailViewModel>();
            _ = services.AddTransient<ContentGridDetailPage>();
            _ = services.AddTransient<SettingsViewModel>();
            _ = services.AddTransient<SettingsPage>();
            return services.BuildServiceProvider();
        }
    }
}
