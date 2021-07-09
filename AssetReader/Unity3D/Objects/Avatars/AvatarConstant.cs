namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using AnimationClips;
    using Utils;

    public class AvatarConstant {
        public Skeleton m_AvatarSkeleton;
        public SkeletonPose m_AvatarSkeletonPose;
        public SkeletonPose m_DefaultPose;
        public uint[] m_SkeletonNameIDArray;
        public Human m_Human;
        public int[] m_HumanSkeletonIndexArray;
        public int[] m_HumanSkeletonReverseIndexArray;
        public int m_RootMotionBoneIndex;
        public xform m_RootMotionBoneX;
        public Skeleton m_RootMotionSkeleton;
        public SkeletonPose m_RootMotionSkeletonPose;
        public int[] m_RootMotionSkeletonIndexArray;

        public AvatarConstant(ObjectReader reader) {
            var version = reader.version;
            m_AvatarSkeleton = new Skeleton(reader);
            m_AvatarSkeletonPose = new SkeletonPose(reader);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) { //4.3 and up
                m_DefaultPose = new SkeletonPose(reader);

                m_SkeletonNameIDArray = reader.ReadUInt32Array();
            }

            m_Human = new Human(reader);

            m_HumanSkeletonIndexArray = reader.ReadInt32Array();

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) { //4.3 and up
                m_HumanSkeletonReverseIndexArray = reader.ReadInt32Array();
            }

            m_RootMotionBoneIndex = reader.ReadInt32();
            m_RootMotionBoneX = new xform(reader);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) { //4.3 and up
                m_RootMotionSkeleton = new Skeleton(reader);
                m_RootMotionSkeletonPose = new SkeletonPose(reader);

                m_RootMotionSkeletonIndexArray = reader.ReadInt32Array();
            }
        }
    }
}
