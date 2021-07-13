// LzmaBase.cs

namespace SoarCraft.QYun.AssetReader._7zip.Compress.LZMA {
    internal abstract class Base {
        public const uint kNumRepDistances = 4;
        public const uint kNumStates = 12;

        // static byte []kLiteralNextStates  = {0, 0, 0, 0, 1, 2, 3, 4,  5,  6,   4, 5};
        // static byte []kMatchNextStates    = {7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10};
        // static byte []kRepNextStates      = {8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11};
        // static byte []kShortRepNextStates = {9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11};

        public struct State {
            public uint Index;
            public void Init() { this.Index = 0; }
            public void UpdateChar() {
                if (this.Index < 4)
                    this.Index = 0;
                else if (this.Index < 10)
                    this.Index -= 3;
                else
                    this.Index -= 6;
            }
            public void UpdateMatch() { this.Index = (uint)(this.Index < 7 ? 7 : 10); }
            public void UpdateRep() { this.Index = (uint)(this.Index < 7 ? 8 : 11); }
            public void UpdateShortRep() { this.Index = (uint)(this.Index < 7 ? 9 : 11); }
            public bool IsCharState() { return this.Index < 7; }
        }

        public const int kNumPosSlotBits = 6;
        public const int kDicLogSizeMin = 0;
        // public const int kDicLogSizeMax = 30;
        // public const uint kDistTableSizeMax = kDicLogSizeMax * 2;

        public const int kNumLenToPosStatesBits = 2; // it's for speed optimization
        public const uint kNumLenToPosStates = 1 << kNumLenToPosStatesBits;

        public const uint kMatchMinLen = 2;

        public static uint GetLenToPosState(uint len) {
            len -= kMatchMinLen;
            if (len < kNumLenToPosStates)
                return len;
            return (uint)(kNumLenToPosStates - 1);
        }

        public const int kNumAlignBits = 4;
        public const uint kAlignTableSize = 1 << kNumAlignBits;
        public const uint kAlignMask = kAlignTableSize - 1;

        public const uint kStartPosModelIndex = 4;
        public const uint kEndPosModelIndex = 14;
        public const uint kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;

        public const uint kNumFullDistances = 1 << ((int)kEndPosModelIndex / 2);

        public const uint kNumLitPosStatesBitsEncodingMax = 4;
        public const uint kNumLitContextBitsMax = 8;

        public const int kNumPosStatesBitsMax = 4;
        public const uint kNumPosStatesMax = 1 << kNumPosStatesBitsMax;
        public const int kNumPosStatesBitsEncodingMax = 4;
        public const uint kNumPosStatesEncodingMax = 1 << kNumPosStatesBitsEncodingMax;

        public const int kNumLowLenBits = 3;
        public const int kNumMidLenBits = 3;
        public const int kNumHighLenBits = 8;
        public const uint kNumLowLenSymbols = 1 << kNumLowLenBits;
        public const uint kNumMidLenSymbols = 1 << kNumMidLenBits;
        public const uint kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols +
                (1 << kNumHighLenBits);
        public const uint kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;
    }
}
