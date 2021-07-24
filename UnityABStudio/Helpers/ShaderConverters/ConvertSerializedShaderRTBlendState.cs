namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System.Text;
    using AssetReader.Unity3D.Objects.Shaders;

    public static partial class ShaderConverter {
        private static string ConvertSerializedShaderRTBlendState(SerializedShaderRTBlendState[] rtBlend, bool rtSeparateBlend) {
            var sb = new StringBuilder();
            for (var i = 0; i < rtBlend.Length; i++) {
                var blend = rtBlend[i];
                if (blend.srcBlend.val != 1f ||
                    blend.destBlend.val != 0f ||
                    blend.srcBlendAlpha.val != 1f ||
                    blend.destBlendAlpha.val != 0f) {
                    _ = sb.Append("  Blend ");
                    if (i != 0 || rtSeparateBlend) {
                        _ = sb.Append($"{i} ");
                    }
                    _ = sb.Append($"{ConvertBlendFactor(blend.srcBlend)} {ConvertBlendFactor(blend.destBlend)}");
                    if (blend.srcBlendAlpha.val != 1f ||
                        blend.destBlendAlpha.val != 0f) {
                        _ = sb.Append($", {ConvertBlendFactor(blend.srcBlendAlpha)} {ConvertBlendFactor(blend.destBlendAlpha)}");
                    }
                    _ = sb.Append('\n');
                }

                if (blend.blendOp.val != 0f ||
                    blend.blendOpAlpha.val != 0f) {
                    _ = sb.Append("  BlendOp ");
                    if (i != 0 || rtSeparateBlend) {
                        _ = sb.Append($"{i} ");
                    }
                    _ = sb.Append(ConvertBlendOp(blend.blendOp));
                    if (blend.blendOpAlpha.val != 0f) {
                        _ = sb.Append($", {ConvertBlendOp(blend.blendOpAlpha)}");
                    }
                    _ = sb.Append('\n');
                }

                var val = (int)blend.colMask.val;
                if (val != 0xf) {
                    _ = sb.Append("  ColorMask ");
                    if (val == 0) {
                        _ = sb.Append(0);
                    } else {
                        if ((val & 0x2) != 0) {
                            _ = sb.Append('R');
                        }
                        if ((val & 0x4) != 0) {
                            _ = sb.Append('G');
                        }
                        if ((val & 0x8) != 0) {
                            _ = sb.Append('B');
                        }
                        if ((val & 0x1) != 0) {
                            _ = sb.Append('A');
                        }
                    }
                    _ = sb.Append($" {i}\n");
                }
            }
            return sb.ToString();
        }

        private static string ConvertBlendOp(SerializedShaderFloatValue op) => op.val switch {
            1f => "Sub",
            2f => "RevSub",
            3f => "Min",
            4f => "Max",
            5f => "LogicalClear",
            6f => "LogicalSet",
            7f => "LogicalCopy",
            8f => "LogicalCopyInverted",
            9f => "LogicalNoop",
            10f => "LogicalInvert",
            11f => "LogicalAnd",
            12f => "LogicalNand",
            13f => "LogicalOr",
            14f => "LogicalNor",
            15f => "LogicalXor",
            16f => "LogicalEquiv",
            17f => "LogicalAndReverse",
            18f => "LogicalAndInverted",
            19f => "LogicalOrReverse",
            20f => "LogicalOrInverted",
            _ => "Add",
        };

        private static string ConvertBlendFactor(SerializedShaderFloatValue factor) => factor.val switch {
            0f => "Zero",
            2f => "DstColor",
            3f => "SrcColor",
            4f => "OneMinusDstColor",
            5f => "SrcAlpha",
            6f => "OneMinusSrcColor",
            7f => "DstAlpha",
            8f => "OneMinusDstAlpha",
            9f => "SrcAlphaSaturate",
            10f => "OneMinusSrcAlpha",
            _ => "One",
        };
    }
}
