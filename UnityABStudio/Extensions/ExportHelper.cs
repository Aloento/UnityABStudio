namespace SoarCraft.QYun.UnityABStudio.Extensions {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AssetReader.Entities.Structs;
    using AssetReader.Unity3D.Objects;
    using Converters;
    using Core.Helpers;
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

        private static void ExportFbx(IImported convert, string exportPath) {
            var eulerFilter = settings.EulerFilter;
            var filterPrecision = (float)settings.FilterPrecision;
            var exportAllNodes = settings.ExportAllNodes;
            var exportSkins = settings.ExportSkins;
            var exportAnimations = settings.ExportAnimations;
            var exportBlendShape = settings.ExportBlendShape;
            var castToBone = settings.CastToBone;
            var boneSize = (int)settings.BoneSize;
            var exportAllUvsAsDiffuseMaps = settings.ExportAllUvsAsDiffuseMaps;
            var scaleFactor = (float)settings.ScaleFactor;
            var fbxVersion = settings.FbxVersion;
            var fbxFormat = settings.FbxFormat;
            ModelExporter.ExportFbx(exportPath, convert, eulerFilter, filterPrecision,
                exportAllNodes, exportSkins, exportAnimations, exportBlendShape, castToBone, boneSize, exportAllUvsAsDiffuseMaps, scaleFactor, fbxVersion, fbxFormat == 1);
        }
    }
}
