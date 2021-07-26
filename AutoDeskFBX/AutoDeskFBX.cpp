#include "pch.h"
#include "AutoDeskFBX.h"
#include "Math.h"

namespace SoarCraft::QYun::AutoDeskFBX::FBXService {
    void AsUtilQuaternionToEuler(float qx, float qy, float qz, float qw, float* vx, float* vy, float* vz) {
        Quaternion q(qx, qy, qz, qw);
        auto v = QuaternionToEuler(q);

        if (vx)
            *vx = v.X;

        if (vy)
            *vy = v.Y;

        if (vz)
            *vz = v.Z;
    }

    void AsUtilEulerToQuaternion(float vx, float vy, float vz, float* qx, float* qy, float* qz, float* qw) {
        Vector3 v(vx, vy, vz);
        auto q = EulerToQuaternion(v);

        if (qx)
            *qx = q.X;

        if (qy)
            *qy = q.Y;

        if (qz)
            *qz = q.Z;

        if (qw)
            *qw = q.W;
    }

    #define MGR_IOS_REF (*(pSdkManager->GetIOSettings()))

    static const char* FBXVersion[] = {
        FBX_2010_00_COMPATIBLE,
        FBX_2011_00_COMPATIBLE,
        FBX_2012_00_COMPATIBLE,
        FBX_2013_00_COMPATIBLE,
        FBX_2014_00_COMPATIBLE,
        FBX_2016_00_COMPATIBLE
    };

    AsFbxContext* AsFbxCreateContext() {
        return new AsFbxContext();
    }

    bool AsFbxInitializeContext(AsFbxContext* pContext, const char* pFileName, float scaleFactor, int32_t versionIndex, bool isAscii, bool is60Fps, const char** pErrMsg) {
        if (pContext == nullptr) {
            if (pErrMsg != nullptr) {
                *pErrMsg = "null pointer for pContext";
            }
            return false;
        }

        auto pSdkManager = FbxManager::Create();
        pContext->pSdkManager = pSdkManager;

        FbxIOSettings* ios = FbxIOSettings::Create(pSdkManager, IOSROOT);
        pSdkManager->SetIOSettings(ios);

        auto pScene = FbxScene::Create(pSdkManager, "");
        pContext->pScene = pScene;

        MGR_IOS_REF.SetBoolProp(EXP_FBX_MATERIAL, true);
        MGR_IOS_REF.SetBoolProp(EXP_FBX_TEXTURE, true);
        MGR_IOS_REF.SetBoolProp(EXP_FBX_EMBEDDED, false);
        MGR_IOS_REF.SetBoolProp(EXP_FBX_SHAPE, true);
        MGR_IOS_REF.SetBoolProp(EXP_FBX_GOBO, true);
        MGR_IOS_REF.SetBoolProp(EXP_FBX_ANIMATION, true);
        MGR_IOS_REF.SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);

        FbxGlobalSettings& globalSettings = pScene->GetGlobalSettings();
        globalSettings.SetSystemUnit(FbxSystemUnit(scaleFactor));

        if (is60Fps) {
            globalSettings.SetTimeMode(FbxTime::eFrames60);
        }

        auto pExporter = FbxExporter::Create(pScene, "");
        pContext->pExporter = pExporter;

        int pFileFormat = 0;

        if (versionIndex == 0) {
            pFileFormat = 3;

            if (isAscii) {
                pFileFormat = 4;
            }
        } else {
            pExporter->SetFileExportVersion(FBXVersion[versionIndex]);

            if (isAscii) {
                pFileFormat = 1;
            }
        }

        if (!pExporter->Initialize(pFileName, pFileFormat, pSdkManager->GetIOSettings())) {
            if (pErrMsg != nullptr) {
                auto errStr = pExporter->GetStatus().GetErrorString();
                *pErrMsg = errStr;
            }
            return false;
        }

        auto pBindPose = FbxPose::Create(pScene, "BindPose");
        pContext->pBindPose = pBindPose;

        pScene->AddPose(pBindPose);

        return true;
    }

    void AsFbxDisposeContext(AsFbxContext** ppContext) {
        if (ppContext == nullptr)
            return;

        delete (*ppContext);
        *ppContext = nullptr;
    }

    void AsFbxSetFramePaths(AsFbxContext* pContext, const char* ppPaths[], int32_t count) {
        if (pContext == nullptr)
            return;

        auto& framePaths = pContext->framePaths;

        for (auto i = 0; i < count; i += 1) {
            const char* path = ppPaths[i];
            framePaths.insert(std::string(path));
        }
    }

    void AsFbxExportScene(AsFbxContext* pContext) {
        if (pContext == nullptr)
            return;

        auto pScene = pContext->pScene;
        auto pExporter = pContext->pExporter;

        if (pExporter != nullptr && pScene != nullptr)
            pExporter->Export(pScene);
    }

    FbxNode* AsFbxGetSceneRootNode(AsFbxContext* pContext) {
        if (pContext == nullptr)
            return nullptr;

        if (pContext->pScene == nullptr)
            return nullptr;

        return pContext->pScene->GetRootNode();
    }

    FbxNode* AsFbxExportSingleFrame(AsFbxContext* pContext, FbxNode* pParentNode,
                                                const char* pFramePath, const char* pFrameName,
                                                float localPositionX, float localPositionY, float localPositionZ,
                                                float localRotationX, float localRotationY, float localRotationZ,
                                                float localScaleX, float localScaleY, float localScaleZ) {

        if (pContext == nullptr || pContext->pScene == nullptr)
            return nullptr;

        const auto& framePaths = pContext->framePaths;

        if (!(framePaths.empty() || framePaths.find(pFramePath) != framePaths.end()))
            return nullptr;

        auto pFrameNode = FbxNode::Create(pContext->pScene, pFrameName);

        pFrameNode->LclScaling.Set(FbxDouble3(localScaleX, localScaleY, localScaleZ));
        pFrameNode->LclRotation.Set(FbxDouble3(localRotationX, localRotationY, localRotationZ));
        pFrameNode->LclTranslation.Set(FbxDouble3(localPositionX, localPositionY, localPositionZ));
        pFrameNode->SetPreferedAngle(pFrameNode->LclRotation.Get());

        pParentNode->AddChild(pFrameNode);

        if (pContext->pBindPose != nullptr)
            pContext->pBindPose->Add(pFrameNode, pFrameNode->EvaluateGlobalTransform());

        return pFrameNode;
    }
}
