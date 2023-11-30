using HarmonyLib;
using SNetwork;
using ExtraObjectiveSetup.Expedition.EMP;
using ExtraObjectiveSetup.Utils;

namespace ExtraObjectiveSetup.Patches
{
    [HarmonyPatch]
    internal class Patch_OnRecallDone
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnRecallDone))]
        private static void Post_MasterReplicationUpdate_Player(SNet_SyncManager __instance)
        {
            if (SNet.IsMaster) return;
            EOSLogger.Warning($"OnRecallDone: Client send request to master for pEMP state sync");
            EMPManager.Current.ClientSyncRequest();
        }
    }
}
