namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using System;
    using Utils;

    public class AnimationCurve<T> {
        public Keyframe<T>[] m_Curve;
        public int m_PreInfinity;
        public int m_PostInfinity;
        public int m_RotationOrder;

        public AnimationCurve(ObjectReader reader, Func<T> readerFunc) {
            var version = reader.version;
            var numCurves = reader.ReadInt32();
            m_Curve = new Keyframe<T>[numCurves];
            for (var i = 0; i < numCurves; i++) {
                m_Curve[i] = new Keyframe<T>(reader, readerFunc);
            }

            m_PreInfinity = reader.ReadInt32();
            m_PostInfinity = reader.ReadInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3)) { //5.3 and up
                m_RotationOrder = reader.ReadInt32();
            }
        }
    }
}
