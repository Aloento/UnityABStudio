namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.Storage;
    using AssetReader;
    using AssetReader.Unity3D;
    using AssetReader.Unity3D.Objects;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Serilog;
    using Services;

    public class OverViewModel : ObservableRecipient {
        private readonly AssetsManager manager = Ioc.Default.GetRequiredService<AssetsManager>();
        private readonly ILogger logger = Ioc.Default.GetRequiredService<LoggerService>().Logger;

        public async Task LoadAssetsDataAsync(IEnumerable<StorageFile> files) {
            this.logger.Information("Start to load Asset Files...");
            await manager.LoadFilesAsync(files.Select(x => x.Path).ToArray());
            this.logger.Information("AB Files loaded... Start to read contents...");

            var objectAssetsDic =
                new Dictionary<UObject, AssetItem>(this.manager.AssetsFileList.Sum(x => x.Objects.Count));
            var containers = new List<(PPtr<UObject>, string)>();
            var companyName = string.Empty;
            var productName = string.Empty;

            foreach (var uObject in this.manager.AssetsFileList.SelectMany(serializedFile => serializedFile.Objects)) {
                objectAssetsDic.Add(uObject, new AssetItem(uObject, out var container, out var names));
                containers.AddRange(container);

                var (company, project) = names;
                if (!string.IsNullOrWhiteSpace(company) && !companyName.Contains(company))
                    companyName = company;
                if (!string.IsNullOrWhiteSpace(project) && !productName.Contains(project))
                    productName = project;
            }

            foreach (var (pptr, container) in containers) {
                if (pptr.TryGet(out var obj))
                    objectAssetsDic[obj].Container = container;
            }

            var gameObjectNodeDic = new Dictionary<GameObject, GameObjectNode>();
            foreach (var obj in from serializedFile in this.manager.AssetsFileList let rootNode =
                new GameObjectNode(serializedFile.fileName) from obj in serializedFile.Objects select obj) {
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



                }
            }
        }
    }
}
