// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace SoarCraft.QYun.UnityABStudio.Core.Unity.CecilTools.Extensions {
    using Mono.Cecil;

    public static class TypeReferenceExtensions {
        public static string SafeNamespace(this TypeReference type) {
            while (true) {
                if (type.IsGenericInstance) {
                    type = ((GenericInstanceType)type).ElementType;
                    continue;
                }

                if (type.IsNested) {
                    type = type.DeclaringType;
                    continue;
                }

                return type.Namespace;
            }
        }

        public static bool IsAssignableTo(this TypeReference typeRef, string typeName) {
            try {
                if (typeRef.IsGenericInstance)
                    return ElementType.For(typeRef).IsAssignableTo(typeName);

                return typeRef.FullName == typeName || typeRef.CheckedResolve().IsSubclassOf(typeName);
            } catch (AssemblyResolutionException) { // If we can't resolve our typeref or one of its base types,
                // let's assume it is not assignable to our target type
                return false;
            }
        }

        public static bool IsEnum(this TypeReference type) =>
            type.IsValueType && !type.IsPrimitive && type.CheckedResolve().IsEnum;

        public static bool IsStruct(this TypeReference type) =>
            type.IsValueType && !type.IsPrimitive && !type.IsEnum() && !IsSystemDecimal(type);

        private static bool IsSystemDecimal(TypeReference type) => type.FullName == "System.Decimal";
    }
}
