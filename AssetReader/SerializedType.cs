namespace SoarCraft.QYun.AssetReader {
    public class SerializedType
    {
        public int ClassId;
        public bool MIsStrippedType;
        public short MScriptTypeIndex = -1;
        public TypeTree MType;
        public byte[] MScriptId; //Hash128
        public byte[] MOldTypeHash; //Hash128
        public int[] MTypeDependencies;
        public string MKlassName;
        public string MNameSpace;
        public string MAsmName;
    }
}
