namespace UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;

    using Microsoft.UI.Xaml.Controls;

    using UnityABStudio.ViewModels;

    public sealed partial class ContentGridPage : Page {
        public ContentGridViewModel ViewModel { get; }

        public ContentGridPage() {
            ViewModel = Ioc.Default.GetService<ContentGridViewModel>();
            InitializeComponent();
        }
    }
}
