namespace SoarCraft.QYun.AssetReader.Unity3D.Contracts {
    using Entities.Enums;
    using Objects;
    using Utils;

    public abstract class EditorExtension : UObject {
        protected EditorExtension(ObjectReader reader) : base(reader) {
            if (platform == BuildTarget.NoTarget) {
                var m_PrefabParentObject = new PPtr<EditorExtension>(reader);
                var m_PrefabInternal = new PPtr<UObject>(reader); //PPtr<Prefab>
            }
        }
    }
}
