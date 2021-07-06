namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using Contracts;
    using Utils;

    public sealed class GameObject : EditorExtension {
        public PPtr<Component>[] m_Components;
        public string m_Name;

        public Transform m_Transform;
        public MeshRenderer m_MeshRenderer;
        public MeshFilter m_MeshFilter;
        public SkinnedMeshRenderer m_SkinnedMeshRenderer;
        public Animator m_Animator;
        public Animation m_Animation;

        public GameObject(ObjectReader reader) : base(reader) {
            var m_Component_size = reader.ReadInt32();
            m_Components = new PPtr<Component>[m_Component_size];
            for (var i = 0; i < m_Component_size; i++) {
                if ((version[0] == 5 && version[1] < 5) || version[0] < 5) //5.5 down
                {
                    var first = reader.ReadInt32();
                }
                m_Components[i] = new PPtr<Component>(reader);
            }

            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
        }
    }
}
