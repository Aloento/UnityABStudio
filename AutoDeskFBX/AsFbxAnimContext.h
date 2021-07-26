#pragma once
#include "fbxsdk.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    public struct AsFbxAnimContext {
        FbxAnimCurveFilterUnroll* lFilter;

        FbxAnimStack* lAnimStack;
        FbxAnimLayer* lAnimLayer;

        FbxAnimCurve* lCurveSX;
        FbxAnimCurve* lCurveSY;
        FbxAnimCurve* lCurveSZ;
        FbxAnimCurve* lCurveRX;
        FbxAnimCurve* lCurveRY;
        FbxAnimCurve* lCurveRZ;
        FbxAnimCurve* lCurveTX;
        FbxAnimCurve* lCurveTY;
        FbxAnimCurve* lCurveTZ;

        FbxMesh* pMesh;
        FbxBlendShape* lBlendShape;
        FbxAnimCurve* lAnimCurve;

        AsFbxAnimContext(bool eulerFilter);
        ~AsFbxAnimContext() = default;
    };
}
