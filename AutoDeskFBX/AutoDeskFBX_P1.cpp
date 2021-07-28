#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxSetJointsNode_CastToBone(IntPtr ptrContext, IntPtr ptrNode, float boneSize) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pNode = (FbxNode*)ptrNode.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pNode == nullptr)
            return;

        FbxSkeleton* pJoint = FbxSkeleton::Create(pContext->pScene, "");
        pJoint->Size.Set(FbxDouble(boneSize));
        pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
        pNode->SetNodeAttribute(pJoint);
    }

    void FBXService::AsFbxSetJointsNode_BoneInPath(IntPtr ptrContext, IntPtr ptrNode, float boneSize) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pNode = (FbxNode*)ptrNode.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pNode == nullptr)
            return;

        FbxSkeleton* pJoint = FbxSkeleton::Create(pContext->pScene, "");
        pJoint->Size.Set(FbxDouble(boneSize));
        pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
        pNode->SetNodeAttribute(pJoint);

        pJoint = FbxSkeleton::Create(pContext->pScene, "");
        pJoint->Size.Set(FbxDouble(boneSize));
        pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
        pNode->GetParent()->SetNodeAttribute(pJoint);
    }

    void FBXService::AsFbxSetJointsNode_Generic(IntPtr ptrContext, IntPtr ptrNode) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pNode = (FbxNode*)ptrNode.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pNode == nullptr)
            return;

        FbxNull* pNull = FbxNull::Create(pContext->pScene, "");

        if (pNode->GetChildCount() > 0)
            pNull->Look.Set(FbxNull::eNone);

        pNode->SetNodeAttribute(pNull);
    }

    void FBXService::AsFbxPrepareMaterials(IntPtr ptrContext, int32_t materialCount, int32_t textureCount) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        if (pContext == nullptr)
            return;

        pContext->pMaterials = new FbxArray<FbxSurfacePhong*>();
        pContext->pTextures = new FbxArray<FbxFileTexture*>();

        pContext->pMaterials->Reserve(materialCount);
        pContext->pTextures->Reserve(textureCount);
    }

    FbxFileTexture* FBXService::AsFbxCreateTexture(IntPtr ptrContext, const char* pMatTexName) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return nullptr;

        auto pTex = FbxFileTexture::Create(pContext->pScene, pMatTexName);
        pTex->SetFileName(pMatTexName);
        pTex->SetTextureUse(FbxTexture::eStandard);
        pTex->SetMappingType(FbxTexture::eUV);
        pTex->SetMaterialUse(FbxFileTexture::eModelMaterial);
        pTex->SetSwapUV(false);
        pTex->SetTranslation(0.0, 0.0);
        pTex->SetScale(1.0, 1.0);
        pTex->SetRotation(0.0, 0.0);

        if (pContext->pTextures != nullptr) 
            pContext->pTextures->Add(pTex);

        return pTex;
    }

    void FBXService::AsFbxLinkTexture(int32_t dest, FbxFileTexture* pTexture, FbxSurfacePhong* pMaterial,
                                      float offsetX, float offsetY, float scaleX, float scaleY) {

        if (pTexture == nullptr || pMaterial == nullptr)
            return;

        pTexture->SetTranslation(offsetX, offsetY);
        pTexture->SetScale(scaleX, scaleY);

        FbxProperty* pProp;

        switch (dest) {
        case 0:
            pProp = &pMaterial->Diffuse;
            break;
        case 1:
            pProp = &pMaterial->NormalMap;
            break;
        case 2:
            pProp = &pMaterial->Specular;
            break;
        case 3:
            pProp = &pMaterial->Bump;
            break;
        default:
            pProp = nullptr;
            break;
        }

        if (pProp != nullptr)
            pProp->ConnectSrcObject(pTexture);
    }

    IntPtr FBXService::AsFbxMeshCreateClusterArray(int32_t boneCount) {
        return IntPtr(new FbxArray<FbxCluster*>(boneCount));
    }

    void FBXService::AsFbxMeshDisposeClusterArray(FbxArray<FbxCluster*>** ppArray) {
        if (ppArray == nullptr)
            return;

        delete (*ppArray);
        *ppArray = nullptr;
    }

    IntPtr FBXService::AsFbxMeshCreateCluster(IntPtr ptrContext, IntPtr ptrBoneNode) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pBoneNode = (FbxNode*)ptrBoneNode.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return IntPtr::Zero;

        if (pBoneNode == nullptr)
            return IntPtr::Zero;

        FbxString lClusterName = pBoneNode->GetNameOnly() + FbxString("Cluster");
        FbxCluster* pCluster = FbxCluster::Create(pContext->pScene, lClusterName.Buffer());
        pCluster->SetLink(pBoneNode);
        pCluster->SetLinkMode(FbxCluster::eTotalOne);

        return IntPtr(pCluster);
    }

    void FBXService::AsFbxMeshAddCluster(IntPtr ptrArray, IntPtr ptrCluster) {
        auto pArray = (FbxArray<FbxCluster*>*)ptrArray.ToPointer();
        auto pCluster = (FbxCluster*)ptrCluster.ToPointer();

        if (pArray == nullptr)
            return;

        pArray->Add(pCluster);
    }

    IntPtr FBXService::AsFbxMeshCreateMesh(IntPtr ptrContext, IntPtr ptrFrameNode) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pFrameNode = (FbxNode*)ptrFrameNode.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return IntPtr::Zero;

        if (pFrameNode == nullptr)
            return IntPtr::Zero;

        FbxMesh* pMesh = FbxMesh::Create(pContext->pScene, pFrameNode->GetName());
        pFrameNode->SetNodeAttribute(pMesh);

        return IntPtr(pMesh);
    }

    void FBXService::AsFbxMeshInitControlPoints(IntPtr ptrMesh, int32_t vertexCount) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        pMesh->InitControlPoints(vertexCount);
    }

    void FBXService::AsFbxMeshCreateElementNormal(IntPtr ptrMesh) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pNormal = pMesh->CreateElementNormal();
        pNormal->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pNormal->SetReferenceMode(FbxGeometryElement::eDirect);
    }

    void FBXService::AsFbxMeshCreateDiffuseUV(IntPtr ptrMesh, int32_t uv) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pUV = pMesh->CreateElementUV(FbxString("UV") + FbxString(uv), FbxLayerElement::eTextureDiffuse);
        pUV->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pUV->SetReferenceMode(FbxGeometryElement::eDirect);
    }

    void FBXService::AsFbxMeshCreateNormalMapUV(IntPtr ptrMesh, int32_t uv) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pUV = pMesh->CreateElementUV(FbxString("UV") + FbxString(uv), FbxLayerElement::eTextureNormalMap);
        pUV->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pUV->SetReferenceMode(FbxGeometryElement::eDirect);
    }
}
