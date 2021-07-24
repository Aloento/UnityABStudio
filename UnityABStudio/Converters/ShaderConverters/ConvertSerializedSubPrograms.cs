namespace SoarCraft.QYun.UnityABStudio.Converters.ShaderConverters {
    using System;
    using System.Linq;
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Shaders;

    public static partial class ShaderConverter {
        private static string ConvertSerializedSubPrograms(SerializedSubProgram[] m_SubPrograms,
            ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms) {
            var sb = new StringBuilder();
            var groups = m_SubPrograms.GroupBy(x => x.m_BlobIndex);
            foreach (var group in groups) {
                var programs = group.GroupBy(x => x.m_GpuProgramType);
                foreach (var program in programs) {
                    for (var i = 0; i < platforms.Length; i++) {
                        var platform = platforms[i];
                        if (CheckGpuProgramUsable(platform, program.Key)) {
                            var subPrograms = program.ToList();
                            var isTier = subPrograms.Count > 1;
                            foreach (var subProgram in subPrograms) {
                                _ = sb.Append($"SubProgram \"{GetPlatformString(platform)} ");
                                if (isTier) {
                                    _ = sb.Append($"hw_tier{subProgram.m_ShaderHardwareTier:00} ");
                                }

                                _ = sb.Append("\" {\n");
                                _ = sb.Append(shaderPrograms[i].m_SubPrograms[subProgram.m_BlobIndex].Export());
                                _ = sb.Append("\n}\n");
                            }

                            break;
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private static bool CheckGpuProgramUsable(ShaderCompilerPlatform platform, ShaderGpuProgramType programType) =>
            platform switch {
                ShaderCompilerPlatform.kShaderCompPlatformGL => programType ==
                                                                ShaderGpuProgramType.kShaderGpuProgramGLLegacy,
                ShaderCompilerPlatform.kShaderCompPlatformD3D9 => programType is ShaderGpuProgramType
                        .kShaderGpuProgramDX9VertexSM20
                    or ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM30
                    or ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM20
                    or ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM30,
                ShaderCompilerPlatform.kShaderCompPlatformXbox360 or ShaderCompilerPlatform.kShaderCompPlatformPS3 or
                    ShaderCompilerPlatform.kShaderCompPlatformPSP2 or ShaderCompilerPlatform.kShaderCompPlatformPS4 or
                    ShaderCompilerPlatform.kShaderCompPlatformXboxOne or ShaderCompilerPlatform.kShaderCompPlatformN3DS
                    or ShaderCompilerPlatform.kShaderCompPlatformWiiU or
                    ShaderCompilerPlatform.kShaderCompPlatformSwitch or
                    ShaderCompilerPlatform.kShaderCompPlatformXboxOneD3D12 or
                    ShaderCompilerPlatform.kShaderCompPlatformGameCoreXboxOne or
                    ShaderCompilerPlatform.kShaderCompPlatformGameCoreScarlett or
                    ShaderCompilerPlatform.kShaderCompPlatformPS5 or ShaderCompilerPlatform.kShaderCompPlatformPS5NGGC
                    => programType is ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        or ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        or ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        or ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        or ShaderGpuProgramType.kShaderGpuProgramConsoleGS,
                ShaderCompilerPlatform.kShaderCompPlatformD3D11 => programType is ShaderGpuProgramType
                        .kShaderGpuProgramDX11VertexSM40
                    or ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM50
                    or ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM40
                    or ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM50
                    or ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM40
                    or ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM50
                    or ShaderGpuProgramType.kShaderGpuProgramDX11HullSM50
                    or ShaderGpuProgramType.kShaderGpuProgramDX11DomainSM50,
                ShaderCompilerPlatform.kShaderCompPlatformGLES20 => programType ==
                                                                    ShaderGpuProgramType.kShaderGpuProgramGLES,
                //Obsolete
                ShaderCompilerPlatform.kShaderCompPlatformNaCl => throw new NotSupportedException(),
                //Obsolete
                ShaderCompilerPlatform.kShaderCompPlatformFlash => throw new NotSupportedException(),
                ShaderCompilerPlatform.kShaderCompPlatformD3D11_9x => programType is ShaderGpuProgramType
                        .kShaderGpuProgramDX10Level9Vertex
                    or ShaderGpuProgramType.kShaderGpuProgramDX10Level9Pixel,
                ShaderCompilerPlatform.kShaderCompPlatformGLES3Plus => programType is ShaderGpuProgramType
                        .kShaderGpuProgramGLES31AEP
                    or ShaderGpuProgramType.kShaderGpuProgramGLES31
                    or ShaderGpuProgramType.kShaderGpuProgramGLES3,
                //Unknown
                ShaderCompilerPlatform.kShaderCompPlatformPSM => throw new NotSupportedException(),
                ShaderCompilerPlatform.kShaderCompPlatformMetal => programType is ShaderGpuProgramType
                        .kShaderGpuProgramMetalVS
                    or ShaderGpuProgramType.kShaderGpuProgramMetalFS,
                ShaderCompilerPlatform.kShaderCompPlatformOpenGLCore => programType is ShaderGpuProgramType
                        .kShaderGpuProgramGLCore32
                    or ShaderGpuProgramType.kShaderGpuProgramGLCore41
                    or ShaderGpuProgramType.kShaderGpuProgramGLCore43,
                ShaderCompilerPlatform.kShaderCompPlatformVulkan => programType ==
                                                                    ShaderGpuProgramType.kShaderGpuProgramSPIRV,
                _ => throw new NotSupportedException(),
            };

        private static string GetPlatformString(ShaderCompilerPlatform platform) => platform switch {
            ShaderCompilerPlatform.kShaderCompPlatformGL => "openGL",
            ShaderCompilerPlatform.kShaderCompPlatformD3D9 => "d3d9",
            ShaderCompilerPlatform.kShaderCompPlatformXbox360 => "xbox360",
            ShaderCompilerPlatform.kShaderCompPlatformPS3 => "ps3",
            ShaderCompilerPlatform.kShaderCompPlatformD3D11 => "d3d11",
            ShaderCompilerPlatform.kShaderCompPlatformGLES20 => "gles",
            ShaderCompilerPlatform.kShaderCompPlatformNaCl => "glesdesktop",
            ShaderCompilerPlatform.kShaderCompPlatformFlash => "flash",
            ShaderCompilerPlatform.kShaderCompPlatformD3D11_9x => "d3d11_9x",
            ShaderCompilerPlatform.kShaderCompPlatformGLES3Plus => "gles3",
            ShaderCompilerPlatform.kShaderCompPlatformPSP2 => "psp2",
            ShaderCompilerPlatform.kShaderCompPlatformPS4 => "ps4",
            ShaderCompilerPlatform.kShaderCompPlatformXboxOne => "xboxone",
            ShaderCompilerPlatform.kShaderCompPlatformPSM => "psm",
            ShaderCompilerPlatform.kShaderCompPlatformMetal => "metal",
            ShaderCompilerPlatform.kShaderCompPlatformOpenGLCore => "glcore",
            ShaderCompilerPlatform.kShaderCompPlatformN3DS => "n3ds",
            ShaderCompilerPlatform.kShaderCompPlatformWiiU => "wiiu",
            ShaderCompilerPlatform.kShaderCompPlatformVulkan => "vulkan",
            ShaderCompilerPlatform.kShaderCompPlatformSwitch => "switch",
            ShaderCompilerPlatform.kShaderCompPlatformXboxOneD3D12 => "xboxone_d3d12",
            ShaderCompilerPlatform.kShaderCompPlatformGameCoreXboxOne => "xboxone",
            ShaderCompilerPlatform.kShaderCompPlatformGameCoreScarlett => "xbox_scarlett",
            ShaderCompilerPlatform.kShaderCompPlatformPS5 => "ps5",
            ShaderCompilerPlatform.kShaderCompPlatformPS5NGGC => "ps5_nggc",
            _ => "unknown",
        };
    }
}
