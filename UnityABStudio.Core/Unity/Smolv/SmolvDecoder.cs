namespace SoarCraft.QYun.UnityABStudio.Core.Unity.Smolv {
    using System;
    using System.IO;
    using System.Text;

    public static class SmolvDecoder {
        public static int GetDecodedBufferSize(byte[] data) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            if (!CheckSmolHeader(data)) {
                return 0;
            }

            var size = BitConverter.ToInt32(data, 5 * sizeof(uint));
            return size;
        }

        public static int GetDecodedBufferSize(Stream stream) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanSeek) {
                throw new ArgumentException(nameof(stream));
            }
            if (stream.Position + HeaderSize > stream.Length) {
                return 0;
            }

            var initPosition = stream.Position;
            stream.Position += HeaderSize - sizeof(uint);
            var size = stream.ReadByte() | stream.ReadByte() << 8 | stream.ReadByte() << 16 | stream.ReadByte() << 24;
            stream.Position = initPosition;
            return size;
        }

        public static byte[] Decode(byte[] data) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            var bufferSize = GetDecodedBufferSize(data);
            if (bufferSize == 0) {
                // invalid SMOL-V
                return null;
            }

            var output = new byte[bufferSize];
            return Decode(data, output) ? output : null;
        }

        public static bool Decode(byte[] data, byte[] output) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            if (output == null) {
                throw new ArgumentNullException(nameof(output));
            }

            var bufferSize = GetDecodedBufferSize(data);
            if (bufferSize > output.Length) {
                return false;
            }

            using (var outputStream = new MemoryStream(output)) {
                return Decode(data, outputStream);
            }
        }

        public static bool Decode(byte[] data, Stream outputStream) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }
            using (var inputStream = new MemoryStream(data)) {
                return Decode(inputStream, data.Length, outputStream);
            }
        }

        public static bool Decode(Stream inputStream, int inputSize, Stream outputStream) {
            if (inputStream == null) {
                throw new ArgumentNullException(nameof(inputStream));
            }
            if (outputStream == null) {
                throw new ArgumentNullException(nameof(outputStream));
            }
            if (inputStream.Length < HeaderSize) {
                return false;
            }

            using (var input = new BinaryReader(inputStream, Encoding.UTF8, true)) {
                using (var output = new BinaryWriter(outputStream, Encoding.UTF8, true)) {
                    var inputEndPosition = input.BaseStream.Position + inputSize;
                    var outputStartPosition = output.BaseStream.Position;

                    // Header
                    output.Write(SpirVHeaderMagic);
                    input.BaseStream.Position += sizeof(uint);
                    var version = input.ReadUInt32();
                    output.Write(version);
                    var generator = input.ReadUInt32();
                    output.Write(generator);
                    var bound = input.ReadInt32();
                    output.Write(bound);
                    var schema = input.ReadUInt32();
                    output.Write(schema);
                    var decodedSize = input.ReadInt32();

                    // Body
                    var prevResult = 0;
                    var prevDecorate = 0;
                    while (input.BaseStream.Position < inputEndPosition) {
                        // read length + opcode
                        if (!ReadLengthOp(input, out var instrLen, out var op)) {
                            return false;
                        }

                        var wasSwizzle = op == SpvOp.VectorShuffleCompact;
                        if (wasSwizzle) {
                            op = SpvOp.VectorShuffle;
                        }
                        output.Write((instrLen << 16) | (uint)op);

                        uint ioffs = 1;
                        // read type as varint, if we have it
                        if (op.OpHasType()) {
                            if (!ReadVarint(input, out var value)) {
                                return false;
                            }

                            output.Write(value);
                            ioffs++;
                        }

                        // read result as delta+varint, if we have it
                        if (op.OpHasResult()) {
                            if (!ReadVarint(input, out var value)) {
                                return false;
                            }

                            var zds = prevResult + ZigDecode(value);
                            output.Write(zds);
                            prevResult = zds;
                            ioffs++;
                        }

                        // Decorate: IDs relative to previous decorate
                        if (op is SpvOp.Decorate or SpvOp.MemberDecorate) {
                            if (!ReadVarint(input, out var value)) {
                                return false;
                            }

                            var zds = prevDecorate + unchecked((int)value);
                            output.Write(zds);
                            prevDecorate = zds;
                            ioffs++;
                        }

                        // Read this many IDs, that are relative to result ID
                        var relativeCount = op.OpDeltaFromResult();
                        var inverted = false;
                        if (relativeCount < 0) {
                            inverted = true;
                            relativeCount = -relativeCount;
                        }
                        for (var i = 0; i < relativeCount && ioffs < instrLen; ++i, ++ioffs) {
                            if (!ReadVarint(input, out var value)) {
                                return false;
                            }

                            var zd = inverted ? ZigDecode(value) : unchecked((int)value);
                            output.Write(prevResult - zd);
                        }

                        if (wasSwizzle && instrLen <= 9) {
                            uint swizzle = input.ReadByte();
                            if (instrLen > 5)
                                output.Write(swizzle >> 6);
                            if (instrLen > 6)
                                output.Write((swizzle >> 4) & 3);
                            if (instrLen > 7)
                                output.Write((swizzle >> 2) & 3);
                            if (instrLen > 8)
                                output.Write(swizzle & 3);
                        } else if (op.OpVarRest()) {
                            // read rest of words with variable encoding
                            for (; ioffs < instrLen; ++ioffs) {
                                if (!ReadVarint(input, out var value)) {
                                    return false;
                                }
                                output.Write(value);
                            }
                        } else {
                            // read rest of words without any encoding
                            for (; ioffs < instrLen; ++ioffs) {
                                if (input.BaseStream.Position + 4 > input.BaseStream.Length) {
                                    return false;
                                }
                                var val = input.ReadUInt32();
                                output.Write(val);
                            }
                        }
                    }

                    return output.BaseStream.Position == outputStartPosition + decodedSize;
                }
            }
        }

        private static bool CheckSmolHeader(byte[] data) => CheckGenericHeader(data, SmolHeaderMagic);

        private static bool CheckGenericHeader(byte[] data, uint expectedMagic) {
            if (data == null) {
                return false;
            }
            if (data.Length < HeaderSize) {
                return false;
            }

            var headerMagic = BitConverter.ToUInt32(data, 0 * sizeof(uint));
            if (headerMagic != expectedMagic) {
                return false;
            }

            var headerVersion = BitConverter.ToUInt32(data, 1 * sizeof(uint));
            return headerVersion is >= 0x00010000 and <= 0x00010300;
        }

        private static bool ReadVarint(BinaryReader input, out uint value) {
            uint v = 0;
            var shift = 0;
            while (input.BaseStream.Position < input.BaseStream.Length) {
                var b = input.ReadByte();
                v |= unchecked((uint)(b & 127) << shift);
                shift += 7;
                if ((b & 128) == 0) {
                    break;
                }
            }

            value = v;
            // @TODO: report failures
            return true;
        }

        private static bool ReadLengthOp(BinaryReader input, out uint len, out SpvOp op) {
            len = default;
            op = default;
            if (!ReadVarint(input, out var value)) {
                return false;
            }
            len = ((value >> 20) << 4) | ((value >> 4) & 0xF);
            op = (SpvOp)(((value >> 4) & 0xFFF0) | (value & 0xF));

            op = RemapOp(op);
            len = DecodeLen(op, len);
            return true;
        }

        /// <summary>
        /// Remap most common Op codes (Load, Store, Decorate, VectorShuffle etc.) to be in &lt; 16 range, for 
        /// more compact varint encoding. This basically swaps rarely used op values that are &lt; 16 with the
        /// ones that are common.
        /// </summary>
        private static SpvOp RemapOp(SpvOp op) => op switch {
            // 0: 24%
            SpvOp.Decorate => SpvOp.Nop,
            SpvOp.Nop => SpvOp.Decorate,
            // 1: 17%
            SpvOp.Load => SpvOp.Undef,
            SpvOp.Undef => SpvOp.Load,
            // 2: 9%
            SpvOp.Store => SpvOp.SourceContinued,
            SpvOp.SourceContinued => SpvOp.Store,
            // 3: 7.2%
            SpvOp.AccessChain => SpvOp.Source,
            SpvOp.Source => SpvOp.AccessChain,
            // 4: 5.0%
            // Name - already small enum value - 5: 4.4%
            // MemberName - already small enum value - 6: 2.9% 
            SpvOp.VectorShuffle => SpvOp.SourceExtension,
            SpvOp.SourceExtension => SpvOp.VectorShuffle,
            // 7: 4.0%
            SpvOp.MemberDecorate => SpvOp.String,
            SpvOp.String => SpvOp.MemberDecorate,
            // 8: 0.9%
            SpvOp.Label => SpvOp.Line,
            SpvOp.Line => SpvOp.Label,
            // 9: 3.9%
            SpvOp.Variable => (SpvOp)9,
            (SpvOp)9 => SpvOp.Variable,
            // 10: 3.9%
            SpvOp.FMul => SpvOp.Extension,
            SpvOp.Extension => SpvOp.FMul,
            // 11: 2.5%
            // ExtInst - already small enum value - 12: 1.2%
            // VectorShuffleCompact - already small enum value - used for compact shuffle encoding
            SpvOp.FAdd => SpvOp.ExtInstImport,
            SpvOp.ExtInstImport => SpvOp.FAdd,
            // 14: 2.2%
            SpvOp.TypePointer => SpvOp.MemoryModel,
            SpvOp.MemoryModel => SpvOp.TypePointer,
            // 15: 1.1%
            SpvOp.FNegate => SpvOp.EntryPoint,
            SpvOp.EntryPoint => SpvOp.FNegate,
            _ => op,
        };

        private static uint DecodeLen(SpvOp op, uint len) {
            len++;
            switch (op) {
                case SpvOp.VectorShuffle:
                    len += 4;
                    break;
                case SpvOp.VectorShuffleCompact:
                    len += 4;
                    break;
                case SpvOp.Decorate:
                    len += 2;
                    break;
                case SpvOp.Load:
                    len += 3;
                    break;
                case SpvOp.AccessChain:
                    len += 3;
                    break;
            }
            return len;
        }

        private static int DecorationExtraOps(int dec) {
            // RelaxedPrecision, Block..ColMajor
            if (dec is 0 or >= 2 and <= 5) {
                return 0;
            }
            // Stream..XfbStride
            if (dec is >= 29 and <= 37) {
                return 1;
            }

            // unknown, encode length
            return -1;
        }

        private static int ZigDecode(uint u) => (u & 1) != 0 ? unchecked((int)(~(u >> 1))) : unchecked((int)(u >> 1));

        public const uint SpirVHeaderMagic = 0x07230203;
        /// <summary>
        /// 'SMOL' ascii
        /// </summary>
        public const uint SmolHeaderMagic = 0x534D4F4C;

        private const int HeaderSize = 6 * sizeof(uint);
    }
}
