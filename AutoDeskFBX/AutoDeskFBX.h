#pragma once
#include "AsFbxContext.h"
#include "AsFbxSkinContext.h"
#include "AsFbxAnimContext.h"
#include "AsFbxMorphContext.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    using namespace System;
    using namespace System::Runtime::InteropServices;
    using namespace AssetReader::Math;

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
        IntPtr FBXService::AsFbxMeshCreateMesh(IntPtr ptrContext, IntPtr ptrFrameNode);
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
        void AsFbxMeshDisposeSkinContext(IntPtr ppTRSkinContext);
        bool FbxClusterArray_HasItemAt(IntPtr ptrClusterArray, int32_t index);
        void AsFbxMeshSkinAddCluster(IntPtr ptrSkinContext, IntPtr ptrClusterArray, int32_t index, array<float>^ aBoneMatrix);
        void AsFbxMeshAddDeformer(IntPtr ptrSkinContext, IntPtr ptrMesh);
        AsFbxAnimContext* AsFbxAnimCreateContext(bool eulerFilter);
        void AsFbxAnimDisposeContext(AsFbxAnimContext** ppAnimContext);
        void AsFbxAnimPrepareStackAndLayer(IntPtr ptrContext, AsFbxAnimContext* pAnimContext, const char* pTakeName);
        void AsFbxAnimLoadCurves(IntPtr ptrNode, AsFbxAnimContext* pAnimContext);
        void AsFbxAnimBeginKeyModify(AsFbxAnimContext* pAnimContext);
        void AsFbxAnimEndKeyModify(AsFbxAnimContext* pAnimContext);
        void AsFbxAnimAddScalingKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);
        void AsFbxAnimAddRotationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);
        void AsFbxAnimAddTranslationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);
        void AsFbxAnimApplyEulerFilter(AsFbxAnimContext* pAnimContext, float filterPrecision);
        int32_t AsFbxAnimGetCurrentBlendShapeChannelCount(AsFbxAnimContext* pAnimContext, IntPtr ptrNode);
        bool AsFbxAnimIsBlendShapeChannelMatch(AsFbxAnimContext* pAnimContext, int32_t channelIndex, const char* channelName);
        void AsFbxAnimBeginBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext, int32_t channelIndex);
        void AsFbxAnimEndBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext);
        void AsFbxAnimAddBlendShapeKeyframe(AsFbxAnimContext* pAnimContext, float time, float value);
        AsFbxMorphContext* AsFbxMorphCreateContext();
        void AsFbxMorphInitializeContext(IntPtr ptrContext, AsFbxMorphContext* pMorphContext, IntPtr ptrNode);
        void AsFbxMorphDisposeContext(AsFbxMorphContext** ppMorphContext);
        void AsFbxMorphAddBlendShapeChannel(IntPtr ptrContext, AsFbxMorphContext* pMorphContext, const char* channelName);
        void AsFbxMorphAddBlendShapeChannelShape(IntPtr ptrContext, AsFbxMorphContext* pMorphContext, float weight, const char* shapeName);
        void AsFbxMorphCopyBlendShapeControlPoints(AsFbxMorphContext* pMorphContext);
        void AsFbxMorphSetBlendShapeVertex(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z);
        void AsFbxMorphCopyBlendShapeControlPointsNormal(AsFbxMorphContext* pMorphContext);
        void AsFbxMorphSetBlendShapeVertexNormal(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z);

    private:
        Quaternion EulerToQuaternion(Vector3 v);

    };
}
