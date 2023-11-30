using Gear;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Expedition.EMP;
using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using Player;

namespace ExtraObjectiveSetup.Patches.EMP
{
    internal static class Inject_PlayerBackpack
    {
        //internal static void SetupToolHandler()
        //{
        //    var backpack = PlayerBackpackManager.LocalBackpack;
        //    if (backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem))
        //    {
        //        if (backpackItem.Instance.gameObject.GetComponent<EMPController>() != null)
        //        {
        //            EOSLogger.Debug("Item already has controller, skipping...");
        //            return;
        //        }

        //        if (backpackItem.Instance.GetComponent<EnemyScanner>() != null)
        //        {
        //            backpackItem.Instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPBioTrackerHandler());
        //        }
        //    }
        //    else
        //    {
        //        EOSLogger.Warning($"Couldn't get item for slot {InventorySlot.GearClass}!");
        //    }
        //}

        internal static void SetupHandlerForSlot(InventorySlot slot)
        {
            var backpack = PlayerBackpackManager.LocalBackpack;
            if (backpack.TryGetBackpackItem(slot, out var backpackItem))
            {
                if (backpackItem.Instance.gameObject.GetComponent<EMPController>() != null)
                {
                    EOSLogger.Debug("Item already has controller, skipping...");
                    return;
                }

                backpackItem.Instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPGunSightHandler());
            }
            else
            {
                EOSLogger.Warning($"Couldn't get item for slot {slot}!");
            }
        }
    }
}
