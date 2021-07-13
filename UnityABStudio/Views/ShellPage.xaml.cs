namespace SoarCraft.QYun.UnityABStudio.Views {
    using Windows.System;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.Services;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using ViewModels;

    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        public ShellViewModel ViewModel { get; }

        public ShellPage(ShellViewModel viewModel) {
            this.ViewModel = viewModel;
            this.InitializeComponent();
            this.ViewModel.NavigationService.Frame = this.shellFrame;
            this.ViewModel.NavigationViewService.Initialize(this.navigationView);
        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            this.KeyboardAccelerators.Add(this._altLeftKeyboardAccelerator);
            this.KeyboardAccelerators.Add(this._backKeyboardAccelerator);
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null) {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue) {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
            var navigationService = Ioc.Default.GetService<INavigationService>();
            var result = navigationService.GoBack();
            args.Handled = result;
        }
    }
}
