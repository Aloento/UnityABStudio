namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Storage;
    using AssetReader;
    using AssetReader.Unity3D;
    using AssetReader.Unity3D.Objects;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Core.Models;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;

    public class OverViewModel : ObservableRecipient {
        private readonly AssetsManager manager = Ioc.Default.GetRequiredService<AssetsManager>();
        private readonly ILogger logger = Ioc.Default.GetRequiredService<ILogger>();

        internal readonly ObservableCollection<BundleItem> bundleList = new();

#if DEBUG
        internal async Task BuildTestBundleListAsync() {
            await this.manager.LoadFolderAsync(@"C:\Codes\C#\UnityABStudio\UnitTest\Assets");

            foreach (var file in this.manager.AssetsFileList) {
                this.bundleList.Add(new BundleItem(file));
            }
        }
#endif

        public Task<(Dictionary<string, List<string>>, List<TreeViewNode>)> LoadAssetsDataAsync(IEnumerable<StorageFile> files) => Task.Run(async () => {
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
    }
}
