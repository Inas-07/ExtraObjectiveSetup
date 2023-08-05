using GameData;
using HarmonyLib;
using ExtraObjectiveSetup.Expedition.Gears;

namespace ExtraObjectiveSetup.Patches.Expedition
{
    [HarmonyPatch]
    internal class RundownManager_SetActiveExpedition
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RundownManager), nameof(RundownManager.SetActiveExpedition))]
        private static void Post_RundownManager_SetActiveExpedition(RundownManager __instance, pActiveExpedition expPackage, ExpeditionInTierData expTierData)
        {
            if (expPackage.tier == eRundownTier.Surface) return;

            ExpeditionGearManager.Current.SetupAllowedGearsForActiveExpedition();
        }
    }
}
