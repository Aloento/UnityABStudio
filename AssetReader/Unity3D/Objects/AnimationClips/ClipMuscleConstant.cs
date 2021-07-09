namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Math;
    using Utils;

    public class ClipMuscleConstant {
        public HumanPose m_DeltaPose;
        public xform m_StartX;
        public xform m_StopX;
        public xform m_LeftFootStartX;
        public xform m_RightFootStartX;
        public xform m_MotionStartX;
        public xform m_MotionStopX;
        public Vector3 m_AverageSpeed;
        public Clip m_Clip;
        public float m_StartTime;
        public float m_StopTime;
        public float m_OrientationOffsetY;
        public float m_Level;
        public float m_CycleOffset;
        public float m_AverageAngularSpeed;
        public int[] m_IndexArray;
        public ValueDelta[] m_ValueArrayDelta;
        public float[] m_ValueArrayReferencePose;
        public bool m_Mirror;
        public bool m_LoopTime;
        public bool m_LoopBlend;
        public bool m_LoopBlendOrientation;
        public bool m_LoopBlendPositionY;
        public bool m_LoopBlendPositionXZ;
        public bool m_StartAtOrigin;
        public bool m_KeepOriginalOrientation;
        public bool m_KeepOriginalPositionY;
        public bool m_KeepOriginalPositionXZ;
        public bool m_HeightFromFeet;

        public ClipMuscleConstant(ObjectReader reader) {
            var version = reader.version;
            m_DeltaPose = new HumanPose(reader);
            m_StartX = new xform(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) {
                //5.5 and up
                m_StopX = new xform(reader);
            }

            m_LeftFootStartX = new xform(reader);
            m_RightFootStartX = new xform(reader);
            if (version[0] < 5) {
                //5.0 down
                m_MotionStartX = new xform(reader);
                m_MotionStopX = new xform(reader);
            }

            m_AverageSpeed = version[0] > 5 || (version[0] == 5 && version[1] >= 4)
                ? reader.ReadVector3()
                : reader.ReadVector4(); //5.4 and up
            m_Clip = new Clip(reader);
            m_StartTime = reader.ReadSingle();
            m_StopTime = reader.ReadSingle();
            m_OrientationOffsetY = reader.ReadSingle();
            m_Level = reader.ReadSingle();
            m_CycleOffset = reader.ReadSingle();
            m_AverageAngularSpeed = reader.ReadSingle();

            m_IndexArray = reader.ReadInt32Array();
            if (version[0] < 4 || (version[0] == 4 && version[1] < 3)) {
                //4.3 down
                var m_AdditionalCurveIndexArray = reader.ReadInt32Array();
            }

            int numDeltas = reader.ReadInt32();
            m_ValueArrayDelta = new ValueDelta[numDeltas];
            for (int i = 0; i < numDeltas; i++) {
                m_ValueArrayDelta[i] = new ValueDelta(reader);
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3)) {
                //5.3 and up
                m_ValueArrayReferencePose = reader.ReadSingleArray();
            }

            m_Mirror = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) {
                //4.3 and up
                m_LoopTime = reader.ReadBoolean();
            }

            m_LoopBlend = reader.ReadBoolean();
            m_LoopBlendOrientation = reader.ReadBoolean();
            m_LoopBlendPositionY = reader.ReadBoolean();
            m_LoopBlendPositionXZ = reader.ReadBoolean();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) {
                //5.5 and up
                m_StartAtOrigin = reader.ReadBoolean();
            }

            m_KeepOriginalOrientation = reader.ReadBoolean();
            m_KeepOriginalPositionY = reader.ReadBoolean();
            m_KeepOriginalPositionXZ = reader.ReadBoolean();
            m_HeightFromFeet = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
