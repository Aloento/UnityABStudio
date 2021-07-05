namespace SoarCraft.QYun.AssetReader {
    public class BundleFile {
        public Header MHeader;
        private StorageBlock[] mBlocksInfo;
        private Node[] mDirectoryInfo;

        public StreamFile[] FileList;

        public BundleFile(EndianBinaryReader reader, string path)
        {
            this.MHeader = new Header();
            this.MHeader.Signature = reader.ReadStringToNull();
            this.MHeader.Version = reader.ReadUInt32();
            this.MHeader.UnityVersion = reader.ReadStringToNull();
            this.MHeader.UnityRevision = reader.ReadStringToNull();
            switch (this.MHeader.Signature)
            {
                case "UnityArchive":
                    break; //TODO
                case "UnityWeb":
                case "UnityRaw":
                    if (this.MHeader.Version == 6)
                    {
                        goto case "UnityFS";
                    }
                    ReadHeaderAndBlocksInfo(reader);
                    using (var blocksStream = CreateBlocksStream(path))
                    {
                        ReadBlocksAndDirectory(reader, blocksStream);
                        ReadFiles(blocksStream, path);
                    }
                    break;
                case "UnityFS":
                    ReadHeader(reader);
                    ReadBlocksInfoAndDirectory(reader);
                    using (var blocksStream = CreateBlocksStream(path))
                    {
                        ReadBlocks(reader, blocksStream);
                        ReadFiles(blocksStream, path);
                    }
                    break;
            }
        }

        private void ReadHeaderAndBlocksInfo(EndianBinaryReader reader)
        {
            var isCompressed = this.MHeader.Signature == "UnityWeb";
            if (this.MHeader.Version >= 4)
            {
                var hash = reader.ReadBytes(16);
                var crc = reader.ReadUInt32();
            }
            var minimumStreamedBytes = reader.ReadUInt32();
            this.MHeader.Size = reader.ReadUInt32();
            var numberOfLevelsToDownloadBeforeStreaming = reader.ReadUInt32();
            var levelCount = reader.ReadInt32();
            this.mBlocksInfo = new StorageBlock[1];
            for (var i = 0; i < levelCount; i++)
            {
                var storageBlock = new StorageBlock()
                {
                    CompressedSize = reader.ReadUInt32(),
                    UncompressedSize = reader.ReadUInt32(),
                    Flags = (ushort)(isCompressed ? 1 : 0)
                };
                if (i == levelCount - 1)
                {
                    this.mBlocksInfo[0] = storageBlock;
                }
            }
            if (this.MHeader.Version >= 2)
            {
                var completeFileSize = reader.ReadUInt32();
            }
            if (this.MHeader.Version >= 3)
            {
                var fileInfoHeaderSize = reader.ReadUInt32();
            }
            reader.Position = this.MHeader.Size;
        }

        private Stream CreateBlocksStream(string path)
        {
            Stream blocksStream;
            var uncompressedSizeSum = this.mBlocksInfo.Sum(x => x.uncompressedSize);
            if (uncompressedSizeSum >= int.MaxValue)
            {
                /*var memoryMappedFile = MemoryMappedFile.CreateNew(Path.GetFileName(path), uncompressedSizeSum);
                assetsDataStream = memoryMappedFile.CreateViewStream();*/
                blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            }
            else
            {
                blocksStream = new MemoryStream((int)uncompressedSizeSum);
            }
            return blocksStream;
        }

        private void ReadBlocksAndDirectory(EndianBinaryReader reader, Stream blocksStream)
        {
            foreach (var blockInfo in this.mBlocksInfo)
            {
                var uncompressedBytes = reader.ReadBytes((int)blockInfo.CompressedSize);
                if (blockInfo.Flags == 1)
                {
                    using (var memoryStream = new MemoryStream(uncompressedBytes))
                    {
                        using (var decompressStream = SevenZipHelper.StreamDecompress(memoryStream))
                        {
                            uncompressedBytes = decompressStream.ToArray();
                        }
                    }
                }
                blocksStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
            }
            blocksStream.Position = 0;
            var blocksReader = new EndianBinaryReader(blocksStream);
            var nodesCount = blocksReader.ReadInt32();
            this.mDirectoryInfo = new Node[nodesCount];
            for (var i = 0; i < nodesCount; i++)
            {
                this.mDirectoryInfo[i] = new Node
                {
                    Path = blocksReader.ReadStringToNull(),
                    Offset = blocksReader.ReadUInt32(),
                    Size = blocksReader.ReadUInt32()
                };
            }
        }

        public void ReadFiles(Stream blocksStream, string path)
        {
            this.FileList = new StreamFile[this.mDirectoryInfo.Length];
            for (var i = 0; i < this.mDirectoryInfo.Length; i++)
            {
                var node = this.mDirectoryInfo[i];
                var file = new StreamFile();
                this.FileList[i] = file;
                file.Path = node.Path;
                file.FileName = Path.GetFileName(node.Path);
                if (node.Size >= int.MaxValue)
                {
                    /*var memoryMappedFile = MemoryMappedFile.CreateNew(file.fileName, entryinfo_size);
                    file.stream = memoryMappedFile.CreateViewStream();*/
                    var extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                    Directory.CreateDirectory(extractPath);
                    file.Stream = new FileStream(extractPath + file.FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                {
                    file.Stream = new MemoryStream((int)node.Size);
                }
                blocksStream.Position = node.Offset;
                blocksStream.CopyTo(file.Stream, node.Size);
                file.Stream.Position = 0;
            }
        }

        private void ReadHeader(EndianBinaryReader reader)
        {
            this.MHeader.Size = reader.ReadInt64();
            this.MHeader.CompressedBlocksInfoSize = reader.ReadUInt32();
            this.MHeader.UncompressedBlocksInfoSize = reader.ReadUInt32();
            this.MHeader.Flags = reader.ReadUInt32();
            if (this.MHeader.Signature != "UnityFS")
            {
                reader.ReadByte();
            }
        }

        private void ReadBlocksInfoAndDirectory(EndianBinaryReader reader)
        {
            byte[] blocksInfoBytes;
            if (this.MHeader.Version >= 7)
            {
                reader.AlignStream(16);
            }
            if ((this.MHeader.Flags & 0x80) != 0) //kArchiveBlocksInfoAtTheEnd
            {
                var position = reader.Position;
                reader.Position = reader.BaseStream.Length - this.MHeader.CompressedBlocksInfoSize;
                blocksInfoBytes = reader.ReadBytes((int)this.MHeader.CompressedBlocksInfoSize);
                reader.Position = position;
            }
            else //0x40 kArchiveBlocksAndDirectoryInfoCombined
            {
                blocksInfoBytes = reader.ReadBytes((int)this.MHeader.CompressedBlocksInfoSize);
            }
            var blocksInfoCompressedStream = new MemoryStream(blocksInfoBytes);
            MemoryStream blocksInfoUncompresseddStream;
            switch (this.MHeader.Flags & 0x3F) //kArchiveCompressionTypeMask
            {
                default: //None
                    {
                        blocksInfoUncompresseddStream = blocksInfoCompressedStream;
                        break;
                    }
                case 1: //LZMA
                    {
                        blocksInfoUncompresseddStream = new MemoryStream((int)(this.MHeader.UncompressedBlocksInfoSize));
                        SevenZipHelper.StreamDecompress(blocksInfoCompressedStream, blocksInfoUncompresseddStream, this.MHeader.CompressedBlocksInfoSize, this.MHeader.UncompressedBlocksInfoSize);
                        blocksInfoUncompresseddStream.Position = 0;
                        blocksInfoCompressedStream.Close();
                        break;
                    }
                case 2: //LZ4
                case 3: //LZ4HC
                    {
                        var uncompressedBytes = new byte[this.MHeader.UncompressedBlocksInfoSize];
                        using (var decoder = new Lz4DecoderStream(blocksInfoCompressedStream))
                        {
                            decoder.Read(uncompressedBytes, 0, uncompressedBytes.Length);
                        }
                        blocksInfoUncompresseddStream = new MemoryStream(uncompressedBytes);
                        break;
                    }
            }
            using (var blocksInfoReader = new EndianBinaryReader(blocksInfoUncompresseddStream))
            {
                var uncompressedDataHash = blocksInfoReader.ReadBytes(16);
                var blocksInfoCount = blocksInfoReader.ReadInt32();
                this.mBlocksInfo = new StorageBlock[blocksInfoCount];
                for (var i = 0; i < blocksInfoCount; i++)
                {
                    this.mBlocksInfo[i] = new StorageBlock
                    {
                        UncompressedSize = blocksInfoReader.ReadUInt32(),
                        CompressedSize = blocksInfoReader.ReadUInt32(),
                        Flags = blocksInfoReader.ReadUInt16()
                    };
                }

                var nodesCount = blocksInfoReader.ReadInt32();
                this.mDirectoryInfo = new Node[nodesCount];
                for (var i = 0; i < nodesCount; i++)
                {
                    this.mDirectoryInfo[i] = new Node
                    {
                        Offset = blocksInfoReader.ReadInt64(),
                        Size = blocksInfoReader.ReadInt64(),
                        Flags = blocksInfoReader.ReadUInt32(),
                        Path = blocksInfoReader.ReadStringToNull(),
                    };
                }
            }
        }

        private void ReadBlocks(EndianBinaryReader reader, Stream blocksStream)
        {
            foreach (var blockInfo in this.mBlocksInfo)
            {
                switch (blockInfo.Flags & 0x3F) //kStorageBlockCompressionTypeMask
                {
                    default: //None
                        {
                            reader.BaseStream.CopyTo(blocksStream, blockInfo.CompressedSize);
                            break;
                        }
                    case 1: //LZMA
                        {
                            SevenZipHelper.StreamDecompress(reader.BaseStream, blocksStream, blockInfo.CompressedSize, blockInfo.UncompressedSize);
                            break;
                        }
                    case 2: //LZ4
                    case 3: //LZ4HC
                        {
                            var compressedStream = new MemoryStream(reader.ReadBytes((int)blockInfo.CompressedSize));
                            using (var lz4Stream = new Lz4DecoderStream(compressedStream))
                            {
                                lz4Stream.CopyTo(blocksStream, blockInfo.UncompressedSize);
                            }
                            break;
                        }
                }
            }
            blocksStream.Position = 0;
        }
    }
}
