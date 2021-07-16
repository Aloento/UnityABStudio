namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using AssetReader.Unity3D.Objects;

    public class GameObjectNode {
        public GameObject GObj;
        public string Name;

        public GameObjectNode(string name) => this.Name = name;

        public GameObjectNode(GameObject gameObject) {
            this.GObj = gameObject;
            this.Name = gameObject.m_Name;
        }
    }
}
