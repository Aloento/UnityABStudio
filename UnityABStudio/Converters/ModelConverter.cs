namespace SoarCraft.QYun.UnityABStudio.Converters {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AssetReader.Entities.Enums;
    using AssetReader.Math;
    using AssetReader.SevenZip.Common;
    using AssetReader.Unity3D.Contracts;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.AnimationClips;
    using AssetReader.Unity3D.Objects.AnimatorControllers;
    using AssetReader.Unity3D.Objects.AnimatorOverrideControllers;
    using AssetReader.Unity3D.Objects.Avatars;
    using AssetReader.Unity3D.Objects.Materials;
    using AssetReader.Unity3D.Objects.Meshes;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using Core.Entities;
    using Core.Helpers;
    using Extensions;

    public class ModelConverter : IImported {
        public ImportedFrame RootFrame { get; protected set; }
        public List<ImportedMesh> MeshList { get; protected set; } = new();
        public List<ImportedMaterial> MaterialList { get; protected set; } = new();
        public List<ImportedTexture> TextureList { get; protected set; } = new();
        public List<ImportedKeyframedAnimation> AnimationList { get; protected set; } = new();
        public List<ImportedMorph> MorphList { get; protected set; } = new();

        private ImageFormat imageFormat;
        private Avatar avatar;
        private HashSet<AnimationClip> animationClipHashSet = new();
        private Dictionary<AnimationClip, string> boundAnimationPathDic = new();
        private Dictionary<uint, string> bonePathHash = new();
        private Dictionary<Texture2D, string> textureNameDictionary = new();
        private Dictionary<Transform, ImportedFrame> transformDictionary = new();
        Dictionary<uint, string> morphChannelNames = new();

        public ModelConverter(GameObject m_GameObject, ImageFormat imageFormat, AnimationClip[] animationList = null) {
            this.imageFormat = imageFormat;
            if (m_GameObject.m_Animator != null) {
                this.InitWithAnimator(m_GameObject.m_Animator);
                if (animationList == null) {
                    this.CollectAnimationClip(m_GameObject.m_Animator);
                }
            } else {
                this.InitWithGameObject(m_GameObject);
            }
            if (animationList != null) {
                foreach (var animationClip in animationList) {
                    _ = this.animationClipHashSet.Add(animationClip);
                }
            }
            this.ConvertAnimations();
        }

        public ModelConverter(string rootName, List<GameObject> m_GameObjects, ImageFormat imageFormat, AnimationClip[] animationList = null) {
            this.imageFormat = imageFormat;
            this.RootFrame = CreateFrame(rootName, Vector3.Zero, new Quaternion(0, 0, 0, 0), Vector3.One);
            foreach (var m_GameObject in m_GameObjects) {
                if (m_GameObject.m_Animator != null && animationList == null) {
                    this.CollectAnimationClip(m_GameObject.m_Animator);
                }

                var m_Transform = m_GameObject.m_Transform;
                this.ConvertTransforms(m_Transform, this.RootFrame);
                this.CreateBonePathHash(m_Transform);
            }
            foreach (var m_Transform in m_GameObjects.Select(m_GameObject => m_GameObject.m_Transform)) {
                this.ConvertMeshRenderer(m_Transform);
            }
            if (animationList != null) {
                foreach (var animationClip in animationList) {
                    _ = this.animationClipHashSet.Add(animationClip);
                }
            }
            this.ConvertAnimations();
        }

        public ModelConverter(Animator m_Animator, ImageFormat imageFormat, AnimationClip[] animationList = null) {
            this.imageFormat = imageFormat;
            this.InitWithAnimator(m_Animator);
            if (animationList == null) {
                this.CollectAnimationClip(m_Animator);
            } else {
                foreach (var animationClip in animationList) {
                    _ = this.animationClipHashSet.Add(animationClip);
                }
            }
            this.ConvertAnimations();
        }

        private void InitWithAnimator(Animator m_Animator) {
            if (m_Animator.m_Avatar.TryGet(out var m_Avatar))
                this.avatar = m_Avatar;

            _ = m_Animator.m_GameObject.TryGet(out var m_GameObject);
            this.InitWithGameObject(m_GameObject, m_Animator.m_HasTransformHierarchy);
        }

        private void InitWithGameObject(GameObject m_GameObject, bool hasTransformHierarchy = true) {
            var m_Transform = m_GameObject.m_Transform;
            if (!hasTransformHierarchy) {
                ConvertTransforms(m_Transform, null);
                this.DeoptimizeTransformHierarchy();
            } else {
                var frameList = new List<ImportedFrame>();
                var tempTransform = m_Transform;
                while (tempTransform.m_Father.TryGet(out var m_Father)) {
                    frameList.Add(this.ConvertTransform(m_Father));
                    tempTransform = m_Father;
                }
                if (frameList.Count > 0) {
                    this.RootFrame = frameList[^1];
                    for (var i = frameList.Count - 2; i >= 0; i--) {
                        var frame = frameList[i];
                        var parent = frameList[i + 1];
                        parent.AddChild(frame);
                    }
                    this.ConvertTransforms(m_Transform, frameList[0]);
                } else {
                    ConvertTransforms(m_Transform, null);
                }

                this.CreateBonePathHash(m_Transform);
            }

            this.ConvertMeshRenderer(m_Transform);
        }

        private void ConvertMeshRenderer(Transform m_Transform) {
            _ = m_Transform.m_GameObject.TryGet(out var m_GameObject);

            if (m_GameObject.m_MeshRenderer != null) {
                ConvertMeshRenderer(m_GameObject.m_MeshRenderer);
            }

            if (m_GameObject.m_SkinnedMeshRenderer != null) {
                ConvertMeshRenderer(m_GameObject.m_SkinnedMeshRenderer);
            }

            if (m_GameObject.m_Animation != null) {
                foreach (var animation in m_GameObject.m_Animation.m_Animations) {
                    if (animation.TryGet(out var animationClip)) {
                        this.boundAnimationPathDic.Add(animationClip, this.GetTransformPath(m_Transform));
                        _ = this.animationClipHashSet.Add(animationClip);
                    }
                }
            }

            foreach (var pptr in m_Transform.m_Children) {
                if (pptr.TryGet(out var child))
                    this.ConvertMeshRenderer(child);
            }
        }

        private void CollectAnimationClip(Animator m_Animator) {
            if (m_Animator.m_Controller.TryGet(out var m_Controller)) {
                switch (m_Controller) {
                    case AnimatorOverrideController m_AnimatorOverrideController: {
                        if (m_AnimatorOverrideController.m_Controller.TryGet<AnimatorController>(out var m_AnimatorController)) {
                            foreach (var pptr in m_AnimatorController.m_AnimationClips) {
                                if (pptr.TryGet(out var m_AnimationClip)) {
                                    _ = this.animationClipHashSet.Add(m_AnimationClip);
                                }
                            }
                        }
                        break;
                    }

                    case AnimatorController m_AnimatorController: {
                        foreach (var pptr in m_AnimatorController.m_AnimationClips) {
                            if (pptr.TryGet(out var m_AnimationClip)) {
                                _ = this.animationClipHashSet.Add(m_AnimationClip);
                            }
                        }
                        break;
                    }
                }
            }
        }

        private ImportedFrame ConvertTransform(Transform trans) {
            var frame = new ImportedFrame(trans.m_Children.Length);
            this.transformDictionary.Add(trans, frame);
            _ = trans.m_GameObject.TryGet(out var m_GameObject);
            frame.Name = m_GameObject.m_Name;
            SetFrame(frame, trans.m_LocalPosition, trans.m_LocalRotation, trans.m_LocalScale);
            return frame;
        }

        private static ImportedFrame CreateFrame(string name, Vector3 t, Quaternion q, Vector3 s) {
            var frame = new ImportedFrame {
                Name = name
            };
            SetFrame(frame, t, q, s);
            return frame;
        }

        private static void SetFrame(ImportedFrame frame, Vector3 t, Quaternion q, Vector3 s) {
            frame.LocalPosition = new Vector3(-t.X, t.Y, t.Z);
            frame.LocalRotation = Fbx.QuaternionToEuler(new Quaternion(q.X, -q.Y, -q.Z, q.W));
            frame.LocalScale = s;
        }

        private void ConvertTransforms(Transform trans, ImportedFrame parent) {
            var frame = this.ConvertTransform(trans);
            if (parent == null) {
                this.RootFrame = frame;
            } else {
                parent.AddChild(frame);
            }
            foreach (var pptr in trans.m_Children) {
                if (pptr.TryGet(out var child))
                    this.ConvertTransforms(child, frame);
            }
        }

        private void ConvertMeshRenderer(Renderer meshR) {
            var mesh = GetMesh(meshR);
            if (mesh == null)
                return;
            var iMesh = new ImportedMesh();
            _ = meshR.m_GameObject.TryGet(out var m_GameObject2);
            iMesh.Path = this.GetTransformPath(m_GameObject2.m_Transform);
            iMesh.SubmeshList = new List<ImportedSubmesh>();
            var subHashSet = new HashSet<int>();
            var combine = false;
            var firstSubMesh = 0;
            if (meshR.m_StaticBatchInfo?.subMeshCount > 0) {
                firstSubMesh = meshR.m_StaticBatchInfo.firstSubMesh;
                var finalSubMesh = meshR.m_StaticBatchInfo.firstSubMesh + meshR.m_StaticBatchInfo.subMeshCount;
                for (int i = meshR.m_StaticBatchInfo.firstSubMesh; i < finalSubMesh; i++) {
                    _ = subHashSet.Add(i);
                }
                combine = true;
            } else if (meshR.m_SubsetIndices?.Length > 0) {
                firstSubMesh = (int)meshR.m_SubsetIndices.Min(x => x);
                foreach (var index in meshR.m_SubsetIndices) {
                    _ = subHashSet.Add((int)index);
                }
                combine = true;
            }

            iMesh.hasNormal = mesh.m_Normals?.Length > 0;
            iMesh.hasUV = new bool[8];
            for (var uv = 0; uv < 8; uv++) {
                iMesh.hasUV[uv] = mesh.GetUV(uv)?.Length > 0;
            }
            iMesh.hasTangent = mesh.m_Tangents != null && mesh.m_Tangents.Length == mesh.m_VertexCount * 4;
            iMesh.hasColor = mesh.m_Colors?.Length > 0;

            var firstFace = 0;
            for (var i = 0; i < mesh.m_SubMeshes.Length; i++) {
                var numFaces = (int)mesh.m_SubMeshes[i].indexCount / 3;
                if (subHashSet.Count > 0 && !subHashSet.Contains(i)) {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.m_SubMeshes[i];
                var iSubmesh = new ImportedSubmesh();
                Material mat = null;
                if (i - firstSubMesh < meshR.m_Materials.Length) {
                    if (meshR.m_Materials[i - firstSubMesh].TryGet(out var m_Material)) {
                        mat = m_Material;
                    }
                }
                var iMat = this.ConvertMaterial(mat);
                iSubmesh.Material = iMat.Name;
                iSubmesh.BaseVertex = (int)mesh.m_SubMeshes[i].firstVertex;

                //Face
                iSubmesh.FaceList = new List<ImportedFace>(numFaces);
                var end = firstFace + numFaces;
                for (var f = firstFace; f < end; f++) {
                    var face = new ImportedFace {
                        VertexIndices = new int[3]
                    };
                    face.VertexIndices[0] = (int)(mesh.m_Indices[f * 3 + 2] - submesh.firstVertex);
                    face.VertexIndices[1] = (int)(mesh.m_Indices[f * 3 + 1] - submesh.firstVertex);
                    face.VertexIndices[2] = (int)(mesh.m_Indices[f * 3] - submesh.firstVertex);
                    iSubmesh.FaceList.Add(face);
                }
                firstFace = end;

                iMesh.SubmeshList.Add(iSubmesh);
            }

            // Shared vertex list
            iMesh.VertexList = new List<ImportedVertex>(mesh.m_VertexCount);
            for (var j = 0; j < mesh.m_VertexCount; j++) {
                var iVertex = new ImportedVertex();
                //Vertices
                var c = 3;
                if (mesh.m_Vertices.Length == mesh.m_VertexCount * 4) {
                    c = 4;
                }
                iVertex.Vertex = new Vector3(-mesh.m_Vertices[j * c], mesh.m_Vertices[j * c + 1], mesh.m_Vertices[j * c + 2]);
                //Normals
                if (iMesh.hasNormal) {
                    if (mesh.m_Normals.Length == mesh.m_VertexCount * 3) {
                        c = 3;
                    } else if (mesh.m_Normals.Length == mesh.m_VertexCount * 4) {
                        c = 4;
                    }
                    iVertex.Normal = new Vector3(-mesh.m_Normals[j * c], mesh.m_Normals[j * c + 1], mesh.m_Normals[j * c + 2]);
                }
                //UV
                iVertex.UV = new float[8][];
                for (var uv = 0; uv < 8; uv++) {
                    if (iMesh.hasUV[uv]) {
                        var m_UV = mesh.GetUV(uv);
                        if (m_UV.Length == mesh.m_VertexCount * 2) {
                            c = 2;
                        } else if (m_UV.Length == mesh.m_VertexCount * 3) {
                            c = 3;
                        }
                        iVertex.UV[uv] = new[] { m_UV[j * c], m_UV[j * c + 1] };
                    }
                }
                //Tangent
                if (iMesh.hasTangent) {
                    iVertex.Tangent = new Vector4(-mesh.m_Tangents[j * 4], mesh.m_Tangents[j * 4 + 1], mesh.m_Tangents[j * 4 + 2], mesh.m_Tangents[j * 4 + 3]);
                }
                //Colors
                if (iMesh.hasColor) {
                    iVertex.Color = mesh.m_Colors.Length == mesh.m_VertexCount * 3 ? new Color(mesh.m_Colors[j * 3], mesh.m_Colors[j * 3 + 1], mesh.m_Colors[j * 3 + 2], 1.0f) : new Color(mesh.m_Colors[j * 4], mesh.m_Colors[j * 4 + 1], mesh.m_Colors[j * 4 + 2], mesh.m_Colors[j * 4 + 3]);
                }
                //BoneInfluence
                if (mesh.m_Skin?.Length > 0) {
                    var inf = mesh.m_Skin[j];
                    iVertex.BoneIndices = new int[4];
                    iVertex.Weights = new float[4];
                    for (var k = 0; k < 4; k++) {
                        iVertex.BoneIndices[k] = inf.boneIndex[k];
                        iVertex.Weights[k] = inf.weight[k];
                    }
                }
                iMesh.VertexList.Add(iVertex);
            }

            if (meshR is SkinnedMeshRenderer sMesh) {
                //Bone
                /*
                 * 0 - None
                 * 1 - m_Bones
                 * 2 - m_BoneNameHashes
                 */
                var boneType = 0;
                if (sMesh.m_Bones.Length > 0) {
                    if (sMesh.m_Bones.Length == mesh.m_BindPose.Length) {
                        var verifiedBoneCount = sMesh.m_Bones.Count(x => x.TryGet(out _));
                        if (verifiedBoneCount > 0) {
                            boneType = 1;
                        }
                        if (verifiedBoneCount != sMesh.m_Bones.Length) {
                            //尝试使用m_BoneNameHashes 4.3 and up
                            if (mesh.m_BindPose.Length > 0 && (mesh.m_BindPose.Length == mesh.m_BoneNameHashes?.Length)) {
                                //有效bone数量是否大于SkinnedMeshRenderer
                                var verifiedBoneCount2 = mesh.m_BoneNameHashes.Count(x => this.FixBonePath(this.GetPathFromHash(x)) != null);
                                if (verifiedBoneCount2 > verifiedBoneCount) {
                                    boneType = 2;
                                }
                            }
                        }
                    }
                }
                if (boneType == 0) {
                    //尝试使用m_BoneNameHashes 4.3 and up
                    if (mesh.m_BindPose.Length > 0 && (mesh.m_BindPose.Length == mesh.m_BoneNameHashes?.Length)) {
                        var verifiedBoneCount = mesh.m_BoneNameHashes.Count(x => this.FixBonePath(this.GetPathFromHash(x)) != null);
                        if (verifiedBoneCount > 0) {
                            boneType = 2;
                        }
                    }
                }

                switch (boneType) {
                    case 1: {
                        var boneCount = sMesh.m_Bones.Length;
                        iMesh.BoneList = new List<ImportedBone>(boneCount);
                        for (var i = 0; i < boneCount; i++) {
                            var bone = new ImportedBone();
                            if (sMesh.m_Bones[i].TryGet(out var m_Transform)) {
                                bone.Path = this.GetTransformPath(m_Transform);
                            }
                            var convert = Matrix4x4.Scale(new Vector3(-1, 1, 1));
                            bone.Matrix = convert * mesh.m_BindPose[i] * convert;
                            iMesh.BoneList.Add(bone);
                        }

                        break;
                    }
                    case 2: {
                        var boneCount = mesh.m_BindPose.Length;
                        iMesh.BoneList = new List<ImportedBone>(boneCount);
                        for (var i = 0; i < boneCount; i++) {
                            var bone = new ImportedBone();
                            var boneHash = mesh.m_BoneNameHashes[i];
                            var path = this.GetPathFromHash(boneHash);
                            bone.Path = this.FixBonePath(path);
                            var convert = Matrix4x4.Scale(new Vector3(-1, 1, 1));
                            bone.Matrix = convert * mesh.m_BindPose[i] * convert;
                            iMesh.BoneList.Add(bone);
                        }

                        break;
                    }
                }

                //Morphs
                if (mesh.m_Shapes?.channels?.Length > 0) {
                    var morph = new ImportedMorph();
                    this.MorphList.Add(morph);
                    morph.Path = iMesh.Path;
                    morph.Channels = new List<ImportedMorphChannel>(mesh.m_Shapes.channels.Length);
                    foreach (var shapeChannel in mesh.m_Shapes.channels) {
                        var channel = new ImportedMorphChannel();
                        morph.Channels.Add(channel);

                        var blendShapeName = "blendShape." + shapeChannel.name;
                        var crc = new CRC();
                        var bytes = Encoding.UTF8.GetBytes(blendShapeName);
                        crc.Update(bytes, 0, (uint)bytes.Length);
                        this.morphChannelNames[crc.GetDigest()] = blendShapeName;

                        channel.Name = shapeChannel.name.Split('.').Last();
                        channel.KeyframeList = new List<ImportedMorphKeyframe>(shapeChannel.frameCount);
                        var frameEnd = shapeChannel.frameIndex + shapeChannel.frameCount;
                        for (var frameIdx = shapeChannel.frameIndex; frameIdx < frameEnd; frameIdx++) {
                            var keyframe = new ImportedMorphKeyframe();
                            channel.KeyframeList.Add(keyframe);
                            keyframe.Weight = mesh.m_Shapes.fullWeights[frameIdx];
                            var shape = mesh.m_Shapes.shapes[frameIdx];
                            keyframe.hasNormals = shape.hasNormals;
                            keyframe.hasTangents = shape.hasTangents;
                            keyframe.VertexList = new List<ImportedMorphVertex>((int)shape.vertexCount);
                            var vertexEnd = shape.firstVertex + shape.vertexCount;
                            for (var j = shape.firstVertex; j < vertexEnd; j++) {
                                var destVertex = new ImportedMorphVertex();
                                keyframe.VertexList.Add(destVertex);
                                var morphVertex = mesh.m_Shapes.vertices[j];
                                destVertex.Index = morphVertex.index;
                                var sourceVertex = iMesh.VertexList[(int)morphVertex.index];
                                destVertex.Vertex = new ImportedVertex();
                                var morphPos = morphVertex.vertex;
                                destVertex.Vertex.Vertex = sourceVertex.Vertex + new Vector3(-morphPos.X, morphPos.Y, morphPos.Z);
                                if (shape.hasNormals) {
                                    var morphNormal = morphVertex.normal;
                                    destVertex.Vertex.Normal = new Vector3(-morphNormal.X, morphNormal.Y, morphNormal.Z);
                                }
                                if (shape.hasTangents) {
                                    var morphTangent = morphVertex.tangent;
                                    destVertex.Vertex.Tangent = new Vector4(-morphTangent.X, morphTangent.Y, morphTangent.Z, 0);
                                }
                            }
                        }
                    }
                }
            }

            //TODO combine mesh
            if (combine) {
                _ = meshR.m_GameObject.TryGet(out var m_GameObject);
                var frame = this.RootFrame.FindChild(m_GameObject.m_Name);
                if (frame != null) {
                    frame.LocalPosition = this.RootFrame.LocalPosition;
                    frame.LocalRotation = this.RootFrame.LocalRotation;
                    while (frame.Parent != null) {
                        frame = frame.Parent;
                        frame.LocalPosition = this.RootFrame.LocalPosition;
                        frame.LocalRotation = this.RootFrame.LocalRotation;
                    }
                }
            }

            this.MeshList.Add(iMesh);
        }

        private static Mesh GetMesh(Renderer meshR) {
            if (meshR is SkinnedMeshRenderer sMesh) {
                if (sMesh.m_Mesh.TryGet(out var m_Mesh)) {
                    return m_Mesh;
                }
            } else {
                _ = meshR.m_GameObject.TryGet(out var m_GameObject);
                if (m_GameObject.m_MeshFilter != null) {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh)) {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }

        private string GetTransformPath(Transform transform) => this.transformDictionary.TryGetValue(transform, out var frame) ? frame.Path : null;

        private string FixBonePath(AnimationClip m_AnimationClip, string path) {
            if (this.boundAnimationPathDic.TryGetValue(m_AnimationClip, out var basePath)) {
                path = basePath + "/" + path;
            }
            return this.FixBonePath(path);
        }

        private string FixBonePath(string path) {
            var frame = this.RootFrame.FindFrameByPath(path);
            return frame?.Path;
        }

        private static string GetTransformPathByFather(Transform transform) {
            _ = transform.m_GameObject.TryGet(out var m_GameObject);
            if (transform.m_Father.TryGet(out var father)) {
                return GetTransformPathByFather(father) + "/" + m_GameObject.m_Name;
            }

            return m_GameObject.m_Name;
        }

        private ImportedMaterial ConvertMaterial(Material mat) {
            ImportedMaterial iMat;
            if (mat != null) {
                iMat = ImportedHelpers.FindMaterial(mat.m_Name, this.MaterialList);
                if (iMat != null) {
                    return iMat;
                }
                iMat = new ImportedMaterial {
                    Name = mat.m_Name,
                    //default values
                    Diffuse = new Color(0.8f, 0.8f, 0.8f, 1),
                    Ambient = new Color(0.2f, 0.2f, 0.2f, 1),
                    Emissive = new Color(0, 0, 0, 1),
                    Specular = new Color(0.2f, 0.2f, 0.2f, 1),
                    Reflection = new Color(0, 0, 0, 1),
                    Shininess = 20f,
                    Transparency = 0f
                };
                foreach (var col in mat.m_SavedProperties.m_Colors) {
                    switch (col.Key) {
                        case "_Color":
                            iMat.Diffuse = col.Value;
                            break;
                        case "_SColor":
                            iMat.Ambient = col.Value;
                            break;
                        case "_EmissionColor":
                            iMat.Emissive = col.Value;
                            break;
                        case "_SpecularColor":
                            iMat.Specular = col.Value;
                            break;
                        case "_ReflectColor":
                            iMat.Reflection = col.Value;
                            break;
                    }
                }

                foreach (var flt in mat.m_SavedProperties.m_Floats) {
                    switch (flt.Key) {
                        case "_Shininess":
                            iMat.Shininess = flt.Value;
                            break;
                        case "_Transparency":
                            iMat.Transparency = flt.Value;
                            break;
                    }
                }

                //textures
                iMat.Textures = new List<ImportedMaterialTexture>();
                foreach (var texEnv in mat.m_SavedProperties.m_TexEnvs) {
                    if (!texEnv.Value.m_Texture.TryGet<Texture2D>(out var m_Texture2D)) //TODO other Texture
                    {
                        continue;
                    }

                    var texture = new ImportedMaterialTexture();
                    iMat.Textures.Add(texture);

                    var dest = -1;
                    switch (texEnv.Key) {
                        case "_MainTex":
                            dest = 0;
                            break;
                        case "_BumpMap":
                            dest = 3;
                            break;
                        default: {
                            if (texEnv.Key.Contains("Specular"))
                                dest = 2;
                            else if (texEnv.Key.Contains("Normal"))
                                dest = 1;
                            break;
                        }
                    }

                    texture.Dest = dest;

                    var ext = $".{this.imageFormat.ToString().ToLower()}";
                    if (this.textureNameDictionary.TryGetValue(m_Texture2D, out var textureName)) {
                        texture.Name = textureName;
                    } else if (ImportedHelpers.FindTexture(m_Texture2D.m_Name + ext, this.TextureList) != null) //已有相同名字的图片
                    {
                        for (var i = 1; ; i++) {
                            var name = m_Texture2D.m_Name + $" ({i}){ext}";
                            if (ImportedHelpers.FindTexture(name, this.TextureList) == null) {
                                texture.Name = name;
                                this.textureNameDictionary.Add(m_Texture2D, name);
                                break;
                            }
                        }
                    } else {
                        texture.Name = m_Texture2D.m_Name + ext;
                        this.textureNameDictionary.Add(m_Texture2D, texture.Name);
                    }

                    texture.Offset = texEnv.Value.m_Offset;
                    texture.Scale = texEnv.Value.m_Scale;
                    this.ConvertTexture2D(m_Texture2D, texture.Name);
                }

                this.MaterialList.Add(iMat);
            } else {
                iMat = new ImportedMaterial();
            }
            return iMat;
        }

        private void ConvertTexture2D(Texture2D m_Texture2D, string name) {
            var iTex = ImportedHelpers.FindTexture(name, this.TextureList);
            if (iTex != null) {
                return;
            }

            var stream = m_Texture2D.ConvertToStream(this.imageFormat, true);
            if (stream != null) {
                using (stream) {
                    iTex = new ImportedTexture(stream, name);
                    this.TextureList.Add(iTex);
                }
            }
        }

        private void ConvertAnimations() {
            foreach (var animationClip in this.animationClipHashSet) {
                var iAnim = new ImportedKeyframedAnimation();
                var name = animationClip.m_Name;
                if (this.AnimationList.Exists(x => x.Name == name)) {
                    for (var i = 1; ; i++) {
                        var fixName = name + $"_{i}";
                        if (!this.AnimationList.Exists(x => x.Name == fixName)) {
                            name = fixName;
                            break;
                        }
                    }
                }
                iAnim.Name = name;
                iAnim.SampleRate = animationClip.m_SampleRate;
                iAnim.TrackList = new List<ImportedAnimationKeyframedTrack>();
                this.AnimationList.Add(iAnim);
                if (animationClip.m_Legacy) {
                    foreach (var m_CompressedRotationCurve in animationClip.m_CompressedRotationCurves) {
                        var track = iAnim.FindTrack(this.FixBonePath(animationClip, m_CompressedRotationCurve.m_Path));

                        var numKeys = m_CompressedRotationCurve.m_Times.m_NumItems;
                        var data = m_CompressedRotationCurve.m_Times.UnpackInts();
                        var times = new float[numKeys];
                        var t = 0;
                        for (var i = 0; i < numKeys; i++) {
                            t += data[i];
                            times[i] = t * 0.01f;
                        }
                        var quats = m_CompressedRotationCurve.m_Values.UnpackQuats();

                        for (var i = 0; i < numKeys; i++) {
                            var quat = quats[i];
                            var value = Fbx.QuaternionToEuler(new Quaternion(quat.X, -quat.Y, -quat.Z, quat.W));
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(times[i], value));
                        }
                    }
                    foreach (var m_RotationCurve in animationClip.m_RotationCurves) {
                        var track = iAnim.FindTrack(this.FixBonePath(animationClip, m_RotationCurve.path));
                        foreach (var m_Curve in m_RotationCurve.curve.m_Curve) {
                            var value = Fbx.QuaternionToEuler(new Quaternion(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z, m_Curve.value.W));
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, value));
                        }
                    }
                    foreach (var m_PositionCurve in animationClip.m_PositionCurves) {
                        var track = iAnim.FindTrack(this.FixBonePath(animationClip, m_PositionCurve.path));
                        foreach (var m_Curve in m_PositionCurve.curve.m_Curve) {
                            track.Translations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, new Vector3(-m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z)));
                        }
                    }
                    foreach (var m_ScaleCurve in animationClip.m_ScaleCurves) {
                        var track = iAnim.FindTrack(this.FixBonePath(animationClip, m_ScaleCurve.path));
                        foreach (var m_Curve in m_ScaleCurve.curve.m_Curve) {
                            track.Scalings.Add(new ImportedKeyframe<Vector3>(m_Curve.time, new Vector3(m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z)));
                        }
                    }
                    if (animationClip.m_EulerCurves != null) {
                        foreach (var m_EulerCurve in animationClip.m_EulerCurves) {
                            var track = iAnim.FindTrack(this.FixBonePath(animationClip, m_EulerCurve.path));
                            foreach (var m_Curve in m_EulerCurve.curve.m_Curve) {
                                track.Rotations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, new Vector3(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z)));
                            }
                        }
                    }
                    foreach (var m_FloatCurve in animationClip.m_FloatCurves) {
                        if (m_FloatCurve.classID == ClassIDType.SkinnedMeshRenderer) //BlendShape
                        {
                            var channelName = m_FloatCurve.attribute;
                            var dotPos = channelName.IndexOf('.');
                            if (dotPos >= 0) {
                                channelName = channelName[(dotPos + 1)..];
                            }

                            var path = this.FixBonePath(animationClip, m_FloatCurve.path);
                            if (string.IsNullOrEmpty(path)) {
                                path = this.GetPathByChannelName(channelName);
                            }
                            var track = iAnim.FindTrack(path);
                            track.BlendShape = new ImportedBlendShape {
                                ChannelName = channelName
                            };
                            foreach (var m_Curve in m_FloatCurve.curve.m_Curve) {
                                track.BlendShape.Keyframes.Add(new ImportedKeyframe<float>(m_Curve.time, m_Curve.value));
                            }
                        }
                    }
                } else {
                    var m_Clip = animationClip.m_MuscleClip.m_Clip;
                    var streamedFrames = m_Clip.m_StreamedClip.ReadData();
                    var m_ClipBindingConstant = animationClip.m_ClipBindingConstant ?? m_Clip.ConvertValueArrayToGenericBinding();
                    for (var frameIndex = 1; frameIndex < streamedFrames.Count - 1; frameIndex++) {
                        var frame = streamedFrames[frameIndex];
                        var streamedValues = frame.keyList.Select(x => x.value).ToArray();
                        for (var curveIndex = 0; curveIndex < frame.keyList.Length;) {
                            this.ReadCurveData(iAnim, m_ClipBindingConstant, frame.keyList[curveIndex].index, frame.time, streamedValues, 0, ref curveIndex);
                        }
                    }
                    var m_DenseClip = m_Clip.m_DenseClip;
                    var streamCount = m_Clip.m_StreamedClip.curveCount;
                    for (var frameIndex = 0; frameIndex < m_DenseClip.m_FrameCount; frameIndex++) {
                        var time = m_DenseClip.m_BeginTime + frameIndex / m_DenseClip.m_SampleRate;
                        var frameOffset = frameIndex * m_DenseClip.m_CurveCount;
                        for (var curveIndex = 0; curveIndex < m_DenseClip.m_CurveCount;) {
                            var index = streamCount + curveIndex;
                            this.ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time, m_DenseClip.m_SampleArray, (int)frameOffset, ref curveIndex);
                        }
                    }
                    if (m_Clip.m_ConstantClip != null) {
                        var m_ConstantClip = m_Clip.m_ConstantClip;
                        var denseCount = m_Clip.m_DenseClip.m_CurveCount;
                        var time2 = 0.0f;
                        for (var i = 0; i < 2; i++) {
                            for (var curveIndex = 0; curveIndex < m_ConstantClip.data.Length;) {
                                var index = streamCount + denseCount + curveIndex;
                                this.ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.data, 0, ref curveIndex);
                            }
                            time2 = animationClip.m_MuscleClip.m_StopTime;
                        }
                    }
                }
            }
        }

        private void ReadCurveData(ImportedKeyframedAnimation iAnim, AnimationClipBindingConstant m_ClipBindingConstant, int index, float time, float[] data, int offset, ref int curveIndex) {
            var binding = m_ClipBindingConstant.FindBinding(index);
            switch (binding.typeID) {
                //BlendShape
                case ClassIDType.SkinnedMeshRenderer: {
                    var channelName = this.GetChannelNameFromHash(binding.attribute);
                    if (string.IsNullOrEmpty(channelName)) {
                        curveIndex++;
                        return;
                    }
                    var dotPos = channelName.IndexOf('.');
                    if (dotPos >= 0) {
                        channelName = channelName[(dotPos + 1)..];
                    }

                    var bPath = this.FixBonePath(this.GetPathFromHash(binding.path));
                    if (string.IsNullOrEmpty(bPath)) {
                        bPath = this.GetPathByChannelName(channelName);
                    }
                    var bTrack = iAnim.FindTrack(bPath);
                    bTrack.BlendShape = new ImportedBlendShape {
                        ChannelName = channelName
                    };
                    bTrack.BlendShape.Keyframes.Add(new ImportedKeyframe<float>(time, data[curveIndex++ + offset]));
                    break;
                }
                case ClassIDType.Transform: {
                    var path = this.FixBonePath(this.GetPathFromHash(binding.path));
                    var track = iAnim.FindTrack(path);

                    switch (binding.attribute) {
                        case 1:
                            track.Translations.Add(new ImportedKeyframe<Vector3>(time, new Vector3
                            (
                                -data[curveIndex++ + offset],
                                data[curveIndex++ + offset],
                                data[curveIndex++ + offset]
                            )));
                            break;
                        case 2:
                            var value = Fbx.QuaternionToEuler(new Quaternion
                            (
                                data[curveIndex++ + offset],
                                -data[curveIndex++ + offset],
                                -data[curveIndex++ + offset],
                                data[curveIndex++ + offset]
                            ));
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(time, value));
                            break;
                        case 3:
                            track.Scalings.Add(new ImportedKeyframe<Vector3>(time, new Vector3
                            (
                                data[curveIndex++ + offset],
                                data[curveIndex++ + offset],
                                data[curveIndex++ + offset]
                            )));
                            break;
                        case 4:
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(time, new Vector3
                            (
                                data[curveIndex++ + offset],
                                -data[curveIndex++ + offset],
                                -data[curveIndex++ + offset]
                            )));
                            break;
                        default:
                            curveIndex++;
                            break;
                    }

                    break;
                }
                default:
                    curveIndex++;
                    break;
            }
        }

        private string GetPathFromHash(uint hash) {
            _ = this.bonePathHash.TryGetValue(hash, out var boneName);
            if (string.IsNullOrEmpty(boneName)) {
                boneName = this.avatar?.FindBonePath(hash);
            }
            if (string.IsNullOrEmpty(boneName)) {
                boneName = "unknown " + hash;
            }
            return boneName;
        }

        private void CreateBonePathHash(Transform m_Transform) {
            var name = GetTransformPathByFather(m_Transform);
            var crc = new CRC();
            var bytes = Encoding.UTF8.GetBytes(name);
            crc.Update(bytes, 0, (uint)bytes.Length);
            this.bonePathHash[crc.GetDigest()] = name;
            int index;
            while ((index = name.IndexOf("/", StringComparison.Ordinal)) >= 0) {
                name = name[(index + 1)..];
                crc = new CRC();
                bytes = Encoding.UTF8.GetBytes(name);
                crc.Update(bytes, 0, (uint)bytes.Length);
                this.bonePathHash[crc.GetDigest()] = name;
            }
            foreach (var pptr in m_Transform.m_Children) {
                if (pptr.TryGet(out var child))
                    this.CreateBonePathHash(child);
            }
        }

        private void DeoptimizeTransformHierarchy() {
            if (this.avatar == null)
                throw new Exception("Transform hierarchy has been optimized, but can't find Avatar to deoptimize.");
            // 1. Figure out the skeletonPaths from the unstripped avatar
            var skeletonPaths = this.avatar.m_Avatar.m_AvatarSkeleton.m_ID.Select(id => this.avatar.FindBonePath(id)).ToList();
            // 2. Restore the original transform hierarchy
            // Prerequisite: skeletonPaths follow pre-order traversal
            for (var i = 1; i < skeletonPaths.Count; i++) // start from 1, skip the root transform because it will always be there.
            {
                var path = skeletonPaths[i];
                var strs = path.Split('/');
                string transformName;
                ImportedFrame parentFrame;
                if (strs.Length == 1) {
                    transformName = path;
                    parentFrame = this.RootFrame;
                } else {
                    transformName = strs.Last();
                    var parentFrameName = strs[^2];
                    parentFrame = this.RootFrame.FindChild(parentFrameName);
                    //var parentFramePath = path.Substring(0, path.LastIndexOf('/'));
                    //parentFrame = RootFrame.FindFrameByPath(parentFramePath);
                }

                var skeletonPose = this.avatar.m_Avatar.m_DefaultPose;
                var xform = skeletonPose.m_X[i];

                var frame = this.RootFrame.FindChild(transformName);
                if (frame != null) {
                    SetFrame(frame, xform.t, xform.q, xform.s);
                    parentFrame.AddChild(frame);
                } else {
                    frame = CreateFrame(transformName, xform.t, xform.q, xform.s);
                    parentFrame.AddChild(frame);
                }
            }
        }

        private string GetPathByChannelName(string channelName) {
            foreach (var morph in this.MorphList) {
                foreach (var channel in morph.Channels) {
                    if (channel.Name == channelName) {
                        return morph.Path;
                    }
                }
            }
            return null;
        }

        private string GetChannelNameFromHash(uint attribute) => this.morphChannelNames.TryGetValue(attribute, out var name) ? name : null;
    }
}
