namespace SoarCraft.QYun.UnityABStudio.Converters.ShaderConverters {
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Shaders;

    public static partial class ShaderConverter {
        private static string ConvertSerializedTagMap(SerializedTagMap m_Tags, int intent) {
            var sb = new StringBuilder();
            if (m_Tags.tags.Length > 0) {
                _ = sb.Append(new string(' ', intent));
                _ = sb.Append("Tags { ");
                foreach (var (key, value) in m_Tags.tags) {
                    _ = sb.Append($"\"{key}\" = \"{value}\" ");
                }
                _ = sb.Append("}\n");
            }
            return sb.ToString();
        }

        private static string ConvertSerializedPass(SerializedPass m_Passe, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms) {
            var sb = new StringBuilder();
            switch (m_Passe.m_Type) {
                case PassType.kPassTypeNormal:
                    _ = sb.Append(" Pass ");
                    break;
                case PassType.kPassTypeUse:
                    _ = sb.Append(" UsePass ");
                    break;
                case PassType.kPassTypeGrab:
                    _ = sb.Append(" GrabPass ");
                    break;
            }
            if (m_Passe.m_Type == PassType.kPassTypeUse) {
                _ = sb.Append($"\"{m_Passe.m_UseName}\"\n");
            } else {
                _ = sb.Append("{\n");

                if (m_Passe.m_Type == PassType.kPassTypeGrab) {
                    if (!string.IsNullOrEmpty(m_Passe.m_TextureName)) {
                        _ = sb.Append($"  \"{m_Passe.m_TextureName}\"\n");
                    }
                } else {
                    _ = sb.Append(ConvertSerializedShaderState(m_Passe.m_State));

                    if (m_Passe.progVertex.m_SubPrograms.Length > 0) {
                        _ = sb.Append("Program \"vp\" {\n");
                        _ = sb.Append(ConvertSerializedSubPrograms(m_Passe.progVertex.m_SubPrograms, platforms, shaderPrograms));
                        _ = sb.Append("}\n");
                    }

                    if (m_Passe.progFragment.m_SubPrograms.Length > 0) {
                        _ = sb.Append("Program \"fp\" {\n");
                        _ = sb.Append(ConvertSerializedSubPrograms(m_Passe.progFragment.m_SubPrograms, platforms, shaderPrograms));
                        _ = sb.Append("}\n");
                    }

                    if (m_Passe.progGeometry.m_SubPrograms.Length > 0) {
                        _ = sb.Append("Program \"gp\" {\n");
                        _ = sb.Append(ConvertSerializedSubPrograms(m_Passe.progGeometry.m_SubPrograms, platforms, shaderPrograms));
                        _ = sb.Append("}\n");
                    }

                    if (m_Passe.progHull.m_SubPrograms.Length > 0) {
                        _ = sb.Append("Program \"hp\" {\n");
                        _ = sb.Append(ConvertSerializedSubPrograms(m_Passe.progHull.m_SubPrograms, platforms, shaderPrograms));
                        _ = sb.Append("}\n");
                    }

                    if (m_Passe.progDomain.m_SubPrograms.Length > 0) {
                        _ = sb.Append("Program \"dp\" {\n");
                        _ = sb.Append(ConvertSerializedSubPrograms(m_Passe.progDomain.m_SubPrograms, platforms, shaderPrograms));
                        _ = sb.Append("}\n");
                    }

                    if (m_Passe.progRayTracing?.m_SubPrograms.Length > 0) {
                        _ = sb.Append("Program \"rtp\" {\n");
                        _ = sb.Append(ConvertSerializedSubPrograms(m_Passe.progRayTracing.m_SubPrograms, platforms, shaderPrograms));
                        _ = sb.Append("}\n");
                    }
                }
                _ = sb.Append("}\n");
            }
            return sb.ToString();
        }
    }
}
