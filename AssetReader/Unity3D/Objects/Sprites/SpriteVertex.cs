namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Sprites {
    using Math;
    using Utils;

    public class SpriteVertex {
        public Vector3 pos;
        public Vector2 uv;

        public SpriteVertex(ObjectReader reader) {
            var version = reader.version;

            pos = reader.ReadVector3();
            if (version[0] < 4 || (version[0] == 4 && version[1] <= 3)) //4.3 and down
            {
                uv = reader.ReadVector2();
            }
        }
    }
}