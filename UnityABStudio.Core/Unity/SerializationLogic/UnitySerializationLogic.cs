// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace SoarCraft.QYun.UnityABStudio.Core.Unity.SerializationLogic {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CecilTools;
    using CecilTools.Extensions;
    using Mono.Cecil;

    internal class GenericInstanceHolder {
        public int Count;
        public IGenericInstance GenericInstance;
    }

    public class TypeResolver {
        private readonly IGenericInstance _typeDefinitionContext;
        private readonly IGenericInstance _methodDefinitionContext;
        private readonly Dictionary<string, GenericInstanceHolder> _context = new();

        public TypeResolver() {
        }

        public TypeResolver(IGenericInstance typeDefinitionContext) => this._typeDefinitionContext = typeDefinitionContext;

        public TypeResolver(GenericInstanceMethod methodDefinitionContext) => this._methodDefinitionContext = methodDefinitionContext;

        public TypeResolver(IGenericInstance typeDefinitionContext, IGenericInstance methodDefinitionContext) {
            this._typeDefinitionContext = typeDefinitionContext;
            this._methodDefinitionContext = methodDefinitionContext;
        }

        public void Add(GenericInstanceType genericInstanceType) => this.Add(ElementTypeFor(genericInstanceType).FullName, genericInstanceType);

        public void Remove(GenericInstanceType genericInstanceType) => this.Remove(genericInstanceType.ElementType.FullName, genericInstanceType);

        public void Add(GenericInstanceMethod genericInstanceMethod) => this.Add(ElementTypeFor(genericInstanceMethod).FullName, genericInstanceMethod);

        private static MemberReference ElementTypeFor(TypeSpecification genericInstanceType) => genericInstanceType.ElementType;

        private static MemberReference ElementTypeFor(MethodSpecification genericInstanceMethod) => genericInstanceMethod.ElementMethod;

        public void Remove(GenericInstanceMethod genericInstanceMethod) => this.Remove(genericInstanceMethod.ElementMethod.FullName, genericInstanceMethod);

        public TypeReference Resolve(TypeReference typeReference) {
            switch (typeReference) {
                case GenericParameter genericParameter: {
                    var resolved = this.ResolveGenericParameter(genericParameter);
                    return genericParameter == resolved ? resolved : this.Resolve(resolved);
                }
                case ArrayType arrayType:
                    return new ArrayType(this.Resolve(arrayType.ElementType), arrayType.Rank);
                case PointerType pointerType:
                    return new PointerType(this.Resolve(pointerType.ElementType));
                case ByReferenceType byReferenceType:
                    return new ByReferenceType(this.Resolve(byReferenceType.ElementType));
                case GenericInstanceType genericInstanceType: {
                    var newGenericInstanceType = new GenericInstanceType(this.Resolve(genericInstanceType.ElementType));
                    foreach (var genericArgument in genericInstanceType.GenericArguments)
                        newGenericInstanceType.GenericArguments.Add(this.Resolve(genericArgument));
                    return newGenericInstanceType;
                }
                case PinnedType pinnedType:
                    return new PinnedType(this.Resolve(pinnedType.ElementType));
                case RequiredModifierType reqModifierType:
                    return this.Resolve(reqModifierType.ElementType);
                case OptionalModifierType optModifierType:
                    return new OptionalModifierType(this.Resolve(optModifierType.ModifierType), this.Resolve(optModifierType.ElementType));
                case SentinelType sentinelType:
                    return new SentinelType(this.Resolve(sentinelType.ElementType));
                case FunctionPointerType funcPtrType:
                    throw new NotSupportedException("Function pointer types are not supported by the SerializationWeaver");
                case TypeSpecification:
                    throw new NotSupportedException();
                default:
                    return typeReference;
            }
        }

        private TypeReference ResolveGenericParameter(GenericParameter genericParameter) {
            if (genericParameter.Owner == null)
                throw new NotSupportedException();

            var memberReference = genericParameter.Owner as MemberReference;
            if (memberReference == null)
                throw new NotSupportedException();

            var key = memberReference.FullName;
            if (!this._context.ContainsKey(key)) {
                if (genericParameter.Type == GenericParameterType.Type) {
                    return this._typeDefinitionContext != null ? this._typeDefinitionContext.GenericArguments[genericParameter.Position] : genericParameter;
                }

                return this._methodDefinitionContext != null ? this._methodDefinitionContext.GenericArguments[genericParameter.Position] : genericParameter;
            }

            return this.GenericArgumentAt(key, genericParameter.Position);
        }

        private TypeReference GenericArgumentAt(string key, int position) => this._context[key].GenericInstance.GenericArguments[position];

        private void Add(string key, IGenericInstance value) {
            GenericInstanceHolder oldValue;

            if (this._context.TryGetValue(key, out oldValue)) {
                var memberReference = value as MemberReference;
                if (memberReference == null)
                    throw new NotSupportedException();

                var storedValue = (MemberReference)oldValue.GenericInstance;

                if (storedValue.FullName != memberReference.FullName)
                    throw new ArgumentException("Duplicate key!", "key");

                oldValue.Count++;
                return;
            }

            this._context.Add(key, new GenericInstanceHolder { Count = 1, GenericInstance = value });
        }

        private void Remove(string key, IGenericInstance value) {
            GenericInstanceHolder oldValue;

            if (this._context.TryGetValue(key, out oldValue)) {
                var memberReference = value as MemberReference;
                if (memberReference == null)
                    throw new NotSupportedException();

                var storedValue = (MemberReference)oldValue.GenericInstance;

                if (storedValue.FullName != memberReference.FullName)
                    throw new ArgumentException("Invalid value!", "value");

                oldValue.Count--;
                if (oldValue.Count == 0)
                    _ = this._context.Remove(key);

                return;
            }

            throw new ArgumentException("Invalid key!", "key");
        }
    }

    public static class UnitySerializationLogic {
        public static bool WillUnitySerialize(FieldDefinition fieldDefinition) => WillUnitySerialize(fieldDefinition, new TypeResolver(null));

        public static bool WillUnitySerialize(FieldDefinition fieldDefinition, TypeResolver typeResolver) {
            if (fieldDefinition == null)
                return false;

            //skip static, const and NotSerialized fields before even checking the type
            if (fieldDefinition.IsStatic || IsConst(fieldDefinition) || fieldDefinition.IsNotSerialized || fieldDefinition.IsInitOnly)
                return false;

            // The field must have correct visibility/decoration to be serialized.
            if (!fieldDefinition.IsPublic &&
                !ShouldHaveHadAllFieldsPublic(fieldDefinition) &&
                !HasSerializeFieldAttribute(fieldDefinition) &&
                !HasSerializeReferenceAttribute(fieldDefinition))
                return false;

            // Don't try to resolve types that come from Windows assembly,
            // as serialization weaver will fail to resolve that (due to it being in platform specific SDKs)
            if (ShouldNotTryToResolve(fieldDefinition.FieldType))
                return false;

            if (IsFixedBuffer(fieldDefinition))
                return true;

            // Resolving types is more complex and slower than checking their names or attributes,
            // thus keep those checks below
            var typeReference = typeResolver.Resolve(fieldDefinition.FieldType);

            //the type of the field must be serializable in the first place.

            if (typeReference.MetadataType == MetadataType.String)
                return true;

            if (typeReference.IsValueType)
                return IsValueTypeSerializable(typeReference);

            if (typeReference is ArrayType || CecilUtils.IsGenericList(typeReference)) {
                if (!HasSerializeReferenceAttribute(fieldDefinition))
                    return IsSupportedCollection(typeReference);
            }


            if (!IsReferenceTypeSerializable(typeReference) && !HasSerializeReferenceAttribute(fieldDefinition))
                return false;

            return !IsDelegate(typeReference);
        }

        private static bool IsDelegate(TypeReference typeReference) => typeReference.IsAssignableTo("System.Delegate");

        public static bool ShouldFieldBePPtrRemapped(FieldDefinition fieldDefinition) => ShouldFieldBePPtrRemapped(fieldDefinition, new TypeResolver(null));

        public static bool ShouldFieldBePPtrRemapped(FieldDefinition fieldDefinition, TypeResolver typeResolver) => WillUnitySerialize(fieldDefinition, typeResolver) && CanTypeContainUnityEngineObjectReference(typeResolver.Resolve(fieldDefinition.FieldType));

        private static bool CanTypeContainUnityEngineObjectReference(TypeReference typeReference) {
            while (true) {
                if (IsUnityEngineObject(typeReference))
                    return true;

                if (typeReference.IsEnum())
                    return false;

                if (IsSerializablePrimitive(typeReference))
                    return false;

                if (IsSupportedCollection(typeReference)) {
                    typeReference = CecilUtils.ElementTypeOfCollection(typeReference);
                    continue;
                }

                var definition = typeReference.Resolve();
                return definition != null && HasFieldsThatCanContainUnityEngineObjectReferences(definition, new TypeResolver(typeReference as GenericInstanceType));
            }
        }

        private static bool HasFieldsThatCanContainUnityEngineObjectReferences(TypeDefinition definition, TypeResolver typeResolver) => AllFieldsFor(definition, typeResolver).Where(kv => kv.Value.Resolve(kv.Key.FieldType).Resolve() != definition).Any(kv => CanFieldContainUnityEngineObjectReference(definition, kv.Key, kv.Value));

        private static IEnumerable<KeyValuePair<FieldDefinition, TypeResolver>> AllFieldsFor(TypeDefinition definition, TypeResolver typeResolver) {
            var baseType = definition.BaseType;

            if (baseType != null) {
                var genericBaseInstanceType = baseType as GenericInstanceType;
                if (genericBaseInstanceType != null)
                    typeResolver.Add(genericBaseInstanceType);
                foreach (var kv in AllFieldsFor(baseType.Resolve(), typeResolver))
                    yield return kv;
                if (genericBaseInstanceType != null)
                    typeResolver.Remove(genericBaseInstanceType);
            }

            foreach (var fieldDefinition in definition.Fields)
                yield return new KeyValuePair<FieldDefinition, TypeResolver>(fieldDefinition, typeResolver);
        }

        private static bool CanFieldContainUnityEngineObjectReference(TypeReference typeReference, FieldDefinition t, TypeResolver typeResolver) {
            if (typeResolver.Resolve(t.FieldType) == typeReference)
                return false;

            if (!WillUnitySerialize(t, typeResolver))
                return false;

            return !UnityEngineTypePredicates.IsUnityEngineValueType(typeReference);
        }

        private static bool IsConst(FieldDefinition fieldDefinition) => fieldDefinition.IsLiteral && !fieldDefinition.IsInitOnly;

        public static bool HasSerializeFieldAttribute(FieldDefinition field) =>
            //return FieldAttributes(field).Any(UnityEngineTypePredicates.IsSerializeFieldAttribute);
            FieldAttributes(field).Any(attribute => UnityEngineTypePredicates.IsSerializeFieldAttribute(attribute));

        public static bool HasSerializeReferenceAttribute(FieldDefinition field) => FieldAttributes(field).Any(attribute => UnityEngineTypePredicates.IsSerializeReferenceAttribute(attribute));

        private static IEnumerable<TypeReference> FieldAttributes(FieldDefinition field) => field.CustomAttributes.Select(_ => _.AttributeType);

        public static bool ShouldNotTryToResolve(TypeReference typeReference) {
            var typeReferenceScopeName = typeReference.Scope.Name;
            switch (typeReferenceScopeName) {
                case "Windows":
                    return true;
                case "mscorlib": {
                    var resolved = typeReference.Resolve();
                    return resolved == null;
                }
                default:
                    try {   // This will throw an exception if typereference thinks it's referencing a .dll,
                        // but actually there's .winmd file in the current directory. RRW will fix this
                        // at a later step, so we will not try to resolve this type. This is OK, as any
                        // type defined in a winmd cannot be serialized.
                        _ = typeReference.Resolve();
                    } catch {
                        return true;
                    }

                    return false;
            }
        }

        private static bool IsFieldTypeSerializable(TypeReference typeReference, FieldDefinition fieldDefinition) => IsTypeSerializable(typeReference) || IsSupportedCollection(typeReference) || IsFixedBuffer(fieldDefinition);

        private static bool IsValueTypeSerializable(TypeReference typeReference) {
            if (typeReference.IsPrimitive)
                return IsSerializablePrimitive(typeReference);
            return UnityEngineTypePredicates.IsSerializableUnityStruct(typeReference) ||
                typeReference.IsEnum() ||
                ShouldImplementIDeserializable(typeReference);
        }

        private static bool IsReferenceTypeSerializable(TypeReference typeReference) {
            if (typeReference.MetadataType == MetadataType.String)
                return IsSerializablePrimitive(typeReference);

            if (IsGenericDictionary(typeReference))
                return false;

            return IsUnityEngineObject(typeReference) ||
                   ShouldImplementIDeserializable(typeReference) ||
                   UnityEngineTypePredicates.IsSerializableUnityClass(typeReference);
        }

        private static bool IsTypeSerializable(TypeReference typeReference) {
            if (typeReference.MetadataType == MetadataType.String)
                return true;
            return typeReference.IsValueType ? IsValueTypeSerializable(typeReference) : IsReferenceTypeSerializable(typeReference);
        }

        private static bool IsGenericDictionary(TypeReference typeReference) {
            var current = typeReference;

            if (current != null) {
                if (CecilUtils.IsGenericDictionary(current))
                    return true;
            }

            return false;
        }

        public static bool IsFixedBuffer(FieldDefinition fieldDefinition) => GetFixedBufferAttribute(fieldDefinition) != null;

        public static CustomAttribute GetFixedBufferAttribute(FieldDefinition fieldDefinition) => !fieldDefinition.HasCustomAttributes ? null : fieldDefinition.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.FixedBufferAttribute");

        public static int GetFixedBufferLength(FieldDefinition fieldDefinition) {
            var fixedBufferAttribute = GetFixedBufferAttribute(fieldDefinition);

            if (fixedBufferAttribute == null)
                throw new ArgumentException(string.Format("Field '{0}' is not a fixed buffer field.", fieldDefinition.FullName));

            var size = (int)fixedBufferAttribute.ConstructorArguments[1].Value;

            return size;
        }

        public static int PrimitiveTypeSize(TypeReference type) => type.MetadataType switch {
            MetadataType.Boolean or MetadataType.Byte or MetadataType.SByte => 1,
            MetadataType.Char or MetadataType.Int16 or MetadataType.UInt16 => 2,
            MetadataType.Int32 or MetadataType.UInt32 or MetadataType.Single => 4,
            MetadataType.Int64 or MetadataType.UInt64 or MetadataType.Double => 8,
            _ => throw new ArgumentException(string.Format("Unsupported {0}", type.MetadataType)),
        };

        private static bool IsSerializablePrimitive(TypeReference typeReference) => typeReference.MetadataType switch {
            MetadataType.SByte or MetadataType.Byte or MetadataType.Char or MetadataType.Int16 or MetadataType.UInt16 or MetadataType.Int64 or MetadataType.UInt64 or MetadataType.Int32 or MetadataType.UInt32 or MetadataType.Single or MetadataType.Double or MetadataType.Boolean or MetadataType.String => true,
            _ => false,
        };

        public static bool IsSupportedCollection(TypeReference typeReference) {
            if (!(typeReference is ArrayType || CecilUtils.IsGenericList(typeReference)))
                return false;

            // We don't support arrays like byte[,] etc
            if (typeReference.IsArray && ((ArrayType)typeReference).Rank > 1)
                return false;

            return IsTypeSerializable(CecilUtils.ElementTypeOfCollection(typeReference));
        }

        private static bool ShouldHaveHadAllFieldsPublic(FieldDefinition field) => UnityEngineTypePredicates.IsUnityEngineValueType(field.DeclaringType);

        private static bool IsUnityEngineObject(TypeReference typeReference) => UnityEngineTypePredicates.IsUnityEngineObject(typeReference);

        public static bool IsNonSerialized(TypeReference typeDeclaration) {
            if (typeDeclaration == null)
                return true;
            if (typeDeclaration.HasGenericParameters)
                return true;
            if (typeDeclaration.MetadataType == MetadataType.Object)
                return true;
            var fullName = typeDeclaration.FullName;
            if (fullName.StartsWith("System.")) //can this be done better?
                return true;
            if (typeDeclaration.IsArray)
                return true;
            return typeDeclaration.FullName switch {
                UnityEngineTypePredicates.MonoBehaviour or UnityEngineTypePredicates.ScriptableObject => true,
                _ => typeDeclaration.IsEnum(),
            };
        }

        public static bool ShouldImplementIDeserializable(TypeReference typeDeclaration) {
            if (typeDeclaration.FullName == "UnityEngine.ExposedReference`1")
                return true;

            if (IsNonSerialized(typeDeclaration))
                return false;

            try {
                if (UnityEngineTypePredicates.ShouldHaveHadSerializableAttribute(typeDeclaration))
                    return true;

                var resolvedTypeDeclaration = typeDeclaration.CheckedResolve();
                if (resolvedTypeDeclaration.IsValueType) {
                    return resolvedTypeDeclaration.IsSerializable && !resolvedTypeDeclaration.CustomAttributes.Any(a => a.AttributeType.FullName.Contains("System.Runtime.CompilerServices.CompilerGenerated"));
                }

                return (resolvedTypeDeclaration.IsSerializable && !resolvedTypeDeclaration.CustomAttributes.Any(a => a.AttributeType.FullName.Contains("System.Runtime.CompilerServices.CompilerGenerated"))) ||
                       resolvedTypeDeclaration.IsSubclassOf(UnityEngineTypePredicates.MonoBehaviour, UnityEngineTypePredicates.ScriptableObject);
            } catch (Exception) {
                return false;
            }
        }
    }
}
