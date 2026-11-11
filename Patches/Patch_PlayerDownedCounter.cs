using ExtraObjectiveSetup.Objectives.ObjectiveCounter;
using GTFO.API;
using HarmonyLib;
using Player;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Patches
{
    /// <summary>
    /// Increments every counter whose definition has <see cref="ObjectiveCounterDefinition.IncrementOnPlayerDowned"/>.
    /// </summary>
    [HarmonyPatch]
    internal static class Patch_PlayerDownedCounter
    {
        private static readonly List<string> downedCounters = new();

        private static bool initialized = false;

        static Patch_PlayerDownedCounter()
        {
            LevelAPI.OnLevelCleanup += () =>
            {
                downedCounters.Clear();
                initialized = false;
            };
        }

        [HarmonyPatch(typeof(PLOC_Downed), nameof(PLOC_Downed.CommonEnter))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Post_PLOC_Downed_CommonEnter()
        {
            if (!initialized)
            {
                foreach (var counter in ObjectiveCounterManager.Current.AllCounters.Values)
                {
                    if (counter.Def.IncrementOnPlayerDowned)
                    {
                        downedCounters.Add(counter.Def.WorldEventObjectFilter);
                    }
                }
                initialized = true;
            }

            foreach (string filter in downedCounters)
            {
                if (ObjectiveCounterManager.Current.AllCounters.TryGetValue(filter, out var counter))
                {
                    counter.Increment(1);
                }
            }
        }
    }
}
