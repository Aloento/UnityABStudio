namespace SoarCraft.QYun.UnityABStudio.Helpers {
    using Microsoft.UI.Xaml.Controls;

    public static class FrameExtensions {
        public static object GetPageViewModel(this Frame frame)
            => frame?.Content?.GetType().GetProperty("Service")?.GetValue(frame.Content, null);
    }
}
