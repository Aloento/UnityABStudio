namespace SoarCraft.QYun.AssetReader.Entities.Structs {
    public class BuildType {
        private readonly string buildType;

        public BuildType(string type) => buildType = type;

        public bool IsAlpha => buildType == "a";
        public bool IsPatch => buildType == "p";
    }
}
