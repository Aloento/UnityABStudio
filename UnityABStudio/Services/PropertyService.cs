namespace SoarCraft.QYun.UnityABStudio.Services {
    public partial class SettingsService {
        private bool convertTexture = true;
        public bool ConvertTexture {
            get => this.convertTexture;
            set => _ = SetProperty(ref this.convertTexture, value);
        }

        private bool convertAudio = true;
        public bool ConvertAudio {
            get => this.convertAudio;
            set => _ = SetProperty(ref this.convertAudio, value);
        }

        private bool restoreExtensionName = true;
        public bool RestoreExtensionName {
            get => this.restoreExtensionName;
            set => _ = SetProperty(ref this.restoreExtensionName, value);
        }

        private bool eulerFilter;
        public bool EulerFilter {
            get => this.eulerFilter;
            set => _ = SetProperty(ref this.eulerFilter, value);
        }

        private float filterPrecision;
        public float FilterPrecision {
            get => this.filterPrecision;
            set => _ = SetProperty(ref this.filterPrecision, value);
        }

        private bool exportAllNodes;
        public bool ExportAllNodes {
            get => this.exportAllNodes;
            set => _ = SetProperty(ref this.exportAllNodes, value);
        }

        private bool exportSkins;
        public bool ExportSkins {
            get => this.exportSkins;
            set => _ = SetProperty(ref this.exportSkins, value);
        }

        private bool exportAnimations;
        public bool ExportAnimations {
            get => this.exportAnimations;
            set => _ = SetProperty(ref this.exportAnimations, value);
        }

        private bool exportBlendShape;
        public bool ExportBlendShape {
            get => this.exportBlendShape;
            set => _ = SetProperty(ref this.exportBlendShape, value);
        }

        private bool castToBone;
        public bool CastToBone {
            get => this.castToBone;
            set => _ = SetProperty(ref this.castToBone, value);
        }

        private int boneSize;
        public int BoneSize {
            get => this.boneSize;
            set => _ = SetProperty(ref this.boneSize, value);
        }

        private bool exportAllUvsAsDiffuseMaps;
        public bool ExportAllUvsAsDiffuseMaps {
            get => this.exportAllUvsAsDiffuseMaps;
            set => _ = SetProperty(ref this.exportAllUvsAsDiffuseMaps, value);
        }

        private float scaleFactor;
        public float ScaleFactor {
            get => this.scaleFactor;
            set => _ = SetProperty(ref this.scaleFactor, value);
        }

        private int fbxVersion;
        public int FbxVersion {
            get => this.fbxVersion;
            set => _ = SetProperty(ref this.fbxVersion, value);
        }

        private int fbxFormat;
        public int FbxFormat {
            get => this.fbxFormat;
            set => _ = SetProperty(ref this.fbxFormat, value);
        }
    }
}
