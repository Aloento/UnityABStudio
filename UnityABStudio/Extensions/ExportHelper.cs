namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AssetReader.Entities.Structs;
    using AssetReader.Unity3D.Objects;
    using Core.Models;
    using Helpers;

    public static partial class ExportExtension {
        public static string FixFileName(string str) => str.Length >= 260 ? Path.GetRandomFileName() : Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));

        private static bool TryExportFile(string dir, AssetItem item, string extension, out string fullPath) {
            var fileName = FixFileName(item.Name);
            fullPath = Path.Combine(dir, fileName + extension);
            if (!File.Exists(fullPath)) {
                _ = Directory.CreateDirectory(dir);
                return true;
            }
            fullPath = Path.Combine(dir, $"{fileName}#{item.BaseID}{extension}");
            if (!File.Exists(fullPath)) {
                _ = Directory.CreateDirectory(dir);
                return true;
            }
            return false;
        }

        public static TypeTree ConvertToTypeTree(this MonoBehaviour m_MonoBehaviour, AssemblyLoader assemblyLoader) {
            var m_Type = new TypeTree {
                m_Nodes = new List<TypeTreeNode>()
            };
            var helper = new SerializedTypeHelper(m_MonoBehaviour.version);
            helper.AddMonoBehaviour(m_Type.m_Nodes, 0);
            if (m_MonoBehaviour.m_Script.TryGet(out var m_Script)) {
                var typeDef = assemblyLoader.GetTypeDefinition(m_Script.m_AssemblyName, string.IsNullOrEmpty(m_Script.m_Namespace) ? m_Script.m_ClassName : $"{m_Script.m_Namespace}.{m_Script.m_ClassName}");
                if (typeDef != null) {
                    var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, helper, 1);
                    m_Type.m_Nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
                }
            }
            return m_Type;
        }
    }
}
