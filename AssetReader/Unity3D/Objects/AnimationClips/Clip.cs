namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Utils;

    public class Clip {
        public StreamedClip m_StreamedClip;
        public DenseClip m_DenseClip;
        public ConstantClip m_ConstantClip;
        public ValueArrayConstant m_Binding;

        public Clip(ObjectReader reader) {
            var version = reader.version;
            m_StreamedClip = new StreamedClip(reader);
            m_DenseClip = new DenseClip(reader);
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) { //4.3 and up
                m_ConstantClip = new ConstantClip(reader);
            }
            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 3)) { //2018.3 down
                m_Binding = new ValueArrayConstant(reader);
            }
        }
    }
}
