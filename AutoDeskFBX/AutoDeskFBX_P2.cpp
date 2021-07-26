#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxMeshCreateElementTangent(FbxMesh* pMesh) {
        if (pMesh == nullptr)
            return;

        auto pTangent = pMesh->CreateElementTangent();
        pTangent->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pTangent->SetReferenceMode(FbxGeometryElement::eDirect);
    }

    void FBXService::AsFbxMeshCreateElementVertexColor(FbxMesh* pMesh) {
        if (pMesh == nullptr)
            return;

        auto pVertexColor = pMesh->CreateElementVertexColor();
        pVertexColor->SetMappingMode(FbxGeometryElement::eByControlPoint);
        pVertexColor->SetReferenceMode(FbxGeometryElement::eDirect);
    }

    void FBXService::AsFbxMeshCreateElementMaterial(FbxMesh* pMesh) {
        if (pMesh == nullptr)
            return;

        auto pMaterial = pMesh->CreateElementMaterial();
        pMaterial->SetMappingMode(FbxGeometryElement::eByPolygon);
        pMaterial->SetReferenceMode(FbxGeometryElement::eIndexToDirect);
    }

    FbxSurfacePhong* FBXService::AsFbxCreateMaterial(AsFbxContext* pContext, const char* pMatName,
                                                     float diffuseR, float diffuseG, float diffuseB,
                                                     float ambientR, float ambientG, float ambientB,
                                                     float emissiveR, float emissiveG, float emissiveB,
                                                     float specularR, float specularG, float specularB,
                                                     float reflectR, float reflectG, float reflectB,
                                                     float shininess, float transparency) {


        if (pContext == nullptr || pContext->pScene == nullptr)
            return nullptr;

        if (pMatName == nullptr)
            return nullptr;

        auto pMat = FbxSurfacePhong::Create(pContext->pScene, pMatName);

        pMat->Diffuse.Set(FbxDouble3(diffuseR, diffuseG, diffuseB));
        pMat->Ambient.Set(FbxDouble3(ambientR, ambientG, ambientB));
        pMat->Emissive.Set(FbxDouble3(emissiveR, emissiveG, emissiveB));
        pMat->Specular.Set(FbxDouble3(specularR, specularG, specularB));
        pMat->Reflection.Set(FbxDouble3(reflectR, reflectG, reflectB));
        pMat->Shininess.Set(FbxDouble(shininess));
        pMat->TransparencyFactor.Set(FbxDouble(transparency));
        pMat->ShadingModel.Set("Phong");

        if (pContext->pMaterials)
            pContext->pMaterials->Add(pMat);

        return pMat;
    }

    int FBXService::AsFbxAddMaterialToFrame(FbxNode* pFrameNode, FbxSurfacePhong* pMaterial) {
        if (pFrameNode == nullptr || pMaterial == nullptr)
            return 0;

        return pFrameNode->AddMaterial(pMaterial);
    }

    void FBXService::AsFbxSetFrameShadingModeToTextureShading(FbxNode* pFrameNode) {
        if (pFrameNode == nullptr)
            return;

        pFrameNode->SetShadingMode(FbxNode::eTextureShading);
    }

    void FBXService::AsFbxMeshSetControlPoint(FbxMesh* pMesh, int32_t index, float x, float y, float z) {
        if (pMesh == nullptr)
            return;

        auto pControlPoints = pMesh->GetControlPoints();
        pControlPoints[index] = FbxVector4(x, y, z, 0);
    }

    void FBXService::AsFbxMeshAddPolygon(FbxMesh* pMesh, int32_t materialIndex, int32_t index0, int32_t index1, int32_t index2) {
        if (pMesh == nullptr)
            return;

        pMesh->BeginPolygon(materialIndex);
        pMesh->AddPolygon(index0);
        pMesh->AddPolygon(index1);
        pMesh->AddPolygon(index2);
        pMesh->EndPolygon();
    }



}
