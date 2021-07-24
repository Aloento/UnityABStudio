namespace SoarCraft.QYun.UnityABStudio.Converters.ShaderConverters.SpirV {
    using System.Collections.Generic;
    using System.Text;

    public class Type {
        public virtual StringBuilder ToString(StringBuilder sb) {
            return sb;
        }
    }

    public class VoidType : Type {
        public override string ToString() {
            return "void";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return sb.Append("void");
        }
    }

    public class ScalarType : Type {
    }

    public class BoolType : ScalarType {
        public override string ToString() {
            return "bool";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return sb.Append("bool");
        }
    }

    public class IntegerType : ScalarType {
        public IntegerType(int width, bool signed) {
            this.Width = width;
            this.Signed = signed;
        }

        public override string ToString() {
            if (this.Signed) {
                return $"i{this.Width}";
            } else {
                return $"u{this.Width}";
            }
        }

        public override StringBuilder ToString(StringBuilder sb) {
            if (this.Signed) {
                sb.Append('i').Append(this.Width);
            } else {
                sb.Append('u').Append(this.Width);
            }
            return sb;
        }

        public int Width { get; }
        public bool Signed { get; }
    }

    public class FloatingPointType : ScalarType {
        public FloatingPointType(int width) {
            this.Width = width;
        }

        public override string ToString() {
            return $"f{this.Width}";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return sb.Append('f').Append(this.Width);
        }

        public int Width { get; }
    }

    public class VectorType : Type {
        public VectorType(ScalarType scalarType, int componentCount) {
            this.ComponentType = scalarType;
            this.ComponentCount = componentCount;
        }

        public override string ToString() {
            return $"{this.ComponentType}_{this.ComponentCount}";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return this.ComponentType.ToString(sb).Append('_').Append(this.ComponentCount);
        }

        public ScalarType ComponentType { get; }
        public int ComponentCount { get; }
    }

    public class MatrixType : Type {
        public MatrixType(VectorType vectorType, int columnCount) {
            this.ColumnType = vectorType;
            this.ColumnCount = columnCount;
        }

        public override string ToString() {
            return $"{this.ColumnType}x{this.ColumnCount}";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return sb.Append(this.ColumnType).Append('x').Append(this.ColumnCount);
        }

        public VectorType ColumnType { get; }
        public int ColumnCount { get; }
        public int RowCount => this.ColumnType.ComponentCount;
    }

    public class ImageType : Type {
        public ImageType(Type sampledType, Dim dim, int depth, bool isArray, bool isMultisampled, int sampleCount,
            ImageFormat imageFormat, AccessQualifier accessQualifier) {
            this.SampledType = sampledType;
            this.Dim = dim;
            this.Depth = depth;
            this.IsArray = isArray;
            this.IsMultisampled = isMultisampled;
            this.SampleCount = sampleCount;
            this.Format = imageFormat;
            this.AccessQualifier = accessQualifier;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            this.ToString(sb);
            return sb.ToString();
        }

        public override StringBuilder ToString(StringBuilder sb) {
            switch (this.AccessQualifier) {
                case AccessQualifier.ReadWrite:
                    sb.Append("read_write ");
                    break;
                case AccessQualifier.WriteOnly:
                    sb.Append("write_only ");
                    break;
                case AccessQualifier.ReadOnly:
                    sb.Append("read_only ");
                    break;
            }

            sb.Append("Texture");
            switch (this.Dim) {
                case Dim.Dim1D:
                    sb.Append("1D");
                    break;
                case Dim.Dim2D:
                    sb.Append("2D");
                    break;
                case Dim.Dim3D:
                    sb.Append("3D");
                    break;
                case Dim.Cube:
                    sb.Append("Cube");
                    break;
            }

            if (this.IsMultisampled) {
                sb.Append("MS");
            }
            if (this.IsArray) {
                sb.Append("Array");
            }
            return sb;
        }

        public Type SampledType { get; }
        public Dim Dim { get; }
        public int Depth { get; }
        public bool IsArray { get; }
        public bool IsMultisampled { get; }
        public int SampleCount { get; }
        public ImageFormat Format { get; }
        public AccessQualifier AccessQualifier { get; }
    }

    public class SamplerType : Type {
        public override string ToString() {
            return "sampler";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return sb.Append("sampler");
        }
    }

    public class SampledImageType : Type {
        public SampledImageType(ImageType imageType) {
            this.ImageType = imageType;
        }

        public override string ToString() {
            return $"{this.ImageType}Sampled";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return this.ImageType.ToString(sb).Append("Sampled");
        }

        public ImageType ImageType { get; }
    }

    public class ArrayType : Type {
        public ArrayType(Type elementType, int elementCount) {
            this.ElementType = elementType;
            this.ElementCount = elementCount;
        }

        public override string ToString() {
            return $"{this.ElementType}[{this.ElementCount}]";
        }

        public override StringBuilder ToString(StringBuilder sb) {
            return this.ElementType.ToString(sb).Append('[').Append(this.ElementCount).Append(']');
        }

        public int ElementCount { get; }
        public Type ElementType { get; }
    }

    public class RuntimeArrayType : Type {
        public RuntimeArrayType(Type elementType) {
            this.ElementType = elementType;
        }

        public Type ElementType { get; }
    }

    public class StructType : Type {
        public StructType(IReadOnlyList<Type> memberTypes) {
            this.MemberTypes = memberTypes;
            this.memberNames_ = new List<string>();

            for (int i = 0; i < memberTypes.Count; ++i) {
                this.memberNames_.Add(string.Empty);
            }
        }

        public void SetMemberName(uint member, string name) {
            this.memberNames_[(int)member] = name;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            this.ToString(sb);
            return sb.ToString();
        }

        public override StringBuilder ToString(StringBuilder sb) {
            sb.Append("struct {");
            for (int i = 0; i < this.MemberTypes.Count; ++i) {
                Type memberType = this.MemberTypes[i];
                memberType.ToString(sb);
                if (!string.IsNullOrEmpty(this.memberNames_[i])) {
                    sb.Append(' ');
                    sb.Append(this.MemberNames[i]);
                }

                sb.Append(';');
                if (i < (this.MemberTypes.Count - 1)) {
                    sb.Append(' ');
                }
            }
            sb.Append('}');
            return sb;
        }

        public IReadOnlyList<Type> MemberTypes { get; }
        public IReadOnlyList<string> MemberNames => this.memberNames_;

        private List<string> memberNames_;
    }

    public class OpaqueType : Type {
    }

    public class PointerType : Type {
        public PointerType(StorageClass storageClass, Type type) {
            this.StorageClass = storageClass;
            this.Type = type;
        }

        public PointerType(StorageClass storageClass) {
            this.StorageClass = storageClass;
        }

        public void ResolveForwardReference(Type t) {
            this.Type = t;
        }

        public override string ToString() {
            if (this.Type == null) {
                return $"{this.StorageClass} *";
            } else {
                return $"{this.StorageClass} {this.Type}*";
            }
        }

        public override StringBuilder ToString(StringBuilder sb) {
            sb.Append(this.StorageClass.ToString()).Append(' ');
            if (this.Type != null) {
                this.Type.ToString(sb);
            }
            sb.Append('*');
            return sb;
        }

        public StorageClass StorageClass { get; }
        public Type Type { get; private set; }
    }

    public class FunctionType : Type {
        public FunctionType(Type returnType, IReadOnlyList<Type> parameterTypes) {
            this.ReturnType = returnType;
            this.ParameterTypes = parameterTypes;
        }

        public Type ReturnType { get; }
        public IReadOnlyList<Type> ParameterTypes { get; }
    }

    public class EventType : Type {
    }

    public class DeviceEventType : Type {
    }

    public class ReserveIdType : Type {
    }

    public class QueueType : Type {
    }

    public class PipeType : Type {
    }

    public class PipeStorage : Type {
    }

    public class NamedBarrier : Type {
    }
}
