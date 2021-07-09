namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using Utils;

    public class SerializedCustomEditorForRenderPipeline {
        public string customEditorName;
        public string renderPipelineType;

        public SerializedCustomEditorForRenderPipeline(UnityReader reader) {
            customEditorName = reader.ReadAlignedString();
            renderPipelineType = reader.ReadAlignedString();
        }
    }
}
