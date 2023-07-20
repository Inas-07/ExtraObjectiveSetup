using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using System;

namespace ExtraObjectiveSetup.Patches.LGFactory
{
    [HarmonyPatch]
    internal class Patch_LG_Factory_FactoryDone
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.FactoryDone))]
        private static void Pre_LG_Factory_FactoryDone(LG_Factory __instance)
        {
            BatchBuildManager.Current.OnBeforeFactoryDone?.Invoke();
        }
    }
}
