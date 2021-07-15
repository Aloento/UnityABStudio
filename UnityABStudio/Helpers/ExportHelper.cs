namespace SoarCraft.QYun.UnityABStudio.Helpers {
    using ViewModels;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Services;

    public static class ExportHelper {
        private static string path;
        private static AssetItem asset;
        private static SettingsService settings = Ioc.Default.GetRequiredService<SettingsService>();

        public static bool ExportConvertFile(this AssetItem asset, string path) {
            ExportHelper.path = path;
            ExportHelper.asset = asset;

            switch (asset.Type) {
                case ClassIDType.Texture2D:
                    return ExportTexture2D(asset, path);
                case ClassIDType.AudioClip:
                case ClassIDType.Shader:
                case ClassIDType.TextAsset:
                case ClassIDType.MonoBehaviour:
                case ClassIDType.Font:
                case ClassIDType.Mesh:
                case ClassIDType.VideoClip:
                case ClassIDType.MovieTexture:
                case ClassIDType.Sprite:
                case ClassIDType.Animator:
                case ClassIDType.AnimationClip:
                    return false;
                default:
                    return false; //
            }
        }

        public static bool ExportTexture2D(this AssetItem asset, string path) {
            var m_Texture2D = (Texture2D)asset.Obj;
            if (settings.ConvertTexture) {

            }
            return false;
        }
    }
}
