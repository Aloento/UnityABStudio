#pragma once
#include "fbxsdk.h"
#include "AsFbxContext.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    public struct AsFbxSkinContext {
        FbxSkin* pSkin;
        FbxAMatrix lMeshMatrix;

        AsFbxSkinContext(AsFbxContext* pContext, FbxNode* pFrameNode);
        ~AsFbxSkinContext() = default;
    };
}
