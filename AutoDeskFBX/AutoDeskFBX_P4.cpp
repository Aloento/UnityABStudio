#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    void FBXService::AsFbxAnimEndBlendShapeAnimCurve(IntPtr ptrAnimContext) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

        if (pAnimContext == nullptr || pAnimContext->lAnimCurve == nullptr)
            return;

        pAnimContext->lAnimCurve->KeyModifyEnd();
    }

    void FBXService::AsFbxAnimAddBlendShapeKeyframe(IntPtr ptrAnimContext, float time, float value) {
        auto pAnimContext = (AsFbxAnimContext*)ptrAnimContext.ToPointer();

        if (pAnimContext == nullptr || pAnimContext->lAnimCurve == nullptr)
            return;

        FbxTime lTime;
        lTime.SetSecondDouble(time);

        auto keyIndex = pAnimContext->lAnimCurve->KeyAdd(lTime);
        pAnimContext->lAnimCurve->KeySetValue(keyIndex, value);
        pAnimContext->lAnimCurve->KeySetInterpolation(keyIndex, FbxAnimCurveDef::eInterpolationCubic);
    }

    IntPtr FBXService::AsFbxMorphCreateContext() {
        return IntPtr(new AsFbxMorphContext());
    }

    void FBXService::AsFbxMorphInitializeContext(IntPtr ptrContext, IntPtr ptrMorphContext, IntPtr ptrNode) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pNode = (FbxNode*)ptrNode.ToPointer();
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();

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

    void FBXService::AsFbxMorphDisposeContext(IntPtr pptrMorphContext) {
        auto ppMorphContext = (AsFbxMorphContext**)pptrMorphContext.ToPointer();

        if (ppMorphContext == nullptr)
            return;

        delete (*ppMorphContext);
        *ppMorphContext = nullptr;
    }

    void FBXService::AsFbxMorphAddBlendShapeChannel(IntPtr ptrContext, IntPtr ptrMorphContext, String^ strChannelName) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();
        auto channelName = (const char*)(Marshal::StringToHGlobalAuto(strChannelName).ToPointer());

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

        Marshal::FreeHGlobal(IntPtr((void*)channelName));
    }

    void FBXService::AsFbxMorphAddBlendShapeChannelShape(IntPtr ptrContext, IntPtr ptrMorphContext, float weight, String^ strShapeName) {
        auto pContext = (AsFbxContext*)ptrContext.ToPointer();
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();
        auto shapeName = (const char*)(Marshal::StringToHGlobalAuto(strShapeName).ToPointer());

        if (pContext == nullptr || pContext->pScene == nullptr)
            return;

        if (pMorphContext == nullptr)
            return;

        auto lShape = FbxShape::Create(pContext->pScene, shapeName);
        pMorphContext->lShape = lShape;

        if (pMorphContext->lBlendShapeChannel != nullptr)
            pMorphContext->lBlendShapeChannel->AddTargetShape(lShape, weight);

        Marshal::FreeHGlobal(IntPtr((void*)shapeName));
    }

    void FBXService::AsFbxMorphCopyBlendShapeControlPoints(IntPtr ptrMorphContext) {
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();

        if (pMorphContext == nullptr || pMorphContext->pMesh == nullptr || pMorphContext->lShape == nullptr)
            return;

        auto vectorCount = pMorphContext->pMesh->GetControlPointsCount();
        auto srcControlPoints = pMorphContext->pMesh->GetControlPoints();

        pMorphContext->lShape->InitControlPoints(vectorCount);

        for (int j = 0; j < vectorCount; j++) {
            pMorphContext->lShape->SetControlPointAt(FbxVector4(srcControlPoints[j]), j);;
        }
    }

    void FBXService::AsFbxMorphSetBlendShapeVertex(IntPtr ptrMorphContext, uint32_t index, Vector3 v) {
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();

        if (pMorphContext == nullptr || pMorphContext->lShape == nullptr)
            return;

        pMorphContext->lShape->SetControlPointAt(FbxVector4(v.X, v.Y, v.Z, 0), index);
    }

    void FBXService::AsFbxMorphCopyBlendShapeControlPointsNormal(IntPtr ptrMorphContext) {
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();

        if (pMorphContext == nullptr || pMorphContext->pMesh == nullptr || pMorphContext->lShape == nullptr)
            return;

        pMorphContext->lShape->InitNormals(pMorphContext->pMesh);
    }

    void FBXService::AsFbxMorphSetBlendShapeVertexNormal(IntPtr ptrMorphContext, uint32_t index, Vector3 v) {
        auto pMorphContext = (AsFbxMorphContext*)ptrMorphContext.ToPointer();

        if (pMorphContext == nullptr || pMorphContext->lShape == nullptr)
            return;

        pMorphContext->lShape->SetControlPointNormalAt(FbxVector4(v.X, v.Y, v.Z, 0), index);
    }
}
