namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using Contracts;
    using Utils;

    public sealed class TextAsset : NamedObject {
        public byte[] m_Script;

        public TextAsset(ObjectReader reader) : base(reader) => m_Script = reader.ReadUInt8Array();
    }
}
