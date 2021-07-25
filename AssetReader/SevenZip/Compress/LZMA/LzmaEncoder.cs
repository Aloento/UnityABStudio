// LzmaEncoder.cs

namespace SoarCraft.QYun.AssetReader.SevenZip.Compress.LZMA {
    using System;
    using System.IO;
    using LZ;
    using RangeCoder;

    public class Encoder : ICoder, ISetCoderProperties, IWriteCoderProperties {
        enum EMatchFinderType {
            BT2,
            BT4,
        }

        const UInt32 kIfinityPrice = 0xFFFFFFF;

        static Byte[] g_FastPos = new Byte[1 << 11];

        static Encoder() {
            const Byte kFastSlots = 22;
            var c = 2;
            g_FastPos[0] = 0;
            g_FastPos[1] = 1;
            for (Byte slotFast = 2; slotFast < kFastSlots; slotFast++) {
                var k = (UInt32)1 << ((slotFast >> 1) - 1);
                for (UInt32 j = 0; j < k; j++, c++)
                    g_FastPos[c] = slotFast;
            }
        }

        static UInt32 GetPosSlot(UInt32 pos) {
            if (pos < 1 << 11)
                return g_FastPos[pos];
            if (pos < 1 << 21)
                return (UInt32)(g_FastPos[pos >> 10] + 20);
            return (UInt32)(g_FastPos[pos >> 20] + 40);
        }

        static UInt32 GetPosSlot2(UInt32 pos) {
            if (pos < 1 << 17)
                return (UInt32)(g_FastPos[pos >> 6] + 12);
            if (pos < 1 << 27)
                return (UInt32)(g_FastPos[pos >> 16] + 32);
            return (UInt32)(g_FastPos[pos >> 26] + 52);
        }

        Base.State _state;
        Byte _previousByte;
        UInt32[] _repDistances = new UInt32[Base.kNumRepDistances];

        void BaseInit() {
            this._state.Init();
            this._previousByte = 0;
            for (UInt32 i = 0; i < Base.kNumRepDistances; i++)
                this._repDistances[i] = 0;
        }

        const int kDefaultDictionaryLogSize = 22;
        const UInt32 kNumFastBytesDefault = 0x20;

        class LiteralEncoder {
            public struct Encoder2 {
                BitEncoder[] m_Encoders;

                public void Create() { this.m_Encoders = new BitEncoder[0x300]; }

                public void Init() { for (var i = 0; i < 0x300; i++) this.m_Encoders[i].Init(); }

                public void Encode(RangeCoder.Encoder rangeEncoder, byte symbol) {
                    uint context = 1;
                    for (var i = 7; i >= 0; i--) {
                        var bit = (uint)((symbol >> i) & 1);
                        this.m_Encoders[context].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public void EncodeMatched(RangeCoder.Encoder rangeEncoder, byte matchByte, byte symbol) {
                    uint context = 1;
                    var same = true;
                    for (var i = 7; i >= 0; i--) {
                        var bit = (uint)((symbol >> i) & 1);
                        var state = context;
                        if (same) {
                            var matchBit = (uint)((matchByte >> i) & 1);
                            state += (1 + matchBit) << 8;
                            same = matchBit == bit;
                        }
                        this.m_Encoders[state].Encode(rangeEncoder, bit);
                        context = (context << 1) | bit;
                    }
                }

                public uint GetPrice(bool matchMode, byte matchByte, byte symbol) {
                    uint price = 0;
                    uint context = 1;
                    var i = 7;
                    if (matchMode) {
                        for (; i >= 0; i--) {
                            var matchBit = (uint)(matchByte >> i) & 1;
                            var bit = (uint)(symbol >> i) & 1;
                            price += this.m_Encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                            context = (context << 1) | bit;
                            if (matchBit != bit) {
                                i--;
                                break;
                            }
                        }
                    }
                    for (; i >= 0; i--) {
                        var bit = (uint)(symbol >> i) & 1;
                        price += this.m_Encoders[context].GetPrice(bit);
                        context = (context << 1) | bit;
                    }
                    return price;
                }
            }

            Encoder2[] m_Coders;
            int m_NumPrevBits;
            int m_NumPosBits;
            uint m_PosMask;

            public void Create(int numPosBits, int numPrevBits) {
                if (this.m_Coders != null && this.m_NumPrevBits == numPrevBits && this.m_NumPosBits == numPosBits)
                    return;
                this.m_NumPosBits = numPosBits;
                this.m_PosMask = ((uint)1 << numPosBits) - 1;
                this.m_NumPrevBits = numPrevBits;
                var numStates = (uint)1 << (this.m_NumPrevBits + this.m_NumPosBits);
                this.m_Coders = new Encoder2[numStates];
                for (uint i = 0; i < numStates; i++)
                    this.m_Coders[i].Create();
            }

            public void Init() {
                var numStates = (uint)1 << (this.m_NumPrevBits + this.m_NumPosBits);
                for (uint i = 0; i < numStates; i++)
                    this.m_Coders[i].Init();
            }

            public Encoder2 GetSubCoder(UInt32 pos, Byte prevByte) { return this.m_Coders[((pos & this.m_PosMask) << this.m_NumPrevBits) + (uint)(prevByte >> (8 - this.m_NumPrevBits))]; }
        }

        class LenEncoder {
            BitEncoder _choice;
            BitEncoder _choice2;
            BitTreeEncoder[] _lowCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
            BitTreeEncoder[] _midCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
            BitTreeEncoder _highCoder = new(Base.kNumHighLenBits);

            public LenEncoder() {
                for (UInt32 posState = 0; posState < Base.kNumPosStatesEncodingMax; posState++) {
                    this._lowCoder[posState] = new BitTreeEncoder(Base.kNumLowLenBits);
                    this._midCoder[posState] = new BitTreeEncoder(Base.kNumMidLenBits);
                }
            }

            public void Init(UInt32 numPosStates) {
                this._choice.Init();
                this._choice2.Init();
                for (UInt32 posState = 0; posState < numPosStates; posState++) {
                    this._lowCoder[posState].Init();
                    this._midCoder[posState].Init();
                }
                this._highCoder.Init();
            }

            public void Encode(RangeCoder.Encoder rangeEncoder, UInt32 symbol, UInt32 posState) {
                if (symbol < Base.kNumLowLenSymbols) {
                    this._choice.Encode(rangeEncoder, 0);
                    this._lowCoder[posState].Encode(rangeEncoder, symbol);
                } else {
                    symbol -= Base.kNumLowLenSymbols;
                    this._choice.Encode(rangeEncoder, 1);
                    if (symbol < Base.kNumMidLenSymbols) {
                        this._choice2.Encode(rangeEncoder, 0);
                        this._midCoder[posState].Encode(rangeEncoder, symbol);
                    } else {
                        this._choice2.Encode(rangeEncoder, 1);
                        this._highCoder.Encode(rangeEncoder, symbol - Base.kNumMidLenSymbols);
                    }
                }
            }

            public void SetPrices(UInt32 posState, UInt32 numSymbols, UInt32[] prices, UInt32 st) {
                var a0 = this._choice.GetPrice0();
                var a1 = this._choice.GetPrice1();
                var b0 = a1 + this._choice2.GetPrice0();
                var b1 = a1 + this._choice2.GetPrice1();
                UInt32 i = 0;
                for (i = 0; i < Base.kNumLowLenSymbols; i++) {
                    if (i >= numSymbols)
                        return;
                    prices[st + i] = a0 + this._lowCoder[posState].GetPrice(i);
                }
                for (; i < Base.kNumLowLenSymbols + Base.kNumMidLenSymbols; i++) {
                    if (i >= numSymbols)
                        return;
                    prices[st + i] = b0 + this._midCoder[posState].GetPrice(i - Base.kNumLowLenSymbols);
                }
                for (; i < numSymbols; i++)
                    prices[st + i] = b1 + this._highCoder.GetPrice(i - Base.kNumLowLenSymbols - Base.kNumMidLenSymbols);
            }
        }

        const UInt32 kNumLenSpecSymbols = Base.kNumLowLenSymbols + Base.kNumMidLenSymbols;

        class LenPriceTableEncoder : LenEncoder {
            UInt32[] _prices = new UInt32[Base.kNumLenSymbols << Base.kNumPosStatesBitsEncodingMax];
            UInt32 _tableSize;
            UInt32[] _counters = new UInt32[Base.kNumPosStatesEncodingMax];

            public void SetTableSize(UInt32 tableSize) { this._tableSize = tableSize; }

            public UInt32 GetPrice(UInt32 symbol, UInt32 posState) {
                return this._prices[posState * Base.kNumLenSymbols + symbol];
            }

            void UpdateTable(UInt32 posState) {
                this.SetPrices(posState, this._tableSize, this._prices, posState * Base.kNumLenSymbols);
                this._counters[posState] = this._tableSize;
            }

            public void UpdateTables(UInt32 numPosStates) {
                for (UInt32 posState = 0; posState < numPosStates; posState++)
                    this.UpdateTable(posState);
            }

            public new void Encode(RangeCoder.Encoder rangeEncoder, UInt32 symbol, UInt32 posState) {
                base.Encode(rangeEncoder, symbol, posState);
                if (--this._counters[posState] == 0)
                    this.UpdateTable(posState);
            }
        }

        const UInt32 kNumOpts = 1 << 12;
        class Optimal {
            public Base.State State;

            public bool Prev1IsChar;
            public bool Prev2;

            public UInt32 PosPrev2;
            public UInt32 BackPrev2;

            public UInt32 Price;
            public UInt32 PosPrev;
            public UInt32 BackPrev;

            public UInt32 Backs0;
            public UInt32 Backs1;
            public UInt32 Backs2;
            public UInt32 Backs3;

            public void MakeAsChar() { this.BackPrev = 0xFFFFFFFF; this.Prev1IsChar = false; }
            public void MakeAsShortRep() { this.BackPrev = 0; ; this.Prev1IsChar = false; }
            public bool IsShortRep() { return this.BackPrev == 0; }
        }
        Optimal[] _optimum = new Optimal[kNumOpts];
        IMatchFinder _matchFinder;
        RangeCoder.Encoder _rangeEncoder = new();

        BitEncoder[] _isMatch = new BitEncoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
        BitEncoder[] _isRep = new BitEncoder[Base.kNumStates];
        BitEncoder[] _isRepG0 = new BitEncoder[Base.kNumStates];
        BitEncoder[] _isRepG1 = new BitEncoder[Base.kNumStates];
        BitEncoder[] _isRepG2 = new BitEncoder[Base.kNumStates];
        BitEncoder[] _isRep0Long = new BitEncoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

        BitTreeEncoder[] _posSlotEncoder = new BitTreeEncoder[Base.kNumLenToPosStates];

        BitEncoder[] _posEncoders = new BitEncoder[Base.kNumFullDistances - Base.kEndPosModelIndex];
        BitTreeEncoder _posAlignEncoder = new(Base.kNumAlignBits);

        LenPriceTableEncoder _lenEncoder = new();
        LenPriceTableEncoder _repMatchLenEncoder = new();

        LiteralEncoder _literalEncoder = new();

        UInt32[] _matchDistances = new UInt32[Base.kMatchMaxLen * 2 + 2];

        UInt32 _numFastBytes = kNumFastBytesDefault;
        UInt32 _longestMatchLength;
        UInt32 _numDistancePairs;

        UInt32 _additionalOffset;

        UInt32 _optimumEndIndex;
        UInt32 _optimumCurrentIndex;

        bool _longestMatchWasFound;

        UInt32[] _posSlotPrices = new UInt32[1 << (Base.kNumPosSlotBits + Base.kNumLenToPosStatesBits)];
        UInt32[] _distancesPrices = new UInt32[Base.kNumFullDistances << Base.kNumLenToPosStatesBits];
        UInt32[] _alignPrices = new UInt32[Base.kAlignTableSize];
        UInt32 _alignPriceCount;

        UInt32 _distTableSize = kDefaultDictionaryLogSize * 2;

        int _posStateBits = 2;
        UInt32 _posStateMask = 4 - 1;
        int _numLiteralPosStateBits;
        int _numLiteralContextBits = 3;

        UInt32 _dictionarySize = 1 << kDefaultDictionaryLogSize;
        UInt32 _dictionarySizePrev = 0xFFFFFFFF;
        UInt32 _numFastBytesPrev = 0xFFFFFFFF;

        Int64 nowPos64;
        bool _finished;
        Stream _inStream;

        EMatchFinderType _matchFinderType = EMatchFinderType.BT4;
        bool _writeEndMark;

        bool _needReleaseMFStream;

        void Create() {
            if (this._matchFinder == null) {
                var bt = new BinTree();
                var numHashBytes = 4;
                if (this._matchFinderType == EMatchFinderType.BT2)
                    numHashBytes = 2;
                bt.SetType(numHashBytes);
                this._matchFinder = bt;
            }
            this._literalEncoder.Create(this._numLiteralPosStateBits, this._numLiteralContextBits);

            if (this._dictionarySize == this._dictionarySizePrev && this._numFastBytesPrev == this._numFastBytes)
                return;
            this._matchFinder.Create(this._dictionarySize, kNumOpts, this._numFastBytes, Base.kMatchMaxLen + 1);
            this._dictionarySizePrev = this._dictionarySize;
            this._numFastBytesPrev = this._numFastBytes;
        }

        public Encoder() {
            for (var i = 0; i < kNumOpts; i++)
                this._optimum[i] = new Optimal();
            for (var i = 0; i < Base.kNumLenToPosStates; i++)
                this._posSlotEncoder[i] = new BitTreeEncoder(Base.kNumPosSlotBits);
        }

        void SetWriteEndMarkerMode(bool writeEndMarker) {
            this._writeEndMark = writeEndMarker;
        }

        void Init() {
            this.BaseInit();
            this._rangeEncoder.Init();

            uint i;
            for (i = 0; i < Base.kNumStates; i++) {
                for (uint j = 0; j <= this._posStateMask; j++) {
                    var complexState = (i << Base.kNumPosStatesBitsMax) + j;
                    this._isMatch[complexState].Init();
                    this._isRep0Long[complexState].Init();
                }
                this._isRep[i].Init();
                this._isRepG0[i].Init();
                this._isRepG1[i].Init();
                this._isRepG2[i].Init();
            }
            this._literalEncoder.Init();
            for (i = 0; i < Base.kNumLenToPosStates; i++)
                this._posSlotEncoder[i].Init();
            for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
                this._posEncoders[i].Init();

            this._lenEncoder.Init((UInt32)1 << this._posStateBits);
            this._repMatchLenEncoder.Init((UInt32)1 << this._posStateBits);

            this._posAlignEncoder.Init();

            this._longestMatchWasFound = false;
            this._optimumEndIndex = 0;
            this._optimumCurrentIndex = 0;
            this._additionalOffset = 0;
        }

        void ReadMatchDistances(out UInt32 lenRes, out UInt32 numDistancePairs) {
            lenRes = 0;
            numDistancePairs = this._matchFinder.GetMatches(this._matchDistances);
            if (numDistancePairs > 0) {
                lenRes = this._matchDistances[numDistancePairs - 2];
                if (lenRes == this._numFastBytes)
                    lenRes += this._matchFinder.GetMatchLen((int)lenRes - 1, this._matchDistances[numDistancePairs - 1],
                        Base.kMatchMaxLen - lenRes);
            }
            this._additionalOffset++;
        }


        void MovePos(UInt32 num) {
            if (num > 0) {
                this._matchFinder.Skip(num);
                this._additionalOffset += num;
            }
        }

        UInt32 GetRepLen1Price(Base.State state, UInt32 posState) {
            return this._isRepG0[state.Index].GetPrice0() +
                    this._isRep0Long[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0();
        }

        UInt32 GetPureRepPrice(UInt32 repIndex, Base.State state, UInt32 posState) {
            UInt32 price;
            if (repIndex == 0) {
                price = this._isRepG0[state.Index].GetPrice0();
                price += this._isRep0Long[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
            } else {
                price = this._isRepG0[state.Index].GetPrice1();
                if (repIndex == 1)
                    price += this._isRepG1[state.Index].GetPrice0();
                else {
                    price += this._isRepG1[state.Index].GetPrice1();
                    price += this._isRepG2[state.Index].GetPrice(repIndex - 2);
                }
            }
            return price;
        }

        UInt32 GetRepPrice(UInt32 repIndex, UInt32 len, Base.State state, UInt32 posState) {
            var price = this._repMatchLenEncoder.GetPrice(len - Base.kMatchMinLen, posState);
            return price + this.GetPureRepPrice(repIndex, state, posState);
        }

        UInt32 GetPosLenPrice(UInt32 pos, UInt32 len, UInt32 posState) {
            UInt32 price;
            var lenToPosState = Base.GetLenToPosState(len);
            if (pos < Base.kNumFullDistances)
                price = this._distancesPrices[(lenToPosState * Base.kNumFullDistances) + pos];
            else
                price = this._posSlotPrices[(lenToPosState << Base.kNumPosSlotBits) + GetPosSlot2(pos)] +
                    this._alignPrices[pos & Base.kAlignMask];
            return price + this._lenEncoder.GetPrice(len - Base.kMatchMinLen, posState);
        }

        UInt32 Backward(out UInt32 backRes, UInt32 cur) {
            this._optimumEndIndex = cur;
            var posMem = this._optimum[cur].PosPrev;
            var backMem = this._optimum[cur].BackPrev;
            do {
                if (this._optimum[cur].Prev1IsChar) {
                    this._optimum[posMem].MakeAsChar();
                    this._optimum[posMem].PosPrev = posMem - 1;
                    if (this._optimum[cur].Prev2) {
                        this._optimum[posMem - 1].Prev1IsChar = false;
                        this._optimum[posMem - 1].PosPrev = this._optimum[cur].PosPrev2;
                        this._optimum[posMem - 1].BackPrev = this._optimum[cur].BackPrev2;
                    }
                }
                var posPrev = posMem;
                var backCur = backMem;

                backMem = this._optimum[posPrev].BackPrev;
                posMem = this._optimum[posPrev].PosPrev;

                this._optimum[posPrev].BackPrev = backCur;
                this._optimum[posPrev].PosPrev = cur;
                cur = posPrev;
            }
            while (cur > 0);
            backRes = this._optimum[0].BackPrev;
            this._optimumCurrentIndex = this._optimum[0].PosPrev;
            return this._optimumCurrentIndex;
        }

        UInt32[] reps = new UInt32[Base.kNumRepDistances];
        UInt32[] repLens = new UInt32[Base.kNumRepDistances];


        UInt32 GetOptimum(UInt32 position, out UInt32 backRes) {
            if (this._optimumEndIndex != this._optimumCurrentIndex) {
                var lenRes = this._optimum[this._optimumCurrentIndex].PosPrev - this._optimumCurrentIndex;
                backRes = this._optimum[this._optimumCurrentIndex].BackPrev;
                this._optimumCurrentIndex = this._optimum[this._optimumCurrentIndex].PosPrev;
                return lenRes;
            }
            this._optimumCurrentIndex = this._optimumEndIndex = 0;

            UInt32 lenMain, numDistancePairs;
            if (!this._longestMatchWasFound) {
                this.ReadMatchDistances(out lenMain, out numDistancePairs);
            } else {
                lenMain = this._longestMatchLength;
                numDistancePairs = this._numDistancePairs;
                this._longestMatchWasFound = false;
            }

            var numAvailableBytes = this._matchFinder.GetNumAvailableBytes() + 1;
            if (numAvailableBytes < 2) {
                backRes = 0xFFFFFFFF;
                return 1;
            }
            if (numAvailableBytes > Base.kMatchMaxLen)
                numAvailableBytes = Base.kMatchMaxLen;

            UInt32 repMaxIndex = 0;
            UInt32 i;
            for (i = 0; i < Base.kNumRepDistances; i++) {
                this.reps[i] = this._repDistances[i];
                this.repLens[i] = this._matchFinder.GetMatchLen(0 - 1, this.reps[i], Base.kMatchMaxLen);
                if (this.repLens[i] > this.repLens[repMaxIndex])
                    repMaxIndex = i;
            }
            if (this.repLens[repMaxIndex] >= this._numFastBytes) {
                backRes = repMaxIndex;
                var lenRes = this.repLens[repMaxIndex];
                this.MovePos(lenRes - 1);
                return lenRes;
            }

            if (lenMain >= this._numFastBytes) {
                backRes = this._matchDistances[numDistancePairs - 1] + Base.kNumRepDistances;
                this.MovePos(lenMain - 1);
                return lenMain;
            }

            var currentByte = this._matchFinder.GetIndexByte(0 - 1);
            var matchByte = this._matchFinder.GetIndexByte((Int32)(0 - this._repDistances[0] - 1 - 1));

            if (lenMain < 2 && currentByte != matchByte && this.repLens[repMaxIndex] < 2) {
                backRes = 0xFFFFFFFF;
                return 1;
            }

            this._optimum[0].State = this._state;

            var posState = position & this._posStateMask;

            this._optimum[1].Price = this._isMatch[(this._state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                                     this._literalEncoder.GetSubCoder(position, this._previousByte).GetPrice(!this._state.IsCharState(), matchByte, currentByte);
            this._optimum[1].MakeAsChar();

            var matchPrice = this._isMatch[(this._state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
            var repMatchPrice = matchPrice + this._isRep[this._state.Index].GetPrice1();

            if (matchByte == currentByte) {
                var shortRepPrice = repMatchPrice + this.GetRepLen1Price(this._state, posState);
                if (shortRepPrice < this._optimum[1].Price) {
                    this._optimum[1].Price = shortRepPrice;
                    this._optimum[1].MakeAsShortRep();
                }
            }

            var lenEnd = lenMain >= this.repLens[repMaxIndex] ? lenMain : this.repLens[repMaxIndex];

            if (lenEnd < 2) {
                backRes = this._optimum[1].BackPrev;
                return 1;
            }

            this._optimum[1].PosPrev = 0;

            this._optimum[0].Backs0 = this.reps[0];
            this._optimum[0].Backs1 = this.reps[1];
            this._optimum[0].Backs2 = this.reps[2];
            this._optimum[0].Backs3 = this.reps[3];

            var len = lenEnd;
            do
                this._optimum[len--].Price = kIfinityPrice;
            while (len >= 2);

            for (i = 0; i < Base.kNumRepDistances; i++) {
                var repLen = this.repLens[i];
                if (repLen < 2)
                    continue;
                var price = repMatchPrice + this.GetPureRepPrice(i, this._state, posState);
                do {
                    var curAndLenPrice = price + this._repMatchLenEncoder.GetPrice(repLen - 2, posState);
                    var optimum = this._optimum[repLen];
                    if (curAndLenPrice < optimum.Price) {
                        optimum.Price = curAndLenPrice;
                        optimum.PosPrev = 0;
                        optimum.BackPrev = i;
                        optimum.Prev1IsChar = false;
                    }
                }
                while (--repLen >= 2);
            }

            var normalMatchPrice = matchPrice + this._isRep[this._state.Index].GetPrice0();

            len = this.repLens[0] >= 2 ? this.repLens[0] + 1 : 2;
            if (len <= lenMain) {
                UInt32 offs = 0;
                while (len > this._matchDistances[offs])
                    offs += 2;
                for (; ; len++) {
                    var distance = this._matchDistances[offs + 1];
                    var curAndLenPrice = normalMatchPrice + this.GetPosLenPrice(distance, len, posState);
                    var optimum = this._optimum[len];
                    if (curAndLenPrice < optimum.Price) {
                        optimum.Price = curAndLenPrice;
                        optimum.PosPrev = 0;
                        optimum.BackPrev = distance + Base.kNumRepDistances;
                        optimum.Prev1IsChar = false;
                    }
                    if (len == this._matchDistances[offs]) {
                        offs += 2;
                        if (offs == numDistancePairs)
                            break;
                    }
                }
            }

            UInt32 cur = 0;

            while (true) {
                cur++;
                if (cur == lenEnd)
                    return this.Backward(out backRes, cur);
                UInt32 newLen;
                this.ReadMatchDistances(out newLen, out numDistancePairs);
                if (newLen >= this._numFastBytes) {
                    this._numDistancePairs = numDistancePairs;
                    this._longestMatchLength = newLen;
                    this._longestMatchWasFound = true;
                    return this.Backward(out backRes, cur);
                }
                position++;
                var posPrev = this._optimum[cur].PosPrev;
                Base.State state;
                if (this._optimum[cur].Prev1IsChar) {
                    posPrev--;
                    if (this._optimum[cur].Prev2) {
                        state = this._optimum[this._optimum[cur].PosPrev2].State;
                        if (this._optimum[cur].BackPrev2 < Base.kNumRepDistances)
                            state.UpdateRep();
                        else
                            state.UpdateMatch();
                    } else
                        state = this._optimum[posPrev].State;
                    state.UpdateChar();
                } else
                    state = this._optimum[posPrev].State;
                if (posPrev == cur - 1) {
                    if (this._optimum[cur].IsShortRep())
                        state.UpdateShortRep();
                    else
                        state.UpdateChar();
                } else {
                    UInt32 pos;
                    if (this._optimum[cur].Prev1IsChar && this._optimum[cur].Prev2) {
                        posPrev = this._optimum[cur].PosPrev2;
                        pos = this._optimum[cur].BackPrev2;
                        state.UpdateRep();
                    } else {
                        pos = this._optimum[cur].BackPrev;
                        if (pos < Base.kNumRepDistances)
                            state.UpdateRep();
                        else
                            state.UpdateMatch();
                    }
                    var opt = this._optimum[posPrev];
                    if (pos < Base.kNumRepDistances) {
                        if (pos == 0) {
                            this.reps[0] = opt.Backs0;
                            this.reps[1] = opt.Backs1;
                            this.reps[2] = opt.Backs2;
                            this.reps[3] = opt.Backs3;
                        } else if (pos == 1) {
                            this.reps[0] = opt.Backs1;
                            this.reps[1] = opt.Backs0;
                            this.reps[2] = opt.Backs2;
                            this.reps[3] = opt.Backs3;
                        } else if (pos == 2) {
                            this.reps[0] = opt.Backs2;
                            this.reps[1] = opt.Backs0;
                            this.reps[2] = opt.Backs1;
                            this.reps[3] = opt.Backs3;
                        } else {
                            this.reps[0] = opt.Backs3;
                            this.reps[1] = opt.Backs0;
                            this.reps[2] = opt.Backs1;
                            this.reps[3] = opt.Backs2;
                        }
                    } else {
                        this.reps[0] = pos - Base.kNumRepDistances;
                        this.reps[1] = opt.Backs0;
                        this.reps[2] = opt.Backs1;
                        this.reps[3] = opt.Backs2;
                    }
                }
                this._optimum[cur].State = state;
                this._optimum[cur].Backs0 = this.reps[0];
                this._optimum[cur].Backs1 = this.reps[1];
                this._optimum[cur].Backs2 = this.reps[2];
                this._optimum[cur].Backs3 = this.reps[3];
                var curPrice = this._optimum[cur].Price;

                currentByte = this._matchFinder.GetIndexByte(0 - 1);
                matchByte = this._matchFinder.GetIndexByte((Int32)(0 - this.reps[0] - 1 - 1));

                posState = position & this._posStateMask;

                var curAnd1Price = curPrice +
                    this._isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice0() +
                    this._literalEncoder.GetSubCoder(position, this._matchFinder.GetIndexByte(0 - 2)).
                    GetPrice(!state.IsCharState(), matchByte, currentByte);

                var nextOptimum = this._optimum[cur + 1];

                var nextIsChar = false;
                if (curAnd1Price < nextOptimum.Price) {
                    nextOptimum.Price = curAnd1Price;
                    nextOptimum.PosPrev = cur;
                    nextOptimum.MakeAsChar();
                    nextIsChar = true;
                }

                matchPrice = curPrice + this._isMatch[(state.Index << Base.kNumPosStatesBitsMax) + posState].GetPrice1();
                repMatchPrice = matchPrice + this._isRep[state.Index].GetPrice1();

                if (matchByte == currentByte &&
                    !(nextOptimum.PosPrev < cur && nextOptimum.BackPrev == 0)) {
                    var shortRepPrice = repMatchPrice + this.GetRepLen1Price(state, posState);
                    if (shortRepPrice <= nextOptimum.Price) {
                        nextOptimum.Price = shortRepPrice;
                        nextOptimum.PosPrev = cur;
                        nextOptimum.MakeAsShortRep();
                        nextIsChar = true;
                    }
                }

                var numAvailableBytesFull = this._matchFinder.GetNumAvailableBytes() + 1;
                numAvailableBytesFull = Math.Min(kNumOpts - 1 - cur, numAvailableBytesFull);
                numAvailableBytes = numAvailableBytesFull;

                if (numAvailableBytes < 2)
                    continue;
                if (numAvailableBytes > this._numFastBytes)
                    numAvailableBytes = this._numFastBytes;
                if (!nextIsChar && matchByte != currentByte) {
                    // try Literal + rep0
                    var t = Math.Min(numAvailableBytesFull - 1, this._numFastBytes);
                    var lenTest2 = this._matchFinder.GetMatchLen(0, this.reps[0], t);
                    if (lenTest2 >= 2) {
                        var state2 = state;
                        state2.UpdateChar();
                        var posStateNext = (position + 1) & this._posStateMask;
                        var nextRepMatchPrice = curAnd1Price +
                            this._isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice1() +
                            this._isRep[state2.Index].GetPrice1();
                        {
                            var offset = cur + 1 + lenTest2;
                            while (lenEnd < offset)
                                this._optimum[++lenEnd].Price = kIfinityPrice;
                            var curAndLenPrice = nextRepMatchPrice + this.GetRepPrice(
                                0, lenTest2, state2, posStateNext);
                            var optimum = this._optimum[offset];
                            if (curAndLenPrice < optimum.Price) {
                                optimum.Price = curAndLenPrice;
                                optimum.PosPrev = cur + 1;
                                optimum.BackPrev = 0;
                                optimum.Prev1IsChar = true;
                                optimum.Prev2 = false;
                            }
                        }
                    }
                }

                UInt32 startLen = 2; // speed optimization 

                for (UInt32 repIndex = 0; repIndex < Base.kNumRepDistances; repIndex++) {
                    var lenTest = this._matchFinder.GetMatchLen(0 - 1, this.reps[repIndex], numAvailableBytes);
                    if (lenTest < 2)
                        continue;
                    var lenTestTemp = lenTest;
                    do {
                        while (lenEnd < cur + lenTest)
                            this._optimum[++lenEnd].Price = kIfinityPrice;
                        var curAndLenPrice = repMatchPrice + this.GetRepPrice(repIndex, lenTest, state, posState);
                        var optimum = this._optimum[cur + lenTest];
                        if (curAndLenPrice < optimum.Price) {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = cur;
                            optimum.BackPrev = repIndex;
                            optimum.Prev1IsChar = false;
                        }
                    }
                    while (--lenTest >= 2);
                    lenTest = lenTestTemp;

                    if (repIndex == 0)
                        startLen = lenTest + 1;

                    // if (_maxMode)
                    if (lenTest < numAvailableBytesFull) {
                        var t = Math.Min(numAvailableBytesFull - 1 - lenTest, this._numFastBytes);
                        var lenTest2 = this._matchFinder.GetMatchLen((Int32)lenTest, this.reps[repIndex], t);
                        if (lenTest2 >= 2) {
                            var state2 = state;
                            state2.UpdateRep();
                            var posStateNext = (position + lenTest) & this._posStateMask;
                            var curAndLenCharPrice =
                                    repMatchPrice + this.GetRepPrice(repIndex, lenTest, state, posState) +
                                    this._isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                    this._literalEncoder.GetSubCoder(position + lenTest,
                                    this._matchFinder.GetIndexByte((Int32)lenTest - 1 - 1)).GetPrice(true,
                                    this._matchFinder.GetIndexByte((Int32)lenTest - 1 - (Int32)(this.reps[repIndex] + 1)),
                                    this._matchFinder.GetIndexByte((Int32)lenTest - 1));
                            state2.UpdateChar();
                            posStateNext = (position + lenTest + 1) & this._posStateMask;
                            var nextMatchPrice = curAndLenCharPrice + this._isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                            var nextRepMatchPrice = nextMatchPrice + this._isRep[state2.Index].GetPrice1();

                            // for(; lenTest2 >= 2; lenTest2--)
                            {
                                var offset = lenTest + 1 + lenTest2;
                                while (lenEnd < cur + offset)
                                    this._optimum[++lenEnd].Price = kIfinityPrice;
                                var curAndLenPrice = nextRepMatchPrice + this.GetRepPrice(0, lenTest2, state2, posStateNext);
                                var optimum = this._optimum[cur + offset];
                                if (curAndLenPrice < optimum.Price) {
                                    optimum.Price = curAndLenPrice;
                                    optimum.PosPrev = cur + lenTest + 1;
                                    optimum.BackPrev = 0;
                                    optimum.Prev1IsChar = true;
                                    optimum.Prev2 = true;
                                    optimum.PosPrev2 = cur;
                                    optimum.BackPrev2 = repIndex;
                                }
                            }
                        }
                    }
                }

                if (newLen > numAvailableBytes) {
                    newLen = numAvailableBytes;
                    for (numDistancePairs = 0; newLen > this._matchDistances[numDistancePairs]; numDistancePairs += 2)
                        ;
                    this._matchDistances[numDistancePairs] = newLen;
                    numDistancePairs += 2;
                }
                if (newLen >= startLen) {
                    normalMatchPrice = matchPrice + this._isRep[state.Index].GetPrice0();
                    while (lenEnd < cur + newLen)
                        this._optimum[++lenEnd].Price = kIfinityPrice;

                    UInt32 offs = 0;
                    while (startLen > this._matchDistances[offs])
                        offs += 2;

                    for (var lenTest = startLen; ; lenTest++) {
                        var curBack = this._matchDistances[offs + 1];
                        var curAndLenPrice = normalMatchPrice + this.GetPosLenPrice(curBack, lenTest, posState);
                        var optimum = this._optimum[cur + lenTest];
                        if (curAndLenPrice < optimum.Price) {
                            optimum.Price = curAndLenPrice;
                            optimum.PosPrev = cur;
                            optimum.BackPrev = curBack + Base.kNumRepDistances;
                            optimum.Prev1IsChar = false;
                        }

                        if (lenTest == this._matchDistances[offs]) {
                            if (lenTest < numAvailableBytesFull) {
                                var t = Math.Min(numAvailableBytesFull - 1 - lenTest, this._numFastBytes);
                                var lenTest2 = this._matchFinder.GetMatchLen((Int32)lenTest, curBack, t);
                                if (lenTest2 >= 2) {
                                    var state2 = state;
                                    state2.UpdateMatch();
                                    var posStateNext = (position + lenTest) & this._posStateMask;
                                    var curAndLenCharPrice = curAndLenPrice +
                                        this._isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice0() +
                                        this._literalEncoder.GetSubCoder(position + lenTest,
                                        this._matchFinder.GetIndexByte((Int32)lenTest - 1 - 1)).
                                        GetPrice(true,
                                        this._matchFinder.GetIndexByte((Int32)lenTest - (Int32)(curBack + 1) - 1),
                                        this._matchFinder.GetIndexByte((Int32)lenTest - 1));
                                    state2.UpdateChar();
                                    posStateNext = (position + lenTest + 1) & this._posStateMask;
                                    var nextMatchPrice = curAndLenCharPrice + this._isMatch[(state2.Index << Base.kNumPosStatesBitsMax) + posStateNext].GetPrice1();
                                    var nextRepMatchPrice = nextMatchPrice + this._isRep[state2.Index].GetPrice1();

                                    var offset = lenTest + 1 + lenTest2;
                                    while (lenEnd < cur + offset)
                                        this._optimum[++lenEnd].Price = kIfinityPrice;
                                    curAndLenPrice = nextRepMatchPrice + this.GetRepPrice(0, lenTest2, state2, posStateNext);
                                    optimum = this._optimum[cur + offset];
                                    if (curAndLenPrice < optimum.Price) {
                                        optimum.Price = curAndLenPrice;
                                        optimum.PosPrev = cur + lenTest + 1;
                                        optimum.BackPrev = 0;
                                        optimum.Prev1IsChar = true;
                                        optimum.Prev2 = true;
                                        optimum.PosPrev2 = cur;
                                        optimum.BackPrev2 = curBack + Base.kNumRepDistances;
                                    }
                                }
                            }
                            offs += 2;
                            if (offs == numDistancePairs)
                                break;
                        }
                    }
                }
            }
        }

        bool ChangePair(UInt32 smallDist, UInt32 bigDist) {
            const int kDif = 7;
            return smallDist < (UInt32)1 << (32 - kDif) && bigDist >= smallDist << kDif;
        }

        void WriteEndMarker(UInt32 posState) {
            if (!this._writeEndMark)
                return;

            this._isMatch[(this._state.Index << Base.kNumPosStatesBitsMax) + posState].Encode(this._rangeEncoder, 1);
            this._isRep[this._state.Index].Encode(this._rangeEncoder, 0);
            this._state.UpdateMatch();
            var len = Base.kMatchMinLen;
            this._lenEncoder.Encode(this._rangeEncoder, len - Base.kMatchMinLen, posState);
            UInt32 posSlot = (1 << Base.kNumPosSlotBits) - 1;
            var lenToPosState = Base.GetLenToPosState(len);
            this._posSlotEncoder[lenToPosState].Encode(this._rangeEncoder, posSlot);
            var footerBits = 30;
            var posReduced = ((UInt32)1 << footerBits) - 1;
            this._rangeEncoder.EncodeDirectBits(posReduced >> Base.kNumAlignBits, footerBits - Base.kNumAlignBits);
            this._posAlignEncoder.ReverseEncode(this._rangeEncoder, posReduced & Base.kAlignMask);
        }

        void Flush(UInt32 nowPos) {
            this.ReleaseMFStream();
            this.WriteEndMarker(nowPos & this._posStateMask);
            this._rangeEncoder.FlushData();
            this._rangeEncoder.FlushStream();
        }

        public void CodeOneBlock(out Int64 inSize, out Int64 outSize, out bool finished) {
            inSize = 0;
            outSize = 0;
            finished = true;

            if (this._inStream != null) {
                this._matchFinder.SetStream(this._inStream);
                this._matchFinder.Init();
                this._needReleaseMFStream = true;
                this._inStream = null;
                if (this._trainSize > 0)
                    this._matchFinder.Skip(this._trainSize);
            }

            if (this._finished)
                return;
            this._finished = true;


            var progressPosValuePrev = this.nowPos64;
            if (this.nowPos64 == 0) {
                if (this._matchFinder.GetNumAvailableBytes() == 0) {
                    this.Flush((UInt32)this.nowPos64);
                    return;
                }
                UInt32 len, numDistancePairs; // it's not used
                this.ReadMatchDistances(out len, out numDistancePairs);
                var posState = (UInt32)this.nowPos64 & this._posStateMask;
                this._isMatch[(this._state.Index << Base.kNumPosStatesBitsMax) + posState].Encode(this._rangeEncoder, 0);
                this._state.UpdateChar();
                var curByte = this._matchFinder.GetIndexByte((Int32)(0 - this._additionalOffset));
                this._literalEncoder.GetSubCoder((UInt32)this.nowPos64, this._previousByte).Encode(this._rangeEncoder, curByte);
                this._previousByte = curByte;
                this._additionalOffset--;
                this.nowPos64++;
            }
            if (this._matchFinder.GetNumAvailableBytes() == 0) {
                this.Flush((UInt32)this.nowPos64);
                return;
            }
            while (true) {
                UInt32 pos;
                var len = this.GetOptimum((UInt32)this.nowPos64, out pos);

                var posState = (UInt32)this.nowPos64 & this._posStateMask;
                var complexState = (this._state.Index << Base.kNumPosStatesBitsMax) + posState;
                if (len == 1 && pos == 0xFFFFFFFF) {
                    this._isMatch[complexState].Encode(this._rangeEncoder, 0);
                    var curByte = this._matchFinder.GetIndexByte((Int32)(0 - this._additionalOffset));
                    var subCoder = this._literalEncoder.GetSubCoder((UInt32)this.nowPos64, this._previousByte);
                    if (!this._state.IsCharState()) {
                        var matchByte = this._matchFinder.GetIndexByte((Int32)(0 - this._repDistances[0] - 1 - this._additionalOffset));
                        subCoder.EncodeMatched(this._rangeEncoder, matchByte, curByte);
                    } else
                        subCoder.Encode(this._rangeEncoder, curByte);
                    this._previousByte = curByte;
                    this._state.UpdateChar();
                } else {
                    this._isMatch[complexState].Encode(this._rangeEncoder, 1);
                    if (pos < Base.kNumRepDistances) {
                        this._isRep[this._state.Index].Encode(this._rangeEncoder, 1);
                        if (pos == 0) {
                            this._isRepG0[this._state.Index].Encode(this._rangeEncoder, 0);
                            if (len == 1)
                                this._isRep0Long[complexState].Encode(this._rangeEncoder, 0);
                            else
                                this._isRep0Long[complexState].Encode(this._rangeEncoder, 1);
                        } else {
                            this._isRepG0[this._state.Index].Encode(this._rangeEncoder, 1);
                            if (pos == 1)
                                this._isRepG1[this._state.Index].Encode(this._rangeEncoder, 0);
                            else {
                                this._isRepG1[this._state.Index].Encode(this._rangeEncoder, 1);
                                this._isRepG2[this._state.Index].Encode(this._rangeEncoder, pos - 2);
                            }
                        }
                        if (len == 1)
                            this._state.UpdateShortRep();
                        else {
                            this._repMatchLenEncoder.Encode(this._rangeEncoder, len - Base.kMatchMinLen, posState);
                            this._state.UpdateRep();
                        }
                        var distance = this._repDistances[pos];
                        if (pos != 0) {
                            for (var i = pos; i >= 1; i--)
                                this._repDistances[i] = this._repDistances[i - 1];
                            this._repDistances[0] = distance;
                        }
                    } else {
                        this._isRep[this._state.Index].Encode(this._rangeEncoder, 0);
                        this._state.UpdateMatch();
                        this._lenEncoder.Encode(this._rangeEncoder, len - Base.kMatchMinLen, posState);
                        pos -= Base.kNumRepDistances;
                        var posSlot = GetPosSlot(pos);
                        var lenToPosState = Base.GetLenToPosState(len);
                        this._posSlotEncoder[lenToPosState].Encode(this._rangeEncoder, posSlot);

                        if (posSlot >= Base.kStartPosModelIndex) {
                            var footerBits = (int)((posSlot >> 1) - 1);
                            var baseVal = (2 | (posSlot & 1)) << footerBits;
                            var posReduced = pos - baseVal;

                            if (posSlot < Base.kEndPosModelIndex)
                                BitTreeEncoder.ReverseEncode(this._posEncoders,
                                        baseVal - posSlot - 1, this._rangeEncoder, footerBits, posReduced);
                            else {
                                this._rangeEncoder.EncodeDirectBits(posReduced >> Base.kNumAlignBits, footerBits - Base.kNumAlignBits);
                                this._posAlignEncoder.ReverseEncode(this._rangeEncoder, posReduced & Base.kAlignMask);
                                this._alignPriceCount++;
                            }
                        }
                        var distance = pos;
                        for (var i = Base.kNumRepDistances - 1; i >= 1; i--)
                            this._repDistances[i] = this._repDistances[i - 1];
                        this._repDistances[0] = distance;
                        this._matchPriceCount++;
                    }
                    this._previousByte = this._matchFinder.GetIndexByte((Int32)(len - 1 - this._additionalOffset));
                }
                this._additionalOffset -= len;
                this.nowPos64 += len;
                if (this._additionalOffset == 0) {
                    // if (!_fastMode)
                    if (this._matchPriceCount >= 1 << 7)
                        this.FillDistancesPrices();
                    if (this._alignPriceCount >= Base.kAlignTableSize)
                        this.FillAlignPrices();
                    inSize = this.nowPos64;
                    outSize = this._rangeEncoder.GetProcessedSizeAdd();
                    if (this._matchFinder.GetNumAvailableBytes() == 0) {
                        this.Flush((UInt32)this.nowPos64);
                        return;
                    }

                    if (this.nowPos64 - progressPosValuePrev >= 1 << 12) {
                        this._finished = false;
                        finished = false;
                        return;
                    }
                }
            }
        }

        void ReleaseMFStream() {
            if (this._matchFinder != null && this._needReleaseMFStream) {
                this._matchFinder.ReleaseStream();
                this._needReleaseMFStream = false;
            }
        }

        void SetOutStream(Stream outStream) { this._rangeEncoder.SetStream(outStream); }
        void ReleaseOutStream() { this._rangeEncoder.ReleaseStream(); }

        void ReleaseStreams() {
            this.ReleaseMFStream();
            this.ReleaseOutStream();
        }

        void SetStreams(Stream inStream, Stream outStream,
                Int64 inSize, Int64 outSize) {
            this._inStream = inStream;
            this._finished = false;
            this.Create();
            this.SetOutStream(outStream);
            this.Init();

            // if (!_fastMode)
            {
                this.FillDistancesPrices();
                this.FillAlignPrices();
            }

            this._lenEncoder.SetTableSize(this._numFastBytes + 1 - Base.kMatchMinLen);
            this._lenEncoder.UpdateTables((UInt32)1 << this._posStateBits);
            this._repMatchLenEncoder.SetTableSize(this._numFastBytes + 1 - Base.kMatchMinLen);
            this._repMatchLenEncoder.UpdateTables((UInt32)1 << this._posStateBits);

            this.nowPos64 = 0;
        }


        public void Code(Stream inStream, Stream outStream,
            Int64 inSize, Int64 outSize, ICodeProgress progress) {
            this._needReleaseMFStream = false;
            try {
                this.SetStreams(inStream, outStream, inSize, outSize);
                while (true) {
                    Int64 processedInSize;
                    Int64 processedOutSize;
                    bool finished;
                    this.CodeOneBlock(out processedInSize, out processedOutSize, out finished);
                    if (finished)
                        return;
                    if (progress != null) {
                        progress.SetProgress(processedInSize, processedOutSize);
                    }
                }
            } finally {
                this.ReleaseStreams();
            }
        }

        const int kPropSize = 5;
        Byte[] properties = new Byte[kPropSize];

        public void WriteCoderProperties(Stream outStream) {
            this.properties[0] = (Byte)((this._posStateBits * 5 + this._numLiteralPosStateBits) * 9 + this._numLiteralContextBits);
            for (var i = 0; i < 4; i++)
                this.properties[1 + i] = (Byte)((this._dictionarySize >> (8 * i)) & 0xFF);
            outStream.Write(this.properties, 0, kPropSize);
        }

        UInt32[] tempPrices = new UInt32[Base.kNumFullDistances];
        UInt32 _matchPriceCount;

        void FillDistancesPrices() {
            for (var i = Base.kStartPosModelIndex; i < Base.kNumFullDistances; i++) {
                var posSlot = GetPosSlot(i);
                var footerBits = (int)((posSlot >> 1) - 1);
                var baseVal = (2 | (posSlot & 1)) << footerBits;
                this.tempPrices[i] = BitTreeEncoder.ReverseGetPrice(this._posEncoders,
                    baseVal - posSlot - 1, footerBits, i - baseVal);
            }

            for (UInt32 lenToPosState = 0; lenToPosState < Base.kNumLenToPosStates; lenToPosState++) {
                UInt32 posSlot;
                var encoder = this._posSlotEncoder[lenToPosState];

                var st = lenToPosState << Base.kNumPosSlotBits;
                for (posSlot = 0; posSlot < this._distTableSize; posSlot++)
                    this._posSlotPrices[st + posSlot] = encoder.GetPrice(posSlot);
                for (posSlot = Base.kEndPosModelIndex; posSlot < this._distTableSize; posSlot++)
                    this._posSlotPrices[st + posSlot] += ((posSlot >> 1) - 1 - Base.kNumAlignBits) << BitEncoder.kNumBitPriceShiftBits;

                var st2 = lenToPosState * Base.kNumFullDistances;
                UInt32 i;
                for (i = 0; i < Base.kStartPosModelIndex; i++)
                    this._distancesPrices[st2 + i] = this._posSlotPrices[st + i];
                for (; i < Base.kNumFullDistances; i++)
                    this._distancesPrices[st2 + i] = this._posSlotPrices[st + GetPosSlot(i)] + this.tempPrices[i];
            }
            this._matchPriceCount = 0;
        }

        void FillAlignPrices() {
            for (UInt32 i = 0; i < Base.kAlignTableSize; i++)
                this._alignPrices[i] = this._posAlignEncoder.ReverseGetPrice(i);
            this._alignPriceCount = 0;
        }


        static string[] kMatchFinderIDs =
        {
            "BT2",
            "BT4",
        };

        static int FindMatchFinder(string s) {
            for (var m = 0; m < kMatchFinderIDs.Length; m++)
                if (s == kMatchFinderIDs[m])
                    return m;
            return -1;
        }

        public void SetCoderProperties(CoderPropID[] propIDs, object[] properties) {
            for (UInt32 i = 0; i < properties.Length; i++) {
                var prop = properties[i];
                switch (propIDs[i]) {
                    case CoderPropID.NumFastBytes: {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        var numFastBytes = (Int32)prop;
                        if (numFastBytes < 5 || numFastBytes > Base.kMatchMaxLen)
                            throw new InvalidParamException();
                        this._numFastBytes = (UInt32)numFastBytes;
                        break;
                    }
                    case CoderPropID.Algorithm: {
                        /*
						if (!(prop is Int32))
							throw new InvalidParamException();
						Int32 maximize = (Int32)prop;
						_fastMode = (maximize == 0);
						_maxMode = (maximize >= 2);
						*/
                        break;
                    }
                    case CoderPropID.MatchFinder: {
                        if (!(prop is String))
                            throw new InvalidParamException();
                        var matchFinderIndexPrev = this._matchFinderType;
                        var m = FindMatchFinder(((string)prop).ToUpper());
                        if (m < 0)
                            throw new InvalidParamException();
                        this._matchFinderType = (EMatchFinderType)m;
                        if (this._matchFinder != null && matchFinderIndexPrev != this._matchFinderType) {
                            this._dictionarySizePrev = 0xFFFFFFFF;
                            this._matchFinder = null;
                        }
                        break;
                    }
                    case CoderPropID.DictionarySize: {
                        const int kDicLogSizeMaxCompress = 30;
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        ;
                        var dictionarySize = (Int32)prop;
                        if (dictionarySize < (UInt32)(1 << Base.kDicLogSizeMin) ||
                            dictionarySize > (UInt32)(1 << kDicLogSizeMaxCompress))
                            throw new InvalidParamException();
                        this._dictionarySize = (UInt32)dictionarySize;
                        int dicLogSize;
                        for (dicLogSize = 0; dicLogSize < (UInt32)kDicLogSizeMaxCompress; dicLogSize++)
                            if (dictionarySize <= (UInt32)1 << dicLogSize)
                                break;
                        this._distTableSize = (UInt32)dicLogSize * 2;
                        break;
                    }
                    case CoderPropID.PosStateBits: {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        var v = (Int32)prop;
                        if (v < 0 || v > (UInt32)Base.kNumPosStatesBitsEncodingMax)
                            throw new InvalidParamException();
                        this._posStateBits = v;
                        this._posStateMask = ((UInt32)1 << this._posStateBits) - 1;
                        break;
                    }
                    case CoderPropID.LitPosBits: {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        var v = (Int32)prop;
                        if (v < 0 || v > Base.kNumLitPosStatesBitsEncodingMax)
                            throw new InvalidParamException();
                        this._numLiteralPosStateBits = v;
                        break;
                    }
                    case CoderPropID.LitContextBits: {
                        if (!(prop is Int32))
                            throw new InvalidParamException();
                        var v = (Int32)prop;
                        if (v < 0 || v > Base.kNumLitContextBitsMax)
                            throw new InvalidParamException();
                        ;
                        this._numLiteralContextBits = v;
                        break;
                    }
                    case CoderPropID.EndMarker: {
                        if (!(prop is Boolean))
                            throw new InvalidParamException();
                        this.SetWriteEndMarkerMode((Boolean)prop);
                        break;
                    }
                    default:
                        throw new InvalidParamException();
                }
            }
        }

        uint _trainSize;
        public void SetTrainSize(uint trainSize) {
            this._trainSize = trainSize;
        }

    }
}
