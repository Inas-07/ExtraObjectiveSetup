using AK;
using Gear;
using HarmonyLib;
using ExtraObjectiveSetup.Expedition.EMP;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch(typeof(EnemyScanner))]
    internal static class Inject_EnemyScanner
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(EnemyScanner.UpdateDetectedEnemies))]
        internal static bool Pre_UpdateDetectedEnemies() => !EMPHandler.IsLocalPlayerDisabled;

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(EnemyScanner.UpdateTagProgress))]
        internal static bool Pre_UpdateTagProgress(EnemyScanner __instance)
        {
            if (EMPHandler.IsLocalPlayerDisabled)
            {
                __instance.Sound.Post(EVENTS.BIOTRACKER_TOOL_LOOP_STOP);
                __instance.m_screen.SetStatusText("ERROR");
                __instance.m_progressBar.SetProgress(1.0f);
                __instance.m_screen.SetGuixColor(UnityEngine.Color.yellow);
                return false;
            }
            __instance.m_screen.SetStatusText("Ready to tag");
            __instance.m_screen.SetGuixColor(UnityEngine.Color.red);
            return true;
        }
    }

}
