namespace SoarCraft.QYun.UnityABStudio.Converters.ShaderConverters.SpirV {
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class Reader {
        public Reader(BinaryReader reader) {
            this.reader_ = reader;
            uint magicNumber = this.reader_.ReadUInt32();
            if (magicNumber == Meta.MagicNumber) {
                this.littleEndian_ = true;
            } else if (Reverse(magicNumber) == Meta.MagicNumber) {
                this.littleEndian_ = false;
            } else {
                throw new Exception("Invalid magic number");
            }
        }

        public uint ReadDWord() {
            if (this.littleEndian_) {
                return this.reader_.ReadUInt32();
            } else {
                return Reverse(this.reader_.ReadUInt32());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Reverse(uint u) {
            return (u << 24) | (u & 0xFF00U) << 8 | (u >> 8) & 0xFF00U | (u >> 24);
        }

        public bool EndOfStream => this.reader_.BaseStream.Position == this.reader_.BaseStream.Length;

        private readonly BinaryReader reader_;
        private readonly bool littleEndian_;
    }
}
