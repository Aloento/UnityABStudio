namespace SoarCraft.QYun.AssetReader.Entities.Structs {
    public class Header {
        public string signature;
        public uint version;
        public string unityVersion;
        public string unityRevision;
        public long size;
        public uint compressedBlocksInfoSize;
        public uint uncompressedBlocksInfoSize;
        public uint flags;
    }
}
