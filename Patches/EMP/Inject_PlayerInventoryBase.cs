using Gear;
using HarmonyLib;
using ExtraObjectiveSetup.Expedition;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch]
    internal static class Inject_PlayerInventoryBase
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerInventoryBase), nameof(PlayerInventoryBase.OnItemEquippableFlashlightWielded))]
        internal static void Post_OnItemEquippableFlashlightWielded(GearPartFlashlight flashlight)
        {
            InventoryEvents.OnWieldFlashLight(flashlight);
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerInventoryLocal), nameof(PlayerInventoryLocal.DoWieldItem))]
        internal static void Post_DoWieldItem(PlayerInventoryLocal __instance)
        {
            if (__instance.WieldedSlot == Player.InventorySlot.GearStandard || __instance.WieldedSlot == Player.InventorySlot.GearSpecial)
                InventoryEvents.OnWieldAmmoWeapon();
        }
    }
}
