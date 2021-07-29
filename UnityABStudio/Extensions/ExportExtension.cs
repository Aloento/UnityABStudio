namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.AnimationClips;
    using AssetReader.Unity3D.Objects.Meshes;
    using AssetReader.Unity3D.Objects.Shaders;
    using AssetReader.Unity3D.Objects.Sprites;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Converters;
    using Converters.ShaderConverters;
    using Core.Models;
    using Helpers;
    using Newtonsoft.Json;
    using Services;

    public static partial class ExportExtension {
        private static readonly SettingsService settings = Ioc.Default.GetRequiredService<SettingsService>();

        public static Task<bool> ExportConvertFile(this AssetItem item, string exportPath) => Task.Run(() =>
            item.Type switch {
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

        private static bool ExportAnimator(AssetItem item, string exportPath,
                                           IReadOnlyCollection<AssetItem> animationList = null) {
            var exportFullPath = Path.Combine(exportPath, item.Name, item.Name + ".fbx");
            if (File.Exists(exportFullPath))
                exportFullPath = Path.Combine(exportPath, item.Name + item.BaseID, item.Name + ".fbx");

            var m_Animator = (Animator)item.Obj;
            var convert = animationList != null
                ? new ModelConverter(m_Animator, settings.ConvertType,
                    animationList.Select(x => (AnimationClip)x.Obj).ToArray())
                : new ModelConverter(m_Animator, settings.ConvertType);

            ExportFbx(convert, exportFullPath);
            return true;
        }

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

        private static bool ExportFont(AssetItem item, string exportPath) {
            var m_Font = (Font)item.Obj;
            if (m_Font.m_FontData != null) {
                var extension = ".ttf";
                if (m_Font.m_FontData[0] == 79 && m_Font.m_FontData[1] == 84 && m_Font.m_FontData[2] == 84 &&
                    m_Font.m_FontData[3] == 79) {
                    extension = ".otf";
                }

                if (!TryExportFile(exportPath, item, extension, out var exportFullPath))
                    return false;
                File.WriteAllBytes(exportFullPath, m_Font.m_FontData);
                return true;
            }

            return false;
        }

        private static bool ExportMesh(AssetItem item, string exportPath) {
            var m_Mesh = (Mesh)item.Obj;
            if (m_Mesh.m_VertexCount <= 0)
                return false;
            if (!TryExportFile(exportPath, item, ".obj", out var exportFullPath))
                return false;
            var sb = new StringBuilder();
            _ = sb.AppendLine("g " + m_Mesh.m_Name);

            #region Vertices

            if (m_Mesh.m_Vertices == null || m_Mesh.m_Vertices.Length == 0) {
                return false;
            }

            var c = 3;
            if (m_Mesh.m_Vertices.Length == m_Mesh.m_VertexCount * 4) {
                c = 4;
            }

            for (var v = 0; v < m_Mesh.m_VertexCount; v++) {
                _ = sb.AppendFormat("v {0} {1} {2}\r\n", -m_Mesh.m_Vertices[v * c], m_Mesh.m_Vertices[v * c + 1],
                    m_Mesh.m_Vertices[v * c + 2]);
            }

            #endregion

            #region UV

            if (m_Mesh.m_UV0?.Length > 0) {
                if (m_Mesh.m_UV0.Length == m_Mesh.m_VertexCount * 2) {
                    c = 2;
                } else if (m_Mesh.m_UV0.Length == m_Mesh.m_VertexCount * 3) {
                    c = 3;
                }

                for (var v = 0; v < m_Mesh.m_VertexCount; v++) {
                    _ = sb.AppendFormat("vt {0} {1}\r\n", m_Mesh.m_UV0[v * c], m_Mesh.m_UV0[v * c + 1]);
                }
            }

            #endregion

            #region Normals

            if (m_Mesh.m_Normals?.Length > 0) {
                if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 3) {
                    c = 3;
                } else if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 4) {
                    c = 4;
                }

                for (var v = 0; v < m_Mesh.m_VertexCount; v++) {
                    _ = sb.AppendFormat("vn {0} {1} {2}\r\n", -m_Mesh.m_Normals[v * c], m_Mesh.m_Normals[v * c + 1],
                        m_Mesh.m_Normals[v * c + 2]);
                }
            }

            #endregion

            #region Face

            var sum = 0;
            for (var i = 0; i < m_Mesh.m_SubMeshes.Length; i++) {
                _ = sb.AppendLine($"g {m_Mesh.m_Name}_{i}");
                var indexCount = (int)m_Mesh.m_SubMeshes[i].indexCount;
                var end = sum + indexCount / 3;
                for (var f = sum; f < end; f++) {
                    _ = sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\r\n", m_Mesh.m_Indices[f * 3 + 2] + 1,
                        m_Mesh.m_Indices[f * 3 + 1] + 1, m_Mesh.m_Indices[f * 3] + 1);
                }

                sum = end;
            }

            #endregion

            _ = sb.Replace("NaN", "0");
            File.WriteAllText(exportFullPath, sb.ToString());
            return true;
        }

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

        private static bool ExportMovieTexture(AssetItem item, string exportPath) {
            var m_MovieTexture = (MovieTexture)item.Obj;
            if (!TryExportFile(exportPath, item, ".ogv", out var exportFullPath))
                return false;
            File.WriteAllBytes(exportFullPath, m_MovieTexture.m_MovieData);
            return true;
        }

        private static bool ExportShader(AssetItem item, string exportPath) {
            if (!TryExportFile(exportPath, item, ".shader", out var exportFullPath))
                return false;
            var m_Shader = (Shader)item.Obj;
            var str = m_Shader.Convert();
            File.WriteAllText(exportFullPath, str);
            return true;
        }

        private static bool ExportSprite(AssetItem item, string exportPath) {
            var type = settings.ConvertType;
            if (!TryExportFile(exportPath, item, "." + type.ToString().ToLower(), out var exportFullPath))
                return false;
            var stream = ((Sprite)item.Obj).GetImage(type);
            if (stream != null) {
                using (stream) {
                    File.WriteAllBytes(exportFullPath, stream.ToArray());
                    return true;
                }
            }
            return false;
        }

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

        private static bool ExportVideoClip(AssetItem item, string exportPath) {
            var m_MovieTexture = (MovieTexture)item.Obj;
            if (!TryExportFile(exportPath, item, ".ogv", out var exportFullPath))
                return false;
            File.WriteAllBytes(exportFullPath, m_MovieTexture.m_MovieData);
            return true;
        }

        private static bool ExportRawFile(AssetItem item, string exportPath) {
            if (!TryExportFile(exportPath, item, ".bytes", out var exportFullPath))
                return false;
            File.WriteAllBytes(exportFullPath, item.Obj.GetRawData());
            return true;
        }
    }
}
