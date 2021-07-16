namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using Microsoft.ApplicationModel.Resources;

    internal static class ResourceExtensions {
        private static readonly ResourceLoader _resLoader = new();

        public static string GetLocalized(this string resourceKey) => _resLoader.GetString(resourceKey);
    }
}
