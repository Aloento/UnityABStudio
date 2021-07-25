// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace SoarCraft.QYun.UnityABStudio.Core.Unity.CecilTools {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Mono.Cecil;

    public static class CecilUtils {
        public static MethodDefinition FindInTypeExplicitImplementationFor(MethodDefinition interfaceMethod, TypeDefinition typeDefinition) => typeDefinition.Methods.SingleOrDefault(m => m.Overrides.Any(o => o.CheckedResolve().SameAs(interfaceMethod)));

        public static IEnumerable<TypeDefinition> AllInterfacesImplementedBy(TypeDefinition typeDefinition) => TypeAndBaseTypesOf(typeDefinition).SelectMany(t => t.Interfaces).Select(i => i.InterfaceType.CheckedResolve()).Distinct();

        public static IEnumerable<TypeDefinition> TypeAndBaseTypesOf(TypeReference typeReference) {
            while (typeReference != null) {
                var typeDefinition = typeReference.CheckedResolve();
                yield return typeDefinition;
                typeReference = typeDefinition.BaseType;
            }
        }

        public static IEnumerable<TypeDefinition> BaseTypesOf(TypeReference typeReference) => TypeAndBaseTypesOf(typeReference).Skip(1);

        public static bool IsGenericList(TypeReference type) => type.Name == "List`1" && type.SafeNamespace() == "System.Collections.Generic";

        public static bool IsGenericDictionary(TypeReference type) {
            if (type is GenericInstanceType)
                type = ((GenericInstanceType)type).ElementType;

            return type.Name == "Dictionary`2" && type.SafeNamespace() == "System.Collections.Generic";
        }

        public static TypeReference ElementTypeOfCollection(TypeReference type) {
            var at = type as ArrayType;
            if (at != null)
                return at.ElementType;

            if (IsGenericList(type))
                return ((GenericInstanceType)type).GenericArguments.Single();

            throw new ArgumentException();
        }
    }
}
