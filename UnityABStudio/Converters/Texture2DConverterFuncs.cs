namespace SoarCraft.QYun.UnityABStudio.Converters {
    using System;
    using System.Linq;
    using AssetReader.Entities.Enums;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using TextureDecoder;
    using TextureDecoderNET;
    using Half = AssetReader.Math.Half;

    public partial class Texture2DConverter {
        private readonly TextureDecoderService decoder = Ioc.Default.GetRequiredService<TextureDecoderService>();

        private void SwapBytesForXbox() {
            if (this.platform == BuildTarget.XBOX360) {
                for (var i = 0; i < this.image_data_size / 2; i++) {
                    var b = this.image_data[i * 2];
                    this.image_data[i * 2] = this.image_data[(i * 2) + 1];
                    this.image_data[(i * 2) + 1] = b;
                }
            }
        }

        private byte[] DecodeAlpha8() {
            var buff = Enumerable.Repeat<byte>(0xFF, this.width * this.height * 4).ToArray();
            for (var i = 0; i < this.width * this.height; i++) {
                buff[(i * 4) + 3] = this.image_data[i];
            }

            return buff;
        }

        private byte[] DecodeARGB4444() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i++) {
                var pixelNew = new byte[4];
                var pixelOldShort = BitConverter.ToUInt16(this.image_data, i * 2);
                pixelNew[0] = (byte)(pixelOldShort & 0x000f);
                pixelNew[1] = (byte)((pixelOldShort & 0x00f0) >> 4);
                pixelNew[2] = (byte)((pixelOldShort & 0x0f00) >> 8);
                pixelNew[3] = (byte)((pixelOldShort & 0xf000) >> 12);
                for (var j = 0; j < 4; j++)
                    pixelNew[j] = (byte)((pixelNew[j] << 4) | pixelNew[j]);
                pixelNew.CopyTo(buff, i * 4);
            }

            return buff;
        }

        private byte[] DecodeRGB24() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i++) {
                buff[i * 4] = this.image_data[(i * 3) + 2];
                buff[(i * 4) + 1] = this.image_data[(i * 3) + 1];
                buff[(i * 4) + 2] = this.image_data[(i * 3) + 0];
                buff[(i * 4) + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeRGBA32() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = this.image_data[i + 2];
                buff[i + 1] = this.image_data[i + 1];
                buff[i + 2] = this.image_data[i + 0];
                buff[i + 3] = this.image_data[i + 3];
            }

            return buff;
        }

        private byte[] DecodeARGB32() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = this.image_data[i + 3];
                buff[i + 1] = this.image_data[i + 2];
                buff[i + 2] = this.image_data[i + 1];
                buff[i + 3] = this.image_data[i + 0];
            }

            return buff;
        }

        private byte[] DecodeRGB565() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i++) {
                var p = BitConverter.ToUInt16(this.image_data, i * 2);
                buff[i * 4] = (byte)((p << 3) | ((p >> 2) & 7));
                buff[(i * 4) + 1] = (byte)(((p >> 3) & 0xfc) | ((p >> 9) & 3));
                buff[(i * 4) + 2] = (byte)(((p >> 8) & 0xf8) | (p >> 13));
                buff[(i * 4) + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeR16() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i++) {
                buff[(i * 4) + 2] = this.image_data[(i * 2) + 1]; //r
                buff[(i * 4) + 3] = 255; //a
            }

            return buff;
        }

        private byte[] DecodeDXT1() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeDXT1(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeDXT5() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeDXT5(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeRGBA4444() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i++) {
                var pixelNew = new byte[4];
                var pixelOldShort = BitConverter.ToUInt16(this.image_data, i * 2);
                pixelNew[0] = (byte)((pixelOldShort & 0x00f0) >> 4);
                pixelNew[1] = (byte)((pixelOldShort & 0x0f00) >> 8);
                pixelNew[2] = (byte)((pixelOldShort & 0xf000) >> 12);
                pixelNew[3] = (byte)(pixelOldShort & 0x000f);
                for (var j = 0; j < 4; j++)
                    pixelNew[j] = (byte)((pixelNew[j] << 4) | pixelNew[j]);
                pixelNew.CopyTo(buff, i * 4);
            }

            return buff;
        }

        private byte[] DecodeBGRA32() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = this.image_data[i];
                buff[i + 1] = this.image_data[i + 1];
                buff[i + 2] = this.image_data[i + 2];
                buff[i + 3] = this.image_data[i + 3];
            }

            return buff;
        }

        private byte[] DecodeRHalf() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = 0;
                buff[i + 1] = 0;
                buff[i + 2] = (byte)Math.Round(Half.ToHalf(this.image_data, i / 2) * 255f);
                buff[i + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeRGHalf() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = 0;
                buff[i + 1] = (byte)Math.Round(Half.ToHalf(this.image_data, i + 2) * 255f);
                buff[i + 2] = (byte)Math.Round(Half.ToHalf(this.image_data, i) * 255f);
                buff[i + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeRGBAHalf() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = (byte)Math.Round(Half.ToHalf(this.image_data, (i * 2) + 4) * 255f);
                buff[i + 1] = (byte)Math.Round(Half.ToHalf(this.image_data, (i * 2) + 2) * 255f);
                buff[i + 2] = (byte)Math.Round(Half.ToHalf(this.image_data, i * 2) * 255f);
                buff[i + 3] = (byte)Math.Round(Half.ToHalf(this.image_data, (i * 2) + 6) * 255f);
            }

            return buff;
        }

        private byte[] DecodeRFloat() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = 0;
                buff[i + 1] = 0;
                buff[i + 2] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, i) * 255f);
                buff[i + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeRGFloat() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = 0;
                buff[i + 1] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, (i * 2) + 4) * 255f);
                buff[i + 2] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, i * 2) * 255f);
                buff[i + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeRGBAFloat() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                buff[i] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, (i * 4) + 8) * 255f);
                buff[i + 1] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, (i * 4) + 4) * 255f);
                buff[i + 2] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, i * 4) * 255f);
                buff[i + 3] = (byte)Math.Round(BitConverter.ToSingle(this.image_data, (i * 4) + 12) * 255f);
            }

            return buff;
        }

        private static byte ClampByte(int x) => (byte)(byte.MaxValue < x ? byte.MaxValue : (x > byte.MinValue ? x : byte.MinValue));

        private byte[] DecodeYUY2() {
            var buff = new byte[this.width * this.height * 4];
            var p = 0;
            var o = 0;
            var halfWidth = this.width / 2;
            for (var j = 0; j < this.height; j++) {
                for (var i = 0; i < halfWidth; ++i) {
                    int y0 = this.image_data[p++];
                    int u0 = this.image_data[p++];
                    int y1 = this.image_data[p++];
                    int v0 = this.image_data[p++];
                    var c = y0 - 16;
                    var d = u0 - 128;
                    var e = v0 - 128;
                    buff[o++] = ClampByte(((298 * c) + (516 * d) + 128) >> 8); // b
                    buff[o++] = ClampByte(((298 * c) - (100 * d) - (208 * e) + 128) >> 8); // g
                    buff[o++] = ClampByte(((298 * c) + (409 * e) + 128) >> 8); // r
                    buff[o++] = 255;
                    c = y1 - 16;
                    buff[o++] = ClampByte(((298 * c) + (516 * d) + 128) >> 8); // b
                    buff[o++] = ClampByte(((298 * c) - (100 * d) - (208 * e) + 128) >> 8); // g
                    buff[o++] = ClampByte(((298 * c) + (409 * e) + 128) >> 8); // r
                    buff[o++] = 255;
                }
            }

            return buff;
        }

        private byte[] DecodeRGB9e5Float() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < buff.Length; i += 4) {
                var n = BitConverter.ToInt32(this.image_data, i);
                var scale = (n >> 27) & 0x1f;
                var scalef = Math.Pow(2, scale - 24);
                var b = (n >> 18) & 0x1ff;
                var g = (n >> 9) & 0x1ff;
                var r = n & 0x1ff;
                buff[i] = (byte)Math.Round(b * scalef * 255f);
                buff[i + 1] = (byte)Math.Round(g * scalef * 255f);
                buff[i + 2] = (byte)Math.Round(r * scalef * 255f);
                buff[i + 3] = 255;
            }

            return buff;
        }

        private byte[] DecodeBC4() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeBC4(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeBC5() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeBC5(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeBC6H() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeBC6(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeBC7() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeBC7(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodePVRTC(bool is2bpp) {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodePVRTC(this.image_data, this.width, this.height, buff, is2bpp) ? buff : null;
        }

        private byte[] DecodeETC1() {
            _ = new ETC1Decoder(this.image_data, (ulong)this.width, (ulong)this.height, out var buff);
            return buff;
        }

        private byte[] DecodeATCRGB4() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeATCRGB4(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeATCRGBA8() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeATCRGBA8(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeEACR() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeEACR(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeEACRSigned() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeEACRSigned(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeEACRG() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeEACRG(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeEACRGSigned() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeEACRGSigned(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeETC2() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeETC2(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeETC2A1() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeETC2A1(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeETC2A8() {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeETC2A8(this.image_data, this.width, this.height, buff) ? buff : null;
        }

        private byte[] DecodeASTC(int blocksize) {
            var buff = new byte[this.width * this.height * 4];
            return this.decoder.DecodeASTC(this.image_data, this.width, this.height, buff, blocksize, blocksize) ? buff : null;
        }

        private byte[] DecodeRG16() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i += 2) {
                buff[(i * 2) + 1] = this.image_data[i + 1]; //G
                buff[(i * 2) + 2] = this.image_data[i]; //R
                buff[(i * 2) + 3] = 255; //A
            }

            return buff;
        }

        private byte[] DecodeR8() {
            var buff = new byte[this.width * this.height * 4];
            for (var i = 0; i < this.width * this.height; i++) {
                buff[(i * 4) + 2] = this.image_data[i]; //R
                buff[(i * 4) + 3] = 255; //A
            }

            return buff;
        }

        private bool UnpackCrunch() {
            byte[] result;
            if (this.version[0] > 2017 || (this.version[0] == 2017 && this.version[1] >= 3) //2017.3 and up
                                       || this.textureFormat is TextureFormat.ETC_RGB4Crunched or TextureFormat.ETC2_RGBA8Crunched) {
                result = this.decoder.UnpackUnityCrunch(this.image_data);
            } else {
                result = this.decoder.UnpackCrunch(this.image_data);
            }

            if (result != null) {
                this.image_data = result;
                this.image_data_size = result.Length;
                return true;
            }

            return false;
        }
    }
}
