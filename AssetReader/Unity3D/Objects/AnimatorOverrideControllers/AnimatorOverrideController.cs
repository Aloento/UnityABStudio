﻿namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorOverrideControllers {
    using Utils;

    public sealed class AnimatorOverrideController : RuntimeAnimatorController {
        public PPtr<RuntimeAnimatorController> m_Controller;
        public AnimationClipOverride[] m_Clips;

        public AnimatorOverrideController(ObjectReader reader) : base(reader) {
            m_Controller = new PPtr<RuntimeAnimatorController>(reader);

            var numOverrides = reader.ReadInt32();
            m_Clips = new AnimationClipOverride[numOverrides];
            for (var i = 0; i < numOverrides; i++) {
                m_Clips[i] = new AnimationClipOverride(reader);
            }
        }
    }
}
