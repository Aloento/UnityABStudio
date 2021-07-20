namespace SoarCraft.QYun.UnityABStudio.Core.Helpers {
    using System.Collections.Generic;
    using System.IO;

    public static class StreamExtension {
        private static readonly Dictionary<MemoryStream, long> markPoses = new();

        public static MemoryStream Move(this MemoryStream stream, long offset) {
            stream.Position += offset;
            return stream;
        }

        public static MemoryStream Mark(this MemoryStream stream) {
            if (markPoses.ContainsKey(stream)) {
                markPoses[stream] = stream.Position;
            } else {
                markPoses.Add(stream, stream.Position);
            }
            return stream;
        }

        public static MemoryStream Back(this MemoryStream stream) {
            stream.Position = markPoses.TryGetValue(stream, out var mark) ? mark : 0;
            return stream;
        }

        public static void ClearMarkDic() => markPoses.Clear();
    }
}
