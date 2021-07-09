namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Meshes {
    using Math;
    using Utils;

    public class BlendShapeVertex {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector3 tangent;
        public uint index;

        public BlendShapeVertex(ObjectReader reader) {
            vertex = reader.ReadVector3();
            normal = reader.ReadVector3();
            tangent = reader.ReadVector3();
            index = reader.ReadUInt32();
        }
    }
}
