namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.Shaders;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Converters;
    using Converters.ShaderConverters;
    using Core.Helpers;
    using Core.Models;
    using Newtonsoft.Json;
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

        private static bool ExportAudioClip(AssetItem item, string exportPath) {
            var m_AudioClip = (AudioClip)item.Obj;
            var m_AudioData = m_AudioClip.m_AudioData.GetData();
            if (m_AudioData == null || m_AudioData.Length == 0)
                return false;
            var converter = new AudioClipConverter(m_AudioClip);
            if (settings.ConvertAudio && converter.IsSupport) {
                if (!TryExportFile(exportPath, item, ".wav", out var exportFullPath))
                    return false;
                var buffer = converter.ConvertToWav();
                if (buffer == null)
                    return false;
                File.WriteAllBytes(exportFullPath, buffer);
            } else {
                if (!TryExportFile(exportPath, item, converter.GetExtensionName(), out var exportFullPath))
                    return false;
                File.WriteAllBytes(exportFullPath, m_AudioData);
            }
            return true;
        }

        private static bool ExportFont(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportMesh(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportMonoBehaviour(AssetItem item, string exportPath) {
            if (!TryExportFile(exportPath, item, ".json", out var exportFullPath))
                return false;
            var m_MonoBehaviour = (MonoBehaviour)item.Obj;
            var type = m_MonoBehaviour.ToType();
            if (type == null) {
                var m_Type = m_MonoBehaviour.ConvertToTypeTree(null);
                type = m_MonoBehaviour.ToType(m_Type);
            }
            var str = JsonConvert.SerializeObject(type, Formatting.Indented);
            File.WriteAllText(exportFullPath, str);
            return true;
        }

        private static bool ExportMovieTexture(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportShader(AssetItem item, string exportPath) {
            if (!TryExportFile(exportPath, item, ".shader", out var exportFullPath))
                return false;
            var m_Shader = (Shader)item.Obj;
            var str = m_Shader.Convert();
            File.WriteAllText(exportFullPath, str);
            return true;
        }

        private static bool ExportSprite(AssetItem item, string exportPath) => throw new NotImplementedException();
        private static bool ExportTextAsset(AssetItem item, string exportPath) {
            var m_TextAsset = (TextAsset)item.Obj;
            var extension = ".txt";
            if (settings.RestoreExtensionName) {
                if (!string.IsNullOrEmpty(item.Container)) {
                    extension = Path.GetExtension(item.Container);
                }
            }
            if (!TryExportFile(exportPath, item, extension, out var exportFullPath))
                return false;
            File.WriteAllBytes(exportFullPath, m_TextAsset.m_Script);
            return true;
        }

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
