namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    public partial class TextureDecoder {
        private readonly uint TRANSPARENT_MASK;

        private uint Color(byte r, byte g, byte b, byte a) {
            if (this.IsLittleEndian) {
                return (uint)(b | (g << 8) | (r << 16) | (a << 24));
            }

            return (uint)(a | (r << 8) | (g << 16) | (b << 24));
        }

        private void RGB565LE(in ushort d, out byte r, out byte g, out byte b) {
            if (this.IsLittleEndian) {
                r = (byte)(((d >> 8) & 0xf8) | (d >> 13));
                g = (byte)(((d >> 3) & 0xfc) | ((d >> 9) & 3));
                b = (byte)((d << 3) | ((d >> 2) & 7));
            } else {
                r = (byte)((d & 0xf8) | ((d >> 5) & 7));
                g = (byte)(((d << 5) & 0xe0) | ((d >> 11) & 0x1c) | ((d >> 1) & 3));
                b = (byte)(((d >> 5) & 0xf8) | ((d >> 10) & 0x7));
            }
        }
    }
}
