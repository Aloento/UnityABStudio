namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Sprites {
    using Utils;

    public class Rectf {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rectf(UnityReader reader) {
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            width = reader.ReadSingle();
            height = reader.ReadSingle();
        }
    }
}