using HarmonyLib;
using LevelGeneration;

namespace ExtraObjectiveSetup.Patches.Uplink
{
    [HarmonyPatch]
    internal static class UplinkGUI_Update
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.Update))]
        private static void Post_LG_ComputerTerminal_Update(LG_ComputerTerminal __instance)
        {
            if (!__instance.m_isWardenObjective && __instance.UplinkPuzzle != null)
                __instance.UplinkPuzzle.UpdateGUI();
        }
    }
}
