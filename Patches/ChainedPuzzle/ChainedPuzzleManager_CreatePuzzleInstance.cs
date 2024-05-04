using ChainedPuzzles;
using ExtraObjectiveSetup.Instances.ChainedPuzzle;
using GameData;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace ExtraObjectiveSetup.Patches.ChainedPuzzle
{
    [HarmonyPatch]
    internal static class ChainedPuzzleManager_CreatePuzzleInstance
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChainedPuzzleManager), nameof(ChainedPuzzleManager.CreatePuzzleInstance), new System.Type[] { 
            typeof(ChainedPuzzleDataBlock),
            typeof(LG_Area),
            typeof(LG_Area),
            typeof(Vector3),
            typeof(Transform),
            typeof(bool),
        })]
        private static void Post_ChainedPuzzleInstance_Setup(ChainedPuzzleInstance __result)
        {
            ChainedPuzzleInstanceManager.Current.Register(__result);
        }
    }
}
