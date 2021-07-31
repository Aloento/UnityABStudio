namespace SoarCraft.QYun.UnityABStudio.Core.Models {
    using System.IO;
    using AssetReader;

    public class BundleItem {
        public string CBAID { get; }
        public string FileName { get; }
        public string Count { get; }
        public SerializedFile Serialized { get; }

        public BundleItem(SerializedFile file) {
            CBAID = file.fileName;
            FileName = Path.GetFileNameWithoutExtension(file.originalPath);
            Count = file.Objects.Count.ToString();
            Serialized = file;
        }

        public string Format(string name, string count) => $"In {name} with {count} Items";
    }
}
