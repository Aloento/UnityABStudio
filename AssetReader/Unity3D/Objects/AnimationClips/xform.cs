namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;
    using Math;
    public class xform {
        public Vector3 t;
        public Quaternion q;
        public Vector3 s;

        public xform(ObjectReader reader) {
            var version = reader.version;
            t = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : reader.ReadVector4();//5.4 and up
            q = reader.ReadQuaternion();
            s = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : reader.ReadVector4();//5.4 and up
        }
    }
}