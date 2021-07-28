#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxMeshCreateElementTangent(IntPtr ptrMesh) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pTangent = pMesh->CreateElementTangent();
        pTangent->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pTangent->SetReferenceMode(FbxGeometryElement::eDirect);
    }

    void FBXService::AsFbxMeshCreateElementVertexColor(IntPtr ptrMesh) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pVertexColor = pMesh->CreateElementVertexColor();
        pVertexColor->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pVertexColor->SetReferenceMode(FbxGeometryElement::eDirect);
    }

    void FBXService::AsFbxMeshCreateElementMaterial(IntPtr ptrMesh) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pMaterial = pMesh->CreateElementMaterial();
        pMaterial->SetMappingMode(FbxGeometryElement::eByPolygon);
        pMaterial->SetReferenceMode(FbxGeometryElement::eIndexToDirect);
    }

    IntPtr FBXService::AsFbxCreateMaterial(IntPtr ptrContext, String^ strMatName,
                                                     Color diffuse, Color ambient, Color emissive,
                                                     Color specular, Color reflect,
                                                     float shininess, float transparency) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();

        if (pContext == nullptr || pContext->pScene == nullptr)
            return IntPtr::Zero;

        if (String::IsNullOrWhiteSpace(strMatName))
            return IntPtr::Zero;

        const char* pMatName = (const char*)(Marshal::StringToHGlobalAuto(strMatName).ToPointer());
        auto pMat = FbxSurfacePhong::Create(pContext->pScene, pMatName);

        pMat->Diffuse.Set(FbxDouble3(diffuse.R, diffuse.G, diffuse.B));
        pMat->Ambient.Set(FbxDouble3(ambient.R, ambient.G, ambient.B));
        pMat->Emissive.Set(FbxDouble3(emissive.R, emissive.G, emissive.B));
        pMat->Specular.Set(FbxDouble3(specular.R, specular.G, specular.B));
        pMat->Reflection.Set(FbxDouble3(reflect.R, reflect.G, reflect.B));
        pMat->Shininess.Set(FbxDouble(shininess));
        pMat->TransparencyFactor.Set(FbxDouble(transparency));
        pMat->ShadingModel.Set("Phong");

        if (pContext->pMaterials)
            pContext->pMaterials->Add(pMat);

        Marshal::FreeHGlobal(IntPtr((void*)pMatName));
        return IntPtr(pMat);
    }

    int FBXService::AsFbxAddMaterialToFrame(IntPtr ptrFrameNode, IntPtr ptrMaterial) {
        auto pFrameNode = (FbxNode*)ptrFrameNode.ToPointer();
        auto pMaterial = (FbxSurfacePhong*)ptrMaterial.ToPointer();

        if (pFrameNode == nullptr || pMaterial == nullptr)
            return 0;

        return pFrameNode->AddMaterial(pMaterial);
    }

    void FBXService::AsFbxSetFrameShadingModeToTextureShading(IntPtr ptrFrameNode) {
        auto pFrameNode = (FbxNode*)ptrFrameNode.ToPointer();

        if (pFrameNode == nullptr)
            return;

        pFrameNode->SetShadingMode(FbxNode::eTextureShading);
    }

    void FBXService::AsFbxMeshSetControlPoint(IntPtr ptrMesh, int32_t index, float x, float y, float z) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pControlPoints = pMesh->GetControlPoints();
        pControlPoints[index] = FbxVector4(x, y, z, 0);
    }

    void FBXService::AsFbxMeshAddPolygon(IntPtr ptrMesh, int32_t materialIndex, int32_t index0, int32_t index1, int32_t index2) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        pMesh->BeginPolygon(materialIndex);
        pMesh->AddPolygon(index0);
        pMesh->AddPolygon(index1);
        pMesh->AddPolygon(index2);
        pMesh->EndPolygon();
    }

    void FBXService::AsFbxMeshElementNormalAdd(IntPtr ptrMesh, int32_t elementIndex, float x, float y, float z) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pElem = pMesh->GetElementNormal(elementIndex);
        auto& array = pElem->GetDirectArray();

        array.Add(FbxVector4(x, y, z, 0));
    }

    void FBXService::AsFbxMeshElementUVAdd(IntPtr ptrMesh, int32_t elementIndex, float u, float v) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pElem = pMesh->GetElementUV(FbxString("UV") + FbxString(elementIndex));
        auto& array = pElem->GetDirectArray();

        array.Add(FbxVector2(u, v));
    }

    void FBXService::AsFbxMeshElementTangentAdd(IntPtr ptrMesh, int32_t elementIndex, float x, float y, float z, float w) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pElem = pMesh->GetElementTangent(elementIndex);
        auto& array = pElem->GetDirectArray();

        array.Add(FbxVector4(x, y, z, w));
    }

    void FBXService::AsFbxMeshElementVertexColorAdd(IntPtr ptrMesh, int32_t elementIndex, float r, float g, float b, float a) {
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pMesh == nullptr)
            return;

        auto pElem = pMesh->GetElementVertexColor(elementIndex);
        auto& array = pElem->GetDirectArray();

        array.Add(FbxVector4(r, g, b, a));
    }

    void FBXService::AsFbxMeshSetBoneWeight(FbxArray<FbxCluster*>* pClusterArray, int32_t boneIndex, int32_t vertexIndex, float weight) {
        if (pClusterArray == nullptr)
            return;

        auto pCluster = pClusterArray->GetAt(boneIndex);

        if (pCluster != nullptr)
            pCluster->AddControlPointIndex(vertexIndex, weight);
    }

    AsFbxSkinContext* FBXService::AsFbxMeshCreateSkinContext(IntPtr ptrContext, IntPtr ptrFrameNode) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pFrameNode = (FbxNode*)ptrFrameNode.ToPointer();
        return new AsFbxSkinContext(pContext, pFrameNode);
    }

    void FBXService::AsFbxMeshDisposeSkinContext(AsFbxSkinContext** ppSkinContext) {
        if (ppSkinContext == nullptr)
            return;

        delete (*ppSkinContext);
        *ppSkinContext = nullptr;
    }

    bool FBXService::FbxClusterArray_HasItemAt(FbxArray<FbxCluster*>* pClusterArray, int32_t index) {
        if (pClusterArray == nullptr)
            return false;

        auto pCluster = pClusterArray->GetAt(index);

        return pCluster != nullptr;
    }

    void FBXService::AsFbxMeshSkinAddCluster(AsFbxSkinContext* pSkinContext, FbxArray<FbxCluster*>* pClusterArray, int32_t index, float pBoneMatrix[16]) {
        if (pSkinContext == nullptr)
            return;

        if (pClusterArray == nullptr)
            return;

        if (pBoneMatrix == nullptr)
            return;

        auto pCluster = pClusterArray->GetAt(index);

        if (pCluster == nullptr)
            return;

        FbxAMatrix boneMatrix;

        for (int m = 0; m < 4; m += 1) {
            for (int n = 0; n < 4; n += 1) {
                auto index = m * 4 + n;
                boneMatrix.mData[m][n] = pBoneMatrix[index];
            }
        }

        pCluster->SetTransformMatrix(pSkinContext->lMeshMatrix);
        pCluster->SetTransformLinkMatrix(pSkinContext->lMeshMatrix * boneMatrix.Inverse());

        if (pSkinContext->pSkin)
            pSkinContext->pSkin->AddCluster(pCluster);
    }
}
