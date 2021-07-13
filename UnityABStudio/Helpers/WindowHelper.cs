namespace SoarCraft.QYun.UnityABStudio.Helpers {
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.UI.Xaml;
    using WinRT;

    internal static class WindowHelper {
        internal static void InitializeWithWindow(object obj) {
            if (Window.Current == null)
                obj.As<IInitializeWithWindow>().Initialize(App.MainWindow.As<IWindowNative>().WindowHandle);
        }
    }

    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInitializeWithWindow {
        void Initialize([In] IntPtr hwnd);
    }
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative {
        IntPtr WindowHandle { get; }
    }
}
