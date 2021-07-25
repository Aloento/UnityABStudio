namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.SpriteAtlases {
    using Math;
    using Sprites;
    using Texture2Ds;
    using Utils;

    public class SpriteAtlasData {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public Rectf textureRect;
        public Vector2 textureRectOffset;
        public Vector2 atlasRectOffset;
        public Vector4 uvTransform;
        public float downscaleMultiplier;
        public SpriteSettings settingsRaw;
        public SecondarySpriteTexture[] secondaryTextures;

        public SpriteAtlasData(ObjectReader reader) {
            var version = reader.version;
            texture = new PPtr<Texture2D>(reader);
            alphaTexture = new PPtr<Texture2D>(reader);
            textureRect = new Rectf(reader);
            textureRectOffset = reader.ReadVector2();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) { //2017.2 and up
                atlasRectOffset = reader.ReadVector2();
            }
            uvTransform = reader.ReadVector4();
            downscaleMultiplier = reader.ReadSingle();
            settingsRaw = new SpriteSettings(reader);
            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) { //2020.2 and up
                var secondaryTexturesSize = reader.ReadInt32();
                secondaryTextures = new SecondarySpriteTexture[secondaryTexturesSize];
                for (var i = 0; i < secondaryTexturesSize; i++) {
                    secondaryTextures[i] = new SecondarySpriteTexture(reader);
                }
                reader.AlignStream();
            }
        }
    }
}
