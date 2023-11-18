using Gear;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Expedition.EMP;
using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using Player;

namespace ExtraObjectiveSetup.Patches.EMP
{
    internal class Inject_PlayerBackpack
    {
        public static void Setup()
        {
            foreach (PlayerBackpack backpack in PlayerBackpackManager.Current.m_backpacks.Values)
            {
                AddHandlerForSlot(backpack, InventorySlot.GearStandard, new EMPGunSightHandler());
                AddHandlerForSlot(backpack, InventorySlot.GearSpecial, new EMPGunSightHandler());
                AddToolHandler(backpack);
            }
        }

        private static void AddToolHandler(PlayerBackpack backpack)
        {
            BackpackItem backpackItem;
            if (backpack.TryGetBackpackItem(InventorySlot.GearClass, out backpackItem))
            {
                if (backpackItem.Instance.gameObject.GetComponent<EMPController>() != null)
                    EOSLogger.Debug("Item already has controller, skipping...");
                if (backpackItem.Instance.GetComponent<EnemyScanner>() != null)
                {
                    backpackItem.Instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPBioTrackerHandler());
                }
            }
            else
            {
                EOSLogger.Warning($"Couldn't get item for slot {InventorySlot.GearClass}!");
            }
        }

        private static void AddHandlerForSlot(
          PlayerBackpack backpack,
          InventorySlot slot,
          EMPHandler handler)
        {
            BackpackItem backpackItem;
            if (backpack.TryGetBackpackItem(slot, out backpackItem))
            {
                if (backpackItem.Instance.gameObject.GetComponent<EMPController>() != null)
                    EOSLogger.Debug("Item already has controller, skipping...");
                backpackItem.Instance.gameObject.AddComponent<EMPController>().AssignHandler(handler);
            }
            else
            {
                EOSLogger.Warning($"Couldn't get item for slot {slot}!");
            }
        }
    }

}
