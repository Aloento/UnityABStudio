namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System;
    using System.IO;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Core.Models;
    using Services;

    public static class ExportExtension {
        private static string exportPath;
        private static AssetItem item;
        private static readonly SettingsService settings = Ioc.Default.GetRequiredService<SettingsService>();

        public static bool ExportConvertFile(this AssetItem item, string exportPath) {
            ExportExtension.exportPath = exportPath;
            ExportExtension.item = item;

            return item.Type switch {
                ClassIDType.AnimationClip => false,
                ClassIDType.Animator => ExportAnimator(),
                ClassIDType.AudioClip => ExportAudioClip(),
                ClassIDType.Font => ExportFont(),
                ClassIDType.Mesh => ExportMesh(),
                ClassIDType.MonoBehaviour => ExportMonoBehaviour(),
                ClassIDType.MovieTexture => ExportMovieTexture(),
                ClassIDType.Shader => ExportShader(),
                ClassIDType.Sprite => ExportSprite(),
                ClassIDType.Texture2D => ExportTexture2D(),
                ClassIDType.TextAsset => ExportTextAsset(),
                ClassIDType.VideoClip => ExportVideoClip(),
                _ => ExportRawFile(),
            };
        }

        private static bool ExportAnimator() => throw new NotImplementedException();

        private static bool ExportAudioClip() => throw new NotImplementedException();

        private static bool ExportFont() => throw new NotImplementedException();
        private static bool ExportMesh() => throw new NotImplementedException();
        private static bool ExportMonoBehaviour() => throw new NotImplementedException();
        private static bool ExportMovieTexture() => throw new NotImplementedException();
        private static bool ExportShader() => throw new NotImplementedException();
        private static bool ExportSprite() => throw new NotImplementedException();
        private static bool ExportTextAsset() => throw new NotImplementedException();

        private static bool ExportTexture2D() {
            var m_Texture2D = (Texture2D)item.Obj;
            if (settings.ConvertTexture) {
                var type = settings.ConvertType;
                if (!TryExportFile(exportPath, item, "." + type.ToString().ToLower(), out var exportFullPath))
                    return false;
                var stream = m_Texture2D.ConvertToStream(type, true);
                if (stream == null)
                    return false;
                using (stream) {
                    File.WriteAllBytes(exportFullPath, stream.ToArray());
                    return true;
                }
            } else {
                if (!TryExportFile(exportPath, item, ".tex", out var exportFullPath))
                    return false;
                File.WriteAllBytes(exportFullPath, m_Texture2D.image_data.GetData());
                return true;
            }
        }

        private static bool ExportVideoClip() => throw new NotImplementedException();
        private static bool ExportRawFile() => throw new NotImplementedException();
    }
}
