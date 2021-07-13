// LzBinTree.cs

namespace SoarCraft.QYun.AssetReader._7zip.Compress.LZ {
    using System;
    using Common;

    public class BinTree : InWindow, IMatchFinder {
        UInt32 _cyclicBufferPos;
        UInt32 _cyclicBufferSize = 0;
        UInt32 _matchMaxLen;

        UInt32[] _son;
        UInt32[] _hash;

        UInt32 _cutValue = 0xFF;
        UInt32 _hashMask;
        UInt32 _hashSizeSum = 0;

        bool HASH_ARRAY = true;

        const UInt32 kHash2Size = 1 << 10;
        const UInt32 kHash3Size = 1 << 16;
        const UInt32 kBT2HashSize = 1 << 16;
        const UInt32 kStartMaxLen = 1;
        const UInt32 kHash3Offset = kHash2Size;
        const UInt32 kEmptyHashValue = 0;
        const UInt32 kMaxValForNormalize = ((UInt32)1 << 31) - 1;

        UInt32 kNumHashDirectBytes = 0;
        UInt32 kMinMatchCheck = 4;
        UInt32 kFixHashSize = kHash2Size + kHash3Size;

        public void SetType(int numHashBytes) {
            this.HASH_ARRAY = numHashBytes > 2;
            if (this.HASH_ARRAY) {
                this.kNumHashDirectBytes = 0;
                this.kMinMatchCheck = 4;
                this.kFixHashSize = kHash2Size + kHash3Size;
            } else {
                this.kNumHashDirectBytes = 2;
                this.kMinMatchCheck = 2 + 1;
                this.kFixHashSize = 0;
            }
        }

        public new void SetStream(System.IO.Stream stream) { base.SetStream(stream); }
        public new void ReleaseStream() { base.ReleaseStream(); }

        public new void Init() {
            base.Init();
            for (UInt32 i = 0; i < this._hashSizeSum; i++)
                this._hash[i] = kEmptyHashValue;
            this._cyclicBufferPos = 0;
            this.ReduceOffsets(-1);
        }

        public new void MovePos() {
            if (++this._cyclicBufferPos >= this._cyclicBufferSize)
                this._cyclicBufferPos = 0;
            base.MovePos();
            if (this._pos == kMaxValForNormalize)
                this.Normalize();
        }

        public new Byte GetIndexByte(Int32 index) { return base.GetIndexByte(index); }

        public new UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit) { return base.GetMatchLen(index, distance, limit); }

        public new UInt32 GetNumAvailableBytes() { return base.GetNumAvailableBytes(); }

        public void Create(UInt32 historySize, UInt32 keepAddBufferBefore,
                UInt32 matchMaxLen, UInt32 keepAddBufferAfter) {
            if (historySize > kMaxValForNormalize - 256)
                throw new Exception();
            this._cutValue = 16 + (matchMaxLen >> 1);

            var windowReservSize = (historySize + keepAddBufferBefore +
                    matchMaxLen + keepAddBufferAfter) / 2 + 256;

            base.Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

            this._matchMaxLen = matchMaxLen;

            var cyclicBufferSize = historySize + 1;
            if (this._cyclicBufferSize != cyclicBufferSize)
                this._son = new UInt32[(this._cyclicBufferSize = cyclicBufferSize) * 2];

            var hs = kBT2HashSize;

            if (this.HASH_ARRAY) {
                hs = historySize - 1;
                hs |= hs >> 1;
                hs |= hs >> 2;
                hs |= hs >> 4;
                hs |= hs >> 8;
                hs >>= 1;
                hs |= 0xFFFF;
                if (hs > 1 << 24)
                    hs >>= 1;
                this._hashMask = hs;
                hs++;
                hs += this.kFixHashSize;
            }
            if (hs != this._hashSizeSum)
                this._hash = new UInt32[this._hashSizeSum = hs];
        }

        public UInt32 GetMatches(UInt32[] distances) {
            UInt32 lenLimit;
            if (this._pos + this._matchMaxLen <= this._streamPos)
                lenLimit = this._matchMaxLen;
            else {
                lenLimit = this._streamPos - this._pos;
                if (lenLimit < this.kMinMatchCheck) {
                    this.MovePos();
                    return 0;
                }
            }

            UInt32 offset = 0;
            var matchMinPos = this._pos > this._cyclicBufferSize ? this._pos - this._cyclicBufferSize : 0;
            var cur = this._bufferOffset + this._pos;
            var maxLen = kStartMaxLen; // to avoid items for len < hashSize;
            UInt32 hashValue, hash2Value = 0, hash3Value = 0;

            if (this.HASH_ARRAY) {
                var temp = CRC.Table[this._bufferBase[cur]] ^ this._bufferBase[cur + 1];
                hash2Value = temp & (kHash2Size - 1);
                temp ^= (UInt32)this._bufferBase[cur + 2] << 8;
                hash3Value = temp & (kHash3Size - 1);
                hashValue = (temp ^ (CRC.Table[this._bufferBase[cur + 3]] << 5)) & this._hashMask;
            } else
                hashValue = this._bufferBase[cur] ^ ((UInt32)this._bufferBase[cur + 1] << 8);

            var curMatch = this._hash[this.kFixHashSize + hashValue];
            if (this.HASH_ARRAY) {
                var curMatch2 = this._hash[hash2Value];
                var curMatch3 = this._hash[kHash3Offset + hash3Value];
                this._hash[hash2Value] = this._pos;
                this._hash[kHash3Offset + hash3Value] = this._pos;
                if (curMatch2 > matchMinPos)
                    if (this._bufferBase[this._bufferOffset + curMatch2] == this._bufferBase[cur]) {
                        distances[offset++] = maxLen = 2;
                        distances[offset++] = this._pos - curMatch2 - 1;
                    }
                if (curMatch3 > matchMinPos)
                    if (this._bufferBase[this._bufferOffset + curMatch3] == this._bufferBase[cur]) {
                        if (curMatch3 == curMatch2)
                            offset -= 2;
                        distances[offset++] = maxLen = 3;
                        distances[offset++] = this._pos - curMatch3 - 1;
                        curMatch2 = curMatch3;
                    }
                if (offset != 0 && curMatch2 == curMatch) {
                    offset -= 2;
                    maxLen = kStartMaxLen;
                }
            }

            this._hash[this.kFixHashSize + hashValue] = this._pos;

            var ptr0 = (this._cyclicBufferPos << 1) + 1;
            var ptr1 = this._cyclicBufferPos << 1;

            UInt32 len0, len1;
            len0 = len1 = this.kNumHashDirectBytes;

            if (this.kNumHashDirectBytes != 0) {
                if (curMatch > matchMinPos) {
                    if (this._bufferBase[this._bufferOffset + curMatch + this.kNumHashDirectBytes] !=
                            this._bufferBase[cur + this.kNumHashDirectBytes]) {
                        distances[offset++] = maxLen = this.kNumHashDirectBytes;
                        distances[offset++] = this._pos - curMatch - 1;
                    }
                }
            }

            var count = this._cutValue;

            while (true) {
                if (curMatch <= matchMinPos || count-- == 0) {
                    this._son[ptr0] = this._son[ptr1] = kEmptyHashValue;
                    break;
                }
                var delta = this._pos - curMatch;
                var cyclicPos = (delta <= this._cyclicBufferPos ?
                            this._cyclicBufferPos - delta :
                            this._cyclicBufferPos - delta + this._cyclicBufferSize) << 1;

                var pby1 = this._bufferOffset + curMatch;
                var len = Math.Min(len0, len1);
                if (this._bufferBase[pby1 + len] == this._bufferBase[cur + len]) {
                    while (++len != lenLimit)
                        if (this._bufferBase[pby1 + len] != this._bufferBase[cur + len])
                            break;
                    if (maxLen < len) {
                        distances[offset++] = maxLen = len;
                        distances[offset++] = delta - 1;
                        if (len == lenLimit) {
                            this._son[ptr1] = this._son[cyclicPos];
                            this._son[ptr0] = this._son[cyclicPos + 1];
                            break;
                        }
                    }
                }
                if (this._bufferBase[pby1 + len] < this._bufferBase[cur + len]) {
                    this._son[ptr1] = curMatch;
                    ptr1 = cyclicPos + 1;
                    curMatch = this._son[ptr1];
                    len1 = len;
                } else {
                    this._son[ptr0] = curMatch;
                    ptr0 = cyclicPos;
                    curMatch = this._son[ptr0];
                    len0 = len;
                }
            }
            this.MovePos();
            return offset;
        }

        public void Skip(UInt32 num) {
            do {
                UInt32 lenLimit;
                if (this._pos + this._matchMaxLen <= this._streamPos)
                    lenLimit = this._matchMaxLen;
                else {
                    lenLimit = this._streamPos - this._pos;
                    if (lenLimit < this.kMinMatchCheck) {
                        this.MovePos();
                        continue;
                    }
                }

                var matchMinPos = this._pos > this._cyclicBufferSize ? this._pos - this._cyclicBufferSize : 0;
                var cur = this._bufferOffset + this._pos;

                UInt32 hashValue;

                if (this.HASH_ARRAY) {
                    var temp = CRC.Table[this._bufferBase[cur]] ^ this._bufferBase[cur + 1];
                    var hash2Value = temp & (kHash2Size - 1);
                    this._hash[hash2Value] = this._pos;
                    temp ^= (UInt32)this._bufferBase[cur + 2] << 8;
                    var hash3Value = temp & (kHash3Size - 1);
                    this._hash[kHash3Offset + hash3Value] = this._pos;
                    hashValue = (temp ^ (CRC.Table[this._bufferBase[cur + 3]] << 5)) & this._hashMask;
                } else
                    hashValue = this._bufferBase[cur] ^ ((UInt32)this._bufferBase[cur + 1] << 8);

                var curMatch = this._hash[this.kFixHashSize + hashValue];
                this._hash[this.kFixHashSize + hashValue] = this._pos;

                var ptr0 = (this._cyclicBufferPos << 1) + 1;
                var ptr1 = this._cyclicBufferPos << 1;

                UInt32 len0, len1;
                len0 = len1 = this.kNumHashDirectBytes;

                var count = this._cutValue;
                while (true) {
                    if (curMatch <= matchMinPos || count-- == 0) {
                        this._son[ptr0] = this._son[ptr1] = kEmptyHashValue;
                        break;
                    }

                    var delta = this._pos - curMatch;
                    var cyclicPos = (delta <= this._cyclicBufferPos ?
                                this._cyclicBufferPos - delta :
                                this._cyclicBufferPos - delta + this._cyclicBufferSize) << 1;

                    var pby1 = this._bufferOffset + curMatch;
                    var len = Math.Min(len0, len1);
                    if (this._bufferBase[pby1 + len] == this._bufferBase[cur + len]) {
                        while (++len != lenLimit)
                            if (this._bufferBase[pby1 + len] != this._bufferBase[cur + len])
                                break;
                        if (len == lenLimit) {
                            this._son[ptr1] = this._son[cyclicPos];
                            this._son[ptr0] = this._son[cyclicPos + 1];
                            break;
                        }
                    }
                    if (this._bufferBase[pby1 + len] < this._bufferBase[cur + len]) {
                        this._son[ptr1] = curMatch;
                        ptr1 = cyclicPos + 1;
                        curMatch = this._son[ptr1];
                        len1 = len;
                    } else {
                        this._son[ptr0] = curMatch;
                        ptr0 = cyclicPos;
                        curMatch = this._son[ptr0];
                        len0 = len;
                    }
                }
                this.MovePos();
            }
            while (--num != 0);
        }

        void NormalizeLinks(UInt32[] items, UInt32 numItems, UInt32 subValue) {
            for (UInt32 i = 0; i < numItems; i++) {
                var value = items[i];
                if (value <= subValue)
                    value = kEmptyHashValue;
                else
                    value -= subValue;
                items[i] = value;
            }
        }

        void Normalize() {
            var subValue = this._pos - this._cyclicBufferSize;
            this.NormalizeLinks(this._son, this._cyclicBufferSize * 2, subValue);
            this.NormalizeLinks(this._hash, this._hashSizeSum, subValue);
            this.ReduceOffsets((Int32)subValue);
        }

        public void SetCutValue(UInt32 cutValue) { this._cutValue = cutValue; }
    }
}
