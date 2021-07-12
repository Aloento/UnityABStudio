namespace SoarCraft.QYun.UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using ViewModels;

    public sealed partial class ContentGridPage : Page {
        public ContentGridViewModel ViewModel { get; }

        public ContentGridPage() {
            this.ViewModel = Ioc.Default.GetService<ContentGridViewModel>();
            this.InitializeComponent();
        }
    }
}
