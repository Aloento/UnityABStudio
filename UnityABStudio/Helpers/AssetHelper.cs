namespace SoarCraft.QYun.UnityABStudio.Helpers {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.UI.Xaml.Controls;
    using SoarCraft.QYun.AssetReader;
    using SoarCraft.QYun.AssetReader.Unity3D;
    using SoarCraft.QYun.AssetReader.Unity3D.Contracts;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.AssetBundles;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.Meshes;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.Sprites;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.Texture2Ds;
    using SoarCraft.QYun.AssetReader.Unity3D.Objects.VideoClips;
    using SoarCraft.QYun.UnityABStudio.Models;

    public struct AssetInstances {
        public static readonly AssetsManager AssetsManager = new();

        public static async (string, List<TreeViewNode>) BuildAssetDataAsync() {
            var objectCount = AssetsManager.AssetsFileList.Sum(x => x.Objects.Count);
            var objectList = new List<AssetItem>(objectCount);
            var containers = new List<(PPtr<UObject>, string)>();

            var productName = "";
            foreach (var assetsFile in AssetsManager.AssetsFileList) {
                foreach (var asset in assetsFile.Objects) {
                    var assetItem = new AssetItem(asset);
                    objectList.Add(assetItem);

                    switch (asset) {
                        case GameObject m_GameObject:
                            assetItem.Content = m_GameObject.m_Name;
                            break;
                        case Texture2D m_Texture2D:
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = asset.byteSize + m_Texture2D.m_StreamData.size;
                            assetItem.Content = m_Texture2D.m_Name;
                            break;
                        case AudioClip m_AudioClip:
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = asset.byteSize + m_AudioClip.m_Size;
                            assetItem.Content = m_AudioClip.m_Name;
                            break;
                        case VideoClip m_VideoClip:
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = asset.byteSize + m_VideoClip.m_ExternalResources.m_Size;
                            assetItem.Content = m_VideoClip.m_Name;
                            break;
                        case Shader m_Shader:
                            assetItem.Content = m_Shader.m_ParsedForm?.m_Name ?? m_Shader.m_Name;
                            break;
                        case Mesh:
                        case TextAsset:
                        case AnimationClip:
                        case Font:
                        case MovieTexture:
                        case Sprite:
                            assetItem.Content = ((NamedObject)asset).m_Name;
                            break;
                        case Animator m_Animator:
                            if (m_Animator.m_GameObject.TryGet(out var gameObject))
                                assetItem.Content = gameObject.m_Name;
                            break;
                        case MonoBehaviour m_MonoBehaviour:
                            if (m_MonoBehaviour.m_Name == "" && m_MonoBehaviour.m_Script.TryGet(out var m_Script))
                                assetItem.Content = m_Script.m_ClassName;
                            else assetItem.Content = m_MonoBehaviour.m_Name;
                            break;
                        case PlayerSettings m_PlayerSettings:
                            productName = m_PlayerSettings.productName;
                            break;
                        case AssetBundle m_AssetBundle:
                            foreach (var m_Container in m_AssetBundle.m_Container) {
                                var preloadIndex = m_Container.Value.preloadIndex;
                                var preloadSize = m_Container.Value.preloadSize;
                                var preloadEnd = preloadIndex + preloadSize;
                                for (var k = preloadIndex; k < preloadEnd; k++) {
                                    containers.Add((m_AssetBundle.m_PreloadTable[k], m_Container.Key));
                                }
                            }
                            assetItem.Content = m_AssetBundle.m_Name;
                            break;
                        case ResourceManager m_ResourceManager:
                            foreach (var m_Container in m_ResourceManager.m_Container) {
                                containers.Add((m_Container.Value, m_Container.Key));
                            }
                            break;
                        case NamedObject m_NamedObject:
                            assetItem.Content = m_NamedObject.m_Name;
                            break;
                    }

                    if (assetItem.Content == null)
                        assetItem.Content = assetItem.m_PathID;
                }
            }



        }


    }
}
