namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using System;
    using System.Collections.Generic;
    using Helpers;

    public partial class FBXHelpService {
        private IntPtr pContext;
        private Dictionary<ImportedFrame, IntPtr> frameToNode = new();
        private List<KeyValuePair<string, IntPtr>> createdMaterials = new();
        private Dictionary<string, IntPtr> createdTextures = new();

        private void InitContext(bool is60Fps) {
            pContext = AsFbxCreateContext();
            if (!AsFbxInitializeContext(pContext, fileName, scaleFactor, versionIndex,
                                        isAscii, is60Fps, out var errorMessage)) {
                throw new ApplicationException($"Failed to initialize FbxExporter: {errorMessage}");
            }
        }

    }
}
