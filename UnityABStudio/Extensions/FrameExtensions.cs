namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using Microsoft.UI.Xaml.Controls;

    public static class FrameExtensions {
        public static object GetPageViewModel(this Frame frame)
            => frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
    }
}
