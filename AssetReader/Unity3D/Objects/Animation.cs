namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using AnimationClips;
    using Contracts;
    using Utils;

    public sealed class Animation : Behaviour {
        public PPtr<AnimationClip>[] m_Animations;

        public Animation(ObjectReader reader) : base(reader) {
            var m_Animation = new PPtr<AnimationClip>(reader);
            var numAnimations = reader.ReadInt32();
            m_Animations = new PPtr<AnimationClip>[numAnimations];
            for (var i = 0; i < numAnimations; i++) {
                m_Animations[i] = new PPtr<AnimationClip>(reader);
            }
        }
    }
}
