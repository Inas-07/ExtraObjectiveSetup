using ChainedPuzzles;
using System;
using pCPState = ChainedPuzzles.pChainedPuzzleState;

namespace ExtraObjectiveSetup.Instances.ChainedPuzzle
{
    public static class ChainedPuzzleInstanceManagerHelper
    {
        public static void Add_OnStateChange(this ChainedPuzzleInstance instance, Action<pCPState, pCPState, bool> action) => ChainedPuzzleInstanceManager.Current.Add_OnStateChange(instance, action);

        public static void Remove_OnStateChange(this ChainedPuzzleInstance instance, Action<pCPState, pCPState, bool> action) => ChainedPuzzleInstanceManager.Current.Remove_OnStateChange(instance, action);
    }
}
