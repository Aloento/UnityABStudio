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
    using Core.Services;
    using Helpers;
    using Newtonsoft.Json;
    using Services;

    public static partial class ExportExtension {
        private static readonly SettingsService settings = Ioc.Default.GetRequiredService<SettingsService>();
        private static readonly CacheService cache = Ioc.Default.GetRequiredService<CacheService>();

        public static async Task<bool> ExportConvertFile(this AssetItem item, dynamic pathOrRes) {
            bool preview = string.IsNullOrWhiteSpace(pathOrRes);

            return item.Type switch {
                ClassIDType.Animator => ExportAnimator(item, pathOrRes, preview),
                ClassIDType.AudioClip => await ExportAudioClipAsync(item, pathOrRes, preview),
                ClassIDType.Font => ExportFont(item, pathOrRes, preview),
                ClassIDType.Mesh => await ExportMeshAsync(item, pathOrRes, preview),
                ClassIDType.MonoBehaviour => await ExportMonoBehaviourAsync(item, pathOrRes, preview),
                ClassIDType.MovieTexture => ExportMovieTexture(item, pathOrRes, preview),
                ClassIDType.Shader => await ExportShaderAsync(item, pathOrRes, preview),
                ClassIDType.Sprite => await ExportSpriteAsync(item, pathOrRes, preview),
                ClassIDType.Texture2D => await ExportTexture2DAsync(item, pathOrRes, preview),
                ClassIDType.TextAsset => ExportTextAsset(item, pathOrRes, preview),
                ClassIDType.VideoClip => ExportVideoClip(item, pathOrRes, preview),
                _ => ExportRawFile(item, pathOrRes, preview)
            };
        }

        private static bool ExportAnimator(AssetItem item, dynamic pathOrRes, bool preview,
                                           IReadOnlyCollection<AssetItem> animationList = null) {
            if (preview) {
                pathOrRes = "Animator does not support Preview";
                return false;
            }

            var exportFullPath = Path.Combine(pathOrRes, item.Name, item.Name + ".fbx");
            if (File.Exists(exportFullPath))
                exportFullPath = Path.Combine(pathOrRes, item.Name + item.BaseID, item.Name + ".fbx");

            var m_Animator = (Animator)item.Obj;
            var convert = animationList != null
                ? new ModelConverter(m_Animator, settings.ConvertType,
                    animationList.Select(x => (AnimationClip)x.Obj).ToArray())
                : new ModelConverter(m_Animator, settings.ConvertType);

            ExportFbx(convert, exportFullPath);
            return true;
        }

        private static async Task<bool> ExportAudioClipAsync(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_AudioClip = (AudioClip)item.Obj;
            var m_AudioData = m_AudioClip.m_AudioData.GetData();
            if (m_AudioData == null || m_AudioData.Length == 0)
                return false;
            var converter = new AudioClipConverter(m_AudioClip);

            if ((settings.ConvertAudio || preview) && converter.IsSupport) {
                var buffer = await cache.TryGetValue<byte[]>(item.BaseID);
                if (buffer == null) {
                    buffer = converter.ConvertToWav();
                    if (buffer == null)
                        return false;
                    _ = cache.TryPutAsync(item.BaseID, buffer);
                }

                if (preview) {
                    pathOrRes = buffer;
                    return true;
                }

                if (!TryExportFile(pathOrRes, item, ".wav", out string exportFullPath))
                    return false;

                _ = File.WriteAllBytesAsync(exportFullPath, buffer);
            } else {
                if (preview) {
                    pathOrRes = "Audio Format not Supported";
                    return false;
                }

                if (!TryExportFile(pathOrRes, item, converter.GetExtensionName(), out string exportFullPath))
                    return false;

                _ = File.WriteAllBytesAsync(exportFullPath, m_AudioData);
            }

            return true;
        }

        private static bool ExportFont(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_Font = (Font)item.Obj;
            if (m_Font.m_FontData != null) {
                if (preview) {
                    pathOrRes = m_Font.m_FontData;
                    return true;
                }

                var extension = ".ttf";
                if (m_Font.m_FontData[0] == 79 && m_Font.m_FontData[1] == 84 &&
                    m_Font.m_FontData[2] == 84 && m_Font.m_FontData[3] == 79) {
                    extension = ".otf";
                }

                if (!TryExportFile(pathOrRes, item, extension, out string exportFullPath))
                    return false;
                _ = File.WriteAllBytesAsync(exportFullPath, m_Font.m_FontData);
                return true;
            }

            return false;
        }

        private static async Task<bool> ExportMeshAsync(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_Mesh = (Mesh)item.Obj;
            if (m_Mesh.m_VertexCount <= 0)
                return false;

            var str = await cache.TryGetValue<string>(item.BaseID);
            if (string.IsNullOrWhiteSpace(str)) {
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
                str = sb.ToString();
                _ = cache.TryPutAsync(item.BaseID, str);
            }

            if (preview) {
                pathOrRes = str;
                return true;
            }

            if (!TryExportFile(pathOrRes, item, ".obj", out string exportFullPath))
                return false;

            _ = File.WriteAllTextAsync(exportFullPath, str);
            return true;
        }

        private static async Task<bool> ExportMonoBehaviourAsync(AssetItem item, dynamic pathOrRes, bool preview) {
            var res = await cache.TryGetValue<string>(item.BaseID);
            if (string.IsNullOrWhiteSpace(res)) {
                var m_MonoBehaviour = (MonoBehaviour)item.Obj;
                var type = m_MonoBehaviour.ToType();
                if (type == null) {
                    var m_Type = m_MonoBehaviour.ConvertToTypeTree(null);
                    type = m_MonoBehaviour.ToType(m_Type);
                }

                res = JsonConvert.SerializeObject(type, Formatting.Indented);
                _ = cache.TryPutAsync(item.BaseID, res);
            }


            if (preview) {
                pathOrRes = res;
                return true;
            }

            if (!TryExportFile(pathOrRes, item, ".json", out string exportFullPath))
                return false;

            _ = File.WriteAllTextAsync(exportFullPath, res);
            return true;
        }

        private static bool ExportMovieTexture(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_MovieTexture = (MovieTexture)item.Obj;
            if (preview) {
                pathOrRes = m_MovieTexture.m_MovieData;
                return true;
            }

            if (!TryExportFile(pathOrRes, item, ".ogv", out string exportFullPath))
                return false;
            _ = File.WriteAllBytesAsync(exportFullPath, m_MovieTexture.m_MovieData);
            return true;
        }

        private static async Task<bool> ExportShaderAsync(AssetItem item, dynamic pathOrRes, bool preview) {
            var res = await cache.TryGetValue<string>(item.BaseID);
            if (string.IsNullOrWhiteSpace(res)) {
                res = ((Shader)item.Obj).Convert();
                _ = cache.TryPutAsync(item.BaseID, res);
            }

            if (preview) {
                pathOrRes = res;
                return true;
            }

            if (!TryExportFile(pathOrRes, item, ".shader", out string exportFullPath))
                return false;

            _ = File.WriteAllTextAsync(exportFullPath, res);
            return true;
        }

        private static async Task<bool> ExportSpriteAsync(AssetItem item, dynamic pathOrRes, bool preview) {
            var res = await cache.TryGetValue<byte[]>(item.BaseID);
            var type = settings.ConvertType;

            if (res == null) {
                await using var stream = ((Sprite)item.Obj).GetImage(type);
                if (stream == null)
                    return false;

                res = stream.ToArray();
                _ = cache.TryPutAsync(item.BaseID, res);
            }

            if (preview) {
                pathOrRes = res;
                return true;
            }

            if (!TryExportFile(pathOrRes, item, $".{type.ToString().ToLower()}", out string exportFullPath))
                return false;

            _ = File.WriteAllBytesAsync(exportFullPath, res);
            return true;
        }

        private static bool ExportTextAsset(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_TextAsset = (TextAsset)item.Obj;

            if (preview) {
                pathOrRes = Encoding.UTF8.GetString(m_TextAsset.m_Script);
                return true;
            }

            var extension = ".txt";
            if (settings.RestoreExtensionName)
                extension = !string.IsNullOrEmpty(item.Container) ? Path.GetExtension(item.Container) : string.Empty;

            if (!TryExportFile(pathOrRes, item, extension, out string exportFullPath))
                return false;
            _ = File.WriteAllBytesAsync(exportFullPath, m_TextAsset.m_Script);
            return true;
        }

        private static async Task<bool> ExportTexture2DAsync(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_Texture2D = (Texture2D)item.Obj;
            if (settings.ConvertTexture) {
                var res = await cache.TryGetValue<byte[]>(item.BaseID);
                var type = settings.ConvertType;

                if (res == null) {
                    await using var stream = m_Texture2D.ConvertToStream(type, true);
                    if (stream == null)
                        return false;

                    res = stream.ToArray();
                    _ = cache.TryPutAsync(item.BaseID, res);
                }

                if (!TryExportFile(pathOrRes, item, $".{type.ToString().ToLower()}", out string exportFullPath))
                    return false;

                _ = File.WriteAllBytesAsync(exportFullPath, res);
                return true;
            } else {
                if (!TryExportFile(pathOrRes, item, ".tex", out string exportFullPath))
                    return false;
                _ = File.WriteAllBytesAsync(exportFullPath, m_Texture2D.image_data.GetData());
                return true;
            }
        }

        private static bool ExportVideoClip(AssetItem item, dynamic pathOrRes, bool preview) {
            var m_MovieTexture = (MovieTexture)item.Obj;

            if (preview) {
                pathOrRes = m_MovieTexture.m_MovieData;
                return true;
            }

            if (!TryExportFile(pathOrRes, item, ".ogv", out string exportFullPath))
                return false;
            _ = File.WriteAllBytesAsync(exportFullPath, m_MovieTexture.m_MovieData);
            return true;
        }

        private static bool ExportRawFile(AssetItem item, dynamic pathOrRes, bool preview) {
            if (preview) {
                pathOrRes = $"{item.Type} does not support Preview";
                return false;
            }

            if (!TryExportFile(pathOrRes, item, ".bytes", out string exportFullPath))
                return false;
            _ = File.WriteAllBytesAsync(exportFullPath, item.Obj.GetRawData());
            return true;
        }
    }
}
