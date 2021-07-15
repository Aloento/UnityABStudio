namespace SoarCraft.QYun.UnityABStudio.Views {
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using Services;

    // TODO WTS: Change the URL for your privacy policy, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page {
        public SettingsService ViewModel { get; }

        public SettingsPage() {
            this.ViewModel = Ioc.Default.GetService<SettingsService>();
            this.InitializeComponent();
        }
    }
}
