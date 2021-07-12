namespace SoarCraft.QYun.UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using CommunityToolkit.WinUI.UI.Animations;
    using Contracts.Services;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using ViewModels;

    public sealed partial class ContentGridDetailPage : Page {
        public ContentGridDetailViewModel ViewModel { get; }

        public ContentGridDetailPage() {
            this.ViewModel = Ioc.Default.GetService<ContentGridDetailViewModel>();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            this.RegisterElementForConnectedAnimation("animationKeyContentGrid", this.itemHero);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back) {
                var navigationService = Ioc.Default.GetService<INavigationService>();
                navigationService.SetListDataItemForNextConnectedAnimation(this.ViewModel.Item);
            }
        }
    }
}
