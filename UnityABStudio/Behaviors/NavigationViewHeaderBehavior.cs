namespace SoarCraft.QYun.UnityABStudio.Behaviors {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.Services;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Microsoft.Xaml.Interactivity;

    public class NavigationViewHeaderBehavior : Behavior<NavigationView> {
        private static NavigationViewHeaderBehavior _current;
        private Page _currentPage;

        public DataTemplate DefaultHeaderTemplate { get; set; }

        public object DefaultHeader {
            get { return this.GetValue(DefaultHeaderProperty); }
            set { this.SetValue(DefaultHeaderProperty, value); }
        }

        public static readonly DependencyProperty DefaultHeaderProperty = DependencyProperty.Register("DefaultHeader", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeader()));

        public static NavigationViewHeaderMode GetHeaderMode(Page item) {
            return (NavigationViewHeaderMode)item.GetValue(HeaderModeProperty);
        }

        public static void SetHeaderMode(Page item, NavigationViewHeaderMode value) {
            item.SetValue(HeaderModeProperty, value);
        }

        public static readonly DependencyProperty HeaderModeProperty =
            DependencyProperty.RegisterAttached("HeaderMode", typeof(bool), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(NavigationViewHeaderMode.Always, (d, e) => _current.UpdateHeader()));

        public static object GetHeaderContext(Page item) {
            return item.GetValue(HeaderContextProperty);
        }

        public static void SetHeaderContext(Page item, object value) {
            item.SetValue(HeaderContextProperty, value);
        }

        public static readonly DependencyProperty HeaderContextProperty =
            DependencyProperty.RegisterAttached("HeaderContext", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeader()));

        public static DataTemplate GetHeaderTemplate(Page item) {
            return (DataTemplate)item.GetValue(HeaderTemplateProperty);
        }

        public static void SetHeaderTemplate(Page item, DataTemplate value) {
            item.SetValue(HeaderTemplateProperty, value);
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeaderTemplate()));

        protected override void OnAttached() {
            base.OnAttached();
            _current = this;
            var navigationService = Ioc.Default.GetService<INavigationService>();
            navigationService.Navigated += this.OnNavigated;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            var navigationService = Ioc.Default.GetService<INavigationService>();
            navigationService.Navigated -= this.OnNavigated;
        }

        private void OnNavigated(object sender, NavigationEventArgs e) {
            var frame = sender as Frame;
            if (frame.Content is Page page) {
                this._currentPage = page;

                this.UpdateHeader();
                this.UpdateHeaderTemplate();
            }
        }

        private void UpdateHeader() {
            if (this._currentPage != null) {
                var headerMode = GetHeaderMode(this._currentPage);
                if (headerMode == NavigationViewHeaderMode.Never) {
                    this.AssociatedObject.Header = null;
                    this.AssociatedObject.AlwaysShowHeader = false;
                } else {
                    var headerFromPage = GetHeaderContext(this._currentPage);
                    if (headerFromPage != null) {
                        this.AssociatedObject.Header = headerFromPage;
                    } else {
                        this.AssociatedObject.Header = this.DefaultHeader;
                    }

                    if (headerMode == NavigationViewHeaderMode.Always) {
                        this.AssociatedObject.AlwaysShowHeader = true;
                    } else {
                        this.AssociatedObject.AlwaysShowHeader = false;
                    }
                }
            }
        }

        private void UpdateHeaderTemplate() {
            if (this._currentPage != null) {
                var headerTemplate = GetHeaderTemplate(this._currentPage);
                this.AssociatedObject.HeaderTemplate = headerTemplate ?? this.DefaultHeaderTemplate;
            }
        }
    }
}
