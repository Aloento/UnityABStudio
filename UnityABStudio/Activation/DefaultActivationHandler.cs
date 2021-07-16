namespace SoarCraft.QYun.UnityABStudio.Activation {
    using System.Threading.Tasks;
    using Contracts.Services;
    using Core.Models;
    using Microsoft.UI.Xaml;

    public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs> {
        private readonly INavigationService _navigationService;

        public DefaultActivationHandler(INavigationService navigationService) {
            this._navigationService = navigationService;
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args) {
            _ = this._navigationService.NavigateTo(typeof(OverViewModel).FullName, args.Arguments);
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args) {
            // None of the ActivationHandlers has handled the app activation
            return this._navigationService.Frame.Content == null;
        }
    }
}
