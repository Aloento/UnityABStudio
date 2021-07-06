namespace SoarCraft.QYun.AssetReader.Unity3D.Contracts {
    using Objects;
    using Utils;

    public abstract class Component : EditorExtension {
        public PPtr<GameObject> m_GameObject;

        protected Component(ObjectReader reader) : base(reader) => m_GameObject = new PPtr<GameObject>(reader);
    }
}
