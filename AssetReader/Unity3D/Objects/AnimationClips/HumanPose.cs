namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Math;
    using Utils;

    public class HumanPose {
        public xform m_RootX;
        public Vector3 m_LookAtPosition;
        public Vector4 m_LookAtWeight;
        public HumanGoal[] m_GoalArray;
        public HandPose m_LeftHandPose;
        public HandPose m_RightHandPose;
        public float[] m_DoFArray;
        public Vector3[] m_TDoFArray;

        public HumanPose(ObjectReader reader) {
            var version = reader.version;
            m_RootX = new xform(reader);
            m_LookAtPosition = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : reader.ReadVector4();//5.4 and up
            m_LookAtWeight = reader.ReadVector4();

            var numGoals = reader.ReadInt32();
            m_GoalArray = new HumanGoal[numGoals];
            for (var i = 0; i < numGoals; i++) {
                m_GoalArray[i] = new HumanGoal(reader);
            }

            m_LeftHandPose = new HandPose(reader);
            m_RightHandPose = new HandPose(reader);

            m_DoFArray = reader.ReadSingleArray();

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) { //5.2 and up
                var numTDof = reader.ReadInt32();
                m_TDoFArray = new Vector3[numTDof];
                for (var i = 0; i < numTDof; i++) {
                    m_TDoFArray[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : reader.ReadVector4();//5.4 and up
                }
            }
        }
    }
}
