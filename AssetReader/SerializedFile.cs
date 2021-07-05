namespace SoarCraft.QYun.AssetReader {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Utils;

    public class SerializedFile {
        public AssetsManager AssetsManager;
        public EndianBinaryReader Reader;
        public string FullName;
        public string OriginalPath;
        public string FileName;
        public int[] Version = { 0, 0, 0, 0 };
        public BuildType BuildType;
        public List<Object> Objects;
        public Dictionary<long, Object> ObjectsDic;

        public SerializedFileHeader Header;
        private byte mFileEndianess;
        public string UnityVersion = "2.5.0f5";
        public BuildTarget MTargetPlatform = BuildTarget.UnknownPlatform;
        private bool mEnableTypeTree = true;
        public List<SerializedType> MTypes;
        public int BigIdEnabled = 0;
        public List<ObjectInfo> MObjects;
        private List<LocalSerializedObjectIdentifier> mScriptTypes;
        public List<FileIdentifier> MExternals;
        public List<SerializedType> MRefTypes;
        public string UserInformation;

        public SerializedFile(AssetsManager assetsManager, string fullName, EndianBinaryReader reader) {
            this.AssetsManager = assetsManager;
            this.Reader = reader;
            this.FullName = fullName;
            this.FileName = Path.GetFileName(fullName);

            // ReadHeader
            this.Header = new SerializedFileHeader();
            this.Header.m_MetadataSize = reader.ReadUInt32();
            this.Header.m_FileSize = reader.ReadUInt32();
            this.Header.m_Version = (SerializedFileFormatVersion)reader.ReadUInt32();
            this.Header.m_DataOffset = reader.ReadUInt32();

            if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_9) {
                this.Header.m_Endianess = reader.ReadByte();
                this.Header.m_Reserved = reader.ReadBytes(3);
                this.mFileEndianess = this.Header.m_Endianess;
            } else {
                reader.Position = this.Header.m_FileSize - this.Header.m_MetadataSize;
                this.mFileEndianess = reader.ReadByte();
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kLargeFilesSupport) {
                this.Header.m_MetadataSize = reader.ReadUInt32();
                this.Header.m_FileSize = reader.ReadInt64();
                this.Header.m_DataOffset = reader.ReadInt64();
                reader.ReadInt64(); // unknown
            }

            // ReadMetadata
            if (this.mFileEndianess == 0) {
                reader.endian = EndianType.LittleEndian;
            }
            if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_7) {
                this.UnityVersion = reader.ReadStringToNull();
                this.SetVersion(this.UnityVersion);
            }
            if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_8) {
                this.MTargetPlatform = (BuildTarget)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(BuildTarget), this.MTargetPlatform)) {
                    this.MTargetPlatform = BuildTarget.UnknownPlatform;
                }
            }
            if (this.Header.m_Version >= SerializedFileFormatVersion.kHasTypeTreeHashes) {
                this.mEnableTypeTree = reader.ReadBoolean();
            }

            // Read Types
            var typeCount = reader.ReadInt32();
            this.MTypes = new List<SerializedType>(typeCount);
            for (var i = 0; i < typeCount; i++) {
                this.MTypes.Add(this.ReadSerializedType(false));
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_7 && this.Header.m_Version < SerializedFileFormatVersion.kUnknown_14) {
                this.BigIdEnabled = reader.ReadInt32();
            }

            // Read Objects
            var objectCount = reader.ReadInt32();
            this.MObjects = new List<ObjectInfo>(objectCount);
            this.Objects = new List<Object>(objectCount);
            this.ObjectsDic = new Dictionary<long, Object>(objectCount);
            for (var i = 0; i < objectCount; i++) {
                var objectInfo = new ObjectInfo();
                if (this.BigIdEnabled != 0) {
                    objectInfo.m_PathID = reader.ReadInt64();
                } else if (this.Header.m_Version < SerializedFileFormatVersion.kUnknown_14) {
                    objectInfo.m_PathID = reader.ReadInt32();
                } else {
                    reader.AlignStream();
                    objectInfo.m_PathID = reader.ReadInt64();
                }

                if (this.Header.m_Version >= SerializedFileFormatVersion.kLargeFilesSupport)
                    objectInfo.byteStart = reader.ReadInt64();
                else
                    objectInfo.byteStart = reader.ReadUInt32();

                objectInfo.byteStart += this.Header.m_DataOffset;
                objectInfo.byteSize = reader.ReadUInt32();
                objectInfo.typeID = reader.ReadInt32();
                if (this.Header.m_Version < SerializedFileFormatVersion.kRefactoredClassId) {
                    objectInfo.classID = reader.ReadUInt16();
                    objectInfo.serializedType = this.MTypes.Find(x => x.ClassId == objectInfo.typeID);
                } else {
                    var type = this.MTypes[objectInfo.typeID];
                    objectInfo.serializedType = type;
                    objectInfo.classID = type.ClassId;
                }
                if (this.Header.m_Version < SerializedFileFormatVersion.kHasScriptTypeIndex) {
                    objectInfo.isDestroyed = reader.ReadUInt16();
                }
                if (this.Header.m_Version >= SerializedFileFormatVersion.kHasScriptTypeIndex && this.Header.m_Version < SerializedFileFormatVersion.kRefactorTypeData) {
                    var m_ScriptTypeIndex = reader.ReadInt16();
                    if (objectInfo.serializedType != null)
                        objectInfo.serializedType.m_ScriptTypeIndex = m_ScriptTypeIndex;
                }
                if (this.Header.m_Version == SerializedFileFormatVersion.kSupportsStrippedObject || this.Header.m_Version == SerializedFileFormatVersion.kRefactoredClassId) {
                    objectInfo.stripped = reader.ReadByte();
                }
                this.MObjects.Add(objectInfo);
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kHasScriptTypeIndex) {
                var scriptCount = reader.ReadInt32();
                this.mScriptTypes = new List<LocalSerializedObjectIdentifier>(scriptCount);
                for (var i = 0; i < scriptCount; i++) {
                    var m_ScriptType = new LocalSerializedObjectIdentifier();
                    m_ScriptType.localSerializedFileIndex = reader.ReadInt32();
                    if (this.Header.m_Version < SerializedFileFormatVersion.kUnknown_14) {
                        m_ScriptType.localIdentifierInFile = reader.ReadInt32();
                    } else {
                        reader.AlignStream();
                        m_ScriptType.localIdentifierInFile = reader.ReadInt64();
                    }
                    this.mScriptTypes.Add(m_ScriptType);
                }
            }

            var externalsCount = reader.ReadInt32();
            this.MExternals = new List<FileIdentifier>(externalsCount);
            for (var i = 0; i < externalsCount; i++) {
                var m_External = new FileIdentifier();
                if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_6) {
                    var tempEmpty = reader.ReadStringToNull();
                }
                if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_5) {
                    m_External.guid = new Guid(reader.ReadBytes(16));
                    m_External.type = reader.ReadInt32();
                }
                m_External.pathName = reader.ReadStringToNull();
                m_External.fileName = Path.GetFileName(m_External.pathName);
                this.MExternals.Add(m_External);
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kSupportsRefObject) {
                var refTypesCount = reader.ReadInt32();
                this.MRefTypes = new List<SerializedType>(refTypesCount);
                for (var i = 0; i < refTypesCount; i++) {
                    this.MRefTypes.Add(this.ReadSerializedType(true));
                }
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_5) {
                this.UserInformation = reader.ReadStringToNull();
            }
        }

        public void SetVersion(string stringVersion) {
            this.UnityVersion = stringVersion;
            var buildSplit = Regex.Replace(stringVersion, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            this.BuildType = new BuildType(buildSplit[0]);
            var versionSplit = Regex.Replace(stringVersion, @"\D", ".").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            this.Version = versionSplit.Select(int.Parse).ToArray();
        }

        private SerializedType ReadSerializedType(bool isRefType) {
            var type = new SerializedType();

            type.ClassId = this.Reader.ReadInt32();

            if (this.Header.m_Version >= SerializedFileFormatVersion.kRefactoredClassId) {
                type.MIsStrippedType = this.Reader.ReadBoolean();
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kRefactorTypeData) {
                type.MScriptTypeIndex = this.Reader.ReadInt16();
            }

            if (this.Header.m_Version >= SerializedFileFormatVersion.kHasTypeTreeHashes) {
                if (isRefType && type.MScriptTypeIndex >= 0) {
                    type.MScriptId = this.Reader.ReadBytes(16);
                } else if ((this.Header.m_Version < SerializedFileFormatVersion.kRefactoredClassId && type.ClassId < 0) || (this.Header.m_Version >= SerializedFileFormatVersion.kRefactoredClassId && type.ClassId == 114)) {
                    type.MScriptId = this.Reader.ReadBytes(16);
                }
                type.MOldTypeHash = this.Reader.ReadBytes(16);
            }

            if (this.mEnableTypeTree) {
                type.MType = new TypeTree();
                type.MType.m_Nodes = new List<TypeTreeNode>();
                if (this.Header.m_Version >= SerializedFileFormatVersion.kUnknown_12 || this.Header.m_Version == SerializedFileFormatVersion.kUnknown_10) {
                    TypeTreeBlobRead(type.MType);
                } else {
                    ReadTypeTree(type.MType);
                }
                if (this.Header.m_Version >= SerializedFileFormatVersion.kStoresTypeDependencies) {
                    if (isRefType) {
                        type.MKlassName = this.Reader.ReadStringToNull();
                        type.MNameSpace = this.Reader.ReadStringToNull();
                        type.MAsmName = this.Reader.ReadStringToNull();
                    } else {
                        type.MTypeDependencies = this.Reader.ReadInt32Array();
                    }
                }
            }

            return type;
        }

        private void ReadTypeTree(TypeTree mType, int level = 0) {
            var typeTreeNode = new TypeTreeNode();
            mType.m_Nodes.Add(typeTreeNode);
            typeTreeNode.m_Level = level;
            typeTreeNode.m_Type = this.Reader.ReadStringToNull();
            typeTreeNode.m_Name = this.Reader.ReadStringToNull();
            typeTreeNode.m_ByteSize = this.Reader.ReadInt32();
            if (this.Header.m_Version == SerializedFileFormatVersion.kUnknown_2) {
                var variableCount = this.Reader.ReadInt32();
            }
            if (this.Header.m_Version != SerializedFileFormatVersion.kUnknown_3) {
                typeTreeNode.m_Index = this.Reader.ReadInt32();
            }
            typeTreeNode.m_TypeFlags = this.Reader.ReadInt32();
            typeTreeNode.m_Version = this.Reader.ReadInt32();
            if (this.Header.m_Version != SerializedFileFormatVersion.kUnknown_3) {
                typeTreeNode.m_MetaFlag = this.Reader.ReadInt32();
            }

            var childrenCount = this.Reader.ReadInt32();
            for (var i = 0; i < childrenCount; i++) {
                this.ReadTypeTree(mType, level + 1);
            }
        }

        private void TypeTreeBlobRead(TypeTree mType) {
            var numberOfNodes = this.Reader.ReadInt32();
            var stringBufferSize = this.Reader.ReadInt32();
            for (var i = 0; i < numberOfNodes; i++) {
                var typeTreeNode = new TypeTreeNode();
                mType.m_Nodes.Add(typeTreeNode);
                typeTreeNode.m_Version = this.Reader.ReadUInt16();
                typeTreeNode.m_Level = this.Reader.ReadByte();
                typeTreeNode.m_TypeFlags = this.Reader.ReadByte();
                typeTreeNode.m_TypeStrOffset = this.Reader.ReadUInt32();
                typeTreeNode.m_NameStrOffset = this.Reader.ReadUInt32();
                typeTreeNode.m_ByteSize = this.Reader.ReadInt32();
                typeTreeNode.m_Index = this.Reader.ReadInt32();
                typeTreeNode.m_MetaFlag = this.Reader.ReadInt32();
                if (this.Header.m_Version >= SerializedFileFormatVersion.kTypeTreeNodeWithTypeFlags) {
                    typeTreeNode.m_RefTypeHash = this.Reader.ReadUInt64();
                }
            }
            mType.m_StringBuffer = this.Reader.ReadBytes(stringBufferSize);

            using (var stringBufferReader = new BinaryReader(new MemoryStream(mType.m_StringBuffer))) {
                for (var i = 0; i < numberOfNodes; i++) {
                    var m_Node = mType.m_Nodes[i];
                    m_Node.m_Type = ReadString(stringBufferReader, m_Node.m_TypeStrOffset);
                    m_Node.m_Name = ReadString(stringBufferReader, m_Node.m_NameStrOffset);
                }
            }

            string ReadString(BinaryReader stringBufferReader, uint value) {
                var isOffset = (value & 0x80000000) == 0;
                if (isOffset) {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }
                var offset = value & 0x7FFFFFFF;
                if (CommonString.StringBuffer.TryGetValue(offset, out var str)) {
                    return str;
                }
                return offset.ToString();
            }
        }

        public void AddObject(Object obj) {
            this.Objects.Add(obj);
            this.ObjectsDic.Add(obj.m_PathID, obj);
        }

        public static bool IsSerializedFile(EndianBinaryReader reader) {
            var fileSize = reader.BaseStream.Length;
            if (fileSize < 20) {
                return false;
            }
            var m_MetadataSize = reader.ReadUInt32();
            long m_FileSize = reader.ReadUInt32();
            var m_Version = reader.ReadUInt32();
            long m_DataOffset = reader.ReadUInt32();
            var m_Endianess = reader.ReadByte();
            var m_Reserved = reader.ReadBytes(3);
            if (m_Version >= 22) {
                if (fileSize < 48) {
                    return false;
                }
                m_MetadataSize = reader.ReadUInt32();
                m_FileSize = reader.ReadInt64();
                m_DataOffset = reader.ReadInt64();
            }
            if (m_FileSize != fileSize) {
                reader.Position = 0;
                return false;
            }
            if (m_DataOffset > fileSize) {
                reader.Position = 0;
                return false;
            }
            reader.Position = 0;
            return true;
        }
    }
}
