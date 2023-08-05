using Gear;
using ExtraObjectiveSetup.Expedition.Gears;
using HarmonyLib;

namespace ExtraObjectiveSetup.Patches.Expedition
{
    [HarmonyPatch]
    internal class GearManager_LoadOfflineGearDatas
    {
        // called on both host and client side
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GearManager), nameof(GearManager.LoadOfflineGearDatas))]
        private static void Post_GearManager_LoadOfflineGearDatas(GearManager __instance)
        {
            ExpeditionGearManager.Current.vanillaGearManager = __instance;

            foreach (var gearSlot in ExpeditionGearManager.Current.gearSlots)
            {
                foreach (GearIDRange gearIDRange in __instance.m_gearPerSlot[(int)gearSlot.inventorySlot])
                {
                    uint playerOfflineDBPID = ExpeditionGearManager.GetOfflineGearPID(gearIDRange);
                    gearSlot.loadedGears.Add(playerOfflineDBPID, gearIDRange);
                }
            }
        }
    }
}
