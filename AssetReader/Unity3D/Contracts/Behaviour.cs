namespace SoarCraft.QYun.AssetReader.Unity3D.Contracts {
    using Utils;

    public abstract class Behaviour : Component {
        public byte m_Enabled;

        protected Behaviour(ObjectReader reader) : base(reader) {
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
