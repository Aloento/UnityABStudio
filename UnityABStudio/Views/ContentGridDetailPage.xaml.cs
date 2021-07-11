namespace UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using CommunityToolkit.WinUI.UI.Animations;

    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    using UnityABStudio.Contracts.Services;
    using UnityABStudio.ViewModels;

    public sealed partial class ContentGridDetailPage : Page {
        public ContentGridDetailViewModel ViewModel { get; }

        public ContentGridDetailPage() {
            ViewModel = Ioc.Default.GetService<ContentGridDetailViewModel>();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            this.RegisterElementForConnectedAnimation("animationKeyContentGrid", itemHero);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back) {
                var navigationService = Ioc.Default.GetService<INavigationService>();
                navigationService.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }
}
