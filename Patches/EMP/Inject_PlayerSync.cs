using HarmonyLib;
using Player;
using ExtraObjectiveSetup.Expedition.EMP;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch(typeof(PlayerSync))]
    internal static class Inject_PlayerSync
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(PlayerSync.WantsToSetFlashlightEnabled))]
        internal static bool Pre_WantsToSetFlashlightEnabled(PlayerSync __instance, bool enable)
        {
            if (EMPHandler.IsLocalPlayerDisabled)
                return false;
            if (enable != __instance.m_agent.Inventory.FlashlightEnabled)
                return true;
            __instance.WantsToSetFlashlightEnabled(!__instance.m_agent.Inventory.FlashlightEnabled);
            return false;
        }
    }
}
