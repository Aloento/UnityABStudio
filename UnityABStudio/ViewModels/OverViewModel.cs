namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using Windows.UI.Core;
    using AssetReader;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D;
    using AssetReader.Unity3D.Objects;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Contracts.ViewModels;
    using Core.Models;
    using Extensions;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;

    public class OverViewModel : ObservableRecipient, INavigationAware {
        private readonly AssetsManager manager = Ioc.Default.GetRequiredService<AssetsManager>();
        private readonly ILogger logger = Ioc.Default.GetRequiredService<ILogger>();

        internal readonly ObservableCollection<BundleItem> bundleList = new();

        #region QuickProperties

        internal bool ExpAnimator;
        internal bool ExpAudioClip = true;
        internal bool ExpFont;
        internal bool ExpMesh;
        internal bool ExpMonoBehaviour;
        internal bool ExpMovieTexture;
        internal bool ExpShader;
        internal bool ExpSprite;
        internal bool ExpTexture2D = true;
        internal bool ExpTextAsset = true;
        internal bool ExpVideoClip;

        #endregion

#if DEBUG
        internal async Task BuildTestBundleListAsync() {
            await this.manager.LoadFolderAsync(@"C:\Codes\C#\UnityABStudio\UnitTest\Assets");

            foreach (var file in this.manager.AssetsFileList) {
                this.bundleList.Add(new BundleItem(file));
            }
        }
#endif

        internal async Task LoadAssetFilesAsync(IReadOnlyList<StorageFile> fileList) {
            var fileArray = fileList.Select((x) => x.Path).ToList();

            foreach (var file in from file in fileArray
                                 from bundle in this.bundleList
                                 where file.Contains(bundle.FileName) select file) {
                _ = fileArray.Remove(file);
            }

            await this.manager.LoadFilesAsync(fileArray.ToArray());
            foreach (var file in this.manager.AssetsFileList) {
                this.bundleList.Add(new BundleItem(file));
                this.logger.Information($"{file.fileName} Loaded");
            }
        }

        internal async Task LoadAssetFolderAsync(StorageFolder folder) {
            this.ClearList();
            await this.manager.LoadFolderAsync(folder.Path);

            foreach (var file in this.manager.AssetsFileList) {
                this.bundleList.Add(new BundleItem(file));
                this.logger.Information($"{file.fileName} Loaded");
            }
        }

        internal void EjectFiles(IList<object> fileList) {
            for (var i = 0; i < fileList.Count;) {
                var file = fileList[0] as BundleItem;
                _ = this.bundleList.Remove(file);
                _ = this.manager.AssetsFileList.Remove(file?.Serialized);
                this.logger.Information($"{file?.CBAID} Released");
            }
        }

        internal Task<int> QuickExportAsync(StorageFolder saveFolder) => Task.Run(async () => {
            var objList = new List<UObject>();
            var filtered = new List<UObject>();
            var timeOuter = DateTime.Now.AddMinutes(3);

            foreach (var bundle in this.bundleList) {
                objList.AddRange(bundle.Serialized.Objects);
            }

            #region Filters

            if (this.ExpAnimator)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.Animator).ToList());

            if (this.ExpAudioClip)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.AudioClip).ToList());

            if (this.ExpFont)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.Font).ToList());

            if (this.ExpMesh)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.Mesh).ToList());

            if (this.ExpMonoBehaviour)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.MonoBehaviour).ToList());

            if (this.ExpMovieTexture)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.MovieTexture).ToList());

            if (this.ExpShader)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.Shader).ToList());

            if (this.ExpSprite)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.Sprite).ToList());

            if (this.ExpTexture2D)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.Texture2D).ToList());

            if (this.ExpTextAsset)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.TextAsset).ToList());

            if (this.ExpVideoClip)
                filtered.AddRange(objList.Where(x => x.type == ClassIDType.VideoClip).ToList());

            #endregion

            if (DateTime.Now.CompareTo(timeOuter) < 0) {
                foreach (var item in filtered.Select(x => new AssetItem(x, out _, out _))) {
                    _ = await item.ExportConvertFile(saveFolder.Path);
                }
            } else {
                this.logger.Error($"QuickExport Timeout, " +
                                  $"{filtered.Count} items need to export, ");
                return -1;
            }

            return filtered.Count;
        });

        private void ClearList() {
            foreach (var bundle in this.bundleList) {
                _ = this.manager.AssetsFileList.Remove(bundle?.Serialized);
                this.logger.Information($"{bundle?.CBAID} Released");
            }

            this.bundleList.Clear();
            this.manager.Clear();
        }

        public Task<(Dictionary<string, List<string>>, List<TreeViewNode>)> LoadAssetsDataAsync(
            IEnumerable<StorageFile> files) => Task.Run(async () => {
                this.logger.Information("Start to load Asset Files...");
                await this.manager.LoadFilesAsync(files.Select(x => x.Path).ToArray());
                this.logger.Information("AB Files loaded... Start to read contents...");

                var objectAssetsDic = new Dictionary<UObject, AssetItem>(
                    this.manager.AssetsFileList.Sum(x => x.Objects.Count));
                var containers = new List<(PPtr<UObject>, string)>();
                var namesDic = new Dictionary<string, List<string>> { { "Unclassified", new List<string>() } };

                foreach (var uObject in this.manager.AssetsFileList.SelectMany(serializedFile => serializedFile.Objects)) {
                    objectAssetsDic.Add(uObject, new AssetItem(uObject, out var container, out var outNames));
                    containers.AddRange(container);

                    var (company, project) = outNames;
                    if (!string.IsNullOrWhiteSpace(company) && !namesDic.TryGetValue(company, out var _))
                        namesDic.Add(company, new List<string>());
                    if (!string.IsNullOrWhiteSpace(project)) {
                        if (namesDic.TryGetValue(company, out var projectList))
                            projectList.Add(project);
                        else
                            namesDic["Unclassified"].Add(project);
                    }
                }

                foreach (var (pptr, container) in containers) {
                    if (pptr.TryGet(out var obj))
                        objectAssetsDic[obj].Container = container;
                }

                var nodeCollection = new List<TreeViewNode>();
                var gameObjectNodeDic = new Dictionary<GameObject, GameObjectNode>();
                foreach (var serializedFile in this.manager.AssetsFileList) {
                    var rootNode = new GameObjectNode(serializedFile.fileName);

                    foreach (var obj in serializedFile.Objects) {
                        if (obj is GameObject gameObject) {
                            if (!gameObjectNodeDic.TryGetValue(gameObject, out var currentNode)) {
                                currentNode = new GameObjectNode(gameObject);
                                gameObjectNodeDic.Add(gameObject, currentNode);
                            }

                            foreach (var pptr in gameObject.m_Components) {
                                if (pptr.TryGet(out var component)) {
                                    objectAssetsDic[component].Node = currentNode;

                                    switch (component) {
                                        case MeshFilter meshFilter:
                                            if (meshFilter.m_Mesh.TryGet(out var fMesh))
                                                objectAssetsDic[fMesh].Node = currentNode;
                                            break;

                                        case SkinnedMeshRenderer renderer:
                                            if (renderer.m_Mesh.TryGet(out var sMesh))
                                                objectAssetsDic[sMesh].Node = currentNode;
                                            break;
                                    }
                                }
                            }

                            var parentNode = rootNode;
                            if (gameObject.m_Transform != null) {
                                if (gameObject.m_Transform.m_Father.TryGet(out var father)) {
                                    if (father.m_GameObject.TryGet(out var parentGameObject)) {
                                        if (!gameObjectNodeDic.TryGetValue(parentGameObject, out parentNode)) {
                                            parentNode = new GameObjectNode(parentGameObject);
                                            gameObjectNodeDic.Add(parentGameObject, parentNode);
                                        }
                                    }
                                }
                            }

                            parentNode.Children.Add(currentNode);
                        }
                    }

                    if (rootNode.Children.Count > 0) {
                        nodeCollection.Add(rootNode);
                    }
                }

                return (namesDic, nodeCollection);
            });

        void INavigationAware.OnNavigatedTo(object parameter) {
            this.logger.Debug("NavigatedTo OverViewPage");
        }

        void INavigationAware.OnNavigatedFrom() {
        }
    }
}
