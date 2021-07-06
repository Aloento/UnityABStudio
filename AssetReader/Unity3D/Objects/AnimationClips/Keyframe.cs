namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using System;
    using Utils;

    public class Keyframe<T> {
        public float time;
        public T value;
        public T inSlope;
        public T outSlope;
        public int weightedMode;
        public T inWeight;
        public T outWeight;

        public Keyframe(ObjectReader reader, Func<T> readerFunc) {
            this.time = reader.ReadSingle();
            this.value = readerFunc();
            this.inSlope = readerFunc();
            this.outSlope = readerFunc();
            if (reader.version[0] >= 2018) { //2018 and up
                this.weightedMode = reader.ReadInt32();
                this.inWeight = readerFunc();
                this.outWeight = readerFunc();
            }
        }
    }
}
