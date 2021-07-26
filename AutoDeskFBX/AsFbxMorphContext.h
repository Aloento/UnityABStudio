#pragma once
#include <fbxsdk.h>

namespace SoarCraft::QYun::AutoDeskFBX {
    public struct AsFbxMorphContext {
        FbxMesh* pMesh;
        FbxBlendShape* lBlendShape;
        FbxBlendShapeChannel* lBlendShapeChannel;
        FbxShape* lShape;

        AsFbxMorphContext();
        ~AsFbxMorphContext() = default;
    };
}
