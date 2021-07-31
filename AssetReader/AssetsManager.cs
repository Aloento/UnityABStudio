namespace SoarCraft.QYun.AssetReader {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Entities.Enums;
    using Unity3D;
    using Unity3D.Objects;
    using Unity3D.Objects.AnimationClips;
    using Unity3D.Objects.AnimatorControllers;
    using Unity3D.Objects.AnimatorOverrideControllers;
    using Unity3D.Objects.AssetBundles;
    using Unity3D.Objects.Avatars;
    using Unity3D.Objects.Materials;
    using Unity3D.Objects.Meshes;
    using Unity3D.Objects.Shaders;
    using Unity3D.Objects.SpriteAtlases;
    using Unity3D.Objects.Sprites;
    using Unity3D.Objects.Texture2Ds;
    using Unity3D.Objects.VideoClips;
    using Utils;
    using static Helpers.ImportHelper;

    public class AssetsManager {
        public string SpecifyUnityVersion;
        public List<SerializedFile> AssetsFileList = new();

        internal Dictionary<string, int> assetsFileIndexCache = new(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, UnityReader> resourceFileReaders = new(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> importFiles = new();
        private readonly HashSet<string> importFilesHash = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> assetsFileListHash = new(StringComparer.OrdinalIgnoreCase);

        public Task LoadFilesAsync(params string[] files) => Task.Run(() => {
            var path = Path.GetDirectoryName(files[0]);
            MergeSplitAssets(path);
            var toReadFile = ProcessingSplitFiles(files.ToList());
            Load(toReadFile);
        });

        public Task LoadFolderAsync(string path) => Task.Run(() => {
            MergeSplitAssets(path, true);
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
            var toReadFile = ProcessingSplitFiles(files);
            Load(toReadFile);
        });

        private void Load(IEnumerable<string> files) {
            foreach (var file in files) {
                importFiles.Add(file);
                _ = importFilesHash.Add(Path.GetFileName(file));
            }

            //use a for loop because list size can change
            for (var i = 0; i < this.importFiles.Count; i++) {
                this.LoadFile(this.importFiles[i]);
            }

            importFiles.Clear();
            importFilesHash.Clear();
            assetsFileListHash.Clear();

            ReadAssets();
            ProcessAssets();
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

        private void LoadAssetsFile(UnityReader reader) {
            if (!assetsFileListHash.Contains(reader.FileName)) {
                Console.WriteLine($"Loading {reader.FileName}");
                try {
                    var assetsFile = new SerializedFile(reader, this);
                    CheckStrippedVersion(assetsFile);
                    AssetsFileList.Add(assetsFile);
                    _ = assetsFileListHash.Add(assetsFile.fileName);

                    foreach (var sharedFile in assetsFile.m_Externals) {
                        var sharedFileName = sharedFile.fileName;

                        if (!importFilesHash.Contains(sharedFileName)) {
                            var sharedFilePath = Path.Combine(Path.GetDirectoryName(reader.FullPath), sharedFileName);
                            if (!File.Exists(sharedFilePath)) {
                                var findFiles = Directory.GetFiles(Path.GetDirectoryName(reader.FullPath), sharedFileName, SearchOption.AllDirectories);
                                if (findFiles.Length > 0) {
                                    sharedFilePath = findFiles[0];
                                }
                            }

                            if (File.Exists(sharedFilePath)) {
                                importFiles.Add(sharedFilePath);
                                _ = importFilesHash.Add(sharedFileName);
                            }
                        }
                    }
                } catch (IOException e) {
                    Console.WriteLine($"Error while reading assets file {reader.FileName}", e);
                    reader.Dispose();
                }
            } else {
                reader.Dispose();
            }
        }

        private void LoadAssetsFromMemory(UnityReader reader, string originalPath, string unityVersion = null) {
            if (!assetsFileListHash.Contains(reader.FileName)) {
                try {
                    var assetsFile = new SerializedFile(reader, this) {
                        originalPath = originalPath
                    };
                    if (!string.IsNullOrEmpty(unityVersion) && assetsFile.header.m_Version < SerializedFileFormatVersion.kUnknown_7) {
                        assetsFile.SetVersion(unityVersion);
                    }
                    CheckStrippedVersion(assetsFile);
                    AssetsFileList.Add(assetsFile);
                    _ = assetsFileListHash.Add(assetsFile.fileName);
                } catch (Exception e) {
                    Console.WriteLine($"Error while reading assets file {reader.FileName} from {Path.GetFileName(originalPath)}", e);
                    resourceFileReaders.Add(reader.FileName, reader);
                }
            }
        }

        private void LoadBundleFile(UnityReader reader, string originalPath = null) {
            Console.WriteLine("Loading " + reader.FileName);
            try {
                var bundleFile = new BundleFile(reader);
                foreach (var file in bundleFile.fileList) {
                    var dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), file.fileName);
                    var subReader = new UnityReader(dummyPath, file.stream);
                    if (subReader.FileType == FileType.AssetsFile) {
                        LoadAssetsFromMemory(subReader, originalPath ?? reader.FullPath, bundleFile.m_Header.unityRevision);
                    } else {
                        resourceFileReaders[file.fileName] = subReader; //TODO
                    }
                }
            } catch (IOException e) {
                var str = $"Error while reading bundle file {reader.FileName}";
                if (originalPath != null) {
                    str += $" from {Path.GetFileName(originalPath)}";
                }
                Console.WriteLine(str, e);
            } finally {
                reader.Dispose();
            }
        }

        private void LoadWebFile(UnityReader reader) {
            Console.WriteLine("Loading " + reader.FileName);
            try {
                var webFile = new WebFile(reader);
                foreach (var file in webFile.fileList) {
                    var dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), file.fileName);
                    var subReader = new UnityReader(dummyPath, file.stream);
                    switch (subReader.FileType) {
                        case FileType.AssetsFile:
                            LoadAssetsFromMemory(subReader, reader.FullPath);
                            break;
                        case FileType.BundleFile:
                            LoadBundleFile(subReader, reader.FullPath);
                            break;
                        case FileType.WebFile:
                            LoadWebFile(subReader);
                            break;
                        case FileType.ResourceFile:
                            resourceFileReaders[file.fileName] = subReader; //TODO
                            break;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine($"Error while reading web file {reader.FileName}", e);
            } finally {
                reader.Dispose();
            }
        }

        public void CheckStrippedVersion(SerializedFile assetsFile) {
            if (assetsFile.IsVersionStripped && string.IsNullOrEmpty(SpecifyUnityVersion)) {
                throw new VersionNotFoundException("The Unity version has been stripped, please set the version in the options");
            }
            if (!string.IsNullOrEmpty(SpecifyUnityVersion)) {
                assetsFile.SetVersion(SpecifyUnityVersion);
            }
        }

        public void Clear() {
            foreach (var assetsFile in AssetsFileList) {
                assetsFile.Objects.Clear();
                assetsFile.ObjectsDic.Clear();
                assetsFile.reader.Dispose();
            }
            AssetsFileList.Clear();

            foreach (var resourceFileReader in resourceFileReaders) {
                resourceFileReader.Value.Dispose();
            }

            resourceFileReaders.Clear();
            assetsFileIndexCache.Clear();
        }

        private void ReadAssets() {
            Console.WriteLine("Read assets...");

            foreach (var assetsFile in AssetsFileList) {
                foreach (var objectInfo in assetsFile.m_Objects) {
                    var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objectInfo);
                    try {
                        var obj = objectReader.type switch {
                            ClassIDType.Animation => new Animation(objectReader),
                            ClassIDType.AnimationClip => new AnimationClip(objectReader),
                            ClassIDType.Animator => new Animator(objectReader),
                            ClassIDType.AnimatorController => new AnimatorController(objectReader),
                            ClassIDType.AnimatorOverrideController => new AnimatorOverrideController(objectReader),
                            ClassIDType.AssetBundle => new AssetBundle(objectReader),
                            ClassIDType.AudioClip => new AudioClip(objectReader),
                            ClassIDType.Avatar => new Avatar(objectReader),
                            ClassIDType.Font => new Font(objectReader),
                            ClassIDType.GameObject => new GameObject(objectReader),
                            ClassIDType.Material => new Material(objectReader),
                            ClassIDType.Mesh => new Mesh(objectReader),
                            ClassIDType.MeshFilter => new MeshFilter(objectReader),
                            ClassIDType.MeshRenderer => new MeshRenderer(objectReader),
                            ClassIDType.MonoBehaviour => new MonoBehaviour(objectReader),
                            ClassIDType.MonoScript => new MonoScript(objectReader),
                            ClassIDType.MovieTexture => new MovieTexture(objectReader),
                            ClassIDType.PlayerSettings => new PlayerSettings(objectReader),
                            ClassIDType.RectTransform => new RectTransform(objectReader),
                            ClassIDType.Shader => new Shader(objectReader),
                            ClassIDType.SkinnedMeshRenderer => new SkinnedMeshRenderer(objectReader),
                            ClassIDType.Sprite => new Sprite(objectReader),
                            ClassIDType.SpriteAtlas => new SpriteAtlas(objectReader),
                            ClassIDType.TextAsset => new TextAsset(objectReader),
                            ClassIDType.Texture2D => new Texture2D(objectReader),
                            ClassIDType.Transform => new Transform(objectReader),
                            ClassIDType.VideoClip => new VideoClip(objectReader),
                            ClassIDType.ResourceManager => new ResourceManager(objectReader),
                            _ => new UObject(objectReader)
                        };
                        assetsFile.AddObject(obj);
                    } catch (Exception e) {
                        var sb = new StringBuilder();
                        _ = sb.AppendLine("Unable to load object")
                            .AppendLine($"Assets {assetsFile.fileName}")
                            .AppendLine($"Type {objectReader.type}")
                            .AppendLine($"PathID {objectInfo.m_PathID}")
                            .Append(e);
                        Console.WriteLine(sb.ToString());
                    }
                }
            }
        }

        private void ProcessAssets() {
            Console.WriteLine("Process Assets...");

            foreach (var obj in this.AssetsFileList.SelectMany(assetsFile => assetsFile.Objects)) {
                switch (obj) {
                    case GameObject m_GameObject: {
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

                        break;
                    }
                    case SpriteAtlas m_SpriteAtlas: {
                        foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites) {
                            if (m_PackedSprite.TryGet(out var m_Sprite)) {
                                if (m_Sprite.m_SpriteAtlas.IsNull) {
                                    m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}
