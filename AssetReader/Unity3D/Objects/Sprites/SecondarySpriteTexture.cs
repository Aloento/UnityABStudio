namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Sprites {
    using Texture2Ds;
    using Utils;

    public class SecondarySpriteTexture {
        public PPtr<Texture2D> texture;
        public string name;

        public SecondarySpriteTexture(ObjectReader reader) {
            texture = new PPtr<Texture2D>(reader);
            name = reader.ReadStringToNull();
        }
    }
}
