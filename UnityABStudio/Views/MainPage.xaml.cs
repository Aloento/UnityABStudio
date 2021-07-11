namespace UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;

    using Microsoft.UI.Xaml.Controls;

    using UnityABStudio.ViewModels;

    public sealed partial class MainPage : Page {
        public MainViewModel ViewModel { get; }

        public MainPage() {
            ViewModel = Ioc.Default.GetService<MainViewModel>();
            InitializeComponent();
        }
    }
}
