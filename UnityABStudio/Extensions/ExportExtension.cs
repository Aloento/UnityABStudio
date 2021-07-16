namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System.IO;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Core.Models;
    using Services;

    public static class ExportExtension {
        private static string path;
        private static AssetItem asset;
        private static readonly SettingsService settings = Ioc.Default.GetRequiredService<SettingsService>();

        public static bool ExportConvertFile(this AssetItem asset, string path) {
            ExportExtension.path = path;
            ExportExtension.asset = asset;

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
                var type = settings.ConvertType;
                if (!TryExportFile(path, asset, "." + type.ToString().ToLower(), out var exportFullPath))
                    return false;
                var stream = m_Texture2D.ConvertToStream(type, true);
                if (stream == null)
                    return false;
                using (stream) {
                    File.WriteAllBytes(exportFullPath, stream.ToArray());
                    return true;
                }
            } else {
                if (!TryExportFile(path, asset, ".tex", out var exportFullPath))
                    return false;
                File.WriteAllBytes(exportFullPath, m_Texture2D.image_data.GetData());
                return true;
            }
        }
    }
}
