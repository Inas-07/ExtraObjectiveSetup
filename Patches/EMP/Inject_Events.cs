using Gear;
using HarmonyLib;
using ExtraObjectiveSetup.Expedition;
using ExtraObjectiveSetup.Utils;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch]
    internal static class Inject_Events
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerInventoryBase), nameof(PlayerInventoryBase.OnItemEquippableFlashlightWielded))]
        internal static void Post_OnItemEquippableFlashlightWielded(GearPartFlashlight flashlight)
        {
            Events.FlashLightWielded?.Invoke(flashlight);
        }

        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerInventoryLocal), nameof(PlayerInventoryLocal.DoWieldItem))]
        internal static void Post_DoWieldItem(PlayerInventoryLocal __instance)
        {
            Events.InventoryWielded?.Invoke(__instance.WieldedSlot);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GS_InLevel), nameof(GS_InLevel.Enter))]
        private static void Post_SetupGear(GS_InLevel __instance)
        {
            Events.EnterGSInLevel?.Invoke();
        }
    }
}
