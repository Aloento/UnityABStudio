namespace SoarCraft.QYun.UnityABStudio.Core.Unity.SpirV {
    using System.Collections.Generic;
    using System.Text;

    public class Type {
        public virtual StringBuilder ToString(StringBuilder sb) => sb;
    }

    public class VoidType : Type {
        public override string ToString() => "void";

        public override StringBuilder ToString(StringBuilder sb) => sb.Append("void");
    }

    public class ScalarType : Type {
    }

    public class BoolType : ScalarType {
        public override string ToString() => "bool";

        public override StringBuilder ToString(StringBuilder sb) => sb.Append("bool");
    }

    public class IntegerType : ScalarType {
        public IntegerType(int width, bool signed) {
            this.Width = width;
            this.Signed = signed;
        }

        public override string ToString() => this.Signed ? $"i{this.Width}" : $"u{this.Width}";

        public override StringBuilder ToString(StringBuilder sb) {
            if (this.Signed) {
                _ = sb.Append('i').Append(this.Width);
            } else {
                _ = sb.Append('u').Append(this.Width);
            }
            return sb;
        }

        public int Width { get; }
        public bool Signed { get; }
    }

    public class FloatingPointType : ScalarType {
        public FloatingPointType(int width) => this.Width = width;

        public override string ToString() => $"f{this.Width}";

        public override StringBuilder ToString(StringBuilder sb) => sb.Append('f').Append(this.Width);

        public int Width { get; }
    }

    public class VectorType : Type {
        public VectorType(ScalarType scalarType, int componentCount) {
            this.ComponentType = scalarType;
            this.ComponentCount = componentCount;
        }

        public override string ToString() => $"{this.ComponentType}_{this.ComponentCount}";

        public override StringBuilder ToString(StringBuilder sb) => this.ComponentType.ToString(sb).Append('_').Append(this.ComponentCount);

        public ScalarType ComponentType { get; }
        public int ComponentCount { get; }
    }

    public class MatrixType : Type {
        public MatrixType(VectorType vectorType, int columnCount) {
            this.ColumnType = vectorType;
            this.ColumnCount = columnCount;
        }

        public override string ToString() => $"{this.ColumnType}x{this.ColumnCount}";

        public override StringBuilder ToString(StringBuilder sb) => sb.Append(this.ColumnType).Append('x').Append(this.ColumnCount);

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
            var sb = new StringBuilder();
            _ = this.ToString(sb);
            return sb.ToString();
        }

        public override StringBuilder ToString(StringBuilder sb) {
            switch (this.AccessQualifier) {
                case AccessQualifier.ReadWrite:
                    _ = sb.Append("read_write ");
                    break;
                case AccessQualifier.WriteOnly:
                    _ = sb.Append("write_only ");
                    break;
                case AccessQualifier.ReadOnly:
                    _ = sb.Append("read_only ");
                    break;
            }

            _ = sb.Append("Texture");
            switch (this.Dim) {
                case Dim.Dim1D:
                    _ = sb.Append("1D");
                    break;
                case Dim.Dim2D:
                    _ = sb.Append("2D");
                    break;
                case Dim.Dim3D:
                    _ = sb.Append("3D");
                    break;
                case Dim.Cube:
                    _ = sb.Append("Cube");
                    break;
            }

            if (this.IsMultisampled) {
                _ = sb.Append("MS");
            }
            if (this.IsArray) {
                _ = sb.Append("Array");
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
        public override string ToString() => "sampler";

        public override StringBuilder ToString(StringBuilder sb) => sb.Append("sampler");
    }

    public class SampledImageType : Type {
        public SampledImageType(ImageType imageType) => this.ImageType = imageType;

        public override string ToString() => $"{this.ImageType}Sampled";

        public override StringBuilder ToString(StringBuilder sb) => this.ImageType.ToString(sb).Append("Sampled");

        public ImageType ImageType { get; }
    }

    public class ArrayType : Type {
        public ArrayType(Type elementType, int elementCount) {
            this.ElementType = elementType;
            this.ElementCount = elementCount;
        }

        public override string ToString() => $"{this.ElementType}[{this.ElementCount}]";

        public override StringBuilder ToString(StringBuilder sb) => this.ElementType.ToString(sb).Append('[').Append(this.ElementCount).Append(']');

        public int ElementCount { get; }
        public Type ElementType { get; }
    }

    public class RuntimeArrayType : Type {
        public RuntimeArrayType(Type elementType) => this.ElementType = elementType;

        public Type ElementType { get; }
    }

    public class StructType : Type {
        public StructType(IReadOnlyList<Type> memberTypes) {
            this.MemberTypes = memberTypes;
            this.memberNames_ = new List<string>();

            for (var i = 0; i < memberTypes.Count; ++i) {
                this.memberNames_.Add(string.Empty);
            }
        }

        public void SetMemberName(uint member, string name) => this.memberNames_[(int)member] = name;

        public override string ToString() {
            var sb = new StringBuilder();
            _ = this.ToString(sb);
            return sb.ToString();
        }

        public override StringBuilder ToString(StringBuilder sb) {
            _ = sb.Append("struct {");
            for (var i = 0; i < this.MemberTypes.Count; ++i) {
                var memberType = this.MemberTypes[i];
                _ = memberType.ToString(sb);
                if (!string.IsNullOrEmpty(this.memberNames_[i])) {
                    _ = sb.Append(' ');
                    _ = sb.Append(this.MemberNames[i]);
                }

                _ = sb.Append(';');
                if (i < (this.MemberTypes.Count - 1)) {
                    _ = sb.Append(' ');
                }
            }
            _ = sb.Append('}');
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

        public PointerType(StorageClass storageClass) => this.StorageClass = storageClass;

        public void ResolveForwardReference(Type t) => this.Type = t;

        public override string ToString() => this.Type == null ? $"{this.StorageClass} *" : $"{this.StorageClass} {this.Type}*";

        public override StringBuilder ToString(StringBuilder sb) {
            _ = sb.Append(this.StorageClass.ToString()).Append(' ');
            if (this.Type != null) {
                _ = this.Type.ToString(sb);
            }
            _ = sb.Append('*');
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
