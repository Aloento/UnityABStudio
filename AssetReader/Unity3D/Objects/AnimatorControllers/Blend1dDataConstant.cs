namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using Utils;

    public class Blend1dDataConstant { // wrong labeled
        public float[] m_ChildThresholdArray;

        public Blend1dDataConstant(ObjectReader reader) => m_ChildThresholdArray = reader.ReadSingleArray();
    }
}
