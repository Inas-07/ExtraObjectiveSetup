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

        public struct Sync // dummy 
        {
            public bool sync;
            public Sync() { }
            public Sync(Sync o) { sync = o.sync; }
        }

        private readonly Dictionary<uint, pEMP> _pEMPs = new();

        public const string CLIENT_SYNC_REQUEST_EVENT = "ClientSyncRequest";
        public const string CLIENT_RECEIVE_SYNC_EVENT = "ClientSyncReceive";

        private HashSet<InventorySlot> _processedSlot = new();

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

        private void SetupGunSights(InventorySlot slot)
        {
            if(_processedSlot.Contains(slot)) return;
            switch(slot)
            {
                case InventorySlot.GearStandard:
                case InventorySlot.GearSpecial:
                    SetupHandlerForSlot(slot);
                    _processedSlot.Add(slot);
                    break;
            }
        }

        private void SetupHandlerForSlot(InventorySlot slot)
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

        private void SetupToolHandler()
        {
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
                }
            }
            else
            {
                EOSLogger.Warning($"Couldn't get item for slot {InventorySlot.GearClass}!");
            }
        }

        internal void ClientSyncRequest()
        {
            if (SNet.IsMaster) return;
            EOSLogger.Debug("pEMPState: Client requesting master to sync");
            NetworkAPI.InvokeEvent(CLIENT_SYNC_REQUEST_EVENT, new Sync());
        }

        private void Master_ReceiveSyncRequest(ulong sender, Sync sync)
        {
            if (!SNet.IsMaster) return;
            EOSLogger.Debug("pEMPState: Master received sync request");
            CoroutineManager.StartCoroutine(MasterSync().WrapToIl2Cpp());
        }

        private System.Collections.IEnumerator MasterSync()
        {
            yield return new WaitForSeconds(0.25f); // this delay is prolly indispensable
            foreach (uint pEMPIndex in _pEMPs.Keys)
            {
                var status = _pEMPs[pEMPIndex].State;
                NetworkAPI.InvokeEvent(CLIENT_RECEIVE_SYNC_EVENT, new pEMPState()
                {
                    pEMPIndex = pEMPIndex,
                    enabled = status == ActiveState.ENABLED
                });
                EOSLogger.Debug($"Syncing - pEMP_{pEMPIndex}, state: {status}");
                yield return new WaitForSeconds(0.25f);
            }
        }

        private void ClientReceivepEMPState(ulong sender, pEMPState state)
        {
            if (SNet.IsMaster) return;
            EOSLogger.Error("pEMPState: Client Receive states from master");
            EOSLogger.Warning($"pEMP_{state.pEMPIndex}, enabled? {state.enabled}");
            TogglepEMPState(state.pEMPIndex, state.enabled);
        }

        public IEnumerable<pEMP> pEMPs => _pEMPs.Values;
    }
}
