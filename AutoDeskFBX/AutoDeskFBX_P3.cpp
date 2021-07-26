#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxMeshAddDeformer(AsFbxSkinContext* pSkinContext, FbxMesh* pMesh) {
        if (pSkinContext == nullptr || pSkinContext->pSkin == nullptr)
            return;

        if (pMesh == nullptr)
            return;

        if (pSkinContext->pSkin->GetClusterCount() > 0)
            pMesh->AddDeformer(pSkinContext->pSkin);
    }

    AsFbxAnimContext* FBXService::AsFbxAnimCreateContext(bool eulerFilter) {
        return new AsFbxAnimContext(eulerFilter);
    }

    void FBXService::AsFbxAnimDisposeContext(AsFbxAnimContext** ppAnimContext) {
        if (ppAnimContext == nullptr)
            return;

        delete (*ppAnimContext);
        *ppAnimContext = nullptr;
    }

    void FBXService::AsFbxAnimPrepareStackAndLayer(AsFbxContext* pContext, AsFbxAnimContext* pAnimContext, const char* pTakeName) {
        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pAnimContext == nullptr)
            return;

        if (pTakeName == nullptr)
            return;

        pAnimContext->lAnimStack = FbxAnimStack::Create(pContext->pScene, pTakeName);
        pAnimContext->lAnimLayer = FbxAnimLayer::Create(pContext->pScene, "Base Layer");

        pAnimContext->lAnimStack->AddMember(pAnimContext->lAnimLayer);
    }

    void FBXService::AsFbxAnimLoadCurves(FbxNode* pNode, AsFbxAnimContext* pAnimContext) {
        if (pNode == nullptr)
            return;

        if (pAnimContext == nullptr)
            return;

        pAnimContext->lCurveSX = pNode->LclScaling.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
        pAnimContext->lCurveSY = pNode->LclScaling.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
        pAnimContext->lCurveSZ = pNode->LclScaling.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
        pAnimContext->lCurveRX = pNode->LclRotation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
        pAnimContext->lCurveRY = pNode->LclRotation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
        pAnimContext->lCurveRZ = pNode->LclRotation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
        pAnimContext->lCurveTX = pNode->LclTranslation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
        pAnimContext->lCurveTY = pNode->LclTranslation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
        pAnimContext->lCurveTZ = pNode->LclTranslation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
    }

    void FBXService::AsFbxAnimBeginKeyModify(AsFbxAnimContext* pAnimContext) {
        if (pAnimContext == nullptr)
            return;

        pAnimContext->lCurveSX->KeyModifyBegin();
        pAnimContext->lCurveSY->KeyModifyBegin();
        pAnimContext->lCurveSZ->KeyModifyBegin();
        pAnimContext->lCurveRX->KeyModifyBegin();
        pAnimContext->lCurveRY->KeyModifyBegin();
        pAnimContext->lCurveRZ->KeyModifyBegin();
        pAnimContext->lCurveTX->KeyModifyBegin();
        pAnimContext->lCurveTY->KeyModifyBegin();
        pAnimContext->lCurveTZ->KeyModifyBegin();
    }

    void FBXService::AsFbxAnimEndKeyModify(AsFbxAnimContext* pAnimContext) {
        if (pAnimContext == nullptr)
            return;

        pAnimContext->lCurveSX->KeyModifyEnd();
        pAnimContext->lCurveSY->KeyModifyEnd();
        pAnimContext->lCurveSZ->KeyModifyEnd();
        pAnimContext->lCurveRX->KeyModifyEnd();
        pAnimContext->lCurveRY->KeyModifyEnd();
        pAnimContext->lCurveRZ->KeyModifyEnd();
        pAnimContext->lCurveTX->KeyModifyEnd();
        pAnimContext->lCurveTY->KeyModifyEnd();
        pAnimContext->lCurveTZ->KeyModifyEnd();
    }

    void FBXService::AsFbxAnimAddScalingKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z) {
        if (pAnimContext == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        pAnimContext->lCurveSX->KeySet(pAnimContext->lCurveSX->KeyAdd(lTime), lTime, x);
        pAnimContext->lCurveSY->KeySet(pAnimContext->lCurveSY->KeyAdd(lTime), lTime, y);
        pAnimContext->lCurveSZ->KeySet(pAnimContext->lCurveSZ->KeyAdd(lTime), lTime, z);
    }

    void FBXService::AsFbxAnimAddRotationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z) {
        if (pAnimContext == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        pAnimContext->lCurveRX->KeySet(pAnimContext->lCurveRX->KeyAdd(lTime), lTime, x);
        pAnimContext->lCurveRY->KeySet(pAnimContext->lCurveRY->KeyAdd(lTime), lTime, y);
        pAnimContext->lCurveRZ->KeySet(pAnimContext->lCurveRZ->KeyAdd(lTime), lTime, z);
    }

    void FBXService::AsFbxAnimAddTranslationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z) {
        if (pAnimContext == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        pAnimContext->lCurveTX->KeySet(pAnimContext->lCurveTX->KeyAdd(lTime), lTime, x);
        pAnimContext->lCurveTY->KeySet(pAnimContext->lCurveTY->KeyAdd(lTime), lTime, y);
        pAnimContext->lCurveTZ->KeySet(pAnimContext->lCurveTZ->KeyAdd(lTime), lTime, z);
    }

    void FBXService::AsFbxAnimApplyEulerFilter(AsFbxAnimContext* pAnimContext, float filterPrecision) {
        if (pAnimContext == nullptr || pAnimContext->lFilter == nullptr)
            return;

        FbxAnimCurve* lCurve[3];
        lCurve[0] = pAnimContext->lCurveRX;
        lCurve[1] = pAnimContext->lCurveRY;
        lCurve[2] = pAnimContext->lCurveRZ;

        auto eulerFilter = pAnimContext->lFilter;

        eulerFilter->Reset();
        eulerFilter->SetQualityTolerance(filterPrecision);
        eulerFilter->Apply(lCurve, 3);
    }

    int32_t FBXService::AsFbxAnimGetCurrentBlendShapeChannelCount(AsFbxAnimContext* pAnimContext, FbxNode* pNode) {
        if (pAnimContext == nullptr)
            return 0;

        if (pNode == nullptr)
            return 0;

        auto pMesh = pNode->GetMesh();
        pAnimContext->pMesh = pMesh;

        if (pMesh == nullptr)
            return 0;

        auto blendShapeDeformerCount = pMesh->GetDeformerCount(FbxDeformer::eBlendShape);

        if (blendShapeDeformerCount <= 0)
            return 0;

        auto lBlendShape = (FbxBlendShape*)pMesh->GetDeformer(0, FbxDeformer::eBlendShape);
        pAnimContext->lBlendShape = lBlendShape;

        if (lBlendShape == nullptr)
            return 0;

        auto lBlendShapeChannelCount = lBlendShape->GetBlendShapeChannelCount();

        return lBlendShapeChannelCount;
    }

    bool FBXService::AsFbxAnimIsBlendShapeChannelMatch(AsFbxAnimContext* pAnimContext, int32_t channelIndex, const char* channelName) {
        if (pAnimContext == nullptr || pAnimContext->lBlendShape == nullptr)
            return false;

        if (channelName == nullptr)
            return false;

        FbxBlendShapeChannel* lChannel = pAnimContext->lBlendShape->GetBlendShapeChannel(channelIndex);
        auto lChannelName = lChannel->GetNameOnly();

        FbxString chanName(channelName);

        return lChannelName == chanName;
    }

    void FBXService::AsFbxAnimBeginBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext, int32_t channelIndex) {
        if (pAnimContext == nullptr || pAnimContext->pMesh == nullptr || pAnimContext->lAnimLayer == nullptr)
            return;

        pAnimContext->lAnimCurve = pAnimContext->pMesh->GetShapeChannel(0, channelIndex, pAnimContext->lAnimLayer, true);
        pAnimContext->lAnimCurve->KeyModifyBegin();
    }
}
