namespace SoarCraft.QYun.AssetReader {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Entities.Enums;
    using Utils;
    using static Helpers.ImportHelper;

    public class AssetsManager {
        public readonly List<SerializedFile> AssetFileList = new();

        internal Dictionary<string, int> AssetsFileIndexCache = new(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, BinaryReader> ResourceFileReaders = new(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> importFiles = new();
        private readonly HashSet<string> importFilesHash = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> assetsFileListHash = new(StringComparer.OrdinalIgnoreCase);

        public void LoadFiles(params string[] files) {
            MergeSplitAssets(Path.GetDirectoryName(files[0]));
            this.Load(ProcessingSplitFiles(files.ToList()));
        }

        public void LoadFolder(string path) {
            MergeSplitAssets(path, true);
            this.Load(ProcessingSplitFiles(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList()));
        }

        private void Load(IEnumerable<string> files) {
            foreach (var file in files) {
                this.importFiles.Add(file);
                _ = this.importFilesHash.Add(Path.GetFileName(file));
            }

            foreach (var file in this.importFiles) {
                this.LoadFile(file);
            }

            this.importFiles.Clear();
            this.importFilesHash.Clear();
            this.assetsFileListHash.Clear();

            this.ReadAssets();
            this.ProcessAssets();
        }

        private void LoadFile(string fullName) {
            var reader = new UnityReader(fullName);
            switch (reader.FileType) {
                case FileType.AssetsFile:
                    LoadAssetsFile(reader);
                    break;
                case FileType.BundleFile:
                    LoadBundleFile(reader);
                    break;
                case FileType.WebFile:
                    LoadWebFile(reader);
                    break;
            }
        }

        private void LoadAssetsFile(string fullName, EndianBinaryReader reader) {
            var fileName = Path.GetFileName(fullName);
            if (!this.assetsFileListHash.Contains(fileName)) {
                Logger.Info($"Loading {fileName}");
                try {
                    var assetsFile = new SerializedFile(this, fullName, reader);
                    assetsFileList.Add(assetsFile);
                    this.assetsFileListHash.Add(assetsFile.FileName);

                    foreach (var sharedFile in assetsFile.MExternals) {
                        var sharedFilePath = Path.Combine(Path.GetDirectoryName(fullName), sharedFile.fileName);
                        var sharedFileName = sharedFile.fileName;

                        if (!this.importFilesHash.Contains(sharedFileName)) {
                            if (!File.Exists(sharedFilePath)) {
                                var findFiles = Directory.GetFiles(Path.GetDirectoryName(fullName), sharedFileName, SearchOption.AllDirectories);
                                if (findFiles.Length > 0) {
                                    sharedFilePath = findFiles[0];
                                }
                            }

                            if (File.Exists(sharedFilePath)) {
                                this.importFiles.Add(sharedFilePath);
                                this.importFilesHash.Add(sharedFileName);
                            }
                        }
                    }
                } catch (Exception e) {
                    Logger.Error($"Error while reading assets file {fileName}", e);
                    reader.Dispose();
                }
            } else {
                reader.Dispose();
            }
        }

        private void LoadAssetsFromMemory(string fullName, EndianBinaryReader reader, string originalPath, string unityVersion = null) {
            var fileName = Path.GetFileName(fullName);
            if (!this.assetsFileListHash.Contains(fileName)) {
                try {
                    var assetsFile = new SerializedFile(this, fullName, reader);
                    assetsFile.OriginalPath = originalPath;
                    if (assetsFile.Header.m_Version < SerializedFileFormatVersion.kUnknown_7) {
                        assetsFile.SetVersion(unityVersion);
                    }
                    assetsFileList.Add(assetsFile);
                    this.assetsFileListHash.Add(assetsFile.FileName);
                } catch (Exception e) {
                    Logger.Error($"Error while reading assets file {fileName} from {Path.GetFileName(originalPath)}", e);
                    this.ResourceFileReaders.Add(fileName, reader);
                }
            }
        }

        private void LoadBundleFile(string fullName, EndianBinaryReader reader, string parentPath = null) {
            var fileName = Path.GetFileName(fullName);
            Logger.Info("Loading " + fileName);
            try {
                var bundleFile = new BundleFile(reader, fullName);
                foreach (var file in bundleFile.FileList) {
                    var subReader = new EndianBinaryReader(file.Stream);
                    if (SerializedFile.IsSerializedFile(subReader)) {
                        var dummyPath = Path.GetDirectoryName(fullName) + Path.DirectorySeparatorChar + file.FileName;
                        LoadAssetsFromMemory(dummyPath, subReader, parentPath ?? fullName, bundleFile.MHeader.UnityRevision);
                    } else {
                        this.ResourceFileReaders[file.FileName] = subReader; //TODO
                    }
                }
            } catch (Exception e) {
                var str = $"Error while reading bundle file {fileName}";
                if (parentPath != null) {
                    str += $" from {Path.GetFileName(parentPath)}";
                }
                Logger.Error(str, e);
            } finally {
                reader.Dispose();
            }
        }

        private void LoadWebFile(string fullName, EndianBinaryReader reader) {
            var fileName = Path.GetFileName(fullName);
            Logger.Info("Loading " + fileName);
            try {
                var webFile = new WebFile(reader);
                foreach (var file in webFile.FileList) {
                    var dummyPath = Path.Combine(Path.GetDirectoryName(fullName), file.FileName);
                    switch (CheckFileType(file.Stream, out var fileReader)) {
                        case FileType.AssetsFile:
                            LoadAssetsFromMemory(dummyPath, fileReader, fullName);
                            break;
                        case FileType.BundleFile:
                            LoadBundleFile(dummyPath, fileReader, fullName);
                            break;
                        case FileType.WebFile:
                            LoadWebFile(dummyPath, fileReader);
                            break;
                        case FileType.ResourceFile:
                            this.ResourceFileReaders[file.FileName] = fileReader; //TODO
                            break;
                    }
                }
            } catch (Exception e) {
                Logger.Error($"Error while reading web file {fileName}", e);
            } finally {
                reader.Dispose();
            }
        }

        public void Clear() {
            foreach (var assetsFile in assetsFileList) {
                assetsFile.Objects.Clear();
                assetsFile.reader.Close();
            }
            assetsFileList.Clear();

            foreach (var resourceFileReader in this.ResourceFileReaders) {
                resourceFileReader.Value.Close();
            }
            this.ResourceFileReaders.Clear();

            this.AssetsFileIndexCache.Clear();
        }

        private void ReadAssets() {
            Logger.Info("Read assets...");

            var progressCount = assetsFileList.Sum(x => x.m_Objects.Count);
            var i = 0;
            Progress.Reset();
            foreach (var assetsFile in assetsFileList) {
                foreach (var objectInfo in assetsFile.m_Objects) {
                    var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objectInfo);
                    try {
                        object obj = (object)objectReader.type switch {
                            ClassIdType.Animation => new Animation(objectReader),
                            ClassIdType.AnimationClip => new AnimationClip(objectReader),
                            ClassIdType.Animator => new Animator(objectReader),
                            ClassIdType.AnimatorController => new AnimatorController(objectReader),
                            ClassIdType.AnimatorOverrideController => new AnimatorOverrideController(objectReader),
                            ClassIdType.AssetBundle => new AssetBundle(objectReader),
                            ClassIdType.AudioClip => new AudioClip(objectReader),
                            ClassIdType.Avatar => new Avatar(objectReader),
                            ClassIdType.Font => new Font(objectReader),
                            ClassIdType.GameObject => new GameObject(objectReader),
                            ClassIdType.Material => new Material(objectReader),
                            ClassIdType.Mesh => new Mesh(objectReader),
                            ClassIdType.MeshFilter => new MeshFilter(objectReader),
                            ClassIdType.MeshRenderer => new MeshRenderer(objectReader),
                            ClassIdType.MonoBehaviour => new MonoBehaviour(objectReader),
                            ClassIdType.MonoScript => new MonoScript(objectReader),
                            ClassIdType.MovieTexture => new MovieTexture(objectReader),
                            ClassIdType.PlayerSettings => new PlayerSettings(objectReader),
                            ClassIdType.RectTransform => new RectTransform(objectReader),
                            ClassIdType.Shader => new Shader(objectReader),
                            ClassIdType.SkinnedMeshRenderer => new SkinnedMeshRenderer(objectReader),
                            ClassIdType.Sprite => new Sprite(objectReader),
                            ClassIdType.SpriteAtlas => new SpriteAtlas(objectReader),
                            ClassIdType.TextAsset => new TextAsset(objectReader),
                            ClassIdType.Texture2D => new Texture2D(objectReader),
                            ClassIdType.Transform => new Transform(objectReader),
                            ClassIdType.VideoClip => new VideoClip(objectReader),
                            ClassIdType.ResourceManager => new ResourceManager(objectReader),
                            _ => new Object(objectReader),
                        };
                        assetsFile.AddObject(obj);
                    } catch (Exception e) {
                        var sb = new StringBuilder();
                        sb.AppendLine("Unable to load object")
                            .AppendLine($"Assets {assetsFile.fileName}")
                            .AppendLine($"Type {objectReader.type}")
                            .AppendLine($"PathID {objectInfo.m_PathID}")
                            .Append(e);
                        Logger.Error(sb.ToString());
                    }

                    Progress.Report(++i, progressCount);
                }
            }
        }

        private void ProcessAssets() {
            Logger.Info("Process Assets...");

            foreach (var assetsFile in assetsFileList) {
                foreach (var obj in assetsFile.Objects) {
                    if (obj is GameObject m_GameObject) {
                        foreach (var pptr in m_GameObject.m_Components) {
                            if (pptr.TryGet(out var m_Component)) {
                                switch (m_Component) {
                                    case Transform m_Transform:
                                        m_GameObject.m_Transform = m_Transform;
                                        break;
                                    case MeshRenderer m_MeshRenderer:
                                        m_GameObject.m_MeshRenderer = m_MeshRenderer;
                                        break;
                                    case MeshFilter m_MeshFilter:
                                        m_GameObject.m_MeshFilter = m_MeshFilter;
                                        break;
                                    case SkinnedMeshRenderer m_SkinnedMeshRenderer:
                                        m_GameObject.m_SkinnedMeshRenderer = m_SkinnedMeshRenderer;
                                        break;
                                    case Animator m_Animator:
                                        m_GameObject.m_Animator = m_Animator;
                                        break;
                                    case Animation m_Animation:
                                        m_GameObject.m_Animation = m_Animation;
                                        break;
                                }
                            }
                        }
                    } else if (obj is SpriteAtlas m_SpriteAtlas) {
                        foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites) {
                            if (m_PackedSprite.TryGet(out var m_Sprite)) {
                                if (m_Sprite.m_SpriteAtlas.IsNull) {
                                    m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
