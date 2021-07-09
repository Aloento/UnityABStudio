namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Meshes {
    using System;

    public static class MeshHelper {
        public enum VertexChannelFormat {
            kChannelFormatFloat,
            kChannelFormatFloat16,
            kChannelFormatColor,
            kChannelFormatByte,
            kChannelFormatUInt32
        }

        public enum VertexFormat2017 {
            kVertexFormatFloat,
            kVertexFormatFloat16,
            kVertexFormatColor,
            kVertexFormatUNorm8,
            kVertexFormatSNorm8,
            kVertexFormatUNorm16,
            kVertexFormatSNorm16,
            kVertexFormatUInt8,
            kVertexFormatSInt8,
            kVertexFormatUInt16,
            kVertexFormatSInt16,
            kVertexFormatUInt32,
            kVertexFormatSInt32
        }

        public enum VertexFormat {
            kVertexFormatFloat,
            kVertexFormatFloat16,
            kVertexFormatUNorm8,
            kVertexFormatSNorm8,
            kVertexFormatUNorm16,
            kVertexFormatSNorm16,
            kVertexFormatUInt8,
            kVertexFormatSInt8,
            kVertexFormatUInt16,
            kVertexFormatSInt16,
            kVertexFormatUInt32,
            kVertexFormatSInt32
        }

        public static VertexFormat ToVertexFormat(int format, int[] version) => version[0] switch {
            < 2017 => (VertexChannelFormat)format switch {
                VertexChannelFormat.kChannelFormatFloat => VertexFormat.kVertexFormatFloat,
                VertexChannelFormat.kChannelFormatFloat16 => VertexFormat.kVertexFormatFloat16,
                VertexChannelFormat.kChannelFormatColor => //in 4.x is size 4
                    VertexFormat.kVertexFormatUNorm8,
                VertexChannelFormat.kChannelFormatByte => VertexFormat.kVertexFormatUInt8,
                VertexChannelFormat.kChannelFormatUInt32 => //in 5.x
                    VertexFormat.kVertexFormatUInt32,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            },
            < 2019 => (VertexFormat2017)format switch {
                VertexFormat2017.kVertexFormatFloat => VertexFormat.kVertexFormatFloat,
                VertexFormat2017.kVertexFormatFloat16 => VertexFormat.kVertexFormatFloat16,
                VertexFormat2017.kVertexFormatColor or VertexFormat2017.kVertexFormatUNorm8 => VertexFormat.kVertexFormatUNorm8,
                VertexFormat2017.kVertexFormatSNorm8 => VertexFormat.kVertexFormatSNorm8,
                VertexFormat2017.kVertexFormatUNorm16 => VertexFormat.kVertexFormatUNorm16,
                VertexFormat2017.kVertexFormatSNorm16 => VertexFormat.kVertexFormatSNorm16,
                VertexFormat2017.kVertexFormatUInt8 => VertexFormat.kVertexFormatUInt8,
                VertexFormat2017.kVertexFormatSInt8 => VertexFormat.kVertexFormatSInt8,
                VertexFormat2017.kVertexFormatUInt16 => VertexFormat.kVertexFormatUInt16,
                VertexFormat2017.kVertexFormatSInt16 => VertexFormat.kVertexFormatSInt16,
                VertexFormat2017.kVertexFormatUInt32 => VertexFormat.kVertexFormatUInt32,
                VertexFormat2017.kVertexFormatSInt32 => VertexFormat.kVertexFormatSInt32,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            },
            _ => (VertexFormat)format,
        };


        public static uint GetFormatSize(VertexFormat format) => format switch {
            VertexFormat.kVertexFormatFloat or VertexFormat.kVertexFormatUInt32 or VertexFormat.kVertexFormatSInt32 => 4u,
            VertexFormat.kVertexFormatFloat16 or VertexFormat.kVertexFormatUNorm16 or VertexFormat.kVertexFormatSNorm16 or VertexFormat.kVertexFormatUInt16 or VertexFormat.kVertexFormatSInt16 => 2u,
            VertexFormat.kVertexFormatUNorm8 or VertexFormat.kVertexFormatSNorm8 or VertexFormat.kVertexFormatUInt8 or VertexFormat.kVertexFormatSInt8 => 1u,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        public static bool IsIntFormat(VertexFormat format) => format >= VertexFormat.kVertexFormatUInt8;

        public static float[] BytesToFloatArray(byte[] inputBytes, VertexFormat format) {
            var size = GetFormatSize(format);
            var len = inputBytes.Length / size;
            var result = new float[len];
            for (var i = 0; i < len; i++) {
                result[i] = format switch {
                    VertexFormat.kVertexFormatFloat => BitConverter.ToSingle(inputBytes, i * 4),
                    VertexFormat.kVertexFormatFloat16 => Half.ToHalf(inputBytes, i * 2),
                    VertexFormat.kVertexFormatUNorm8 => inputBytes[i] / 255f,
                    VertexFormat.kVertexFormatSNorm8 => Math.Max((sbyte)inputBytes[i] / 127f, -1f),
                    VertexFormat.kVertexFormatUNorm16 => BitConverter.ToUInt16(inputBytes, i * 2) / 65535f,
                    VertexFormat.kVertexFormatSNorm16 => Math.Max(
                        BitConverter.ToInt16(inputBytes, i * 2) / 32767f, -1f),
                    _ => result[i]
                };
            }
            return result;
        }

        public static int[] BytesToIntArray(byte[] inputBytes, VertexFormat format) {
            var size = GetFormatSize(format);
            var len = inputBytes.Length / size;
            var result = new int[len];
            for (var i = 0; i < len; i++) {
                switch (format) {
                    case VertexFormat.kVertexFormatUInt8:
                    case VertexFormat.kVertexFormatSInt8:
                        result[i] = inputBytes[i];
                        break;
                    case VertexFormat.kVertexFormatUInt16:
                    case VertexFormat.kVertexFormatSInt16:
                        result[i] = BitConverter.ToInt16(inputBytes, i * 2);
                        break;
                    case VertexFormat.kVertexFormatUInt32:
                    case VertexFormat.kVertexFormatSInt32:
                        result[i] = BitConverter.ToInt32(inputBytes, i * 4);
                        break;
                }
            }
            return result;
        }
    }
}
