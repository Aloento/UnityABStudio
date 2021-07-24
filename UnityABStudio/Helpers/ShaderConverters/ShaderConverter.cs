namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System.IO;
    using System.Text;
    using AssetReader.Unity3D.Objects.Shaders;
    using AssetReader.Utils;

    public static partial class ShaderConverter {
        private const string header = "//////////////////////////////////////////\n" +
                                      "//\n" +
                                      "// NOTE: This is *NOT* a valid shader file\n" +
                                      "//\n" +
                                      "///////////////////////////////////////////\n";

        public static string Convert(this Shader shader) {
            if (shader.m_SubProgramBlob != null) {
                //5.3 - 5.4
                var decompressedBytes = new byte[shader.decompressedSize];
                using (var decoder = new Lz4DecoderStream(new MemoryStream(shader.m_SubProgramBlob))) {
                    _ = decoder.Read(decompressedBytes, 0, (int)shader.decompressedSize);
                }

                using var blobReader = new UnityReader(new MemoryStream(decompressedBytes), false);
                var program = new ShaderProgram(blobReader, shader.version);
                return header + program.Export(Encoding.UTF8.GetString(shader.m_Script));
            }

            if (shader.compressedBlob != null) {
                //5.5 and up
                return header + ConvertSerializedShader(shader);
            }

            return header + Encoding.UTF8.GetString(shader.m_Script);
        }
    }
}
