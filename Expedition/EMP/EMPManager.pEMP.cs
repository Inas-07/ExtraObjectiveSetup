using UnityEngine;
using System.Collections.Generic;
using GTFO.API;
using ExtraObjectiveSetup.Utils;
using Player;
using GameData;
using LevelGeneration;
using SNetwork;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtraObjectiveSetup.Patches.EMP;
using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using Gear;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public partial class EMPManager
    {
        public struct pEMPState
        {
            public uint pEMPIndex;
            public bool enabled;
            public pEMPState() { }
            public pEMPState(pEMPState o) { o.pEMPIndex = pEMPIndex; o.enabled = enabled; }
        }

        private readonly Dictionary<uint, pEMP> _pEMPs = new();
        
        public IEnumerable<pEMP> pEMPs => _pEMPs.Values;

        private HashSet<string> _processed = new();

        public void TogglepEMPState(uint pEMPIndex, bool enabled)
        {
            if (!_pEMPs.ContainsKey(pEMPIndex))
            {
                EOSLogger.Error($"TogglepEMPState: pEMPIndex {pEMPIndex} not defined");
                return;
            }

            ActiveState newState = enabled ? ActiveState.ENABLED : ActiveState.DISABLED;
            var pEMP = _pEMPs[pEMPIndex];
            pEMP.ChangeState(newState);
            foreach (EMPController empTarget in _empTargets)
            {
                if (empTarget.GetComponent<LG_Light>() == null) continue;

                if (enabled)
                {
                    if (pEMP.InRange(empTarget.Position))
                    {
                        empTarget.AddTime(float.PositiveInfinity);
                    }
                }
                else
                {
                    empTarget.ClearTime();
                }
            }
        }

        public void TogglepEMPState(WardenObjectiveEventData e)
        {
            uint pEMPIndex = (uint)e.Count;
            bool enabled = e.Enabled;
            TogglepEMPState(pEMPIndex, enabled);
        }

        private void pEMPInit()
        {
            //LevelAPI.OnBuildDone += SetupHUD;
            //LevelAPI.OnBuildDone += SetupPlayerFlashLight;
            //LevelAPI.OnBuildDone += SetupToolHandler;
            Events.InventoryWielded += SetupAmmoWeaponHandlers;
        }

        private void BuildpEMPs()
        {
            var expDef = ExpeditionDefinitionManager.Current.GetDefinition(ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.PersistentEMPs == null || expDef.PersistentEMPs.Count < 1) return;

            foreach(var pEMPDef in expDef.PersistentEMPs)
            {
                var pEMP = new pEMP(pEMPDef);
                _pEMPs[pEMPDef.pEMPIndex] = pEMP;

                EOSLogger.Warning($"pEMP_{pEMPDef.pEMPIndex} built");
            }
        }

        private void pEMPs_Clear()
        {
            _pEMPs.Clear();
            _processed.Clear();
        }
        
        internal void SetupHUD()
        {
            if(LocalPlayerAgent == null)
            {
                EOSLogger.Error($"{nameof(SetupHUD): LocalPlayerAgent is not set!}"); 
                return;
            }

            if (_processed.Contains("HUD")) return;
            LocalPlayerAgent.gameObject.AddComponent<EMPController>().AssignHandler(new EMPPlayerHudHandler());
            EOSLogger.Log($"pEMP: PlayerHUD setup completed");
            _processed.Add("HUD");
        }

        internal void SetupPlayerFlashLight()
        {
            if (LocalPlayerAgent == null)
            {
                EOSLogger.Error($"{nameof(SetupPlayerFlashLight): LocalPlayerAgent is not set!}");
                return;
            }

            if (_processed.Contains("PlayerFlashLight")) return;
            LocalPlayerAgent.gameObject.AddComponent<EMPController>().AssignHandler(new EMPPlayerFlashLightHandler());
            EOSLogger.Log($"pEMP: PlayerFlashLight setup completed");
            _processed.Add("PlayerFlashLight");
        }

        private void SetupAmmoWeaponHandlers(InventorySlot slot)
        {
            if(_processed.Contains(slot.ToString())) return;

            void SetupAmmoWeaponHandlerForSlot(InventorySlot slot)
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

            switch (slot)
            {
                case InventorySlot.GearStandard:
                case InventorySlot.GearSpecial:
                    SetupAmmoWeaponHandlerForSlot(slot);
                    break;
                default: return;
            }
            EOSLogger.Log($"pEMP: Backpack {slot} setup completed");
            _processed.Add(slot.ToString());
        }

        internal void SetupToolHandler()
        {
            if (_processed.Contains(InventorySlot.GearClass.ToString())) return;

            var backpack = PlayerBackpackManager.LocalBackpack;
            if (backpack.TryGetBackpackItem(InventorySlot.GearClass, out var backpackItem))
            {
                if (backpackItem.Instance.gameObject.GetComponent<EMPController>() != null)
                {
                    EOSLogger.Debug("Item already has controller, skipping...");
                    return;
                }

                if (backpackItem.Instance.GetComponent<EnemyScanner>() != null)
                {
                    backpackItem.Instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPBioTrackerHandler());
                    EOSLogger.Warning($"pEMP: Backpack {InventorySlot.GearClass} setup completed");
                    _processed.Add(InventorySlot.GearClass.ToString());
                }
            }
            else
            {
                EOSLogger.Warning($"Couldn't get item for slot {InventorySlot.GearClass}!");
            }
        }

    }
}
