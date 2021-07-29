namespace SoarCraft.QYun.UnityABStudio.Core.Services {
    using System;
    using System.Collections.Generic;
    using System.IO;
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
                var childNode = AsFbxExportSingleFrame(pContext, parentNode, frame.Path, frame.Name,
                    frame.LocalPosition, frame.LocalRotation, frame.LocalScale);

                if (imported.MeshList != null && ImportedHelpers.FindMesh(frame.Path, imported.MeshList) != null)
                    meshFrames.Add(frame);

                frameToNode.Add(frame, childNode);
                for (var i = frame.Count - 1; i >= 0; i -= 1) {
                    nodeStack.Push(childNode);
                    frameStack.Push(frame[i]);
                }
            }
        }

        private void SetJointsNode(ImportedFrame rootFrame, HashSet<string> bonePaths, bool castToBone) {
            var frameStack = new Stack<ImportedFrame>();
            frameStack.Push(rootFrame);

            while (frameStack.Count > 0) {
                var frame = frameStack.Pop();

                if (frameToNode.TryGetValue(frame, out var node)) {
                    if (node == IntPtr.Zero)
                        throw new NullReferenceException($"node：{node}");

                    if (castToBone) {
                        AsFbxSetJointsNode_CastToBone(pContext, node, boneSize);
                    } else {
                        if (bonePaths == null)
                            throw new NullReferenceException($"bonePaths：{bonePaths}");

                        if (bonePaths.Contains(frame.Path))
                            AsFbxSetJointsNode_BoneInPath(pContext, node, boneSize);
                        else
                            AsFbxSetJointsNode_Generic(pContext, node);
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1) {
                    frameStack.Push(frame[i]);
                }
            }
        }

        private void ExportMesh(ImportedFrame rootFrame, List<ImportedMaterial> materialList,
            List<ImportedTexture> textureList, IntPtr frameNode, ImportedMesh importedMesh) {
            var boneList = importedMesh.BoneList;
            var totalBoneCount = 0;
            var hasBones = false;

            if (this.exportSkins && boneList.Count > 0) {
                totalBoneCount = boneList.Count;
                hasBones = true;
            }

            var pClusterArray = IntPtr.Zero;
            if (hasBones) {
                pClusterArray = AsFbxMeshCreateClusterArray(totalBoneCount);

                foreach (var bone in boneList) {
                    if (bone.Path != null) {
                        var frame = rootFrame.FindFrameByPath(bone.Path);
                        var boneNode = this.frameToNode[frame];
                        var cluster = AsFbxMeshCreateCluster(pContext, boneNode);
                        AsFbxMeshAddCluster(pClusterArray, cluster);
                    } else {
                        AsFbxMeshAddCluster(pClusterArray, IntPtr.Zero);
                    }
                }
            }

            var mesh = AsFbxMeshCreateMesh(this.pContext, frameNode);
            AsFbxMeshInitControlPoints(mesh, importedMesh.VertexList.Count);

            if (importedMesh.hasNormal)
                AsFbxMeshCreateElementNormal(mesh);

            for (var i = 0; i < importedMesh.hasUV.Length; i++) {
                if (!importedMesh.hasUV[i])
                    continue;

                if (i == 1 && !this.exportAllUvsAsDiffuseMaps)
                    AsFbxMeshCreateNormalMapUV(mesh, 1);
                else
                    AsFbxMeshCreateDiffuseUV(mesh, i);
            }

            if (importedMesh.hasTangent)
                AsFbxMeshCreateElementTangent(mesh);

            if (importedMesh.hasColor)
                AsFbxMeshCreateElementVertexColor(mesh);

            AsFbxMeshCreateElementMaterial(mesh);

            foreach (var meshObj in importedMesh.SubmeshList) {
                var materialIndex = 0;
                var mat = ImportedHelpers.FindMaterial(meshObj.Material, materialList);

                if (mat != null) {
                    var foundMat = this.createdMaterials.FindIndex(kv => kv.Key == mat.Name);
                    IntPtr pMat;

                    if (foundMat >= 0) {
                        pMat = this.createdMaterials[foundMat].Value;
                    } else {
                        var diffuse = mat.Diffuse;
                        var ambient = mat.Ambient;
                        var emissive = mat.Emissive;
                        var specular = mat.Specular;
                        var reflection = mat.Reflection;

                        pMat = this.AsFbxCreateMaterial(this.pContext, mat.Name, diffuse, ambient, emissive,
                            specular, reflection, mat.Shininess, mat.Transparency);
                        this.createdMaterials.Add(new KeyValuePair<string, IntPtr>(mat.Name, pMat));
                    }

                    materialIndex = this.AsFbxAddMaterialToFrame(frameNode, pMat);
                    var hasTexture = false;

                    foreach (var texture in mat.Textures) {
                        var tex = ImportedHelpers.FindTexture(texture.Name, textureList);
                        var pTexture = this.ExportTexture(tex);

                        if (pTexture != IntPtr.Zero) {
                            switch (texture.Dest) {
                                case 0:
                                case 1:
                                case 2:
                                case 3: {
                                    this.AsFbxLinkTexture(texture.Dest, pTexture, pMat, texture.Offset.X,
                                        texture.Offset.Y,
                                        texture.Scale.X, texture.Scale.Y);
                                    hasTexture = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (hasTexture)
                        this.AsFbxSetFrameShadingModeToTextureShading(frameNode);
                }

                foreach (var face in meshObj.FaceList) {
                    var index0 = face.VertexIndices[0] + meshObj.BaseVertex;
                    var index1 = face.VertexIndices[1] + meshObj.BaseVertex;
                    var index2 = face.VertexIndices[2] + meshObj.BaseVertex;

                    this.AsFbxMeshAddPolygon(mesh, materialIndex, index0, index1, index2);
                }

                var vertexList = importedMesh.VertexList;
                var vertexCount = vertexList.Count;

                for (var j = 0; j < vertexCount; j++) {
                    var importedVertex = vertexList[j];
                    var vertex = importedVertex.Vertex;
                    this.AsFbxMeshSetControlPoint(mesh, j, vertex);

                    if (importedMesh.hasNormal)
                        AsFbxMeshElementNormalAdd(mesh, 0, importedVertex.Normal);

                    for (var uvI = 0; uvI < importedMesh.hasUV.Length; uvI++) {
                        if (importedMesh.hasUV[uvI]) {
                            var uv = importedVertex.UV[uvI];
                            this.AsFbxMeshElementUVAdd(mesh, uvI, uv[0], uv[1]);
                        }
                    }

                    if (importedMesh.hasTangent)
                        this.AsFbxMeshElementTangentAdd(mesh, 0, importedVertex.Tangent);

                    if (importedMesh.hasColor)
                        this.AsFbxMeshElementVertexColorAdd(mesh, 0, importedVertex.Color);

                    if (hasBones && importedVertex.BoneIndices != null) {
                        var boneIndices = importedVertex.BoneIndices;
                        var boneWeights = importedVertex.Weights;

                        for (var k = 0; k < 4; k++) {
                            if (boneIndices[k] < totalBoneCount && boneWeights[k] > 0)
                                this.AsFbxMeshSetBoneWeight(pClusterArray, boneIndices[k], j, boneWeights[k]);
                        }
                    }
                }

                if (hasBones) {
                    var pSkinContext = this.AsFbxMeshCreateSkinContext(this.pContext, frameNode);
                    var boneMatrix = new float[16];

                    for (var j = 0; j < totalBoneCount; j++) {
                        if (!this.FbxClusterArray_HasItemAt(pClusterArray, j))
                            continue;

                        var m = boneList[j].Matrix;
                        for (var mI = 0; mI < 4; mI += 1) {
                            for (var n = 0; n < 4; n += 1) {
                                var index = (4 * mI) + n;
                                boneMatrix[index] = m[mI, n];
                            }
                        }

                        this.AsFbxMeshSkinAddCluster(pSkinContext, pClusterArray, j, boneMatrix);
                    }

                    this.AsFbxMeshAddDeformer(pSkinContext, mesh);
                    this.AsFbxMeshDisposeSkinContext(pSkinContext);
                }
            }

            AsFbxMeshDisposeClusterArray(pClusterArray);
        }

        private IntPtr ExportTexture(ImportedTexture texture) {
            if (texture == null)
                return IntPtr.Zero;

            if (createdTextures.ContainsKey(texture.Name))
                return createdTextures[texture.Name];

            var pTex = AsFbxCreateTexture(pContext, texture.Name);
            createdTextures.Add(texture.Name, pTex);
            var file = new FileInfo(texture.Name);

            using var writer = new BinaryWriter(file.Create());
            writer.Write(texture.Data);

            return pTex;
        }

        private void ExportMorphs(ImportedFrame rootFrame, List<ImportedMorph> morphList) {
            if (morphList == null || morphList.Count == 0)
                return;

            foreach (var morph in morphList) {
                var frame = rootFrame.FindFrameByPath(morph.Path);
                if (frame == null)
                    continue;

                var pNode = this.frameToNode[frame];
                var pMorphContext = AsFbxMorphCreateContext();

                AsFbxMorphInitializeContext(this.pContext, pMorphContext, pNode);
                foreach (var channel in morph.Channels) {
                    AsFbxMorphAddBlendShapeChannel(this.pContext, pMorphContext, channel.Name);

                    for (var i = 0; i < channel.KeyframeList.Count; i++) {
                        var keyframe = channel.KeyframeList[i];

                        AsFbxMorphAddBlendShapeChannelShape(pContext, pMorphContext,
                            keyframe.Weight, i == 0 ? channel.Name : $"{channel.Name}_{i + 1}");
                        AsFbxMorphCopyBlendShapeControlPoints(pMorphContext);

                        foreach (var vertex in keyframe.VertexList) {
                            var v = vertex.Vertex.Vertex;
                            AsFbxMorphSetBlendShapeVertex(pMorphContext, vertex.Index, v);
                        }

                        if (keyframe.hasNormals) {
                            AsFbxMorphCopyBlendShapeControlPointsNormal(pMorphContext);

                            foreach (var vertex in keyframe.VertexList) {
                                var v = vertex.Vertex.Normal;
                                AsFbxMorphSetBlendShapeVertexNormal(pMorphContext, vertex.Index, v);
                            }
                        }
                    }
                }

                AsFbxMorphDisposeContext(pMorphContext);
            }
        }

        private void ExportAnimations(ImportedFrame rootFrame, List<ImportedKeyframedAnimation> animationList,
            bool eulerFilter, float filterPrecision) {
            if (animationList == null || animationList.Count == 0)
                return;

            var pAnimContext = AsFbxAnimCreateContext(eulerFilter);
            for (var i = 0; i < animationList.Count; i++) {
                var importedAnimation = animationList[i];
                var takeName = importedAnimation.Name ?? $"Take{i}";

                AsFbxAnimPrepareStackAndLayer(pContext, pAnimContext, takeName);
                ExportKeyframedAnimation(rootFrame, importedAnimation, pAnimContext, filterPrecision);
            }

            AsFbxAnimDisposeContext(pAnimContext);
        }

        private void ExportKeyframedAnimation(ImportedFrame rootFrame, ImportedKeyframedAnimation parser,
                                              IntPtr pAnimContext, float filterPrecision) {
            foreach (var track in parser.TrackList) {
                if (track.Path == null)
                    continue;

                var frame = rootFrame.FindFrameByPath(track.Path);
                if (frame == null)
                    continue;

                var pNode = frameToNode[frame];
                AsFbxAnimLoadCurves(pNode, pAnimContext);
                AsFbxAnimBeginKeyModify(pAnimContext);

                foreach (var scaling in track.Scalings) {
                    var value = scaling.value;
                    AsFbxAnimAddScalingKey(pAnimContext, scaling.time, value);
                }

                foreach (var rotation in track.Rotations) {
                    var value = rotation.value;
                    AsFbxAnimAddRotationKey(pAnimContext, rotation.time, value);
                }

                foreach (var translation in track.Translations) {
                    var value = translation.value;
                    AsFbxAnimAddTranslationKey(pAnimContext, translation.time, value);
                }

                AsFbxAnimEndKeyModify(pAnimContext);
                AsFbxAnimApplyEulerFilter(pAnimContext, filterPrecision);

                var blendShape = track.BlendShape;
                if (blendShape != null) {
                    var channelCount = AsFbxAnimGetCurrentBlendShapeChannelCount(pAnimContext, pNode);

                    if (channelCount > 0) {
                        for (var channelIndex = 0; channelIndex < channelCount; channelIndex += 1) {
                            if (!AsFbxAnimIsBlendShapeChannelMatch(pAnimContext, channelIndex, blendShape.ChannelName))
                                continue;

                            AsFbxAnimBeginBlendShapeAnimCurve(pAnimContext, channelIndex);

                            foreach (var keyframe in blendShape.Keyframes) {
                                AsFbxAnimAddBlendShapeKeyframe(pAnimContext, keyframe.time, keyframe.value);
                            }

                            AsFbxAnimEndBlendShapeAnimCurve(pAnimContext);
                        }
                    }
                }
            }
        }
    }
}
