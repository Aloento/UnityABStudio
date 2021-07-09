namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimatorControllers {
    using AnimationClips;
    using Utils;

    public class ControllerConstant {
        public LayerConstant[] m_LayerArray;
        public StateMachineConstant[] m_StateMachineArray;
        public ValueArrayConstant m_Values;
        public ValueArray m_DefaultValues;

        public ControllerConstant(ObjectReader reader) {
            var numLayers = reader.ReadInt32();
            m_LayerArray = new LayerConstant[numLayers];
            for (var i = 0; i < numLayers; i++) {
                m_LayerArray[i] = new LayerConstant(reader);
            }

            var numStates = reader.ReadInt32();
            m_StateMachineArray = new StateMachineConstant[numStates];
            for (var i = 0; i < numStates; i++) {
                m_StateMachineArray[i] = new StateMachineConstant(reader);
            }

            m_Values = new ValueArrayConstant(reader);
            m_DefaultValues = new ValueArray(reader);
        }
    }
}
