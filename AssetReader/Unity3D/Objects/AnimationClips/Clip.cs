namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using System.Collections.Generic;
    using Entities.Enums;
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

        public AnimationClipBindingConstant ConvertValueArrayToGenericBinding() {
            var bindings = new AnimationClipBindingConstant();
            var genericBindings = new List<GenericBinding>();
            var values = m_Binding;
            for (var i = 0; i < values.m_ValueArray.Length;) {
                var curveID = values.m_ValueArray[i].m_ID;
                var curveTypeID = values.m_ValueArray[i].m_TypeID;
                var binding = new GenericBinding();
                genericBindings.Add(binding);
                switch (curveTypeID) {
                    //CRC(PositionX))
                    case 4174552735:
                        binding.path = curveID;
                        binding.attribute = 1; //kBindTransformPosition
                        binding.typeID = ClassIDType.Transform;
                        i += 3;
                        break;
                    //CRC(QuaternionX))
                    case 2211994246:
                        binding.path = curveID;
                        binding.attribute = 2; //kBindTransformRotation
                        binding.typeID = ClassIDType.Transform;
                        i += 4;
                        break;
                    //CRC(ScaleX))
                    case 1512518241:
                        binding.path = curveID;
                        binding.attribute = 3; //kBindTransformScale
                        binding.typeID = ClassIDType.Transform;
                        i += 3;
                        break;
                    default:
                        binding.typeID = ClassIDType.Animator;
                        binding.path = 0;
                        binding.attribute = curveID;
                        i++;
                        break;
                }
            }
            bindings.genericBindings = genericBindings.ToArray();
            return bindings;
        }
    }
}
