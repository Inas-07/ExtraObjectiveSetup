using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition;
using ExtraObjectiveSetup.Expedition;
using ExtraObjectiveSetup.Tweaks.TerminalTweak;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using System.Collections.Generic;
using System.Linq;

namespace ExtraObjectiveSetup.Patches.Uplink
{
    /// <summary>
    /// Vanilla lays the uplink codes out for exactly 6 candidates of 4 letters each, and builds the
    /// password of linker-job terminals with the vanilla word length. Both are rewritten here, and
    /// both are skipped together when Autogen or LongerCodes is installed.
    /// </summary>
    [HarmonyPatch]
    internal static class UplinkPuzzleCodesString
    {
        // Autogen and LongerCodes replace the same generator, leave their layout alone
        [HarmonyPrepare]
        private static bool PreparePatches()
        {
            var plugins = IL2CPPChainloader.Instance.Plugins;
            return !plugins.ContainsKey(SerialGeneratorManager.AUTOGEN_GUID)
                && !plugins.ContainsKey(SerialGeneratorManager.LONGERCODES_GUID);
        }

        [HarmonyPatch(typeof(TerminalUplinkPuzzle), nameof(TerminalUplinkPuzzle.GetCodesString))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyWrapSafe]
        public static bool TerminalUplinkPuzzle_GetCodesString(ref string __result, TerminalUplinkPuzzleRound round, bool newLine = false) // newLine = isLogfile
        {
            int candidatesCount = round.Codes.Count;
            int wordLength = round.Codes[0].Length;
            bool customWord = candidatesCount > 6 || wordLength > 4;
            bool hyphenated = round.Prefixes[0].Contains('-');
            string delimiter = hyphenated ? " | " : " - ";
            string text = customWord && !newLine ? "\n" : string.Empty;

            int perLine = 3;
            int perLog = 3;
            if (customWord)
            {
                int entryLength = round.Prefixes[0].Length + 1 + wordLength;
                int maxPerLine = (68 + 3) / (entryLength + 3);
                int maxPerLog = (42 + 2) / (entryLength + 2);
                perLine = FitEntries(candidatesCount, maxPerLine);
                perLog = FitEntries(candidatesCount, maxPerLog);
            }

            for (int index = 0; index < round.Codes.Length; index++)
            {
                text += $"<color=orange>{round.Prefixes[index]}</color>:{round.Codes[index]}";
                if (index >= round.Codes.Length - 1)
                    continue;

                if (newLine)
                    text += ((index + 1) % perLog) != 0 ? "  " : "\n";
                else
                    text += ((index + 1) % perLine) != 0 ? delimiter : "\n";
            }

            __result = text;
            return false;
        }

        private static int FitEntries(int count, int max)
        {
            if (max < 3)
                return System.Math.Max(1, max);

            for (int n = System.Math.Min(max, count - 1); n > 3; n--)
                if (count % n == 0)
                    return n;

            return 3;
        }

        [HarmonyPatch(typeof(LG_TerminalPasswordLinkerJob), nameof(LG_TerminalPasswordLinkerJob.Build))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        private static bool Pre_TerminalPasswordLinkerBuild(LG_TerminalPasswordLinkerJob __instance, ref bool __result)
        {
            if (!ExpeditionDefinitionManager.Current.TryGetTerminalDefinitionFromInstance(__instance.m_lockedTerminal, out var def))
                return true;

            var passwordData = new TerminalPasswordData()
            {
                PasswordProtected = true,
                Password = string.Empty,
                PasswordHintText = __instance.m_lockedTerminal.StartStateData?.PasswordHintText ?? "Password Required.",
                GeneratePassword = true,
                PasswordPartCount = __instance.m_passwordParts,
                ShowPasswordLength = __instance.m_showPasswordLength,
                ShowPasswordPartPositions = __instance.m_showPasswordPartPositions,
                PasswordWordLength = def.PasswordWordLength,
                TerminalZoneSelectionDatas = EOSUtils.ToManaged(__instance.m_zoneSelectionDatas)
                    .Select(range => EOSUtils.ToManaged(range)
                        .Select(zone => new CustomTerminalZoneSelectionData()
                        {
                            DimensionIndex = def.DimensionIndex,
                            LayerType = def.LayerType,
                            LocalIndex = zone.LocalIndex,
                            SeedType = zone.SeedType,
                            TerminalIndex = zone.TerminalIndex,
                            StaticSeed = zone.StaticSeed
                        }).ToList())
                    .ToList() ?? new List<List<CustomTerminalZoneSelectionData>>()
            };

            EOSTerminalUtils.BuildPassword(__instance.m_lockedTerminal, passwordData);
            __result = true;
            return false;
        }
    }
}
