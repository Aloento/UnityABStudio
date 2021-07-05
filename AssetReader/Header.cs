namespace SoarCraft.QYun.AssetReader {
    public class Header
    {
        public string Signature;
        public uint Version;
        public string UnityVersion;
        public string UnityRevision;
        public long Size;
        public uint CompressedBlocksInfoSize;
        public uint UncompressedBlocksInfoSize;
        public uint Flags;
    }
}
