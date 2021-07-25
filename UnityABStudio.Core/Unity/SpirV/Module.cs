namespace SoarCraft.QYun.UnityABStudio.Core.Unity.SpirV {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    public class Module {
        [StructLayout(LayoutKind.Explicit)]
        private struct FloatUIntUnion {
            [FieldOffset(0)]
            public uint Int;
            [FieldOffset(0)]
            public float Float;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleULongUnion {
            [FieldOffset(0)]
            public ulong Long;
            [FieldOffset(0)]
            public double Double;
        }

        public Module(ModuleHeader header, IReadOnlyList<ParsedInstruction> instructions) {
            this.Header = header;
            this.Instructions = instructions;

            Read(this.Instructions, this.objects_);
        }

        public static bool IsDebugInstruction(ParsedInstruction instruction) => debugInstructions_.Contains(instruction.Instruction.Name);

        private static void Read(IReadOnlyList<ParsedInstruction> instructions, Dictionary<uint, ParsedInstruction> objects) {
            // Debug instructions can be only processed after everything
            // else has been parsed, as they may reference types which haven't
            // been seen in the file yet
            var debugInstructions = new List<ParsedInstruction>();
            // Entry points contain forward references
            // Those need to be resolved afterwards
            var entryPoints = new List<ParsedInstruction>();

            foreach (var instruction in instructions) {
                if (IsDebugInstruction(instruction)) {
                    debugInstructions.Add(instruction);
                    continue;
                }
                if (instruction.Instruction is OpEntryPoint) {
                    entryPoints.Add(instruction);
                    continue;
                }

                if (instruction.Instruction.Name.StartsWith("OpType", StringComparison.Ordinal)) {
                    ProcessTypeInstruction(instruction, objects);
                }

                instruction.ResolveResultType(objects);
                if (instruction.HasResult) {
                    objects[instruction.ResultId] = instruction;
                }

                switch (instruction.Instruction) {
                    // Constants require that the result type has been resolved
                    case OpSpecConstant sc:
                    case OpConstant oc: {
                        var t = instruction.ResultType;
                        Debug.Assert(t != null);
                        Debug.Assert(t is ScalarType);

                        var constant = ConvertConstant(instruction.ResultType as ScalarType, instruction.Words, 3);
                        instruction.Operands[2].Value = constant;
                        instruction.Value = constant;
                    }
                    break;
                }
            }

            foreach (var instruction in debugInstructions) {
                switch (instruction.Instruction) {
                    case OpMemberName mn: {
                        var t = (StructType)objects[instruction.Words[1]].ResultType;
                        t.SetMemberName((uint)instruction.Operands[1].Value, (string)instruction.Operands[2].Value);
                    }
                    break;

                    case OpName n: {
                        // We skip naming objects we don't know about
                        var t = objects[instruction.Words[1]];
                        t.Name = (string)instruction.Operands[1].Value;
                    }
                    break;
                }
            }

            foreach (var instruction in instructions) {
                instruction.ResolveReferences(objects);
            }
        }

        public static Module ReadFrom(Stream stream) {
            var br = new BinaryReader(stream);
            var reader = new Reader(br);

            var versionNumber = reader.ReadDWord();
            var majorVersion = (int)(versionNumber >> 16);
            var minorVersion = (int)((versionNumber >> 8) & 0xFF);
            var version = new Version(majorVersion, minorVersion);

            var generatorMagicNumber = reader.ReadDWord();
            var generatorToolId = (int)(generatorMagicNumber >> 16);
            var generatorVendor = "unknown";
            string generatorName = null;

            if (Meta.Tools.ContainsKey(generatorToolId)) {
                var toolInfo = Meta.Tools[generatorToolId];
                generatorVendor = toolInfo.Vendor;
                if (toolInfo.Name != null) {
                    generatorName = toolInfo.Name;
                }
            }

            // Read header
            var header = new ModuleHeader {
                Version = version,
                GeneratorName = generatorName,
                GeneratorVendor = generatorVendor,
                GeneratorVersion = (int)(generatorMagicNumber & 0xFFFF),
                Bound = reader.ReadDWord(),
                Reserved = reader.ReadDWord()
            };

            var instructions = new List<ParsedInstruction>();
            while (!reader.EndOfStream) {
                var instructionStart = reader.ReadDWord();
                var wordCount = (ushort)(instructionStart >> 16);
                var opCode = (int)(instructionStart & 0xFFFF);

                var words = new uint[wordCount];
                words[0] = instructionStart;
                for (ushort i = 1; i < wordCount; ++i) {
                    words[i] = reader.ReadDWord();
                }

                var instruction = new ParsedInstruction(opCode, words);
                instructions.Add(instruction);
            }

            return new Module(header, instructions);
        }

        /// <summary>
        /// Collect types from OpType* instructions
        /// </summary>
        private static void ProcessTypeInstruction(ParsedInstruction i, IReadOnlyDictionary<uint, ParsedInstruction> objects) {
            switch (i.Instruction) {
                case OpTypeInt t: {
                    i.ResultType = new IntegerType((int)i.Words[2], i.Words[3] == 1u);
                }
                break;

                case OpTypeFloat t: {
                    i.ResultType = new FloatingPointType((int)i.Words[2]);
                }
                break;

                case OpTypeVector t: {
                    i.ResultType = new VectorType((ScalarType)objects[i.Words[2]].ResultType, (int)i.Words[3]);
                }
                break;

                case OpTypeMatrix t: {
                    i.ResultType = new MatrixType((VectorType)objects[i.Words[2]].ResultType, (int)i.Words[3]);
                }
                break;

                case OpTypeArray t: {
                    var constant = objects[i.Words[3]].Value;
                    var size = 0;

                    switch (constant) {
                        case ushort u16:
                            size = u16;
                            break;

                        case uint u32:
                            size = (int)u32;
                            break;

                        case ulong u64:
                            size = (int)u64;
                            break;

                        case short i16:
                            size = i16;
                            break;

                        case int i32:
                            size = i32;
                            break;

                        case long i64:
                            size = (int)i64;
                            break;
                    }

                    i.ResultType = new ArrayType(objects[i.Words[2]].ResultType, size);
                }
                break;

                case OpTypeRuntimeArray t: {
                    i.ResultType = new RuntimeArrayType(objects[i.Words[2]].ResultType);
                }
                break;

                case OpTypeBool t: {
                    i.ResultType = new BoolType();
                }
                break;

                case OpTypeOpaque t: {
                    i.ResultType = new OpaqueType();
                }
                break;

                case OpTypeVoid t: {
                    i.ResultType = new VoidType();
                }
                break;

                case OpTypeImage t: {
                    var sampledType = objects[i.Operands[1].GetId()].ResultType;
                    var dim = i.Operands[2].GetSingleEnumValue<Dim>();
                    var depth = (uint)i.Operands[3].Value;
                    var isArray = (uint)i.Operands[4].Value != 0;
                    var isMultiSampled = (uint)i.Operands[5].Value != 0;
                    var sampled = (uint)i.Operands[6].Value;
                    var imageFormat = i.Operands[7].GetSingleEnumValue<ImageFormat>();

                    i.ResultType = new ImageType(sampledType,
                        dim,
                        (int)depth, isArray, isMultiSampled,
                        (int)sampled, imageFormat,
                        i.Operands.Count > 8 ? i.Operands[8].GetSingleEnumValue<AccessQualifier>() : AccessQualifier.ReadOnly);
                }
                break;

                case OpTypeSampler st: {
                    i.ResultType = new SamplerType();
                    break;
                }

                case OpTypeSampledImage t: {
                    i.ResultType = new SampledImageType((ImageType)objects[i.Words[2]].ResultType);
                }
                break;

                case OpTypeFunction t: {
                    var parameterTypes = new List<Type>();
                    for (var j = 3; j < i.Words.Count; ++j) {
                        parameterTypes.Add(objects[i.Words[j]].ResultType);
                    }
                    i.ResultType = new FunctionType(objects[i.Words[2]].ResultType, parameterTypes);
                }
                break;

                case OpTypeForwardPointer t: {
                    // We create a normal pointer, but with unspecified type
                    // This will get resolved later on
                    i.ResultType = new PointerType((StorageClass)i.Words[2]);
                }
                break;

                case OpTypePointer t: {
                    if (objects.ContainsKey(i.Words[1])) {
                        // If there is something present, it must have been
                        // a forward reference. The storage type must
                        // match
                        var pt = (PointerType)i.ResultType;
                        Debug.Assert(pt != null);
                        Debug.Assert(pt.StorageClass == (StorageClass)i.Words[2]);
                        pt.ResolveForwardReference(objects[i.Words[3]].ResultType);
                    } else {
                        i.ResultType = new PointerType((StorageClass)i.Words[2], objects[i.Words[3]].ResultType);
                    }
                }
                break;

                case OpTypeStruct t: {
                    var memberTypes = new List<Type>();
                    for (var j = 2; j < i.Words.Count; ++j) {
                        memberTypes.Add(objects[i.Words[j]].ResultType);
                    }
                    i.ResultType = new StructType(memberTypes);
                }
                break;
            }
        }

        private static object ConvertConstant(ScalarType type, IReadOnlyList<uint> words, int index) {
            switch (type) {
                case IntegerType i: {
                    if (i.Signed) {
                        if (i.Width == 16) {
                            return unchecked((short)(words[index]));
                        }

                        if (i.Width == 32) {
                            return unchecked((int)(words[index]));
                        }
                        if (i.Width == 64) {
                            return unchecked((long)(words[index] | (ulong)(words[index + 1]) << 32));
                        }
                    } else {
                        if (i.Width == 16) {
                            return unchecked((ushort)(words[index]));
                        }

                        if (i.Width == 32) {
                            return words[index];
                        }
                        if (i.Width == 64) {
                            return words[index] | (ulong)(words[index + 1]) << 32;
                        }
                    }

                    throw new Exception("Cannot construct integer literal.");
                }

                case FloatingPointType f: {
                    if (f.Width == 32) {
                        return new FloatUIntUnion { Int = words[0] }.Float;
                    }

                    if (f.Width == 64) {
                        return new DoubleULongUnion { Long = (words[index] | (ulong)(words[index + 1]) << 32) }.Double;
                    }
                    throw new Exception("Cannot construct floating point literal.");
                }
            }

            return null;
        }

        public ModuleHeader Header { get; }
        public IReadOnlyList<ParsedInstruction> Instructions { get; }

        private static HashSet<string> debugInstructions_ = new() {
            "OpSourceContinued",
            "OpSource",
            "OpSourceExtension",
            "OpName",
            "OpMemberName",
            "OpString",
            "OpLine",
            "OpNoLine",
            "OpModuleProcessed"
        };

        private readonly Dictionary<uint, ParsedInstruction> objects_ = new();
    }
}
