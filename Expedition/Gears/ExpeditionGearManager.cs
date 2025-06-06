using ExtraObjectiveSetup.Utils;
using Gear;
using Player;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ExtraObjectiveSetup.Expedition.Gears
{
    public class ExpeditionGearManager
    {
        public static ExpeditionGearManager Current { get; private set; } = new();

        public GearManager VanillaGearManager { internal set; get; } = null; // setup in patch: GearManager.LoadOfflineGearDatas

        private Mode mode = Mode.DISALLOW;

        private HashSet<uint> GearIds = new();

        public readonly IList<(InventorySlot inventorySlot, Dictionary<uint, GearIDRange> loadedGears)> GearSlots = new List<(InventorySlot, Dictionary<uint, GearIDRange>)>() {
            (InventorySlot.GearStandard, new()),
            (InventorySlot.GearSpecial, new()),
            (InventorySlot.GearMelee, new()),
            (InventorySlot.GearClass, new()),
        }.ToImmutableList();

        private void ClearLoadedGears()
        {
            foreach (var slot in GearSlots)
            {
                VanillaGearManager.m_gearPerSlot[(int)slot.inventorySlot].Clear();
            }
        }

        private bool IsGearAllowed(uint playerOfflineGearDBPID)
        {
            switch (mode)
            {
                case Mode.ALLOW: return GearIds.Contains(playerOfflineGearDBPID);
                case Mode.DISALLOW: return !GearIds.Contains(playerOfflineGearDBPID);
                default:
                    EOSLogger.Error($"Unimplemented Mode: {mode}, will allow gears anyway...");
                    return true;
            }
        }

        private void AddGearForCurrentExpedition()
        {
            foreach (var slot in GearSlots)
            {
                var vanillaSlot = VanillaGearManager.m_gearPerSlot[(int)slot.inventorySlot];
                var loadedGearsInCategory = slot.loadedGears;

                if (loadedGearsInCategory.Count == 0)
                {
                    EOSLogger.Debug($"No gear has been loaded for {slot.inventorySlot}.");
                    continue;
                }

                foreach (uint offlineGearPID in loadedGearsInCategory.Keys)
                {
                    if (IsGearAllowed(offlineGearPID))
                    {
                        vanillaSlot.Add(loadedGearsInCategory[offlineGearPID]);
                    }
                }

                if (vanillaSlot.Count == 0)
                {
                    EOSLogger.Error($"No gear is allowed for {slot.inventorySlot}, there must be at least 1 allowed gear!");
                    vanillaSlot.Add(loadedGearsInCategory.First().Value);
                }
            }
        }

        private void ResetPlayerSelectedGears()
        {
            VanillaGearManager.RescanFavorites();
            foreach (var gearSlot in GearSlots)
            {
                var inventorySlotIndex = (int)gearSlot.inventorySlot;

                try
                {
                    if (VanillaGearManager.m_lastEquippedGearPerSlot[inventorySlotIndex] != null)
                        PlayerBackpackManager.EquipLocalGear(VanillaGearManager.m_lastEquippedGearPerSlot[inventorySlotIndex]);
                    else if (VanillaGearManager.m_favoriteGearPerSlot[inventorySlotIndex].Count > 0)
                        PlayerBackpackManager.EquipLocalGear(VanillaGearManager.m_favoriteGearPerSlot[inventorySlotIndex][0]);
                    else if (VanillaGearManager.m_gearPerSlot[inventorySlotIndex].Count > 0)
                        PlayerBackpackManager.EquipLocalGear(VanillaGearManager.m_gearPerSlot[inventorySlotIndex][0]);
                }
                catch (Il2CppInterop.Runtime.Il2CppException e)
                {
                    EOSLogger.Error($"Error attempting to equip gear for slot {gearSlot.inventorySlot}:\n{e.StackTrace}");
                }
            }
        }

        private void ConfigExpeditionGears()
        {
            mode = Mode.DISALLOW;
            GearIds.Clear();

            var expDef = ExpeditionDefinitionManager.Current.GetDefinition(ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.ExpeditionGears == null) 
            {
                return;
            }

            mode = expDef.ExpeditionGears.Mode;
            expDef.ExpeditionGears.GearIds.ForEach(id => GearIds.Add(id));
        }

        internal void SetupAllowedGearsForActiveExpedition()
        {
            ConfigExpeditionGears();
            ClearLoadedGears();
            AddGearForCurrentExpedition();
            ResetPlayerSelectedGears();
        }

        public void Init() { }

        private ExpeditionGearManager() { }

        public static uint GetOfflineGearPID(GearIDRange gearIDRange)
        {
            string itemInstanceId = gearIDRange.PlayfabItemInstanceId;
            if (!itemInstanceId.Contains("OfflineGear_ID_"))
            {
                EOSLogger.Error($"Find PlayfabItemInstanceId without substring 'OfflineGear_ID_'! {itemInstanceId}");
                return 0;
            }

            try
            {
                uint offlineGearPersistentID = uint.Parse(itemInstanceId.Substring("OfflineGear_ID_".Length));
                return offlineGearPersistentID;
            }
            catch
            {
                EOSLogger.Error("Caught exception while trying to parse persistentID of PlayerOfflineGearDB from GearIDRange, which means itemInstanceId could be ill-formated");
                return 0;
            }
        }

        static ExpeditionGearManager() { }
    }

}
