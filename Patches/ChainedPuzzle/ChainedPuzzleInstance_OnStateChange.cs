using ChainedPuzzles;
using ExtraObjectiveSetup.Instances.ChainedPuzzle;
using HarmonyLib;

namespace ExtraObjectiveSetup.Patches.ChainedPuzzle
{
    [HarmonyPatch]
    internal static class ChainedPuzzleInstance_OnStateChange
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.OnStateChange))]
        private static void Post_ChainedPuzzleOnActivationInstance_OnStateChange(ChainedPuzzleInstance __instance,
            pChainedPuzzleState oldState, pChainedPuzzleState newState, bool isRecall)
        {
            var actions = ChainedPuzzleInstanceManager.Current.Get_OnStateChange(__instance);
            actions?.Invoke(oldState, newState, isRecall);
        }
    }
}
