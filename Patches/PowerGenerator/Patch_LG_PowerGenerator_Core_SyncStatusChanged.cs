using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using GameData;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Objectives.IndividualGenerator;
using ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtraObjectiveSetup.Objectives.GeneratorCluster;

namespace ExtraObjectiveSetup.Patches.PowerGenerator
{
    [HarmonyPatch]
    internal static class Patch_LG_PowerGenerator_Core_SyncStatusChanged
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_PowerGenerator_Core), nameof(LG_PowerGenerator_Core.SyncStatusChanged))]
        private static void Post_SyncStatusChanged(LG_PowerGenerator_Core __instance, pPowerGeneratorState state, bool isDropinState)
        {

            var zoneInstanceIndex = PowerGeneratorInstanceManager.Current.GetZoneInstanceIndex(__instance);
            var globalZoneIndex = PowerGeneratorInstanceManager.Current.GetGlobalZoneIndex(__instance);

            var gcParent = PowerGeneratorInstanceManager.Current.GetParentGeneratorCluster(__instance);
            GeneratorClusterDefinition gcDef = null;
            if(gcParent != null)
            {
                uint gcZoneInstanceIndex = GeneratorClusterInstanceManager.Current.GetZoneInstanceIndex(gcParent);
                var gcGlobalZoneIndex = GeneratorClusterInstanceManager.Current.GetGlobalZoneIndex(gcParent);
                gcDef = GeneratorClusterObjectiveManager.Current.GetDefinition(gcGlobalZoneIndex, gcZoneInstanceIndex);
            }

            // ==================== ==================== ====================
            // ====================   generator cluster  ====================
            // ==================== ==================== ====================
            var status = state.status;
            if (gcDef != null)
            {
                EOSLogger.Log($"LG_PowerGeneratorCluster.powerGenerator.OnSyncStatusChanged! status: {status}, isDropinState: {isDropinState}");

                if (status == ePowerGeneratorStatus.Powered)
                {
                    uint poweredGenerators = 0u;

                    for (int m = 0; m < gcParent.m_generators.Length; ++m)
                    {
                        if (gcParent.m_generators[m].m_stateReplicator.State.status == ePowerGeneratorStatus.Powered)
                            poweredGenerators++;
                    }

                    EOSLogger.Log($"Generator Cluster PowerCell inserted ({poweredGenerators} / {gcParent.m_generators.Count})");
                    var EventsOnInsertCell = gcDef.EventsOnInsertCell;

                    int eventsIndex = (int)(poweredGenerators - 1);

                    if(!isDropinState)
                    {
                        if (eventsIndex >= 0 && eventsIndex < EventsOnInsertCell.Count)
                        {
                            EOSLogger.Log($"Executing events ({poweredGenerators} / {gcParent.m_generators.Count}). Event count: {EventsOnInsertCell[eventsIndex].Count}");
                            EventsOnInsertCell[eventsIndex].ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
                        }

                        if (poweredGenerators == gcParent.m_generators.Count && !gcParent.m_endSequenceTriggered)
                        {
                            EOSLogger.Log("All generators powered, executing end sequence");
                            __instance.StartCoroutine(gcParent.ObjectiveEndSequence());
                            gcParent.m_endSequenceTriggered = true;
                        }
                    }
                    else
                    {
                        if(poweredGenerators != gcParent.m_generators.Count)
                        {
                            gcParent.m_endSequenceTriggered = false;
                        }
                    }
                } 
                else if (isDropinState)
                {
                    // Reloading from checkpoint and generator is unpowered, thus cluster is implicitly unfinished
                    gcParent.m_endSequenceTriggered = false;
                }
            }

            // ==================== ==================== ====================
            // ==================== individual generator ====================
            // ==================== ==================== ====================
            var igDef = IndividualGeneratorObjectiveManager.Current.GetDefinition(globalZoneIndex, zoneInstanceIndex);
            if(igDef != null && igDef.EventsOnInsertCell != null && status == ePowerGeneratorStatus.Powered && !isDropinState)
            {
                igDef.EventsOnInsertCell.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
            }

            // ==================== ==================== ====================
            // =============== individual generator group ===================
            // ==================== ==================== ====================
            var igGroupDef = ExpeditionIGGroupManager.Current.FindGroupDefOf(__instance);
            if (igGroupDef != null)
            {
                int poweredGeneratorCount = 0;
                foreach (var g in igGroupDef.GeneratorInstances)
                {
                    if (g.m_stateReplicator.State.status == ePowerGeneratorStatus.Powered)
                    {
                        poweredGeneratorCount += 1;
                    }
                }

                if(!isDropinState)
                {
                    if (poweredGeneratorCount == igGroupDef.GeneratorInstances.Count && igGroupDef.PlayEndSequenceOnGroupComplete)
                    {
                        var coroutine = CoroutineManager.StartCoroutine(ExpeditionIGGroupManager.PlayGroupEndSequence(igGroupDef).WrapToIl2Cpp());
                        WorldEventManager.m_worldEventEventCoroutines.Add(coroutine);
                    }

                    else
                    {
                        int eventIndex = poweredGeneratorCount - 1;
                        if (eventIndex >= 0 && eventIndex < igGroupDef.EventsOnInsertCell.Count)
                        {
                            igGroupDef.EventsOnInsertCell[eventIndex].ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
                        }
                    }
                }
            }
        }
    }
}
