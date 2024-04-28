using ChainedPuzzles;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Tweaks.TerminalTweak;
using HarmonyLib;

namespace ExtraObjectiveSetup.Patches.Terminal
{
    [HarmonyPatch]
    internal static class Patch_FixRepeatablePuzzleBugs
    {
        // vanilla bug fix: CP_Cluster_Core.OnPuzzleDone is executed on checkpoint restore
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CP_Cluster_Core), nameof(CP_Cluster_Core.OnSyncStateChange))]
        private static bool Pre_CheckEventsOnPuzzleSolved(CP_Cluster_Core __instance,
            eClusterStatus newStatus, bool isDropinState)
        {
            //if(!TerminalInstanceManager.Current.TryGetParentTerminal(__instance.m_owner.Pointer, out var terminal))
            //{
            //    return true;
            //}

            //var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(terminal);
            //var def = TerminalTweakManager.Current.GetDefinition(TerminalInstanceManager.Current.GetGlobalZoneIndex(terminal), instanceIndex);
            //if (def == null || !def.FixRepeatablePuzzleBugs) return true;

            pClusterState currentState = __instance.m_sync.GetCurrentState();

            // CP_Cluster_Core checkpoint restore fix
            if (isDropinState && newStatus == eClusterStatus.Finished)
            {
                __instance.m_spline.SetVisible(false);
                for (int k = 0; k < __instance.m_childCores.Length; k++)
                {
                    __instance.m_childCores[k].Deactivate();
                }

                // NOTE: unwanted line in R6 mono
                // HOWEVER I don't know if this would break any shit
                //__instance.OnPuzzleDone?.Invoke(__instance.m_puzzleIndex);
                return false;
            }

            // repeatable command event fix: 
            else if (!isDropinState && currentState.status == eClusterStatus.Finished && newStatus == eClusterStatus.SplineReveal)
            {
                __instance.m_spline.Reveal();
                return false;
            }

            return true;
        }
    }
}
