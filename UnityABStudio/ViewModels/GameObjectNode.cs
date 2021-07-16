namespace SoarCraft.QYun.UnityABStudio.ViewModels {
    using AssetReader.Unity3D.Objects;
    using Microsoft.UI.Xaml.Controls;

    public class GameObjectNode : TreeViewNode {
        public GameObject GObj;

        public GameObjectNode(string name) => Content = name;

        public GameObjectNode(GameObject gameObject) {
            this.GObj = gameObject;
            Content = gameObject.m_Name;
        }
    }
}
