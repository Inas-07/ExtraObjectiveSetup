using AmorLib.Utils;
using ExtraObjectiveSetup.Utils;
using BepInEx.Unity.IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using LevelGeneration;
using GTFO.API;

namespace ExtraObjectiveSetup.Tweaks.TerminalTweak
{
    public static partial class SerialGeneratorManager
    {
        class ShuffledStepArray
        {
            private readonly string[] _values;
            private readonly int[] _order;
            private int _step = -1;

            public ShuffledStepArray(IEnumerable<string> values)
            {
                _values = values.Distinct().ToArray();
                _order = Enumerable.Range(0, _values.Length).ToArray();
                for (int i = _order.Length - 1; i > 0; i--)
                {
                    int j = _random.Next(0, i + 1);
                    (_order[i], _order[j]) = (_order[j], _order[i]);
                }
            }

            public string Next()
            {
                _step++;
                if (_step >= _order.Length) _step = 0; 
                return _values[_order[_step]];
            }
        }

        public const string AUTOGEN_GUID = "000-the_tavern-AutogenRundown";
        public const string LONGERCODES_GUID = "com.Brandont.LongerCodes";

        private static System.Random _random = null!;
        private static readonly bool _hasPluginConflict;
        private static readonly Dictionary<CodeWordLength, ShuffledStepArray> _codeWords = new();
        private static ShuffledStepArray _hardPrefixes = null!;

        static SerialGeneratorManager()
        {
            _hasPluginConflict = IL2CPPChainloader.Instance.Plugins.ContainsKey(AUTOGEN_GUID) || IL2CPPChainloader.Instance.Plugins.ContainsKey(LONGERCODES_GUID);

            if (_hasPluginConflict)
                EOSLogger.Warning($"Conflicting plugin: \"{AUTOGEN_GUID}\" and/or \"{LONGERCODES_GUID}\". Using default SerialGenerator settings");
        }

        private static void OnBuildStart()
        {
            _random = RandomUtil.CreateSessionRandom("EOS_SerialGenerator");
            _codeWords.Clear();
            _codeWords[CodeWordLength.Three] = new(ThreeLetterWords);
            _codeWords[CodeWordLength.Five] = new(FiveLetterWords);
            _codeWords[CodeWordLength.Six] = new(SixLetterWords);
            _codeWords[CodeWordLength.Seven] = new(SevenLetterWords);
            _hardPrefixes = new(HardCodeWordPrefixes);
        }

        public static void Init()
        {
            LevelAPI.OnBuildStart += OnBuildStart;
        }

        public static int GetUniqueSerialNo() => SerialGenerator.GetUniqueSerialNo();

        public static string GetIPAddress(bool useIPv6)
        {
            if (_hasPluginConflict || !useIPv6)
                return SerialGenerator.GetIpAddress();

            string[] codes = new string[] { RandomHex(0x100, 0x1200), RandomHex(0x1000, 0xFFFF), RandomHex(0, 0x1FF), RandomHex(0, 0x3FFF) };
            int num = codes.Length;
            while (num > 1)
            {
                int num2 = _random.Next(0, num--);
                (codes[num2], codes[num]) = (codes[num], codes[num2]);
            }
            return $"{RandomHex(0x2000, 0x3FFF)}::{string.Join(':', codes)}";
        }

        private static string RandomHex(int min, int max)
        {
            return _random.Next(min, max).ToString("x");
        }

        public static string GetCodeWord(CodeWordLength wordLength)
        {
            if (_hasPluginConflict || !_codeWords.TryGetValue(wordLength, out var stepArr))
                return SerialGenerator.GetCodeWord();
            return stepArr.Next();
        }

        public static string GetCodeWordPrefix(bool useHardPrefixes, bool hyphenate = false)
        {
            string prefix;
            if (_hasPluginConflict || !useHardPrefixes) 
                prefix = SerialGenerator.GetCodeWordPrefix();
            else
                prefix = _hardPrefixes.Next();
            return hyphenate ? prefix.Insert(1, "-") : prefix;
        }

        public static void RerollCorrectIndex(TerminalUplinkPuzzle uplinkPuzzle, int retryCount)
        {
            for (int i = 0; i < retryCount; i++)
            {
                foreach (var uplinkRound in uplinkPuzzle.m_rounds)
                {
                    uplinkRound.CorrectIndex = _random.Next(0, uplinkRound.Codes.Length);
                }
            }
        }
    }
}
