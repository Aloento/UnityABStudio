#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxAnimEndBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext) {
        if (pAnimContext == nullptr || pAnimContext->lAnimCurve == nullptr)
            return;

        pAnimContext->lAnimCurve->KeyModifyEnd();
    }

    void FBXService::AsFbxAnimAddBlendShapeKeyframe(AsFbxAnimContext* pAnimContext, float time, float value) {
        if (pAnimContext == nullptr || pAnimContext->lAnimCurve == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        auto keyIndex = pAnimContext->lAnimCurve->KeyAdd(lTime);
        pAnimContext->lAnimCurve->KeySetValue(keyIndex, value);
        pAnimContext->lAnimCurve->KeySetInterpolation(keyIndex, FbxAnimCurveDef::eInterpolationCubic);
    }

    AsFbxMorphContext* FBXService::AsFbxMorphCreateContext() {
        return new AsFbxMorphContext();
    }

    void FBXService::AsFbxMorphInitializeContext(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, FbxNode* pNode) {
        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pMorphContext == nullptr)
            return;

        if (pNode == nullptr)
            return;

        auto pMesh = pNode->GetMesh();
        pMorphContext->pMesh = pMesh;

        auto lBlendShape = FbxBlendShape::Create(pContext->pScene, pMesh->GetNameOnly() + FbxString("BlendShape"));
        pMorphContext->lBlendShape = lBlendShape;

        pMesh->AddDeformer(lBlendShape);
    }

    void FBXService::AsFbxMorphDisposeContext(AsFbxMorphContext** ppMorphContext) {
        if (ppMorphContext == nullptr)
            return;

        delete (*ppMorphContext);
        *ppMorphContext = nullptr;
    }

    void FBXService::AsFbxMorphAddBlendShapeChannel(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, const char* channelName) {
        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pMorphContext == nullptr)
            return;

        if (channelName == nullptr)
            return;

        auto lBlendShapeChannel = FbxBlendShapeChannel::Create(pContext->pScene, channelName);
        pMorphContext->lBlendShapeChannel = lBlendShapeChannel;

        if (pMorphContext->lBlendShape != nullptr)
            pMorphContext->lBlendShape->AddBlendShapeChannel(lBlendShapeChannel);
    }

    void FBXService::AsFbxMorphAddBlendShapeChannelShape(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, float weight, const char* shapeName) {
        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pMorphContext == nullptr)
            return;

        auto lShape = FbxShape::Create(pContext->pScene, shapeName);
        pMorphContext->lShape = lShape;

        if (pMorphContext->lBlendShapeChannel != nullptr)
            pMorphContext->lBlendShapeChannel->AddTargetShape(lShape, weight);
    }

    void FBXService::AsFbxMorphCopyBlendShapeControlPoints(AsFbxMorphContext* pMorphContext) {
        if (pMorphContext == nullptr || pMorphContext->pMesh == nullptr || pMorphContext->lShape == nullptr)
            return;

        auto vectorCount = pMorphContext->pMesh->GetControlPointsCount();

        auto srcControlPoints = pMorphContext->pMesh->GetControlPoints();

        pMorphContext->lShape->InitControlPoints(vectorCount);

        for (int j = 0; j < vectorCount; j++) {
            pMorphContext->lShape->SetControlPointAt(FbxVector4(srcControlPoints[j]), j);;
        }
    }

    void FBXService::AsFbxMorphSetBlendShapeVertex(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z) {
        if (pMorphContext == nullptr || pMorphContext->lShape == nullptr)
            return;

        pMorphContext->lShape->SetControlPointAt(FbxVector4(x, y, z, 0), index);
    }

    void FBXService::AsFbxMorphCopyBlendShapeControlPointsNormal(AsFbxMorphContext* pMorphContext) {
        if (pMorphContext == nullptr || pMorphContext->pMesh == nullptr || pMorphContext->lShape == nullptr)
            return;

        pMorphContext->lShape->InitNormals(pMorphContext->pMesh);
    }

    void FBXService::AsFbxMorphSetBlendShapeVertexNormal(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z) {
        if (pMorphContext == nullptr || pMorphContext->lShape == nullptr)
            return;

        pMorphContext->lShape->SetControlPointNormalAt(FbxVector4(x, y, z, 0), index);
    }
}
