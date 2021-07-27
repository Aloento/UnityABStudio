namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using AutoDeskFBX;
    using Helpers;

    public partial class FBXHelpService : FBXService {
        private string fileName;
        private IImported imported;
        private bool allNodes;
        private bool exportSkins;
        private bool castToBone;
        private float boneSize;
        private bool exportAllUvsAsDiffuseMaps;
        private float scaleFactor;
        private int versionIndex;
        private bool isAscii;

        public void Init(string fileName, IImported imported, bool allNodes, bool exportSkins, bool castToBone,
            float boneSize, bool exportAllUvsAsDiffuseMaps, float scaleFactor, int versionIndex, bool isAscii) {

            this.fileName = fileName;
            this.imported = imported;
            this.allNodes = allNodes;
            this.exportSkins = exportSkins;
            this.castToBone = castToBone;
            this.boneSize = boneSize;
            this.exportAllUvsAsDiffuseMaps = exportAllUvsAsDiffuseMaps;
            this.scaleFactor = scaleFactor;
            this.versionIndex = versionIndex;
            this.isAscii = isAscii;

            var is60Fps = imported.AnimationList.Count > 0 && imported.AnimationList[0].SampleRate.Equals(60.0f);


        }


    }
}
