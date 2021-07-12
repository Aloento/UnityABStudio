namespace SoarCraft.QYun.UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using ViewModels;

    // TODO WTS: Change the URL for your privacy policy, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage() {
            this.ViewModel = Ioc.Default.GetService<SettingsViewModel>();
            this.InitializeComponent();
        }
    }
}
