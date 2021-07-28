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
        void AsFbxExportScene(AsFbxContext* pContext);
        IntPtr AsFbxGetSceneRootNode(IntPtr ptrContext);
        IntPtr AsFbxExportSingleFrame(IntPtr ptrContext, IntPtr ptrParentNode,
                                      String^ strFramePath, String^ strFrameName,
                                      Vector3 localPosition, Vector3 localRotation, Vector3 localScale);
        void AsFbxSetJointsNode_CastToBone(AsFbxContext* pContext, FbxNode* pNode, float boneSize);
        void AsFbxSetJointsNode_BoneInPath(AsFbxContext* pContext, FbxNode* pNode, float boneSize);
        void AsFbxSetJointsNode_Generic(AsFbxContext* pContext, FbxNode* pNode);
        void AsFbxPrepareMaterials(IntPtr ptrContext, int32_t materialCount, int32_t textureCount);
        FbxFileTexture* AsFbxCreateTexture(AsFbxContext* pContext, const char* pMatTexName);
        void AsFbxLinkTexture(int32_t dest, FbxFileTexture* pTexture, FbxSurfacePhong* pMaterial,
                              float offsetX, float offsetY, float scaleX, float scaleY);
        FbxArray<FbxCluster*>* AsFbxMeshCreateClusterArray(int32_t boneCount);
        void AsFbxMeshDisposeClusterArray(FbxArray<FbxCluster*>** ppArray);
        FbxCluster* AsFbxMeshCreateCluster(AsFbxContext* pContext, FbxNode* pBoneNode);
        void AsFbxMeshAddCluster(FbxArray<FbxCluster*>* pArray, /* CanBeNull */ FbxCluster* pCluster);
        FbxMesh* AsFbxMeshCreateMesh(AsFbxContext* pContext, FbxNode* pFrameNode);
        void AsFbxMeshInitControlPoints(FbxMesh* pMesh, int32_t vertexCount);
        void AsFbxMeshCreateElementNormal(FbxMesh* pMesh);
        void AsFbxMeshCreateDiffuseUV(FbxMesh* pMesh, int32_t uv);
        void AsFbxMeshCreateNormalMapUV(FbxMesh* pMesh, int32_t uv);
        void AsFbxMeshCreateElementTangent(FbxMesh* pMesh);
        void AsFbxMeshCreateElementVertexColor(FbxMesh* pMesh);
        void AsFbxMeshCreateElementMaterial(FbxMesh* pMesh);
        FbxSurfacePhong* AsFbxCreateMaterial(AsFbxContext* pContext, const char* pMatName,
                                             float diffuseR, float diffuseG, float diffuseB,
                                             float ambientR, float ambientG, float ambientB,
                                             float emissiveR, float emissiveG, float emissiveB,
                                             float specularR, float specularG, float specularB,
                                             float reflectR, float reflectG, float reflectB,
                                             float shininess, float transparency);
        int AsFbxAddMaterialToFrame(FbxNode* pFrameNode, FbxSurfacePhong* pMaterial);
        void AsFbxSetFrameShadingModeToTextureShading(FbxNode* pFrameNode);
        void AsFbxMeshSetControlPoint(FbxMesh* pMesh, int32_t index, float x, float y, float z);
        void AsFbxMeshAddPolygon(FbxMesh* pMesh, int32_t materialIndex, int32_t index0, int32_t index1, int32_t index2);
        void AsFbxMeshElementNormalAdd(FbxMesh* pMesh, int32_t elementIndex, float x, float y, float z);
        void AsFbxMeshElementUVAdd(FbxMesh* pMesh, int32_t elementIndex, float u, float v);
        void AsFbxMeshElementTangentAdd(FbxMesh* pMesh, int32_t elementIndex, float x, float y, float z, float w);
        void AsFbxMeshElementVertexColorAdd(FbxMesh* pMesh, int32_t elementIndex, float r, float g, float b, float a);
        void AsFbxMeshSetBoneWeight(FbxArray<FbxCluster*>* pClusterArray, int32_t boneIndex, int32_t vertexIndex, float weight);
        AsFbxSkinContext* AsFbxMeshCreateSkinContext(AsFbxContext* pContext, FbxNode* pFrameNode);
        void AsFbxMeshDisposeSkinContext(AsFbxSkinContext** ppSkinContext);
        bool FbxClusterArray_HasItemAt(FbxArray<FbxCluster*>* pClusterArray, int32_t index);
        void AsFbxMeshSkinAddCluster(AsFbxSkinContext* pSkinContext, FbxArray<FbxCluster*>* pClusterArray, int32_t index, float pBoneMatrix[16]);
        void AsFbxMeshAddDeformer(AsFbxSkinContext* pSkinContext, FbxMesh* pMesh);
        AsFbxAnimContext* AsFbxAnimCreateContext(bool eulerFilter);
        void AsFbxAnimDisposeContext(AsFbxAnimContext** ppAnimContext);
        void AsFbxAnimPrepareStackAndLayer(AsFbxContext* pContext, AsFbxAnimContext* pAnimContext, const char* pTakeName);
        void AsFbxAnimLoadCurves(FbxNode* pNode, AsFbxAnimContext* pAnimContext);
        void AsFbxAnimBeginKeyModify(AsFbxAnimContext* pAnimContext);
        void AsFbxAnimEndKeyModify(AsFbxAnimContext* pAnimContext);
        void AsFbxAnimAddScalingKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);
        void AsFbxAnimAddRotationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);
        void AsFbxAnimAddTranslationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);
        void AsFbxAnimApplyEulerFilter(AsFbxAnimContext* pAnimContext, float filterPrecision);
        int32_t AsFbxAnimGetCurrentBlendShapeChannelCount(AsFbxAnimContext* pAnimContext, FbxNode* pNode);
        bool AsFbxAnimIsBlendShapeChannelMatch(AsFbxAnimContext* pAnimContext, int32_t channelIndex, const char* channelName);
        void AsFbxAnimBeginBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext, int32_t channelIndex);
        void AsFbxAnimEndBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext);
        void AsFbxAnimAddBlendShapeKeyframe(AsFbxAnimContext* pAnimContext, float time, float value);
        AsFbxMorphContext* AsFbxMorphCreateContext();
        void AsFbxMorphInitializeContext(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, FbxNode* pNode);
        void AsFbxMorphDisposeContext(AsFbxMorphContext** ppMorphContext);
        void AsFbxMorphAddBlendShapeChannel(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, const char* channelName);
        void AsFbxMorphAddBlendShapeChannelShape(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, float weight, const char* shapeName);
        void AsFbxMorphCopyBlendShapeControlPoints(AsFbxMorphContext* pMorphContext);
        void AsFbxMorphSetBlendShapeVertex(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z);
        void AsFbxMorphCopyBlendShapeControlPointsNormal(AsFbxMorphContext* pMorphContext);
        void AsFbxMorphSetBlendShapeVertexNormal(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z);

    private:
        Quaternion EulerToQuaternion(Vector3 v);

    };
}
