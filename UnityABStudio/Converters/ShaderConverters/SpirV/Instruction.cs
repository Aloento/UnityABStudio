namespace SoarCraft.QYun.UnityABStudio.Converters.ShaderConverters.SpirV {
    using System.Collections.Generic;

    public enum OperandQuantifier {
        /// <summary>
        /// 1
        /// </summary>
        Default,
        /// <summary>
        /// 0 or 1
        /// </summary>
        Optional,
        /// <summary>
        /// 0+
        /// </summary>
        Varying
    }

    public class Operand {
        public Operand(OperandType kind, string name, OperandQuantifier quantifier) {
            this.Name = name;
            this.Type = kind;
            this.Quantifier = quantifier;
        }

        public string Name { get; }
        public OperandType Type { get; }
        public OperandQuantifier Quantifier { get; }
    }

    public class Instruction {
        public Instruction(string name)
            : this(name, new List<Operand>()) {
        }

        public Instruction(string name, IReadOnlyList<Operand> operands) {
            this.Operands = operands;
            this.Name = name;
        }

        public string Name { get; }
        public IReadOnlyList<Operand> Operands { get; }
    }
}
