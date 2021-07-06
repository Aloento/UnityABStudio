namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.AnimationClips {
    using Entities.Enums;
    using Utils;

    public class GenericBinding {
        public uint path;
        public uint attribute;
        public PPtr<UObject> script;
        public ClassIDType typeID;
        public byte customType;
        public byte isPPtrCurve;

        public GenericBinding(ObjectReader reader) {
            var version = reader.version;
            path = reader.ReadUInt32();
            attribute = reader.ReadUInt32();
            script = new PPtr<UObject>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                typeID = (ClassIDType)reader.ReadInt32();
            } else {
                typeID = (ClassIDType)reader.ReadUInt16();
            }
            customType = reader.ReadByte();
            isPPtrCurve = reader.ReadByte();
            reader.AlignStream();
        }
    }
}