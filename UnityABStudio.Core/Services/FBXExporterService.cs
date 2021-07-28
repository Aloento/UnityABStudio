namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using System.Collections.Generic;
    using System.Linq;
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
            InitContext(is60Fps);

            if (!this.allNodes)
                SetFramePaths(SearchHierarchy());
        }

        public void ExportAll(bool blendShape, bool animation, bool eulerFilter, float filterPrecision) {
            var meshFrames = new List<ImportedFrame>();
            ExportFrame(meshFrames);

            if (imported.MeshList != null) {
                SetJointsFromImportedMeshes();
                AsFbxPrepareMaterials(pContext, imported.MaterialList.Count, imported.TextureList.Count);
                ExportMeshFrames(meshFrames);
            } else {
                SetJointsNode(imported.RootFrame, null, true);
            }

            if (blendShape)
                ExportMorphs();

            if (animation)
                ExportAnimations(eulerFilter, filterPrecision);

            ExportScene();
        }

        private void ExportMeshFrames(List<ImportedFrame> meshFrames) {
            foreach (var meshFrame in meshFrames) {
                var meshNode = frameToNode[meshFrame];
                var mesh = ImportedHelpers.FindMesh(meshFrame.Path, imported.MeshList);
                ExportMesh(imported.RootFrame, imported.MaterialList,
                           imported.TextureList, meshNode, mesh);
            }
        }

        private void SetJointsFromImportedMeshes() {
            if (!exportSkins)
                return;

            var bonePaths = new HashSet<string>();
            foreach (var bone in this.imported.MeshList.Select(mesh => mesh.BoneList)
                .Where(boneList => boneList != null).SelectMany(boneList => boneList)) {
                _ = bonePaths.Add(bone.Path);
            }
        }

        private HashSet<string> SearchHierarchy() {
            if (this.imported.MeshList == null || this.imported.MeshList.Count == 0)
                return null;

            this.SearchHierarchy(this.imported.RootFrame, this.imported.MeshList, out var exportFrames);
            return exportFrames;
        }

        private void SearchHierarchy(ImportedFrame rootFrame, List<ImportedMesh> meshList,
            out HashSet<string> exportFrames) {
            exportFrames = new HashSet<string>();
            var frameStack = new Stack<ImportedFrame>();
            frameStack.Push(rootFrame);

            while (frameStack.Count > 0) {
                var frame = frameStack.Pop();
                var meshListSome = ImportedHelpers.FindMesh(frame.Path, meshList);

                if (meshListSome != null) {
                    var parent = frame;
                    while (parent != null) {
                        _ = exportFrames.Add(parent.Path);
                        parent = parent.Parent;
                    }

                    var boneList = meshListSome.BoneList;
                    if (boneList != null) {
                        foreach (var bone in boneList) {
                            if (!exportFrames.Contains(bone.Path)) {
                                var boneParent = rootFrame.FindFrameByPath(bone.Path);

                                while (boneParent != null) {
                                    _ = exportFrames.Add(boneParent.Path);
                                    boneParent = boneParent.Parent;
                                }
                            }
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1) {
                    frameStack.Push(frame[i]);
                }
            }
        }
    }
}
