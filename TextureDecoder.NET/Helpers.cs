namespace SoarCraft.QYun.TextureDecoderNET {
    internal static class Helpers {
        internal static unsafe void CopyBlockBuffer(ulong width, ulong height, ulong x, ulong y, uint* block, byte* pImage) {
            var xl = (4 * (x + 1) > width ? width - (4 * x) : 4) * 4;
            var buffer_end = block + (4 * 4);
            for (var y1 = y * 4; block < buffer_end && y1 < height; block += 4, y1++) {
                for (ulong i = 0; i < xl / sizeof(uint); i++) {
                    ((uint*)pImage + (y1 * width) + (4 * x))[i] = block[i];
                }
            }
        }

        internal static uint ApplicateColor(byte[,] c, int m, int d) =>
            Color(Clamp(c[d, 0] + m), Clamp(c[d, 1] + m), Clamp(c[d, 2] + m), 255);

        internal static uint Color(byte r, byte g, byte b, byte a) => (uint)(b | (g << 8) | (r << 16) | (a << 24));

        internal static byte Clamp(int n) => (byte)(n < 0 ? 0 : n > 255 ? 255 : n);
    }
}
