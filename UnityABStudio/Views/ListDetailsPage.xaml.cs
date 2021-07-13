namespace SoarCraft.QYun.UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using CommunityToolkit.WinUI.UI.Controls;
    using Microsoft.UI.Xaml.Controls;
    using ViewModels;

    public sealed partial class ListDetailsPage : Page {
        public ListDetailsViewModel ViewModel { get; }

        public ListDetailsPage() {
            this.ViewModel = Ioc.Default.GetService<ListDetailsViewModel>();
            this.InitializeComponent();
        }

        private void OnViewStateChanged(object sender, ListDetailsViewState e) {
            if (e == ListDetailsViewState.Both) {
                this.ViewModel.EnsureItemSelected();
            }
        }
    }
}
