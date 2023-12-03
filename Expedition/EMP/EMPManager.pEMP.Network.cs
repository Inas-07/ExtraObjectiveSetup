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
        public struct Sync // dummy 
        {
            public bool sync;
            public Sync() { }
            public Sync(Sync o) { sync = o.sync; }
        }

        public const string CLIENT_SYNC_REQUEST_EVENT = "ClientSyncRequest";
        public const string CLIENT_RECEIVE_SYNC_EVENT = "ClientSyncReceive";

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
    }
}
