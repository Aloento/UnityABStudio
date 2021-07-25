namespace SoarCraft.QYun.UnityABStudio.Services {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Activation;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.Services;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Views;

    public class ActivationService : IActivationService {
        private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
        private readonly IEnumerable<IActivationHandler> _activationHandlers;
        private readonly INavigationService _navigationService;
        private readonly IThemeSelectorService _themeSelectorService;
        private UIElement _shell;

        public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, INavigationService navigationService, IThemeSelectorService themeSelectorService) {
            this._defaultHandler = defaultHandler;
            this._activationHandlers = activationHandlers;
            this._navigationService = navigationService;
            this._themeSelectorService = themeSelectorService;
        }

        public async Task ActivateAsync(object activationArgs) {
            // Initialize services that you need before app activation
            // take into account that the splash screen is shown while this code runs.
            await this.InitializeAsync();

            if (App.MainWindow.Content == null) {
                this._shell = Ioc.Default.GetService<ShellPage>();
                App.MainWindow.Content = this._shell ?? new Frame();
            }

            // Depending on activationArgs one of ActivationHandlers or DefaultActivationHandler
            // will navigate to the first page
            await this.HandleActivationAsync(activationArgs);

            // Ensure the current window is active
            App.MainWindow.Activate();

            // Tasks after activation
            await this.StartupAsync();
        }

        private async Task HandleActivationAsync(object activationArgs) {
            var activationHandler = this._activationHandlers
                                                .FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null) {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (this._defaultHandler.CanHandle(activationArgs)) {
                await this._defaultHandler.HandleAsync(activationArgs);
            }
        }

        private async Task InitializeAsync() {
            await this._themeSelectorService.InitializeAsync().ConfigureAwait(false);
            await Task.CompletedTask;
        }

        private async Task StartupAsync() {
            await this._themeSelectorService.SetRequestedThemeAsync();
            await Task.CompletedTask;
        }
    }
}
