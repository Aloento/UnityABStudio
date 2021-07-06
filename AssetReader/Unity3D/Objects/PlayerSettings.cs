namespace SoarCraft.QYun.AssetReader.Unity3D.Objects {
    using Utils;

    public sealed class PlayerSettings : UObject {
        public string companyName;
        public string productName;

        public PlayerSettings(ObjectReader reader) : base(reader) {
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4.0 nad up
            {
                var productGUID = reader.ReadBytes(16);
            }

            var AndroidProfiler = reader.ReadBoolean();
            //bool AndroidFilterTouchesWhenObscured 2017.2 and up
            //bool AndroidEnableSustainedPerformanceMode 2018 and up
            reader.AlignStream();
            var defaultScreenOrientation = reader.ReadInt32();
            var targetDevice = reader.ReadInt32();
            if (version[0] < 5 || (version[0] == 5 && version[1] < 3)) //5.3 down
            {
                if (version[0] < 5) //5.0 down
                {
                    var targetPlatform = reader.ReadInt32(); //4.0 and up targetGlesGraphics
                    if (version[0] > 4 || (version[0] == 4 && version[1] >= 6)) //4.6 and up
                    {
                        var targetIOSGraphics = reader.ReadInt32();
                    }
                }
                var targetResolution = reader.ReadInt32();
            } else {
                var useOnDemandResources = reader.ReadBoolean();
                reader.AlignStream();
            }
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 5)) //3.5 and up
            {
                var accelerometerFrequency = reader.ReadInt32();
            }
            companyName = reader.ReadAlignedString();
            productName = reader.ReadAlignedString();
        }
    }
}
