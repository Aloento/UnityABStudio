namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D;
    using AssetReader.Unity3D.Contracts;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.AnimationClips;
    using AssetReader.Unity3D.Objects.Meshes;
    using AssetReader.Unity3D.Objects.Sprites;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using AssetReader.Unity3D.Objects.AudioClip;
    using Base62;

    public class AssetItem {
        public UObject Obj;
        public ClassIDType Type;
        public long PathID;
        public long FullSize;

        public string Name;
        public string BaseID;
        public string Container;
        public string InfoText;
        public bool Exportable;
        public string ProductName;

        public AssetItem(UObject obj) {
            this.Obj = obj;
            this.Type = obj.type;
            this.PathID = obj.m_PathID;
            this.FullSize = obj.byteSize;

            this.BaseID = obj.m_PathID.ToBase62();

            switch (obj) {
                case Mesh:
                case Font:
                case Sprite:
                case TextAsset:
                case MovieTexture:
                case AnimationClip:
                    this.Name = ((NamedObject)obj).m_Name;
                    this.Exportable = true;
                    break;

                case GameObject gameObject:
                    this.Name = gameObject.m_Name;
                    break;

                case Texture2D texture2D:
                    if (!string.IsNullOrEmpty(texture2D.m_StreamData?.path))
                        this.FullSize = obj.byteSize + texture2D.m_StreamData.size;

                    this.Name = texture2D.m_Name;
                    this.Exportable = true;
                    break;

                case AudioClip audioClip:
                    if (!string.IsNullOrEmpty(audioClip.m_Source))
                        this.FullSize = obj.byteSize + audioClip.m_Size;

                    this.Name = audioClip.m_Name;
                    this.Exportable = true;
                    break;

                case VideoClip videoClip:
                    if (!string.IsNullOrEmpty(videoClip.m_OriginalPath))
                        this.FullSize = obj.byteSize + (long)videoClip.m_ExternalResources.m_Size;

                    this.Name = videoClip.m_Name;
                    this.Exportable = true;
                    break;

                case Shader shader:
                    this.Name = shader.m_ParsedForm?.m_Name ?? shader.m_Name;
                    this.Exportable = true;
                    break;

                case Animator animator:
                    if (animator.m_GameObject.TryGet(out var gameObject))
                        this.Name = gameObject.m_Name;

                    this.Exportable = true;
                    break;

                case MonoBehaviour monoBehaviour:
                    if (monoBehaviour.m_Name == "" && monoBehaviour.m_Script.TryGet(out var m_Script))
                        this.Name = m_Script.m_ClassName;
                    else
                        this.Name = monoBehaviour.m_ClassName;

                    this.Exportable = true;
                    break;

                case PlayerSettings playerSettings:
                    ProductName = playerSettings.productName;
                    break;

                case AssetBundle assetBundle:
                    foreach (var m_Container in assetBundle.m_Container){
                        var preloadIndex = m_Container.Value.preloadIndex;
                        var preloadSize = m_Container.Value.preloadSize;
                        var preloadEnd = preloadIndex + preloadSize;
                        for (int k = preloadIndex; k < preloadEnd; k++){
                            containers.Add((assetBundle.m_PreloadTable[k], m_Container.Key));
                        }
                    }

                    this.Name = assetBundle.m_Name;
                    break;

                case ResourceManager resourceManager:
                    foreach (var m_Container in resourceManager.m_Container){
                        containers.Add((m_Container.Value, m_Container.Key));
                    }
                    break;

                case NamedObject namedObject:
                    this.Name = namedObject.m_Name;
                    break;

            }
        }
    }
}
