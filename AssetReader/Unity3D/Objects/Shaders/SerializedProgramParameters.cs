namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedProgramParameters {
        public VectorParameter[] m_VectorParams;
        public MatrixParameter[] m_MatrixParams;
        public TextureParameter[] m_TextureParams;
        public BufferBinding[] m_BufferParams;
        public ConstantBuffer[] m_ConstantBuffers;
        public BufferBinding[] m_ConstantBufferBindings;
        public UAVParameter[] m_UAVParams;
        public SamplerParameter[] m_Samplers;

        public SerializedProgramParameters(ObjectReader reader) {
            var numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (var i = 0; i < numVectorParams; i++) {
                m_VectorParams[i] = new VectorParameter(reader);
            }

            var numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (var i = 0; i < numMatrixParams; i++) {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }

            var numTextureParams = reader.ReadInt32();
            m_TextureParams = new TextureParameter[numTextureParams];
            for (var i = 0; i < numTextureParams; i++) {
                m_TextureParams[i] = new TextureParameter(reader);
            }

            var numBufferParams = reader.ReadInt32();
            m_BufferParams = new BufferBinding[numBufferParams];
            for (var i = 0; i < numBufferParams; i++) {
                m_BufferParams[i] = new BufferBinding(reader);
            }

            var numConstantBuffers = reader.ReadInt32();
            m_ConstantBuffers = new ConstantBuffer[numConstantBuffers];
            for (var i = 0; i < numConstantBuffers; i++) {
                m_ConstantBuffers[i] = new ConstantBuffer(reader);
            }

            var numConstantBufferBindings = reader.ReadInt32();
            m_ConstantBufferBindings = new BufferBinding[numConstantBufferBindings];
            for (var i = 0; i < numConstantBufferBindings; i++) {
                m_ConstantBufferBindings[i] = new BufferBinding(reader);
            }

            var numUAVParams = reader.ReadInt32();
            m_UAVParams = new UAVParameter[numUAVParams];
            for (var i = 0; i < numUAVParams; i++) {
                m_UAVParams[i] = new UAVParameter(reader);
            }

            var numSamplers = reader.ReadInt32();
            m_Samplers = new SamplerParameter[numSamplers];
            for (var i = 0; i < numSamplers; i++) {
                m_Samplers[i] = new SamplerParameter(reader);
            }
        }
    }
}
