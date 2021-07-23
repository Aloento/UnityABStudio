namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System;
    using System.IO;
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Utils;

    public class ShaderSubProgram {
        private int m_Version;
        public ShaderGpuProgramType m_ProgramType;
        public string[] m_Keywords;
        public string[] m_LocalKeywords;
        public byte[] m_ProgramCode;

        public ShaderSubProgram(UnityReader reader) {
            //LoadGpuProgramFromData
            //201509030 - Unity 5.3
            //201510240 - Unity 5.4
            //201608170 - Unity 5.5
            //201609010 - Unity 5.6, 2017.1 & 2017.2
            //201708220 - Unity 2017.3, Unity 2017.4 & Unity 2018.1
            //201802150 - Unity 2018.2 & Unity 2018.3
            //201806140 - Unity 2019.1~2020.1
            m_Version = reader.ReadInt32();
            m_ProgramType = (ShaderGpuProgramType)reader.ReadInt32();
            reader.BaseStream.Position += 12;
            if (m_Version >= 201608170) {
                reader.BaseStream.Position += 4;
            }
            var m_KeywordsSize = reader.ReadInt32();
            m_Keywords = new string[m_KeywordsSize];
            for (var i = 0; i < m_KeywordsSize; i++) {
                m_Keywords[i] = reader.ReadAlignedString();
            }
            if (m_Version >= 201806140) {
                var m_LocalKeywordsSize = reader.ReadInt32();
                m_LocalKeywords = new string[m_LocalKeywordsSize];
                for (var i = 0; i < m_LocalKeywordsSize; i++) {
                    m_LocalKeywords[i] = reader.ReadAlignedString();
                }
            }
            m_ProgramCode = reader.ReadUInt8Array();
            reader.AlignStream();

            //TODO
        }

        public string Export() {
            var sb = new StringBuilder();
            if (m_Keywords.Length > 0) {
                _ = sb.Append("Keywords { ");
                foreach (var keyword in m_Keywords) {
                    _ = sb.Append($"\"{keyword}\" ");
                }
                _ = sb.Append("}\n");
            }
            if (m_LocalKeywords != null && m_LocalKeywords.Length > 0) {
                _ = sb.Append("Local Keywords { ");
                foreach (var keyword in m_LocalKeywords) {
                    _ = sb.Append($"\"{keyword}\" ");
                }
                _ = sb.Append("}\n");
            }

            _ = sb.Append('"');
            if (m_ProgramCode.Length > 0) {
                switch (m_ProgramType) {
                    case ShaderGpuProgramType.kShaderGpuProgramGLLegacy:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES31AEP:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES31:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES3:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES:
                    case ShaderGpuProgramType.kShaderGpuProgramGLCore32:
                    case ShaderGpuProgramType.kShaderGpuProgramGLCore41:
                    case ShaderGpuProgramType.kShaderGpuProgramGLCore43:
                        _ = sb.Append(Encoding.UTF8.GetString(m_ProgramCode));
                        break;
                    case ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM20:
                    case ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM30:
                    case ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM20:
                    case ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM30: {
                        /*var shaderBytecode = new ShaderBytecode(m_ProgramCode);
                        sb.Append(shaderBytecode.Disassemble());*/
                        _ = sb.Append("// shader disassembly not supported on DXBC");
                        break;
                    }
                    case ShaderGpuProgramType.kShaderGpuProgramDX10Level9Vertex:
                    case ShaderGpuProgramType.kShaderGpuProgramDX10Level9Pixel:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM40:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM40:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM40:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11HullSM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11DomainSM50: {
                        /*int start = 6;
                        if (m_Version == 201509030) // 5.3
                        {
                            start = 5;
                        }
                        var buff = new byte[m_ProgramCode.Length - start];
                        Buffer.BlockCopy(m_ProgramCode, start, buff, 0, buff.Length);
                        var shaderBytecode = new ShaderBytecode(buff);
                        sb.Append(shaderBytecode.Disassemble());*/
                        _ = sb.Append("// shader disassembly not supported on DXBC");
                        break;
                    }
                    case ShaderGpuProgramType.kShaderGpuProgramMetalVS:
                    case ShaderGpuProgramType.kShaderGpuProgramMetalFS:
                        using (var reader = new UnityReader(new MemoryStream(m_ProgramCode))) {
                            var fourCC = reader.ReadUInt32();
                            if (fourCC == 0xf00dcafe) {
                                var offset = reader.ReadInt32();
                                reader.BaseStream.Position = offset;
                            }
                            var entryName = reader.ReadStringToNull();
                            var buff = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                            _ = sb.Append(Encoding.UTF8.GetString(buff));
                        }
                        break;
                    case ShaderGpuProgramType.kShaderGpuProgramSPIRV:
                        try {
                            _ = sb.Append(SpirVShaderConverter.Convert(m_ProgramCode));
                        } catch (Exception e) {
                            _ = sb.Append($"// disassembly error {e.Message}\n");
                        }
                        break;
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleVS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleFS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleHS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleDS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleGS:
                        _ = sb.Append(Encoding.UTF8.GetString(m_ProgramCode));
                        break;
                    default:
                        _ = sb.Append($"//shader disassembly not supported on {m_ProgramType}");
                        break;
                }
            }
            _ = sb.Append('"');
            return sb.ToString();
        }
    }
}
