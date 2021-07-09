namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorOverrideControllers {
    using AnimationClips;
    using Utils;

    public class AnimationClipOverride {
        public PPtr<AnimationClip> m_OriginalClip;
        public PPtr<AnimationClip> m_OverrideClip;

        public AnimationClipOverride(ObjectReader reader) {
            m_OriginalClip = new PPtr<AnimationClip>(reader);
            m_OverrideClip = new PPtr<AnimationClip>(reader);
        }
    }
}
