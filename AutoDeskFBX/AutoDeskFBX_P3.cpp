#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxMeshAddDeformer(IntPtr ptrSkinContext, IntPtr ptrMesh) {
        // AsFbxSkinContext* pSkinContext
        auto pSkinContext = (AsFbxSkinContext*)ptrSkinContext.ToPointer();
        auto pMesh = (FbxMesh*)ptrMesh.ToPointer();

        if (pSkinContext == nullptr || pSkinContext->pSkin == nullptr)
            return;

        if (pMesh == nullptr)
            return;

        if (pSkinContext->pSkin->GetClusterCount() > 0)
            pMesh->AddDeformer(pSkinContext->pSkin);
    }

    IntPtr FBXService::AsFbxAnimCreateContext(bool eulerFilter) {
        return IntPtr(new AsFbxAnimContext(eulerFilter));
    }

    void FBXService::AsFbxAnimDisposeContext(IntPtr pptrAnimContext) {
        auto ppAnimContext = (AsFbxAnimContext**)pptrAnimContext.ToPointer();

        if (ppAnimContext == nullptr)
            return;

        delete (*ppAnimContext);
        *ppAnimContext = nullptr;
    }

    void FBXService::AsFbxAnimPrepareStackAndLayer(IntPtr ptrContext, IntPtr ptrAnimContext, String^ strTakeName) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();
        auto pTakeName = (const char*)(Marshal::StringToHGlobalAuto(strTakeName).ToPointer());

        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pAnimContext == nullptr)
            return;

        if (pTakeName == nullptr)
            return;

        pAnimContext->lAnimStack = FbxAnimStack::Create(pContext->pScene, pTakeName);
        pAnimContext->lAnimLayer = FbxAnimLayer::Create(pContext->pScene, "Base Layer");

        pAnimContext->lAnimStack->AddMember(pAnimContext->lAnimLayer);
        Marshal::FreeHGlobal(IntPtr((void*)pTakeName));
    }

    void FBXService::AsFbxAnimLoadCurves(IntPtr ptrNode, IntPtr ptrAnimContext) {
        auto pNode = (FbxNode*)ptrNode.ToPointer();
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

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

    void FBXService::AsFbxAnimBeginKeyModify(IntPtr ptrAnimContext) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

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

    void FBXService::AsFbxAnimEndKeyModify(IntPtr ptrAnimContext) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

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

    void FBXService::AsFbxAnimAddScalingKey(IntPtr ptrAnimContext, float time, Vector3 v) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

        if (pAnimContext == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        pAnimContext->lCurveSX->KeySet(pAnimContext->lCurveSX->KeyAdd(lTime), lTime, v.X);
        pAnimContext->lCurveSY->KeySet(pAnimContext->lCurveSY->KeyAdd(lTime), lTime, v.Y);
        pAnimContext->lCurveSZ->KeySet(pAnimContext->lCurveSZ->KeyAdd(lTime), lTime, v.Z);
    }

    void FBXService::AsFbxAnimAddRotationKey(IntPtr ptrAnimContext, float time, Vector3 v) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

        if (pAnimContext == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        pAnimContext->lCurveRX->KeySet(pAnimContext->lCurveRX->KeyAdd(lTime), lTime, v.X);
        pAnimContext->lCurveRY->KeySet(pAnimContext->lCurveRY->KeyAdd(lTime), lTime, v.Y);
        pAnimContext->lCurveRZ->KeySet(pAnimContext->lCurveRZ->KeyAdd(lTime), lTime, v.Z);
    }

    void FBXService::AsFbxAnimAddTranslationKey(IntPtr ptrAnimContext, float time, Vector3 v) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

        if (pAnimContext == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        pAnimContext->lCurveTX->KeySet(pAnimContext->lCurveTX->KeyAdd(lTime), lTime, v.X);
        pAnimContext->lCurveTY->KeySet(pAnimContext->lCurveTY->KeyAdd(lTime), lTime, v.Y);
        pAnimContext->lCurveTZ->KeySet(pAnimContext->lCurveTZ->KeyAdd(lTime), lTime, v.Z);
    }

    void FBXService::AsFbxAnimApplyEulerFilter(IntPtr ptrAnimContext, float filterPrecision) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

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

    int32_t FBXService::AsFbxAnimGetCurrentBlendShapeChannelCount(IntPtr ptrAnimContext, IntPtr ptrNode) {
        auto pNode = (FbxNode*)ptrNode.ToPointer();
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

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

    bool FBXService::AsFbxAnimIsBlendShapeChannelMatch(IntPtr ptrAnimContext, int32_t channelIndex, String^ strChannelName) {
        // const char* channelName
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();
        auto channelName = (const char*)(Marshal::StringToHGlobalAuto(strChannelName).ToPointer());

        if (pAnimContext == nullptr || pAnimContext->lBlendShape == nullptr)
            return false;

        if (channelName == nullptr)
            return false;

        FbxBlendShapeChannel* lChannel = pAnimContext->lBlendShape->GetBlendShapeChannel(channelIndex);
        auto lChannelName = lChannel->GetNameOnly();

        FbxString chanName(channelName);

        Marshal::FreeHGlobal(IntPtr((void*)channelName));
        return lChannelName == chanName;
    }

    void FBXService::AsFbxAnimBeginBlendShapeAnimCurve(IntPtr ptrAnimContext, int32_t channelIndex) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

        if (pAnimContext == nullptr || pAnimContext->pMesh == nullptr || pAnimContext->lAnimLayer == nullptr)
            return;

        pAnimContext->lAnimCurve = pAnimContext->pMesh->GetShapeChannel(0, channelIndex, pAnimContext->lAnimLayer, true);
        pAnimContext->lAnimCurve->KeyModifyBegin();
    }
}
