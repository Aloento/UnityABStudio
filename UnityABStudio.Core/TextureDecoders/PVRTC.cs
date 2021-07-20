namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System.Collections.Generic;

    public partial class TextureDecoder {
        private struct PVRTCTexelColor {
            public byte r;
            public byte g;
            public byte b;
            public byte a;
        }

        private struct PVRTCTexelColorInt {
            public int r;
            public int g;
            public int b;
            public int a;
        }

        private struct PVRTCTexelInfo {
            public PVRTCTexelColor a;
            public PVRTCTexelColor b;
            public List<byte> weight;
            public uint punch_through_flag;
        }
    }
}
