#include "pch.h"
#include "AsFbxSkinContext.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    AsFbxSkinContext::AsFbxSkinContext(AsFbxContext* pContext, FbxNode* pFrameNode) : pSkin(nullptr) {
        if (pContext != nullptr && pContext->pScene != nullptr) {
            pSkin = FbxSkin::Create(pContext->pScene, "");
        }

        if (pFrameNode != nullptr) {
            lMeshMatrix = pFrameNode->EvaluateGlobalTransform();
        }
    }
}
