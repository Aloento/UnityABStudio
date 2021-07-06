namespace SoarCraft.QYun.AssetReader {
    using System.IO;
    using System.Linq;
    using Entities.Structs;
    using Helpers;
    using Utils;

    public class BundleFile {
        public Header m_Header;
        private StorageBlock[] m_BlocksInfo;
        private Node[] m_DirectoryInfo;

        public StreamFile[] fileList;

        public BundleFile(UnityReader reader) {
            m_Header = new Header {
                signature = reader.ReadStringToNull(),
                version = reader.ReadUInt32(),
                unityVersion = reader.ReadStringToNull(),
                unityRevision = reader.ReadStringToNull()
            };
            switch (m_Header.signature) {
                case "UnityArchive":
                    break; //TODO
                case "UnityWeb":
                case "UnityRaw":
                    if (m_Header.version == 6) {
                        goto case "UnityFS";
                    }
                    ReadHeaderAndBlocksInfo(reader);
                    using (var blocksStream = CreateBlocksStream(reader.FullPath)) {
                        ReadBlocksAndDirectory(reader, blocksStream);
                        ReadFiles(blocksStream, reader.FullPath);
                    }
                    break;
                case "UnityFS":
                    ReadHeader(reader);
                    ReadBlocksInfoAndDirectory(reader);
                    using (var blocksStream = CreateBlocksStream(reader.FullPath)) {
                        ReadBlocks(reader, blocksStream);
                        ReadFiles(blocksStream, reader.FullPath);
                    }
                    break;
            }
        }

        private void ReadHeaderAndBlocksInfo(UnityReader reader) {
            var isCompressed = m_Header.signature == "UnityWeb";
            if (m_Header.version >= 4) {
                var hash = reader.ReadBytes(16);
                var crc = reader.ReadUInt32();
            }
            var minimumStreamedBytes = reader.ReadUInt32();
            m_Header.size = reader.ReadUInt32();
            var numberOfLevelsToDownloadBeforeStreaming = reader.ReadUInt32();
            var levelCount = reader.ReadInt32();
            m_BlocksInfo = new StorageBlock[1];
            for (var i = 0; i < levelCount; i++) {
                var storageBlock = new StorageBlock() {
                    compressedSize = reader.ReadUInt32(),
                    uncompressedSize = reader.ReadUInt32(),
                    flags = (ushort)(isCompressed ? 1 : 0)
                };
                if (i == levelCount - 1) {
                    m_BlocksInfo[0] = storageBlock;
                }
            }
            if (m_Header.version >= 2) {
                var completeFileSize = reader.ReadUInt32();
            }
            if (m_Header.version >= 3) {
                var fileInfoHeaderSize = reader.ReadUInt32();
            }
            reader.Position = m_Header.size;
        }

        private Stream CreateBlocksStream(string path) {
            Stream blocksStream;
            var uncompressedSizeSum = m_BlocksInfo.Sum(x => x.uncompressedSize);
            if (uncompressedSizeSum >= int.MaxValue) {
                /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, uncompressedSizeSum);
                assetsDataStream = memoryMappedFile.CreateViewStream();*/
                blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            } else {
                blocksStream = new MemoryStream((int)uncompressedSizeSum);
            }
            return blocksStream;
        }

        private void ReadBlocksAndDirectory(UnityReader reader, Stream blocksStream) {
            foreach (var blockInfo in m_BlocksInfo) {
                var uncompressedBytes = reader.ReadBytes((int)blockInfo.compressedSize);
                if (blockInfo.flags == 1) {
                    using var memoryStream = new MemoryStream(uncompressedBytes);
                    using var decompressStream = SevenZipHelper.StreamDecompress(memoryStream);
                    uncompressedBytes = decompressStream.ToArray();
                }
                blocksStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
            }
            blocksStream.Position = 0;
            var blocksReader = new UnityReader(blocksStream);
            var nodesCount = blocksReader.ReadInt32();
            m_DirectoryInfo = new Node[nodesCount];
            for (var i = 0; i < nodesCount; i++) {
                m_DirectoryInfo[i] = new Node {
                    path = blocksReader.ReadStringToNull(),
                    offset = blocksReader.ReadUInt32(),
                    size = blocksReader.ReadUInt32()
                };
            }
        }

        public void ReadFiles(Stream blocksStream, string path) {
            fileList = new StreamFile[m_DirectoryInfo.Length];
            for (var i = 0; i < m_DirectoryInfo.Length; i++) {
                var node = m_DirectoryInfo[i];
                var file = new StreamFile();
                fileList[i] = file;
                file.path = node.path;
                file.fileName = Path.GetFileName(node.path);
                if (node.size >= int.MaxValue) {
                    /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, entryinfo_size);
                    file.stream = memoryMappedFile.CreateViewStream();*/
                    var extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                    _ = Directory.CreateDirectory(extractPath);
                    file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                } else {
                    file.stream = new MemoryStream((int)node.size);
                }
                blocksStream.Position = node.offset;
                blocksStream.CopyTo(file.stream, node.size);
                file.stream.Position = 0;
            }
        }

        private void ReadHeader(UnityReader reader) {
            m_Header.size = reader.ReadInt64();
            m_Header.compressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.uncompressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.flags = reader.ReadUInt32();
            if (m_Header.signature != "UnityFS") {
                _ = reader.ReadByte();
            }
        }

        private void ReadBlocksInfoAndDirectory(UnityReader reader) {
            byte[] blocksInfoBytes;
            if (m_Header.version >= 7) {
                reader.AlignStream(16);
            }
            if ((m_Header.flags & 0x80) != 0) { //kArchiveBlocksInfoAtTheEnd
                var position = reader.Position;
                reader.Position = reader.BaseStream.Length - m_Header.compressedBlocksInfoSize;
                blocksInfoBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
                reader.Position = position;
            } else { //0x40 kArchiveBlocksAndDirectoryInfoCombined
                blocksInfoBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
            }
            var blocksInfoCompressedStream = new MemoryStream(blocksInfoBytes);
            MemoryStream blocksInfoUncompressedStream;
            switch (m_Header.flags & 0x3F) { //kArchiveCompressionTypeMask
                default: { //None
                    blocksInfoUncompressedStream = blocksInfoCompressedStream;
                    break;
                }
                case 1: { //LZMA
                    blocksInfoUncompressedStream = new MemoryStream((int)this.m_Header.uncompressedBlocksInfoSize);
                    SevenZipHelper.StreamDecompress(blocksInfoCompressedStream, blocksInfoUncompressedStream, m_Header.compressedBlocksInfoSize, m_Header.uncompressedBlocksInfoSize);
                    blocksInfoUncompressedStream.Position = 0;
                    blocksInfoCompressedStream.Close();
                    break;
                }
                case 2: //LZ4
                case 3: { //LZ4HC
                    var uncompressedBytes = new byte[m_Header.uncompressedBlocksInfoSize];
                    using (var decoder = new Lz4DecoderStream(blocksInfoCompressedStream)) {
                        _ = decoder.Read(uncompressedBytes, 0, uncompressedBytes.Length);
                    }
                    blocksInfoUncompressedStream = new MemoryStream(uncompressedBytes);
                    break;
                }
            }

            using var blocksInfoReader = new UnityReader(blocksInfoUncompressedStream);
            var uncompressedDataHash = blocksInfoReader.ReadBytes(16);
            var blocksInfoCount = blocksInfoReader.ReadInt32();
            this.m_BlocksInfo = new StorageBlock[blocksInfoCount];
            for (var i = 0; i < blocksInfoCount; i++) {
                this.m_BlocksInfo[i] = new StorageBlock {
                    uncompressedSize = blocksInfoReader.ReadUInt32(),
                    compressedSize = blocksInfoReader.ReadUInt32(),
                    flags = blocksInfoReader.ReadUInt16()
                };
            }

            var nodesCount = blocksInfoReader.ReadInt32();
            this.m_DirectoryInfo = new Node[nodesCount];
            for (var i = 0; i < nodesCount; i++) {
                this.m_DirectoryInfo[i] = new Node {
                    offset = blocksInfoReader.ReadInt64(),
                    size = blocksInfoReader.ReadInt64(),
                    flags = blocksInfoReader.ReadUInt32(),
                    path = blocksInfoReader.ReadStringToNull(),
                };
            }
        }

        private void ReadBlocks(EndianBinaryReader reader, Stream blocksStream) {
            foreach (var blockInfo in m_BlocksInfo) {
                switch (blockInfo.flags & 0x3F) //kStorageBlockCompressionTypeMask
                {
                    default: //None
                        {
                        reader.BaseStream.CopyTo(blocksStream, blockInfo.compressedSize);
                        break;
                    }
                    case 1: //LZMA
                        {
                        SevenZipHelper.StreamDecompress(reader.BaseStream, blocksStream, blockInfo.compressedSize, blockInfo.uncompressedSize);
                        break;
                    }
                    case 2: //LZ4
                    case 3: //LZ4HC
                        {
                        var compressedStream = new MemoryStream(reader.ReadBytes((int)blockInfo.compressedSize));
                        using var lz4Stream = new Lz4DecoderStream(compressedStream);
                        lz4Stream.CopyTo(blocksStream, blockInfo.uncompressedSize);
                        break;
                    }
                }
            }
            blocksStream.Position = 0;
        }
    }
}
