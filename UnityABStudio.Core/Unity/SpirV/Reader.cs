namespace SoarCraft.QYun.UnityABStudio.Core.Unity.SpirV {
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class Reader {
        public Reader(BinaryReader reader) {
            this.reader_ = reader;
            var magicNumber = this.reader_.ReadUInt32();
            if (magicNumber == Meta.MagicNumber) {
                this.littleEndian_ = true;
            } else if (Reverse(magicNumber) == Meta.MagicNumber) {
                this.littleEndian_ = false;
            } else {
                throw new Exception("Invalid magic number");
            }
        }

        public uint ReadDWord() => this.littleEndian_ ? this.reader_.ReadUInt32() : Reverse(this.reader_.ReadUInt32());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Reverse(uint u) => (u << 24) | (u & 0xFF00U) << 8 | (u >> 8) & 0xFF00U | (u >> 24);

        public bool EndOfStream => this.reader_.BaseStream.Position == this.reader_.BaseStream.Length;

        private readonly BinaryReader reader_;
        private readonly bool littleEndian_;
    }
}
