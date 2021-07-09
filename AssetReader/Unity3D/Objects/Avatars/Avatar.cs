namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Avatars {
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Utils;

    public class Avatar : NamedObject {
        public uint m_AvatarSize;
        public AvatarConstant m_Avatar;
        public KeyValuePair<uint, string>[] m_TOS;

        public Avatar(ObjectReader reader) : base(reader) {
            m_AvatarSize = reader.ReadUInt32();
            m_Avatar = new AvatarConstant(reader);

            var numTOS = reader.ReadInt32();
            m_TOS = new KeyValuePair<uint, string>[numTOS];
            for (var i = 0; i < numTOS; i++) {
                m_TOS[i] = new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            //HumanDescription m_HumanDescription 2019 and up
        }

        public string FindBonePath(uint hash) => m_TOS.FirstOrDefault(pair => pair.Key == hash).Value;
    }
}
