#pragma once
#include "AsFbxContext.h"
#include "AsFbxSkinContext.h"
#include "AsFbxAnimContext.h"
#include "AsFbxMorphContext.h"
#include <msclr/marshal_cppstd.h>
#include <msclr/marshal.h>

namespace SoarCraft::QYun::AutoDeskFBX {
    using namespace System;
    using namespace System::Runtime::InteropServices;
    using namespace AssetReader::Math;
    using namespace msclr::interop;

    public ref class FBXService
    {
    public:
        Vector3 AsUtilQuaternionToEuler(Quaternion q);
        void AsUtilEulerToQuaternion(float vx, float vy, float vz, float* qx, float* qy, float* qz, float* qw);
        IntPtr AsFbxCreateContext();
        bool AsFbxInitializeContext(IntPtr ptrContext, String^ fileName, float scaleFactor,
                                    int32_t versionIndex, bool isAscii, bool is60Fps, [Out] String^% errorMessage);
        void AsFbxDisposeContext(AsFbxContext** ppContext);
        void AsFbxSetFramePaths(IntPtr ptrContext, array<String^>^ ppPaths);
        void AsFbxExportScene(IntPtr ptrContext);
        IntPtr AsFbxGetSceneRootNode(IntPtr ptrContext);
        IntPtr AsFbxExportSingleFrame(IntPtr ptrContext, IntPtr ptrParentNode,
                                      String^ strFramePath, String^ strFrameName,
                                      Vector3 localPosition, Vector3 localRotation, Vector3 localScale);
        void AsFbxSetJointsNode_CastToBone(IntPtr ptrContext, IntPtr ptrNode, float boneSize);
        void AsFbxSetJointsNode_BoneInPath(IntPtr ptrContext, IntPtr ptrNode, float boneSize);
        void AsFbxSetJointsNode_Generic(IntPtr ptrContext, IntPtr ptrNode);
        void AsFbxPrepareMaterials(IntPtr ptrContext, int32_t materialCount, int32_t textureCount);
        IntPtr AsFbxCreateTexture(IntPtr ptrContext, String^ strMatTexName);
        void AsFbxLinkTexture(int32_t dest, IntPtr ptrTexture, IntPtr ptrMaterial,
                              float offsetX, float offsetY, float scaleX, float scaleY);
        IntPtr AsFbxMeshCreateClusterArray(int32_t boneCount);
        void AsFbxMeshDisposeClusterArray(IntPtr pptrArray);
        IntPtr AsFbxMeshCreateCluster(IntPtr ptrContext, IntPtr ptrBoneNode);
        void AsFbxMeshAddCluster(IntPtr ptrArray, IntPtr ptrCluster);
        IntPtr AsFbxMeshCreateMesh(IntPtr ptrContext, IntPtr ptrFrameNode);
        void AsFbxMeshInitControlPoints(IntPtr ptrMesh, int32_t vertexCount);
        void AsFbxMeshCreateElementNormal(IntPtr ptrMesh);
        void AsFbxMeshCreateDiffuseUV(IntPtr ptrMesh, int32_t uv);
        void AsFbxMeshCreateNormalMapUV(IntPtr ptrMesh, int32_t uv);
        void AsFbxMeshCreateElementTangent(IntPtr ptrMesh);
        void AsFbxMeshCreateElementVertexColor(IntPtr ptrMesh);
        void AsFbxMeshCreateElementMaterial(IntPtr ptrMesh);
        IntPtr AsFbxCreateMaterial(IntPtr ptrContext, String^ strMatName,
                                             Color diffuse, Color ambient, Color emissive,
                                             Color specular, Color reflect,
                                             float shininess, float transparency);
        int AsFbxAddMaterialToFrame(IntPtr ptrFrameNode, IntPtr ptrMaterial);
        void AsFbxSetFrameShadingModeToTextureShading(IntPtr ptrFrameNode);
        void AsFbxMeshSetControlPoint(IntPtr ptrMesh, int32_t index, Vector3 v);
        void AsFbxMeshAddPolygon(IntPtr ptrMesh, int32_t materialIndex, int32_t index0, int32_t index1, int32_t index2);
        void AsFbxMeshElementNormalAdd(IntPtr ptrMesh, int32_t elementIndex, Vector3 v);
        void AsFbxMeshElementUVAdd(IntPtr ptrMesh, int32_t elementIndex, float u, float v);
        void AsFbxMeshElementTangentAdd(IntPtr ptrMesh, int32_t elementIndex, Vector4 v);
        void AsFbxMeshElementVertexColorAdd(IntPtr ptrMesh, int32_t elementIndex, Color c);
        void AsFbxMeshSetBoneWeight(IntPtr ptrClusterArray, int32_t boneIndex, int32_t vertexIndex, float weight);
        IntPtr AsFbxMeshCreateSkinContext(IntPtr ptrContext, IntPtr ptrFrameNode);
        void AsFbxMeshDisposeSkinContext(IntPtr pptrTRSkinContext);
        bool FbxClusterArray_HasItemAt(IntPtr ptrClusterArray, int32_t index);
        void AsFbxMeshSkinAddCluster(IntPtr ptrSkinContext, IntPtr ptrClusterArray, int32_t index, array<float>^ aBoneMatrix);
        void AsFbxMeshAddDeformer(IntPtr ptrSkinContext, IntPtr ptrMesh);
        IntPtr AsFbxAnimCreateContext(bool eulerFilter);
        void AsFbxAnimDisposeContext(IntPtr pptrAnimContext);
        void AsFbxAnimPrepareStackAndLayer(IntPtr ptrContext, IntPtr ptrAnimContext, String^ strTakeName);
        void AsFbxAnimLoadCurves(IntPtr ptrNode, IntPtr ptrAnimContext);
        void AsFbxAnimBeginKeyModify(IntPtr ptrAnimContext);
        void AsFbxAnimEndKeyModify(IntPtr ptrAnimContext);
        void AsFbxAnimAddScalingKey(IntPtr ptrAnimContext, float time, Vector3 v);
        void AsFbxAnimAddRotationKey(IntPtr ptrAnimContext, float time, Vector3 v);
        void AsFbxAnimAddTranslationKey(IntPtr ptrAnimContext, float time, Vector3 v);
        void AsFbxAnimApplyEulerFilter(IntPtr ptrAnimContext, float filterPrecision);
        int32_t AsFbxAnimGetCurrentBlendShapeChannelCount(IntPtr ptrAnimContext, IntPtr ptrNode);
        bool AsFbxAnimIsBlendShapeChannelMatch(IntPtr ptrAnimContext, int32_t channelIndex, String^ strChannelName);
        void AsFbxAnimBeginBlendShapeAnimCurve(IntPtr ptrAnimContext, int32_t channelIndex);
        void AsFbxAnimEndBlendShapeAnimCurve(IntPtr ptrAnimContext);
        void AsFbxAnimAddBlendShapeKeyframe(IntPtr ptrAnimContext, float time, float value);
        IntPtr AsFbxMorphCreateContext();
        void AsFbxMorphInitializeContext(IntPtr ptrContext, IntPtr ptrMorphContext, IntPtr ptrNode);
        void AsFbxMorphDisposeContext(IntPtr pptrMorphContext);
        void AsFbxMorphAddBlendShapeChannel(IntPtr ptrContext, IntPtr ptrMorphContext, String^ strChannelName);
        void AsFbxMorphAddBlendShapeChannelShape(IntPtr ptrContext, IntPtr ptrMorphContext, float weight, String^ strShapeName);
        void AsFbxMorphCopyBlendShapeControlPoints(IntPtr ptrMorphContext);
        void AsFbxMorphSetBlendShapeVertex(IntPtr ptrMorphContext, uint32_t index, Vector3 v);
        void AsFbxMorphCopyBlendShapeControlPointsNormal(IntPtr ptrMorphContext);
        void AsFbxMorphSetBlendShapeVertexNormal(IntPtr ptrMorphContext, uint32_t index, Vector3 v);

    private:
        marshal_context^ context = gcnew marshal_context();
        Quaternion EulerToQuaternion(Vector3 v);

    };
}
