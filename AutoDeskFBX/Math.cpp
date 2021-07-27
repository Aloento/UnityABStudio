#include "pch.h"
#include "AutoDeskFBX.h"

namespace SoarCraft::QYun::AutoDeskFBX {
    Quaternion FBXService::EulerToQuaternion(Vector3 v) {
        FbxAMatrix lMatrixRot;
        lMatrixRot.SetR(FbxVector4(v.X, v.Y, v.Z));
        FbxQuaternion lQuaternion = lMatrixRot.GetQ();
        return Quaternion((float)lQuaternion[0], (float)lQuaternion[1], (float)lQuaternion[2], (float)lQuaternion[3]);
    }
}
