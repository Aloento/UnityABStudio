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

        private void SetFramePaths(HashSet<string> framePaths) {
            if (framePaths == null || framePaths.Count == 0)
                return;

            var framePathList = new List<string>(framePaths);
            var framePathArray = framePathList.ToArray();

            AsFbxSetFramePaths(pContext, framePathArray);
        }

        private void ExportFrame(List<ImportedFrame> meshFrames) {
            var rootNode = AsFbxGetSceneRootNode(this.pContext);
            if (rootNode == IntPtr.Zero)
                throw new NullReferenceException($"rootNode: {rootNode}");

            var nodeStack = new Stack<IntPtr>();
            var frameStack = new Stack<ImportedFrame>();

            nodeStack.Push(rootNode);
            frameStack.Push(imported.RootFrame);

            while (nodeStack.Count > 0) {
                var parentNode = nodeStack.Pop();
                var frame = frameStack.Pop();
                var childNode = AsFbxExportSingleFrame(pContext, parentNode, frame.Path, frame.Name, frame.LocalPosition, frame.LocalRotation, frame.LocalScale);

                if (imported.MeshList != null && ImportedHelpers.FindMesh(frame.Path, imported.MeshList) != null)
                    meshFrames.Add(frame);

                frameToNode.Add(frame, childNode);
                for (var i = frame.Count - 1; i >= 0; i -= 1) {
                    nodeStack.Push(childNode);
                    frameStack.Push(frame[i]);
                }
            }
        }
    }
}
