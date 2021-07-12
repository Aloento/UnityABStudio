namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.Services;
    using Microsoft.UI.Xaml.Navigation;
    using Views;

    public class ShellViewModel : ObservableRecipient {
        private bool _isBackEnabled;
        private object _selected;

        public INavigationService NavigationService { get; }

        public INavigationViewService NavigationViewService { get; }

        public bool IsBackEnabled {
            get { return this._isBackEnabled; }
            set { _ = this.SetProperty(ref this._isBackEnabled, value); }
        }

        public object Selected {
            get { return this._selected; }
            set { _ = this.SetProperty(ref this._selected, value); }
        }

        public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService) {
            this.NavigationService = navigationService;
            this.NavigationService.Navigated += this.OnNavigated;
            this.NavigationViewService = navigationViewService;
        }

        private void OnNavigated(object sender, NavigationEventArgs e) {
            this.IsBackEnabled = this.NavigationService.CanGoBack;
            if (e.SourcePageType == typeof(SettingsPage)) {
                this.Selected = this.NavigationViewService.SettingsItem;
                return;
            }

            var selectedItem = this.NavigationViewService.GetSelectedItem(e.SourcePageType);
            if (selectedItem != null) {
                this.Selected = selectedItem;
            }
        }
    }
}
