#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    Vector3 FBXService::AsUtilQuaternionToEuler(Quaternion q) {
        FbxAMatrix lMatrixRot;
        lMatrixRot.SetQ(FbxQuaternion(q.X, q.Y, q.Z, q.W));
        FbxVector4 lEuler = lMatrixRot.GetR();
        return Vector3((float)lEuler[0], (float)lEuler[1], (float)lEuler[2]);
    }

    void FBXService::AsUtilEulerToQuaternion(float vx, float vy, float vz, float* qx, float* qy, float* qz, float* qw) {
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
        FBX_2016_00_COMPATIBLE,
        FBX_2018_00_COMPATIBLE,
        FBX_2019_00_COMPATIBLE,
        FBX_2020_00_COMPATIBLE
    };

    IntPtr FBXService::AsFbxCreateContext() {
        return IntPtr(new AsFbxContext());;
    }

    bool FBXService::AsFbxInitializeContext(IntPtr ptrContext, String^ fileName, float scaleFactor,
                                            int32_t versionIndex, bool isAscii, bool is60Fps, [Out] String^% errorMessage) {
        auto pContext = (AsFbxContext*) ptrContext.ToPointer();

        if (pContext == nullptr) {
            errorMessage = "null pointer for pContext";
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

        const char* pFileName = (const char*)(Marshal::StringToHGlobalAnsi(fileName).ToPointer());
        if (!pExporter->Initialize(pFileName, pFileFormat, pSdkManager->GetIOSettings())) {
            errorMessage = gcnew String(pExporter->GetStatus().GetErrorString());
            return false;
        }

        auto pBindPose = FbxPose::Create(pScene, "BindPose");
        pContext->pBindPose = pBindPose;

        pScene->AddPose(pBindPose);
        Marshal::FreeHGlobal(IntPtr((void*)pFileName));
        return true;
    }

    void FBXService::AsFbxDisposeContext(AsFbxContext** ppContext) {
        if (ppContext == nullptr)
            return;

        delete (*ppContext);
        *ppContext = nullptr;
    }

    void FBXService::AsFbxSetFramePaths(AsFbxContext* pContext, const char* ppPaths[], int32_t count) {
        if (pContext == nullptr)
            return;

        auto& framePaths = pContext->framePaths;

        for (auto i = 0; i < count; i += 1) {
            const char* path = ppPaths[i];
            framePaths.insert(std::string(path));
        }
    }

    void FBXService::AsFbxExportScene(AsFbxContext* pContext) {
        if (pContext == nullptr)
            return;

        auto pScene = pContext->pScene;
        auto pExporter = pContext->pExporter;

        if (pExporter != nullptr && pScene != nullptr)
            pExporter->Export(pScene);
    }

    FbxNode* FBXService::AsFbxGetSceneRootNode(AsFbxContext* pContext) {
        if (pContext == nullptr)
            return nullptr;

        if (pContext->pScene == nullptr)
            return nullptr;

        return pContext->pScene->GetRootNode();
    }

    FbxNode* FBXService::AsFbxExportSingleFrame(AsFbxContext* pContext, FbxNode* pParentNode,
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
