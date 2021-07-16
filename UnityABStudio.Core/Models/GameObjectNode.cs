namespace SoarCraft.QYun.UnityABStudio.Core.Models {
    using AssetReader.Unity3D.Objects;
    using Microsoft.UI.Xaml.Controls;

    public class GameObjectNode : TreeViewNode {
        public GameObject GObj;

        public GameObjectNode(string name) => this.Content = name;

        public GameObjectNode(GameObject gameObject) {
            this.GObj = gameObject;
            this.Content = gameObject.m_Name;
        }
    }
}
