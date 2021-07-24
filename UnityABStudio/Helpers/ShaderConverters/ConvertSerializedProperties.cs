namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System;
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D.Objects.Shaders;

    public static partial class ShaderConverter {
        private static string ConvertSerializedProperties(SerializedProperties m_PropInfo) {
            var sb = new StringBuilder();
            _ = sb.Append("Properties {\n");
            foreach (var m_Prop in m_PropInfo.m_Props) {
                _ = sb.Append(ConvertSerializedProperty(m_Prop));
            }

            _ = sb.Append("}\n");
            return sb.ToString();
        }

        private static string ConvertSerializedProperty(SerializedProperty m_Prop) {
            var sb = new StringBuilder();
            foreach (var m_Attribute in m_Prop.m_Attributes) {
                _ = sb.Append($"[{m_Attribute}] ");
            }

            //TODO Flag
            _ = sb.Append($"{m_Prop.m_Name} (\"{m_Prop.m_Description}\", ");
            switch (m_Prop.m_Type) {
                case SerializedPropertyType.kColor:
                    _ = sb.Append("Color");
                    break;
                case SerializedPropertyType.kVector:
                    _ = sb.Append("Vector");
                    break;
                case SerializedPropertyType.kFloat:
                    _ = sb.Append("Float");
                    break;
                case SerializedPropertyType.kRange:
                    _ = sb.Append($"Range({m_Prop.m_DefValue[1]}, {m_Prop.m_DefValue[2]})");
                    break;
                case SerializedPropertyType.kTexture:
                    switch (m_Prop.m_DefTexture.m_TexDim) {
                        case TextureDimension.kTexDimAny:
                            _ = sb.Append("any");
                            break;
                        case TextureDimension.kTexDim2D:
                            _ = sb.Append("2D");
                            break;
                        case TextureDimension.kTexDim3D:
                            _ = sb.Append("3D");
                            break;
                        case TextureDimension.kTexDimCUBE:
                            _ = sb.Append("Cube");
                            break;
                        case TextureDimension.kTexDim2DArray:
                            _ = sb.Append("2DArray");
                            break;
                        case TextureDimension.kTexDimCubeArray:
                            _ = sb.Append("CubeArray");
                            break;
                    }

                    break;
            }

            _ = sb.Append(") = ");
            _ = m_Prop.m_Type switch {
                SerializedPropertyType.kColor or SerializedPropertyType.kVector => sb.Append(
                    $"({m_Prop.m_DefValue[0]},{m_Prop.m_DefValue[1]},{m_Prop.m_DefValue[2]},{m_Prop.m_DefValue[3]})"),
                SerializedPropertyType.kFloat or SerializedPropertyType.kRange => sb.Append(m_Prop.m_DefValue[0]),
                SerializedPropertyType.kTexture => sb.Append($"\"{m_Prop.m_DefTexture.m_DefaultName}\" {{ }}"),
                _ => throw new ArgumentOutOfRangeException(),
            };
            _ = sb.Append('\n');
            return sb.ToString();
        }
    }
}
