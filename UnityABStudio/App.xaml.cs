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
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();

            // Views and ViewModels
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ListDetailsViewModel>();
            services.AddTransient<ListDetailsPage>();
            services.AddTransient<DataGridViewModel>();
            services.AddTransient<DataGridPage>();
            services.AddTransient<ContentGridViewModel>();
            services.AddTransient<ContentGridPage>();
            services.AddTransient<ContentGridDetailViewModel>();
            services.AddTransient<ContentGridDetailPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            return services.BuildServiceProvider();
        }
    }
}
