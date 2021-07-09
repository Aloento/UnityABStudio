namespace SoarCraft.QYun.AssetReader.Unity3D.Objects.Sprites {
    using Entities.Enums;
    using Utils;

    public class SpriteSettings {
        public uint settingsRaw;

        public uint packed;
        public SpritePackingMode packingMode;
        public SpritePackingRotation packingRotation;
        public SpriteMeshType meshType;

        public SpriteSettings(UnityReader reader) {
            settingsRaw = reader.ReadUInt32();

            packed = settingsRaw & 1; //1
            packingMode = (SpritePackingMode)((settingsRaw >> 1) & 1); //1
            packingRotation = (SpritePackingRotation)((settingsRaw >> 2) & 0xf); //4
            meshType = (SpriteMeshType)((settingsRaw >> 6) & 1); //1
            //reserved
        }
    }
}
