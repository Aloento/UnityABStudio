namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Core.Models;
    using Services;

    public static partial class ExportExtension {
        private static readonly SettingsService settings = Ioc.Default.GetRequiredService<SettingsService>();

        public static Task<bool> ExportConvertFile(this AssetItem item, string exportPath) => Task.Run(() => item.Type switch {
            ClassIDType.AnimationClip => false,
            ClassIDType.Animator => ExportAnimator(item, exportPath),
            ClassIDType.AudioClip => ExportAudioClip(item, exportPath),
            ClassIDType.Font => ExportFont(item, exportPath),
            ClassIDType.Mesh => ExportMesh(item, exportPath),
            ClassIDType.MonoBehaviour => ExportMonoBehaviour(item, exportPath),
            ClassIDType.MovieTexture => ExportMovieTexture(item, exportPath),
            ClassIDType.Shader => ExportShader(item, exportPath),
            ClassIDType.Sprite => ExportSprite(item, exportPath),
            ClassIDType.Texture2D => ExportTexture2D(item, exportPath),
            ClassIDType.TextAsset => ExportTextAsset(item, exportPath),
            ClassIDType.VideoClip => ExportVideoClip(item, exportPath),
            _ => ExportRawFile(item, exportPath),
        });

        private static bool ExportAnimator(AssetItem item, string exportPath) => throw new NotImplementedException();

        private static bool ExportAudioClip(AssetItem item, string exportPath) => throw new NotImplementedException();

        private static bool ExportFont(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportMesh(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportMonoBehaviour(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportMovieTexture(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportShader(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportSprite(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportTextAsset(AssetItem item, string exportPath) => throw new NotImplementedException();

        private static bool ExportTexture2D(AssetItem item, string exportPath) {
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

        private static bool ExportVideoClip(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportRawFile(AssetItem item, string exportPath) => throw new NotImplementedException();
    }
}
