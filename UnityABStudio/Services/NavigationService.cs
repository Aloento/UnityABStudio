namespace SoarCraft.QYun.UnityABStudio.Services {
    using CommunityToolkit.WinUI.UI.Animations;
    using Contracts.Services;
    using Contracts.ViewModels;
    using Extensions;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    // For more information on navigation between pages see
    // https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/WinUI/navigation.md
    public class NavigationService : INavigationService {
        private readonly IPageService _pageService;
        private object _lastParameterUsed;
        private Frame _frame;

        public event NavigatedEventHandler Navigated;

        public Frame Frame {
            get {
                if (this._frame == null) {
                    this._frame = App.MainWindow.Content as Frame;
                    this.RegisterFrameEvents();
                }

                return this._frame;
            }

            set {
                this.UnregisterFrameEvents();
                this._frame = value;
                this.RegisterFrameEvents();
            }
        }

        public bool CanGoBack => this.Frame.CanGoBack;

        public NavigationService(IPageService pageService) {
            this._pageService = pageService;
        }

        private void RegisterFrameEvents() {
            if (this._frame != null) {
                this._frame.Navigated += this.OnNavigated;
            }
        }

        private void UnregisterFrameEvents() {
            if (this._frame != null) {
                this._frame.Navigated -= this.OnNavigated;
            }
        }

        public bool GoBack() {
            if (this.CanGoBack) {
                var vmBeforeNavigation = this._frame.GetPageViewModel();
                this._frame.GoBack();
                if (vmBeforeNavigation is INavigationAware navigationAware) {
                    navigationAware.OnNavigatedFrom();
                }

                return true;
            }

            return false;
        }

        public bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false) {
            var pageType = this._pageService.GetPageType(pageKey);

            if (this._frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(this._lastParameterUsed))) {
                this._frame.Tag = clearNavigation;
                var vmBeforeNavigation = this._frame.GetPageViewModel();
                var navigated = this._frame.Navigate(pageType, parameter);
                if (navigated) {
                    this._lastParameterUsed = parameter;
                    if (vmBeforeNavigation is INavigationAware navigationAware) {
                        navigationAware.OnNavigatedFrom();
                    }
                }

                return navigated;
            }

            return false;
        }

        public void CleanNavigation()
            => this._frame.BackStack.Clear();

        private void OnNavigated(object sender, NavigationEventArgs e) {
            if (sender is Frame frame) {
                var clearNavigation = (bool)frame.Tag;
                if (clearNavigation) {
                    frame.BackStack.Clear();
                }

                if (frame.GetPageViewModel() is INavigationAware navigationAware) {
                    navigationAware.OnNavigatedTo(e.Parameter);
                }

                this.Navigated?.Invoke(sender, e);
            }
        }

        public void SetListDataItemForNextConnectedAnimation(object item)
            => this.Frame.SetListDataItemForNextConnectedAnimation(item);
    }
}
