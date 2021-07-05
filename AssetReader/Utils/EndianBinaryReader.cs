namespace SoarCraft.QYun.AssetReader.Utils {
    using System;
    using System.IO;

    public abstract class EndianBinaryReader : BinaryReader {
        public bool IsBigEndian;

        protected EndianBinaryReader(Stream stream, bool isBigEndian = false) : base(stream) => this.IsBigEndian = isBigEndian;

        public long Position {
            get => this.BaseStream.Position;
            set => this.BaseStream.Position = value;
        }

        public override short ReadInt16() {
            if (!this.IsBigEndian) {
                return base.ReadInt16();
            }

            var buff = this.ReadBytes(2);
            Array.Reverse(buff);
            return BitConverter.ToInt16(buff, 0);
        }

        public override int ReadInt32() {
            if (!this.IsBigEndian) {
                return base.ReadInt32();
            }

            var buff = this.ReadBytes(4);
            Array.Reverse(buff);
            return BitConverter.ToInt32(buff, 0);
        }

        public override long ReadInt64() {
            if (!this.IsBigEndian) {
                return base.ReadInt64();
            }

            var buff = this.ReadBytes(8);
            Array.Reverse(buff);
            return BitConverter.ToInt64(buff, 0);
        }

        public override ushort ReadUInt16() {
            if (!this.IsBigEndian) {
                return base.ReadUInt16();
            }

            var buff = this.ReadBytes(2);
            Array.Reverse(buff);
            return BitConverter.ToUInt16(buff, 0);
        }

        public override uint ReadUInt32() {
            if (!this.IsBigEndian) {
                return base.ReadUInt32();
            }

            var buff = this.ReadBytes(4);
            Array.Reverse(buff);
            return BitConverter.ToUInt32(buff, 0);
        }

        public override ulong ReadUInt64() {
            if (!this.IsBigEndian) {
                return base.ReadUInt64();
            }

            var buff = this.ReadBytes(8);
            Array.Reverse(buff);
            return BitConverter.ToUInt64(buff, 0);
        }

        public override float ReadSingle() {
            if (!this.IsBigEndian) {
                return base.ReadSingle();
            }

            var buff = this.ReadBytes(4);
            Array.Reverse(buff);
            return BitConverter.ToSingle(buff, 0);
        }

        public override double ReadDouble() {
            if (!this.IsBigEndian) {
                return base.ReadDouble();
            }

            var buff = this.ReadBytes(8);
            Array.Reverse(buff);
            return BitConverter.ToUInt64(buff, 0);
        }
    }
}
