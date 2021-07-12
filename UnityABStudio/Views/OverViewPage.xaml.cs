namespace SoarCraft.QYun.UnityABStudio.Views {
    using AssetReader;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;
    using Services;
    using ViewModels;

    public sealed partial class OverViewPage : Page {
        public OverViewModel ViewModel { get; }

        private readonly ILogger logger = Ioc.Default.GetRequiredService<LoggerService>().Logger;

        public OverViewPage() {
            this.ViewModel = Ioc.Default.GetService<OverViewModel>();
            this.InitializeComponent();
        }

        private void FileButton_OnClick(object sender, RoutedEventArgs e) {
            var manager = Ioc.Default.GetRequiredService<AssetsManager>();
            logger.Information("OnClicked");
            ((Button)sender).Content = manager.ToString();
        }
    }
}
