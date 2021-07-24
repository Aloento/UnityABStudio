namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System.Globalization;
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Shaders;

    public static partial class ShaderConverter {
        private static string ConvertSerializedShaderState(SerializedShaderState m_State) {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(m_State.m_Name)) {
                _ = sb.Append($"  Name \"{m_State.m_Name}\"\n");
            }
            if (m_State.m_LOD != 0) {
                _ = sb.Append($"  LOD {m_State.m_LOD}\n");
            }

            _ = sb.Append(ConvertSerializedTagMap(m_State.m_Tags, 2));

            _ = sb.Append(ConvertSerializedShaderRTBlendState(m_State.rtBlend, m_State.rtSeparateBlend));

            if (m_State.alphaToMask.val > 0f) {
                _ = sb.Append("  AlphaToMask On\n");
            }

            if (m_State.zClip?.val != 1f) { //ZClip On
                _ = sb.Append("  ZClip Off\n");
            }

            if (m_State.zTest.val != 4f) { //ZTest LEqual
                _ = sb.Append("  ZTest ");
                switch (m_State.zTest.val) { //enum CompareFunction
                    case 0f: //kFuncDisabled
                        _ = sb.Append("Off");
                        break;
                    case 1f: //kFuncNever
                        _ = sb.Append("Never");
                        break;
                    case 2f: //kFuncLess
                        _ = sb.Append("Less");
                        break;
                    case 3f: //kFuncEqual
                        _ = sb.Append("Equal");
                        break;
                    case 5f: //kFuncGreater
                        _ = sb.Append("Greater");
                        break;
                    case 6f: //kFuncNotEqual
                        _ = sb.Append("NotEqual");
                        break;
                    case 7f: //kFuncGEqual
                        _ = sb.Append("GEqual");
                        break;
                    case 8f: //kFuncAlways
                        _ = sb.Append("Always");
                        break;
                }

                _ = sb.Append('\n');
            }

            if (m_State.zWrite.val != 1f) { //ZWrite On
                _ = sb.Append("  ZWrite Off\n");
            }

            if (m_State.culling.val != 2f) { //Cull Back
                _ = sb.Append("  Cull ");
                switch (m_State.culling.val) { //enum CullMode
                    case 0f: //kCullOff
                        _ = sb.Append("Off");
                        break;
                    case 1f: //kCullFront
                        _ = sb.Append("Front");
                        break;
                }
                _ = sb.Append('\n');
            }

            if (m_State.offsetFactor.val != 0f || m_State.offsetUnits.val != 0f) {
                _ = sb.Append($"  Offset {m_State.offsetFactor.val}, {m_State.offsetUnits.val}\n");
            }

            if (m_State.stencilRef.val != 0f ||
                m_State.stencilReadMask.val != 255f ||
                m_State.stencilWriteMask.val != 255f ||
                m_State.stencilOp.pass.val != 0f ||
                m_State.stencilOp.fail.val != 0f ||
                m_State.stencilOp.zFail.val != 0f ||
                m_State.stencilOp.comp.val != 8f ||
                m_State.stencilOpFront.pass.val != 0f ||
                m_State.stencilOpFront.fail.val != 0f ||
                m_State.stencilOpFront.zFail.val != 0f ||
                m_State.stencilOpFront.comp.val != 8f ||
                m_State.stencilOpBack.pass.val != 0f ||
                m_State.stencilOpBack.fail.val != 0f ||
                m_State.stencilOpBack.zFail.val != 0f ||
                m_State.stencilOpBack.comp.val != 8f) {
                _ = sb.Append("  Stencil {\n");
                if (m_State.stencilRef.val != 0f) {
                    _ = sb.Append($"   Ref {m_State.stencilRef.val}\n");
                }
                if (m_State.stencilReadMask.val != 255f) {
                    _ = sb.Append($"   ReadMask {m_State.stencilReadMask.val}\n");
                }
                if (m_State.stencilWriteMask.val != 255f) {
                    _ = sb.Append($"   WriteMask {m_State.stencilWriteMask.val}\n");
                }
                if (m_State.stencilOp.pass.val != 0f ||
                    m_State.stencilOp.fail.val != 0f ||
                    m_State.stencilOp.zFail.val != 0f ||
                    m_State.stencilOp.comp.val != 8f) {
                    _ = sb.Append(ConvertSerializedStencilOp(m_State.stencilOp, ""));
                }
                if (m_State.stencilOpFront.pass.val != 0f ||
                    m_State.stencilOpFront.fail.val != 0f ||
                    m_State.stencilOpFront.zFail.val != 0f ||
                    m_State.stencilOpFront.comp.val != 8f) {
                    _ = sb.Append(ConvertSerializedStencilOp(m_State.stencilOpFront, "Front"));
                }
                if (m_State.stencilOpBack.pass.val != 0f ||
                    m_State.stencilOpBack.fail.val != 0f ||
                    m_State.stencilOpBack.zFail.val != 0f ||
                    m_State.stencilOpBack.comp.val != 8f) {
                    _ = sb.Append(ConvertSerializedStencilOp(m_State.stencilOpBack, "Back"));
                }
                _ = sb.Append("  }\n");
            }

            if (m_State.fogMode != FogMode.kFogUnknown ||
                m_State.fogColor.x.val != 0f ||
                m_State.fogColor.y.val != 0f ||
                m_State.fogColor.z.val != 0f ||
                m_State.fogColor.w.val != 0f ||
                m_State.fogDensity.val != 0f ||
                m_State.fogStart.val != 0f ||
                m_State.fogEnd.val != 0f) {
                _ = sb.Append("  Fog {\n");
                if (m_State.fogMode != FogMode.kFogUnknown) {
                    _ = sb.Append("   Mode ");
                    switch (m_State.fogMode) {
                        case FogMode.kFogDisabled:
                            _ = sb.Append("Off");
                            break;
                        case FogMode.kFogLinear:
                            _ = sb.Append("Linear");
                            break;
                        case FogMode.kFogExp:
                            _ = sb.Append("Exp");
                            break;
                        case FogMode.kFogExp2:
                            _ = sb.Append("Exp2");
                            break;
                    }
                    _ = sb.Append('\n');
                }
                if (m_State.fogColor.x.val != 0f ||
                    m_State.fogColor.y.val != 0f ||
                    m_State.fogColor.z.val != 0f ||
                    m_State.fogColor.w.val != 0f) {
                    _ = sb.AppendFormat("   Color ({0},{1},{2},{3})\n",
                        m_State.fogColor.x.val.ToString(CultureInfo.InvariantCulture),
                        m_State.fogColor.y.val.ToString(CultureInfo.InvariantCulture),
                        m_State.fogColor.z.val.ToString(CultureInfo.InvariantCulture),
                        m_State.fogColor.w.val.ToString(CultureInfo.InvariantCulture));
                }
                if (m_State.fogDensity.val != 0f) {
                    _ = sb.Append($"   Density {m_State.fogDensity.val.ToString(CultureInfo.InvariantCulture)}\n");
                }
                if (m_State.fogStart.val != 0f ||
                    m_State.fogEnd.val != 0f) {
                    _ = sb.Append($"   Range {m_State.fogStart.val.ToString(CultureInfo.InvariantCulture)}, {m_State.fogEnd.val.ToString(CultureInfo.InvariantCulture)}\n");
                }
                _ = sb.Append("  }\n");
            }

            if (m_State.lighting) {
                _ = sb.Append($"  Lighting {(m_State.lighting ? "On" : "Off")}\n");
            }

            _ = sb.Append($"  GpuProgramID {m_State.gpuProgramID}\n");

            return sb.ToString();
        }
    }
}
