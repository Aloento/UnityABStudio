namespace SoarCraft.QYun.UnityABStudio.Converters.ShaderConverters {
    using System;
    using System.IO;
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Shaders;
    using AssetReader.Utils;

    public static partial class ShaderConverter {
        private static string ConvertSerializedShader(Shader shader) {
            var shaderPrograms = new ShaderProgram[shader.platforms.Length];
            for (var i = 0; i < shader.platforms.Length; i++) {
                var compressedBytes = new byte[shader.compressedLengths[i]];
                Buffer.BlockCopy(shader.compressedBlob, (int)shader.offsets[i], compressedBytes, 0, (int)shader.compressedLengths[i]);
                var decompressedBytes = new byte[shader.decompressedLengths[i]];
                using (var decoder = new Lz4DecoderStream(new MemoryStream(compressedBytes))) {
                    _ = decoder.Read(decompressedBytes, 0, (int)shader.decompressedLengths[i]);
                }

                using var blobReader = new UnityReader(new MemoryStream(decompressedBytes), false);
                shaderPrograms[i] = new ShaderProgram(blobReader, shader.version);
            }

            return ConvertSerializedShader(shader.m_ParsedForm, shader.platforms, shaderPrograms);
        }

        private static string ConvertSerializedShader(SerializedShader m_ParsedForm, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms) {
            var sb = new StringBuilder();
            _ = sb.Append($"Shader \"{m_ParsedForm.m_Name}\" {{\n");

            _ = sb.Append(ConvertSerializedProperties(m_ParsedForm.m_PropInfo));

            foreach (var m_SubShader in m_ParsedForm.m_SubShaders) {
                _ = sb.Append(ConvertSerializedSubShader(m_SubShader, platforms, shaderPrograms));
            }

            if (!string.IsNullOrEmpty(m_ParsedForm.m_FallbackName)) {
                _ = sb.Append($"Fallback \"{m_ParsedForm.m_FallbackName}\"\n");
            }

            if (!string.IsNullOrEmpty(m_ParsedForm.m_CustomEditorName)) {
                _ = sb.Append($"CustomEditor \"{m_ParsedForm.m_CustomEditorName}\"\n");
            }

            _ = sb.Append('}');
            return sb.ToString();
        }

        private static string ConvertSerializedSubShader(SerializedSubShader m_SubShader, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms) {
            var sb = new StringBuilder();
            _ = sb.Append("SubShader {\n");
            if (m_SubShader.m_LOD != 0) {
                _ = sb.Append($" LOD {m_SubShader.m_LOD}\n");
            }

            _ = sb.Append(ConvertSerializedTagMap(m_SubShader.m_Tags, 1));

            foreach (var m_Passe in m_SubShader.m_Passes) {
                _ = sb.Append(ConvertSerializedPass(m_Passe, platforms, shaderPrograms));
            }
            _ = sb.Append("}\n");
            return sb.ToString();
        }
    }
}
