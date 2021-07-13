namespace SoarCraft.QYun.AssetReader.Helpers {
    using System.IO;
    using _7zip.Compress.LZMA;

    public static class SevenZipHelper {
        public static MemoryStream StreamDecompress(MemoryStream inStream) {
            var decoder = new Decoder();

            _ = inStream.Seek(0, SeekOrigin.Begin);
            var newOutStream = new MemoryStream();

            var properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new IOException("input .lzma is too short");
            long outSize = 0;
            for (var i = 0; i < 8; i++) {
                var v = inStream.ReadByte();
                if (v < 0)
                    throw new IOException("Can't Read 1");
                outSize |= (long)(byte)v << (8 * i);
            }
            decoder.SetDecoderProperties(properties);

            var compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, newOutStream, compressedSize, outSize, null);

            newOutStream.Position = 0;
            return newOutStream;
        }

        public static void StreamDecompress(Stream compressedStream, Stream decompressedStream, long compressedSize, long decompressedSize) {
            var basePosition = compressedStream.Position;
            var decoder = new Decoder();
            var properties = new byte[5];
            if (compressedStream.Read(properties, 0, 5) != 5)
                throw new IOException("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            decoder.Code(compressedStream, decompressedStream, compressedSize - 5, decompressedSize, null);
            compressedStream.Position = basePosition + compressedSize;
        }
    }
}
