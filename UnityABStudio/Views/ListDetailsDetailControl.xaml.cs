namespace SoarCraft.QYun.UnityABStudio.Views {
    using Core.Models;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class ListDetailsDetailControl : UserControl {
        public SampleOrder ListDetailsMenuItem {
            get { return this.GetValue(ListDetailsMenuItemProperty) as SampleOrder; }
            set { this.SetValue(ListDetailsMenuItemProperty, value); }
        }

        public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(SampleOrder), typeof(ListDetailsDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

        public ListDetailsDetailControl() {
            this.InitializeComponent();
        }

        private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as ListDetailsDetailControl;
            _ = control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
