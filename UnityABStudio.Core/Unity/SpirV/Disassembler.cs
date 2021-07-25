namespace SoarCraft.QYun.UnityABStudio.Core.Unity.SpirV {
    using System;
    using System.Collections.Generic;
    using System.Text;

    public struct ModuleHeader {
        public Version Version { get; set; }
        public string GeneratorVendor { get; set; }
        public string GeneratorName { get; set; }
        public int GeneratorVersion { get; set; }
        public uint Bound { get; set; }
        public uint Reserved { get; set; }
    }

    [Flags]
    public enum DisassemblyOptions {
        None,
        ShowTypes,
        ShowNames,
        Default = ShowTypes | ShowNames
    }

    public class Disassembler {
        public string Disassemble(Module module) => this.Disassemble(module, DisassemblyOptions.Default);

        public string Disassemble(Module module, DisassemblyOptions options) {
            _ = this.m_sb.AppendLine("; SPIR-V");
            _ = this.m_sb.Append("; Version: ").Append(module.Header.Version).AppendLine();
            if (module.Header.GeneratorName == null) {
                _ = this.m_sb.Append("; Generator: unknown; ").Append(module.Header.GeneratorVersion).AppendLine();
            } else {
                _ = this.m_sb.Append("; Generator: ").Append(module.Header.GeneratorVendor).Append(' ').
                    Append(module.Header.GeneratorName).Append("; ").Append(module.Header.GeneratorVersion).AppendLine();
            }
            _ = this.m_sb.Append("; Bound: ").Append(module.Header.Bound).AppendLine();
            _ = this.m_sb.Append("; Schema: ").Append(module.Header.Reserved).AppendLine();

            var lines = new string[module.Instructions.Count + 1];
            lines[0] = this.m_sb.ToString();
            _ = this.m_sb.Clear();

            for (var i = 0; i < module.Instructions.Count; i++) {
                var instruction = module.Instructions[i];
                PrintInstruction(this.m_sb, instruction, options);
                lines[i + 1] = this.m_sb.ToString();
                _ = this.m_sb.Clear();
            }

            var longestPrefix = 0;
            for (var i = 0; i < lines.Length; i++) {
                var line = lines[i];
                longestPrefix = Math.Max(longestPrefix, line.IndexOf('='));
                if (longestPrefix > 50) {
                    longestPrefix = 50;
                    break;
                }
            }

            _ = this.m_sb.Append(lines[0]);
            for (var i = 1; i < lines.Length; i++) {
                var line = lines[i];
                var index = line.IndexOf('=');
                if (index == -1) {
                    _ = this.m_sb.Append(' ', longestPrefix + 4);
                    _ = this.m_sb.Append(line);
                } else {
                    var pad = Math.Max(0, longestPrefix - index);
                    _ = this.m_sb.Append(' ', pad);
                    _ = this.m_sb.Append(line, 0, index);
                    _ = this.m_sb.Append('=');
                    _ = this.m_sb.Append(line, index + 1, line.Length - index - 1);
                }
                _ = this.m_sb.AppendLine();
            }

            var result = this.m_sb.ToString();
            _ = this.m_sb.Clear();
            return result;
        }

        private static void PrintInstruction(StringBuilder sb, ParsedInstruction instruction, DisassemblyOptions options) {
            if (instruction.Operands.Count == 0) {
                _ = sb.Append(instruction.Instruction.Name);
                return;
            }

            var currentOperand = 0;
            if (instruction.Instruction.Operands[currentOperand].Type is IdResultType) {
                if (options.HasFlag(DisassemblyOptions.ShowTypes)) {
                    _ = instruction.ResultType.ToString(sb).Append(' ');
                }
                ++currentOperand;
            }

            if (currentOperand < instruction.Operands.Count && instruction.Instruction.Operands[currentOperand].Type is IdResult) {
                if (!options.HasFlag(DisassemblyOptions.ShowNames) || string.IsNullOrWhiteSpace(instruction.Name)) {
                    PrintOperandValue(sb, instruction.Operands[currentOperand].Value, options);
                } else {
                    _ = sb.Append(instruction.Name);
                }
                _ = sb.Append(" = ");

                ++currentOperand;
            }

            _ = sb.Append(instruction.Instruction.Name);
            _ = sb.Append(' ');

            for (; currentOperand < instruction.Operands.Count; ++currentOperand) {
                PrintOperandValue(sb, instruction.Operands[currentOperand].Value, options);
                _ = sb.Append(' ');
            }
        }

        private static void PrintOperandValue(StringBuilder sb, object value, DisassemblyOptions options) {
            switch (value) {
                case System.Type t:
                    _ = sb.Append(t.Name);
                    break;

                case string s: {
                    _ = sb.Append('"');
                    _ = sb.Append(s);
                    _ = sb.Append('"');
                }
                break;

                case ObjectReference or: {
                    if (options.HasFlag(DisassemblyOptions.ShowNames) && or.Reference != null && !string.IsNullOrWhiteSpace(or.Reference.Name)) {
                        _ = sb.Append(or.Reference.Name);
                    } else {
                        _ = or.ToString(sb);
                    }
                }
                break;

                case IBitEnumOperandValue beov:
                    PrintBitEnumValue(sb, beov, options);
                    break;

                case IValueEnumOperandValue veov:
                    PrintValueEnumValue(sb, veov, options);
                    break;

                case VaryingOperandValue varOpVal:
                    _ = varOpVal.ToString(sb);
                    break;

                default:
                    _ = sb.Append(value);
                    break;
            }
        }

        private static void PrintBitEnumValue(StringBuilder sb, IBitEnumOperandValue enumOperandValue, DisassemblyOptions options) {
            foreach (var key in enumOperandValue.Values.Keys) {
                _ = sb.Append(enumOperandValue.EnumerationType.GetEnumName(key));
                var value = enumOperandValue.Values[key];
                if (value.Count != 0) {
                    _ = sb.Append(' ');
                    foreach (var v in value) {
                        PrintOperandValue(sb, v, options);
                    }
                }
            }
        }

        private static void PrintValueEnumValue(StringBuilder sb, IValueEnumOperandValue valueOperandValue, DisassemblyOptions options) {
            _ = sb.Append(valueOperandValue.Key);
            if (valueOperandValue.Value is IList<object> valueList && valueList.Count > 0) {
                _ = sb.Append(' ');
                foreach (var v in valueList) {
                    PrintOperandValue(sb, v, options);
                }
            }
        }

        private readonly StringBuilder m_sb = new();
    }
}
