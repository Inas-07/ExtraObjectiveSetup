using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using System;

namespace ExtraObjectiveSetup.Patches.LGFactory
{
    [HarmonyPatch]
    internal class Patch_LG_Factory_NextBatch
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.NextBatch))]
        private static void Prefix(LG_Factory __instance)
        {
            if(__instance.m_batchStep > -1)
            {
                var lastBatch = __instance.m_currentBatchName;
                BatchBuildManager.Current.Get_OnBatchDone(lastBatch)?.Invoke();
            }
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.NextBatch))]
        private static void Postfix(LG_Factory __instance)
        {
            BatchBuildManager.Current.Get_OnBatchStart(__instance.m_currentBatchName)?.Invoke();
        }
    }
}
