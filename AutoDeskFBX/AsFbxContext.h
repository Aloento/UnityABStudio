#pragma once
#include <string>
#include <unordered_set>
#include "fbxsdk.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    public struct AsFbxContext {
        fbxsdk::FbxManager* pSdkManager;
        fbxsdk::FbxScene* pScene;
        fbxsdk::FbxArray<fbxsdk::FbxFileTexture*>* pTextures;
        fbxsdk::FbxArray<fbxsdk::FbxSurfacePhong*>* pMaterials;
        fbxsdk::FbxExporter* pExporter;
        fbxsdk::FbxPose* pBindPose;

        std::unordered_set<std::string> framePaths;

        AsFbxContext();
        ~AsFbxContext();
    };
}
