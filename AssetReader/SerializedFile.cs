namespace SoarCraft.QYun.AssetReader {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Entities.Enums;
    using Entities.Structs;
    using Unity3D;
    using Utils;

    public class SerializedFile {
        public AssetsManager assetsManager;
        public UnityReader reader;
        public string fullName;
        public string originalPath;
        public string fileName;
        public int[] version = { 0, 0, 0, 0 };
        public BuildType buildType;
        public List<UObject> Objects;
        public Dictionary<long, UObject> ObjectsDic;

        public SerializedFileHeader header;
        private byte m_FileEndianess;
        public string unityVersion = "2.5.0f5";
        public BuildTarget m_TargetPlatform = BuildTarget.UnknownPlatform;
        private bool m_EnableTypeTree = true;
        public List<SerializedType> m_Types;
        public int bigIDEnabled;
        public List<ObjectInfo> m_Objects;
        private List<LocalSerializedObjectIdentifier> m_ScriptTypes;
        public List<FileIdentifier> m_Externals;
        public List<SerializedType> m_RefTypes;
        public string userInformation;

        public SerializedFile(UnityReader reader, AssetsManager assetsManager) {
            this.assetsManager = assetsManager;
            this.reader = reader;
            fullName = reader.FullPath;
            fileName = reader.FileName;

            // ReadHeader
            header = new SerializedFileHeader {
                m_MetadataSize = reader.ReadUInt32(),
                m_FileSize = reader.ReadUInt32(),
                m_Version = (SerializedFileFormatVersion)reader.ReadUInt32(),
                m_DataOffset = reader.ReadUInt32()
            };

            if (header.m_Version >= SerializedFileFormatVersion.kUnknown_9) {
                header.m_Endianess = reader.ReadByte();
                header.m_Reserved = reader.ReadBytes(3);
                m_FileEndianess = header.m_Endianess;
            } else {
                reader.Position = header.m_FileSize - header.m_MetadataSize;
                m_FileEndianess = reader.ReadByte();
            }

            if (header.m_Version >= SerializedFileFormatVersion.kLargeFilesSupport) {
                header.m_MetadataSize = reader.ReadUInt32();
                header.m_FileSize = reader.ReadInt64();
                header.m_DataOffset = reader.ReadInt64();
                _ = reader.ReadInt64(); // unknown
            }

            // ReadMetadata
            reader.IsBigEndian = this.m_FileEndianess != 0;
            if (header.m_Version >= SerializedFileFormatVersion.kUnknown_7) {
                unityVersion = reader.ReadStringToNull();
                SetVersion(unityVersion);
            }

            if (header.m_Version >= SerializedFileFormatVersion.kUnknown_8) {
                m_TargetPlatform = (BuildTarget)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(BuildTarget), m_TargetPlatform)) {
                    m_TargetPlatform = BuildTarget.UnknownPlatform;
                }
            }

            if (header.m_Version >= SerializedFileFormatVersion.kHasTypeTreeHashes) {
                m_EnableTypeTree = reader.ReadBoolean();
            }

            // Read Types
            var typeCount = reader.ReadInt32();
            m_Types = new List<SerializedType>(typeCount);
            for (var i = 0; i < typeCount; i++) {
                m_Types.Add(ReadSerializedType(false));
            }

            if (header.m_Version is >= SerializedFileFormatVersion.kUnknown_7 and < SerializedFileFormatVersion
                .kUnknown_14) {
                bigIDEnabled = reader.ReadInt32();
            }

            // Read Objects
            var objectCount = reader.ReadInt32();
            m_Objects = new List<ObjectInfo>(objectCount);
            Objects = new List<UObject>(objectCount);
            ObjectsDic = new Dictionary<long, UObject>(objectCount);
            for (var i = 0; i < objectCount; i++) {
                var objectInfo = new ObjectInfo();
                if (bigIDEnabled != 0) {
                    objectInfo.m_PathID = reader.ReadInt64();
                } else if (header.m_Version < SerializedFileFormatVersion.kUnknown_14) {
                    objectInfo.m_PathID = reader.ReadInt32();
                } else {
                    reader.AlignStream();
                    objectInfo.m_PathID = reader.ReadInt64();
                }

                objectInfo.byteStart = this.header.m_Version >= SerializedFileFormatVersion.kLargeFilesSupport
                    ? reader.ReadInt64()
                    : reader.ReadUInt32();

                objectInfo.byteStart += header.m_DataOffset;
                objectInfo.byteSize = reader.ReadUInt32();
                objectInfo.typeID = reader.ReadInt32();
                if (header.m_Version < SerializedFileFormatVersion.kRefactoredClassId) {
                    objectInfo.classID = reader.ReadUInt16();
                    objectInfo.serializedType = m_Types.Find(x => x.classID == objectInfo.typeID);
                } else {
                    var type = m_Types[objectInfo.typeID];
                    objectInfo.serializedType = type;
                    objectInfo.classID = type.classID;
                }

                switch (this.header.m_Version) {
                    case < SerializedFileFormatVersion.kHasScriptTypeIndex:
                        objectInfo.isDestroyed = reader.ReadUInt16();
                        break;
                    case >= SerializedFileFormatVersion.kHasScriptTypeIndex and < SerializedFileFormatVersion
                        .kRefactorTypeData: {
                        var m_ScriptTypeIndex = reader.ReadInt16();
                        if (objectInfo.serializedType != null)
                            objectInfo.serializedType.m_ScriptTypeIndex = m_ScriptTypeIndex;
                        break;
                    }
                }

                if (header.m_Version is SerializedFileFormatVersion.kSupportsStrippedObject or
                    SerializedFileFormatVersion.kRefactoredClassId) {
                    objectInfo.stripped = reader.ReadByte();
                }

                m_Objects.Add(objectInfo);
            }

            if (header.m_Version >= SerializedFileFormatVersion.kHasScriptTypeIndex) {
                var scriptCount = reader.ReadInt32();
                m_ScriptTypes = new List<LocalSerializedObjectIdentifier>(scriptCount);
                for (var i = 0; i < scriptCount; i++) {
                    var m_ScriptType = new LocalSerializedObjectIdentifier {
                        localSerializedFileIndex = reader.ReadInt32()
                    };
                    if (header.m_Version < SerializedFileFormatVersion.kUnknown_14) {
                        m_ScriptType.localIdentifierInFile = reader.ReadInt32();
                    } else {
                        reader.AlignStream();
                        m_ScriptType.localIdentifierInFile = reader.ReadInt64();
                    }

                    m_ScriptTypes.Add(m_ScriptType);
                }
            }

            var externalsCount = reader.ReadInt32();
            m_Externals = new List<FileIdentifier>(externalsCount);
            for (var i = 0; i < externalsCount; i++) {
                var m_External = new FileIdentifier();
                if (header.m_Version >= SerializedFileFormatVersion.kUnknown_6) {
                    var tempEmpty = reader.ReadStringToNull();
                }

                if (header.m_Version >= SerializedFileFormatVersion.kUnknown_5) {
                    m_External.guid = new Guid(reader.ReadBytes(16));
                    m_External.type = reader.ReadInt32();
                }

                m_External.pathName = reader.ReadStringToNull();
                m_External.fileName = Path.GetFileName(m_External.pathName);
                m_Externals.Add(m_External);
            }

            if (header.m_Version >= SerializedFileFormatVersion.kSupportsRefObject) {
                var refTypesCount = reader.ReadInt32();
                m_RefTypes = new List<SerializedType>(refTypesCount);
                for (var i = 0; i < refTypesCount; i++) {
                    m_RefTypes.Add(ReadSerializedType(true));
                }
            }

            if (header.m_Version >= SerializedFileFormatVersion.kUnknown_5) {
                userInformation = reader.ReadStringToNull();
            }

            //reader.AlignStream(16);
        }

        public void SetVersion(string stringVersion) {
            if (stringVersion != StrippedVersion) {
                unityVersion = stringVersion;
                var buildSplit = Regex.Replace(stringVersion, @"\d", "")
                    .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                buildType = new BuildType(buildSplit[0]);
                var versionSplit = Regex.Replace(stringVersion, @"\D", ".")
                    .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                version = versionSplit.Select(int.Parse).ToArray();
            }
        }

        private SerializedType ReadSerializedType(bool isRefType) {
            var type = new SerializedType {
                classID = reader.ReadInt32()
            };

            if (header.m_Version >= SerializedFileFormatVersion.kRefactoredClassId) {
                type.m_IsStrippedType = reader.ReadBoolean();
            }

            if (header.m_Version >= SerializedFileFormatVersion.kRefactorTypeData) {
                type.m_ScriptTypeIndex = reader.ReadInt16();
            }

            if (header.m_Version >= SerializedFileFormatVersion.kHasTypeTreeHashes) {
                if (isRefType && type.m_ScriptTypeIndex >= 0) {
                    type.m_ScriptID = reader.ReadBytes(16);
                } else if ((header.m_Version < SerializedFileFormatVersion.kRefactoredClassId && type.classID < 0) ||
                           (header.m_Version >= SerializedFileFormatVersion.kRefactoredClassId &&
                            type.classID == 114)) {
                    type.m_ScriptID = reader.ReadBytes(16);
                }

                type.m_OldTypeHash = reader.ReadBytes(16);
            }

            if (m_EnableTypeTree) {
                type.m_Type = new TypeTree {
                    m_Nodes = new List<TypeTreeNode>()
                };
                if (header.m_Version is >= SerializedFileFormatVersion.kUnknown_12 or SerializedFileFormatVersion
                    .kUnknown_10) {
                    TypeTreeBlobRead(type.m_Type);
                } else {
                    ReadTypeTree(type.m_Type);
                }

                if (header.m_Version >= SerializedFileFormatVersion.kStoresTypeDependencies) {
                    if (isRefType) {
                        type.m_KlassName = reader.ReadStringToNull();
                        type.m_NameSpace = reader.ReadStringToNull();
                        type.m_AsmName = reader.ReadStringToNull();
                    } else {
                        type.m_TypeDependencies = reader.ReadInt32Array();
                    }
                }
            }

            return type;
        }

        private void ReadTypeTree(TypeTree m_Type, int level = 0) {
            var typeTreeNode = new TypeTreeNode();
            m_Type.m_Nodes.Add(typeTreeNode);
            typeTreeNode.m_Level = level;
            typeTreeNode.m_Type = reader.ReadStringToNull();
            typeTreeNode.m_Name = reader.ReadStringToNull();
            typeTreeNode.m_ByteSize = reader.ReadInt32();
            if (header.m_Version == SerializedFileFormatVersion.kUnknown_2) {
                var variableCount = reader.ReadInt32();
            }

            if (header.m_Version != SerializedFileFormatVersion.kUnknown_3) {
                typeTreeNode.m_Index = reader.ReadInt32();
            }

            typeTreeNode.m_TypeFlags = reader.ReadInt32();
            typeTreeNode.m_Version = reader.ReadInt32();
            if (header.m_Version != SerializedFileFormatVersion.kUnknown_3) {
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }

            var childrenCount = reader.ReadInt32();
            for (var i = 0; i < childrenCount; i++) {
                ReadTypeTree(m_Type, level + 1);
            }
        }

        private void TypeTreeBlobRead(TypeTree m_Type) {
            var numberOfNodes = reader.ReadInt32();
            var stringBufferSize = reader.ReadInt32();
            for (var i = 0; i < numberOfNodes; i++) {
                var typeTreeNode = new TypeTreeNode();
                m_Type.m_Nodes.Add(typeTreeNode);
                typeTreeNode.m_Version = reader.ReadUInt16();
                typeTreeNode.m_Level = reader.ReadByte();
                typeTreeNode.m_TypeFlags = reader.ReadByte();
                typeTreeNode.m_TypeStrOffset = reader.ReadUInt32();
                typeTreeNode.m_NameStrOffset = reader.ReadUInt32();
                typeTreeNode.m_ByteSize = reader.ReadInt32();
                typeTreeNode.m_Index = reader.ReadInt32();
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
                if (header.m_Version >= SerializedFileFormatVersion.kTypeTreeNodeWithTypeFlags) {
                    typeTreeNode.m_RefTypeHash = reader.ReadUInt64();
                }
            }

            m_Type.m_StringBuffer = reader.ReadBytes(stringBufferSize);

            using (var stringBufferReader = new UnityReader(new MemoryStream(m_Type.m_StringBuffer))) {
                for (var i = 0; i < numberOfNodes; i++) {
                    var m_Node = m_Type.m_Nodes[i];
                    m_Node.m_Type = ReadString(stringBufferReader, m_Node.m_TypeStrOffset);
                    m_Node.m_Name = ReadString(stringBufferReader, m_Node.m_NameStrOffset);
                }
            }

            static string ReadString(UnityReader stringBufferReader, uint value) {
                var isOffset = (value & 0x80000000) == 0;
                if (isOffset) {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }

                var offset = value & 0x7FFFFFFF;
                return CommonString.StringBuffer.TryGetValue(offset, out var str) ? str : offset.ToString();
            }
        }

        public void AddObject(UObject obj) {
            Objects.Add(obj);
            ObjectsDic.Add(obj.m_PathID, obj);
        }

        ~SerializedFile() {
            try {
                _ = this.assetsManager.AssetsFileList.Remove(this);
                foreach (var (key, value) in this.assetsManager.resourceFileReaders.
                    Where(x => x.Key.Contains(this.fileName)).ToList()) {
                    value.Dispose();
                    _ = this.assetsManager.resourceFileReaders.Remove(key);
                }

                this.reader.Dispose();
                foreach (var obj in this.Objects) {
                    obj.reader.Dispose();
                    obj.reader = null;
                }

                this.Objects.Clear();
                this.ObjectsDic.Clear();
                this.m_Types.Clear();
                this.m_ScriptTypes.Clear();
                this.m_Externals.Clear();
                this.m_RefTypes.Clear();
            } catch (Exception) {
                // ignored
            }
        }

        public bool IsVersionStripped => unityVersion == StrippedVersion;

        private const string StrippedVersion = "0.0.0";
    }
}
