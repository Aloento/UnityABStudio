namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using Utils;

    public sealed class BuildSettings : UObject {
        public string m_Version;

        public BuildSettings(ObjectReader reader) : base(reader) {
            var levels = reader.ReadStringArray();

            var hasRenderTexture = reader.ReadBoolean();
            var hasPROVersion = reader.ReadBoolean();
            var hasPublishingRights = reader.ReadBoolean();
            var hasShadows = reader.ReadBoolean();

            m_Version = reader.ReadAlignedString();
        }
    }
}
