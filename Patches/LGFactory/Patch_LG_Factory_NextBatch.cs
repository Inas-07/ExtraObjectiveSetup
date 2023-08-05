using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using System;

namespace ExtraObjectiveSetup.Patches.LGFactory
{
    [HarmonyPatch]
    internal class Patch_LG_Factory_NextBatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.NextBatch))]
        private static void Post_LG_Factory_NextBatch(LG_Factory __instance)
        {
            int lastBatchStep = __instance.m_batchStep - 1;
            if (lastBatchStep < 0) return;

            var lastBatchName = __instance.m_batches[lastBatchStep].m_batchName;
            Action onBatchDone = BatchBuildManager.Current.Get_OnBatchDone(lastBatchName); 

            if(onBatchDone != null)
            {
                EOSLogger.Warning($"On Batch '{lastBatchName}' Done: {onBatchDone.GetInvocationList().Length} injected jobs");
                onBatchDone();
            }
        }
    }
}
