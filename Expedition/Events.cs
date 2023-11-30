using Gear;
using Player;
using System;
namespace ExtraObjectiveSetup.Expedition
{
    public static class Events
    {
        public static Action<GearPartFlashlight> FlashLightWielded;


        public static Action<InventorySlot> InventoryWielded; // standard / special weapon


        public static Action EnterGSInLevel;
    }
}
