using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;

using Microsoft.UI.Xaml.Controls;

using UnityABStudio.ViewModels;

namespace UnityABStudio.Views {
    public sealed partial class ListDetailsPage : Page {
        public ListDetailsViewModel ViewModel { get; }

        public ListDetailsPage() {
            ViewModel = Ioc.Default.GetService<ListDetailsViewModel>();
            InitializeComponent();
        }

        private void OnViewStateChanged(object sender, ListDetailsViewState e) {
            if (e == ListDetailsViewState.Both) {
                ViewModel.EnsureItemSelected();
            }
        }
    }
}
