namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using Utils;
    using Entities.Enums;
    using Entities.Structs;
    using System;

    public sealed class PPtr<T> where T : UObject {
        public int m_FileID;
        public long m_PathID;

        private SerializedFile assetsFile;
        private int index = -2; //-2 - Prepare, -1 - Missing

        public PPtr(ObjectReader reader) {
            m_FileID = reader.ReadInt32();
            m_PathID = reader.m_Version < SerializedFileFormatVersion.kUnknown_14 ? reader.ReadInt32() : reader.ReadInt64();
            assetsFile = reader.assetsFile;
        }

        private bool TryGetAssetsFile(out SerializedFile result) {
            result = null;
            switch (this.m_FileID) {
                case 0:
                    result = this.assetsFile;
                    return true;
                case > 0 when this.m_FileID - 1 < this.assetsFile.m_Externals.Count: {
                    var assetsManager = this.assetsFile.assetsManager;
                    var assetsFileList = assetsManager.assetsFileList;
                    var assetsFileIndexCache = assetsManager.assetsFileIndexCache;

                    if (this.index == -2) {
                        var m_External = this.assetsFile.m_Externals[this.m_FileID - 1];
                        var name = m_External.fileName;
                        if (!assetsFileIndexCache.TryGetValue(name, out this.index)) {
                            this.index = assetsFileList.FindIndex(x => x.fileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                            assetsFileIndexCache.Add(name, this.index);
                        }
                    }

                    if (this.index >= 0) {
                        result = assetsFileList[this.index];
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public bool TryGet(out T result) {
            if (TryGetAssetsFile(out var sourceFile)) {
                if (sourceFile.ObjectsDic.TryGetValue(m_PathID, out var obj)) {
                    if (obj is T variable) {
                        result = variable;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public bool TryGet<T2>(out T2 result) where T2 : UObject {
            if (TryGetAssetsFile(out var sourceFile)) {
                if (sourceFile.ObjectsDic.TryGetValue(m_PathID, out var obj)) {
                    if (obj is T2 variable) {
                        result = variable;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public void Set(T m_Object) {
            var name = m_Object.assetsFile.fileName;
            if (string.Equals(assetsFile.fileName, name, StringComparison.OrdinalIgnoreCase)) {
                m_FileID = 0;
            } else {
                m_FileID = assetsFile.m_Externals.FindIndex(x => string.Equals(x.fileName, name, StringComparison.OrdinalIgnoreCase));
                if (m_FileID == -1) {
                    assetsFile.m_Externals.Add(new FileIdentifier {
                        fileName = m_Object.assetsFile.fileName
                    });
                    m_FileID = assetsFile.m_Externals.Count;
                } else {
                    m_FileID += 1;
                }
            }

            var assetsManager = assetsFile.assetsManager;
            var assetsFileList = assetsManager.assetsFileList;
            var assetsFileIndexCache = assetsManager.assetsFileIndexCache;

            if (!assetsFileIndexCache.TryGetValue(name, out index)) {
                index = assetsFileList.FindIndex(x => x.fileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                assetsFileIndexCache.Add(name, index);
            }

            m_PathID = m_Object.m_PathID;
        }

        public bool IsNull => m_PathID == 0 || m_FileID < 0;
    }
}
