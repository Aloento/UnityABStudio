// CommandLineParser.cs

namespace SoarCraft.QYun.AssetReader.SevenZip.Common {
    using System;
    using System.Collections;

    public enum SwitchType {
        Simple,
        PostMinus,
        LimitedPostString,
        UnLimitedPostString,
        PostChar
    }

    public class SwitchForm {
        public string IDString;
        public SwitchType Type;
        public bool Multi;
        public int MinLen;
        public int MaxLen;
        public string PostCharSet;

        public SwitchForm(string idString, SwitchType type, bool multi,
            int minLen, int maxLen, string postCharSet) {
            this.IDString = idString;
            this.Type = type;
            this.Multi = multi;
            this.MinLen = minLen;
            this.MaxLen = maxLen;
            this.PostCharSet = postCharSet;
        }
        public SwitchForm(string idString, SwitchType type, bool multi, int minLen) :
            this(idString, type, multi, minLen, 0, "") {
        }
        public SwitchForm(string idString, SwitchType type, bool multi) :
            this(idString, type, multi, 0) {
        }
    }

    public class SwitchResult {
        public bool ThereIs;
        public bool WithMinus;
        public ArrayList PostStrings = new();
        public int PostCharIndex;
        public SwitchResult() {
            this.ThereIs = false;
        }
    }

    public class Parser {
        public ArrayList NonSwitchStrings = new();
        SwitchResult[] _switches;

        public Parser(int numSwitches) {
            this._switches = new SwitchResult[numSwitches];
            for (var i = 0; i < numSwitches; i++)
                this._switches[i] = new SwitchResult();
        }

        bool ParseString(string srcString, SwitchForm[] switchForms) {
            var len = srcString.Length;
            if (len == 0)
                return false;
            var pos = 0;
            if (!IsItSwitchChar(srcString[pos]))
                return false;
            while (pos < len) {
                if (IsItSwitchChar(srcString[pos]))
                    pos++;
                const int kNoLen = -1;
                var matchedSwitchIndex = 0;
                var maxLen = kNoLen;
                for (var switchIndex = 0; switchIndex < this._switches.Length; switchIndex++) {
                    var switchLen = switchForms[switchIndex].IDString.Length;
                    if (switchLen <= maxLen || pos + switchLen > len)
                        continue;
                    if (string.Compare(switchForms[switchIndex].IDString, 0,
                            srcString, pos, switchLen, true) == 0) {
                        matchedSwitchIndex = switchIndex;
                        maxLen = switchLen;
                    }
                }
                if (maxLen == kNoLen)
                    throw new Exception("maxLen == kNoLen");
                var matchedSwitch = this._switches[matchedSwitchIndex];
                var switchForm = switchForms[matchedSwitchIndex];
                if (!switchForm.Multi && matchedSwitch.ThereIs)
                    throw new Exception("switch must be single");
                matchedSwitch.ThereIs = true;
                pos += maxLen;
                var tailSize = len - pos;
                var type = switchForm.Type;
                switch (type) {
                    case SwitchType.PostMinus: {
                        if (tailSize == 0)
                            matchedSwitch.WithMinus = false;
                        else {
                            matchedSwitch.WithMinus = srcString[pos] == kSwitchMinus;
                            if (matchedSwitch.WithMinus)
                                pos++;
                        }
                        break;
                    }
                    case SwitchType.PostChar: {
                        if (tailSize < switchForm.MinLen)
                            throw new Exception("switch is not full");
                        var charSet = switchForm.PostCharSet;
                        const int kEmptyCharValue = -1;
                        if (tailSize == 0)
                            matchedSwitch.PostCharIndex = kEmptyCharValue;
                        else {
                            var index = charSet.IndexOf(srcString[pos]);
                            if (index < 0)
                                matchedSwitch.PostCharIndex = kEmptyCharValue;
                            else {
                                matchedSwitch.PostCharIndex = index;
                                pos++;
                            }
                        }
                        break;
                    }
                    case SwitchType.LimitedPostString:
                    case SwitchType.UnLimitedPostString: {
                        var minLen = switchForm.MinLen;
                        if (tailSize < minLen)
                            throw new Exception("switch is not full");
                        if (type == SwitchType.UnLimitedPostString) {
                            _ = matchedSwitch.PostStrings.Add(srcString[pos..]);
                            return true;
                        }
                        var stringSwitch = srcString.Substring(pos, minLen);
                        pos += minLen;
                        for (var i = minLen; i < switchForm.MaxLen && pos < len; i++, pos++) {
                            var c = srcString[pos];
                            if (IsItSwitchChar(c))
                                break;
                            stringSwitch += c;
                        }
                        _ = matchedSwitch.PostStrings.Add(stringSwitch);
                        break;
                    }
                }
            }
            return true;

        }

        public void ParseStrings(SwitchForm[] switchForms, string[] commandStrings) {
            var numCommandStrings = commandStrings.Length;
            var stopSwitch = false;
            for (var i = 0; i < numCommandStrings; i++) {
                var s = commandStrings[i];
                if (stopSwitch)
                    _ = this.NonSwitchStrings.Add(s);
                else
                    if (s == kStopSwitchParsing)
                    stopSwitch = true;
                else
                    if (!this.ParseString(s, switchForms))
                    _ = this.NonSwitchStrings.Add(s);
            }
        }

        public SwitchResult this[int index] { get { return this._switches[index]; } }

        public static int ParseCommand(CommandForm[] commandForms, string commandString,
            out string postString) {
            for (var i = 0; i < commandForms.Length; i++) {
                var id = commandForms[i].IDString;
                if (commandForms[i].PostStringMode) {
                    if (commandString.IndexOf(id) == 0) {
                        postString = commandString[id.Length..];
                        return i;
                    }
                } else
                    if (commandString == id) {
                    postString = "";
                    return i;
                }
            }
            postString = "";
            return -1;
        }

        static bool ParseSubCharsCommand(int numForms, CommandSubCharsSet[] forms,
            string commandString, ArrayList indices) {
            indices.Clear();
            var numUsedChars = 0;
            for (var i = 0; i < numForms; i++) {
                var charsSet = forms[i];
                var currentIndex = -1;
                var len = charsSet.Chars.Length;
                for (var j = 0; j < len; j++) {
                    var c = charsSet.Chars[j];
                    var newIndex = commandString.IndexOf(c);
                    if (newIndex >= 0) {
                        if (currentIndex >= 0)
                            return false;
                        if (commandString.IndexOf(c, newIndex + 1) >= 0)
                            return false;
                        currentIndex = j;
                        numUsedChars++;
                    }
                }
                if (currentIndex == -1 && !charsSet.EmptyAllowed)
                    return false;
                _ = indices.Add(currentIndex);
            }
            return numUsedChars == commandString.Length;
        }
        const char kSwitchID1 = '-';
        const char kSwitchID2 = '/';

        const char kSwitchMinus = '-';
        const string kStopSwitchParsing = "--";

        static bool IsItSwitchChar(char c) {
            return c is kSwitchID1 or kSwitchID2;
        }
    }

    public class CommandForm {
        public string IDString = "";
        public bool PostStringMode;
        public CommandForm(string idString, bool postStringMode) {
            this.IDString = idString;
            this.PostStringMode = postStringMode;
        }
    }

    class CommandSubCharsSet {
        public string Chars = "";
        public bool EmptyAllowed = false;
    }
}
