using Gear;

namespace ExtraObjectiveSetup.Expedition
{
    public delegate void InventoryEventHandler(GearPartFlashlight flashlight);

    public static class InventoryEvents
    {
        public static event InventoryEventHandler ItemWielded;

        internal static void OnWieldItem(GearPartFlashlight flashlight)
        {
            InventoryEventHandler itemWielded = ItemWielded;
            itemWielded?.Invoke(flashlight);
        }
    }
}
