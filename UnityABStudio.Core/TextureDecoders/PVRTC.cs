namespace SoarCraft.QYun.UnityABStudio.Core.TextureDecoders {
    using System.Collections.Generic;

    public partial class TextureDecoder {
        private struct PVRTCTexelColor {
            byte r;
            byte g;
            byte b;
            byte a;
        }

        private struct PVRTCTexelColorInt {
            int r;
            int g;
            int b;
            int a;
        }

        private struct PVRTCTexelInfo {
            PVRTCTexelColor a;
            PVRTCTexelColor b;
            List<byte> weight;
            uint punch_through_flag;
        }
    }
}
