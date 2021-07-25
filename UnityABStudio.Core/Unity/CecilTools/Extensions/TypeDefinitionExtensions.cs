// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace SoarCraft.QYun.UnityABStudio.Core.Unity.CecilTools.Extensions {
    using Mono.Cecil;

    public static class TypeDefinitionExtensions {
        public static bool IsSubclassOf(this TypeDefinition type, string baseTypeName) {
            var baseType = type.BaseType;
            if (baseType == null)
                return false;
            if (baseType.FullName == baseTypeName)
                return true;

            var baseTypeDef = baseType.Resolve();
            return baseTypeDef != null && IsSubclassOf(baseTypeDef, baseTypeName);
        }

        public static bool IsSubclassOf(this TypeDefinition type, params string[] baseTypeNames) {
            var baseType = type.BaseType;
            if (baseType == null)
                return false;

            for (var i = 0; i < baseTypeNames.Length; i++)
                if (baseType.FullName == baseTypeNames[i])
                    return true;

            var baseTypeDef = baseType.Resolve();
            return baseTypeDef != null && IsSubclassOf(baseTypeDef, baseTypeNames);
        }
    }
}
