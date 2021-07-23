namespace SoarCraft.QYun.UnityABStudio.Helpers.ShaderConverters {
    using System.Text.RegularExpressions;
    using AssetReader.Utils;

    public class ShaderProgram {
        public ShaderSubProgram[] m_SubPrograms;

        public ShaderProgram(UnityReader reader, int[] version) {
            var subProgramsCapacity = reader.ReadInt32();
            m_SubPrograms = new ShaderSubProgram[subProgramsCapacity];
            int entrySize;
            if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) { //2019.3 and up
                entrySize = 12;
            } else {
                entrySize = 8;
            }
            for (var i = 0; i < subProgramsCapacity; i++) {
                reader.BaseStream.Position = 4 + (i * entrySize);
                var offset = reader.ReadInt32();
                reader.BaseStream.Position = offset;
                m_SubPrograms[i] = new ShaderSubProgram(reader);
            }
        }

        public string Export(string shader) {
            var evaluator = new MatchEvaluator(match => {
                var index = int.Parse(match.Groups[1].Value);
                return m_SubPrograms[index].Export();
            });
            shader = Regex.Replace(shader, "GpuProgramIndex (.+)", evaluator);
            return shader;
        }
    }
}
