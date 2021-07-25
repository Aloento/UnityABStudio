// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace SoarCraft.QYun.UnityABStudio.Core.Unity.CecilTools.Extensions {
    using Mono.Cecil;

    static class MethodDefinitionExtensions {
        public static bool SameAs(this MethodDefinition self, MethodDefinition other) =>
            // FIXME: should be able to compare MethodDefinition references directly
            self.FullName == other.FullName;

        public static string PropertyName(this MethodDefinition self) => self.Name.Substring(4);

        public static bool IsConversionOperator(this MethodDefinition method) {
            if (!method.IsSpecialName)
                return false;

            return method.Name is "op_Implicit" or "op_Explicit";
        }

        public static bool IsSimpleSetter(this MethodDefinition original) => original.IsSetter && original.Parameters.Count == 1;

        public static bool IsSimpleGetter(this MethodDefinition original) => original.IsGetter && original.Parameters.Count == 0;

        public static bool IsSimplePropertyAccessor(this MethodDefinition method) => method.IsSimpleGetter() || method.IsSimpleSetter();

        public static bool IsDefaultConstructor(MethodDefinition m) => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0;
    }
}
