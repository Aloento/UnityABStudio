namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System;

    public partial class TextureDecoder {
        private readonly bool IsLittleEndian = BitConverter.IsLittleEndian;

        public TextureDecoder() => this.TRANSPARENT_MASK = this.IsLittleEndian ? 0x00ffffff : 0xffffff00;
    }
}
