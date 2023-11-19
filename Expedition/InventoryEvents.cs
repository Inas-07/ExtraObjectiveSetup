using Gear;
using System;
namespace ExtraObjectiveSetup.Expedition
{
    public static class InventoryEvents
    {
        public static event Action<GearPartFlashlight> FlashLightWielded;


        public static event Action AmmoWeaponWielded; // standard / special weapon

        internal static void OnWieldFlashLight(GearPartFlashlight flashlight)
        {
            FlashLightWielded?.Invoke(flashlight);
        }

        internal static void OnWieldAmmoWeapon()
        {
            AmmoWeaponWielded?.Invoke();
        }
    }
}
