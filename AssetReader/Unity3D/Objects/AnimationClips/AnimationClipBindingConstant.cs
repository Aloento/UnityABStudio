namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Entities.Enums;
    using Utils;

    public class AnimationClipBindingConstant {
        public GenericBinding[] genericBindings;
        public PPtr<UObject>[] pptrCurveMapping;

        public AnimationClipBindingConstant() { }

        public AnimationClipBindingConstant(ObjectReader reader) {
            var numBindings = reader.ReadInt32();
            genericBindings = new GenericBinding[numBindings];
            for (var i = 0; i < numBindings; i++) {
                genericBindings[i] = new GenericBinding(reader);
            }

            var numMappings = reader.ReadInt32();
            pptrCurveMapping = new PPtr<UObject>[numMappings];
            for (var i = 0; i < numMappings; i++) {
                pptrCurveMapping[i] = new PPtr<UObject>(reader);
            }
        }

        public GenericBinding FindBinding(int index) {
            var curves = 0;
            foreach (var b in genericBindings) {
                if (b.typeID == ClassIDType.Transform) {
                    switch (b.attribute) {
                        case 1: //kBindTransformPosition
                        case 3: //kBindTransformScale
                        case 4: //kBindTransformEuler
                            curves += 3;
                            break;
                        case 2: //kBindTransformRotation
                            curves += 4;
                            break;
                        default:
                            curves += 1;
                            break;
                    }
                } else {
                    curves += 1;
                }
                if (curves > index) {
                    return b;
                }
            }

            return null;
        }
    }
}
