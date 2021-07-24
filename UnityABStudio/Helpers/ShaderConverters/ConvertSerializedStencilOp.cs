namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System.Text;
    using AssetReader.Unity3D.Objects.Shaders;

    public static partial class ShaderConverter {
        private static string ConvertSerializedStencilOp(SerializedStencilOp stencilOp, string suffix) {
            var sb = new StringBuilder();
            _ = sb.Append($"   Comp{suffix} {ConvertStencilComp(stencilOp.comp)}\n");
            _ = sb.Append($"   Pass{suffix} {ConvertStencilOp(stencilOp.pass)}\n");
            _ = sb.Append($"   Fail{suffix} {ConvertStencilOp(stencilOp.fail)}\n");
            _ = sb.Append($"   ZFail{suffix} {ConvertStencilOp(stencilOp.zFail)}\n");
            return sb.ToString();
        }

        private static string ConvertStencilOp(SerializedShaderFloatValue op) => op.val switch {
            1f => "Zero",
            2f => "Replace",
            3f => "IncrSat",
            4f => "DecrSat",
            5f => "Invert",
            6f => "IncrWrap",
            7f => "DecrWrap",
            _ => "Keep",
        };

        private static string ConvertStencilComp(SerializedShaderFloatValue comp) => comp.val switch {
            0f => "Disabled",
            1f => "Never",
            2f => "Less",
            3f => "Equal",
            4f => "LEqual",
            5f => "Greater",
            6f => "NotEqual",
            7f => "GEqual",
            _ => "Always",
        };
    }
}
