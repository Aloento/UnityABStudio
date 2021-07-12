namespace SoarCraft.QYun.AssetReader._7zip.Compress.RangeCoder {
    using System;

    struct BitTreeEncoder {
        BitEncoder[] Models;
        int NumBitLevels;

        public BitTreeEncoder(int numBitLevels) {
            this.NumBitLevels = numBitLevels;
            this.Models = new BitEncoder[1 << numBitLevels];
        }

        public void Init() {
            for (uint i = 1; i < 1 << this.NumBitLevels; i++)
                this.Models[i].Init();
        }

        public void Encode(Encoder rangeEncoder, UInt32 symbol) {
            UInt32 m = 1;
            for (var bitIndex = this.NumBitLevels; bitIndex > 0;) {
                bitIndex--;
                var bit = (symbol >> bitIndex) & 1;
                this.Models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
            }
        }

        public void ReverseEncode(Encoder rangeEncoder, UInt32 symbol) {
            UInt32 m = 1;
            for (UInt32 i = 0; i < this.NumBitLevels; i++) {
                var bit = symbol & 1;
                this.Models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }

        public UInt32 GetPrice(UInt32 symbol) {
            UInt32 price = 0;
            UInt32 m = 1;
            for (var bitIndex = this.NumBitLevels; bitIndex > 0;) {
                bitIndex--;
                var bit = (symbol >> bitIndex) & 1;
                price += this.Models[m].GetPrice(bit);
                m = (m << 1) + bit;
            }
            return price;
        }

        public UInt32 ReverseGetPrice(UInt32 symbol) {
            UInt32 price = 0;
            UInt32 m = 1;
            for (var i = this.NumBitLevels; i > 0; i--) {
                var bit = symbol & 1;
                symbol >>= 1;
                price += this.Models[m].GetPrice(bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static UInt32 ReverseGetPrice(BitEncoder[] Models, UInt32 startIndex,
            int NumBitLevels, UInt32 symbol) {
            UInt32 price = 0;
            UInt32 m = 1;
            for (var i = NumBitLevels; i > 0; i--) {
                var bit = symbol & 1;
                symbol >>= 1;
                price += Models[startIndex + m].GetPrice(bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static void ReverseEncode(BitEncoder[] Models, UInt32 startIndex,
            Encoder rangeEncoder, int NumBitLevels, UInt32 symbol) {
            UInt32 m = 1;
            for (var i = 0; i < NumBitLevels; i++) {
                var bit = symbol & 1;
                Models[startIndex + m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }
    }

    struct BitTreeDecoder {
        BitDecoder[] Models;
        int NumBitLevels;

        public BitTreeDecoder(int numBitLevels) {
            this.NumBitLevels = numBitLevels;
            this.Models = new BitDecoder[1 << numBitLevels];
        }

        public void Init() {
            for (uint i = 1; i < 1 << this.NumBitLevels; i++)
                this.Models[i].Init();
        }

        public uint Decode(Decoder rangeDecoder) {
            uint m = 1;
            for (var bitIndex = this.NumBitLevels; bitIndex > 0; bitIndex--)
                m = (m << 1) + this.Models[m].Decode(rangeDecoder);
            return m - ((uint)1 << this.NumBitLevels);
        }

        public uint ReverseDecode(Decoder rangeDecoder) {
            uint m = 1;
            uint symbol = 0;
            for (var bitIndex = 0; bitIndex < this.NumBitLevels; bitIndex++) {
                var bit = this.Models[m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }
            return symbol;
        }

        public static uint ReverseDecode(BitDecoder[] Models, UInt32 startIndex,
            Decoder rangeDecoder, int NumBitLevels) {
            uint m = 1;
            uint symbol = 0;
            for (var bitIndex = 0; bitIndex < NumBitLevels; bitIndex++) {
                var bit = Models[startIndex + m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }
            return symbol;
        }
    }
}
