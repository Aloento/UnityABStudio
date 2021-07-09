namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Shaders {
    using System.Collections.Generic;
    using Utils;

    public class SerializedTagMap {
        public KeyValuePair<string, string>[] tags;

        public SerializedTagMap(UnityReader reader) {
            int numTags = reader.ReadInt32();
            tags = new KeyValuePair<string, string>[numTags];
            for (int i = 0; i < numTags; i++) {
                tags[i] = new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString());
            }
        }
    }
}
