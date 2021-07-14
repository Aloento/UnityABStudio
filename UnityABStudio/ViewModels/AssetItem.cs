namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D;
    using AssetReader.Unity3D.Contracts;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.AnimationClips;
    using AssetReader.Unity3D.Objects.Meshes;
    using AssetReader.Unity3D.Objects.Sprites;
    using AssetReader.Unity3D.Objects.Texture2Ds;
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


            }
        }
    }
}
