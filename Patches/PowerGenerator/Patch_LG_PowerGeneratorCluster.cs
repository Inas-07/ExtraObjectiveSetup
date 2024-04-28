using HarmonyLib;
using System.Collections.Generic;
using LevelGeneration;
using GameData;
using UnityEngine;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Objectives.GeneratorCluster;

namespace ExtraObjectiveSetup.Patches.PowerGenerator
{
    [HarmonyPatch]
    internal static class Patch_LG_PowerGeneratorCluster
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_PowerGeneratorCluster), nameof(LG_PowerGeneratorCluster.Setup))]
        private static void Post_PowerGeneratorCluster_Setup(LG_PowerGeneratorCluster __instance)
        {
            uint zoneInstanceIndex = GeneratorClusterInstanceManager.Current.Register(__instance);

            var globalZoneIndex = GeneratorClusterInstanceManager.Current.GetGlobalZoneIndex(__instance);
            var def = GeneratorClusterObjectiveManager.Current.GetDefinition(globalZoneIndex, zoneInstanceIndex);
            if (def == null) return;

            //if (WardenObjectiveManager.Current.m_activeWardenObjectives[__instance.SpawnNode.LayerType].Type == eWardenObjectiveType.CentralGeneratorCluster)
            //{
            //    EOSLogger.Error("Found built Warden Objective LG_PowerGeneratorCluster but there's also a config for it! Won't apply this config");
            //    EOSLogger.Error($"{globalZoneIndex}");
            //    return;
            //}

            EOSLogger.Debug("Found LG_PowerGeneratorCluster and its definition! Building this Generator cluster...");

            // ========== vanilla build =================
            __instance.m_serialNumber = SerialGenerator.GetUniqueSerialNo();
            __instance.m_itemKey = "GENERATOR_CLUSTER_" + __instance.m_serialNumber.ToString();
            __instance.m_terminalItem = GOUtil.GetInterfaceFromComp<iTerminalItem>(__instance.m_terminalItemComp);
            __instance.m_terminalItem.Setup(__instance.m_itemKey);
            __instance.m_terminalItem.FloorItemStatus = eFloorInventoryObjectStatus.UnPowered;
            if (__instance.SpawnNode != null)
                __instance.m_terminalItem.FloorItemLocation = __instance.SpawnNode.m_zone.NavInfo.GetFormattedText(LG_NavInfoFormat.Full_And_Number_With_Underscore);

            List<Transform> transformList = new List<Transform>(__instance.m_generatorAligns);
            uint numberOfGenerators = def.NumberOfGenerators;
            __instance.m_generators = new LG_PowerGenerator_Core[numberOfGenerators];

            if (transformList.Count >= numberOfGenerators)
            {
                for (int j = 0; j < numberOfGenerators; ++j)
                {
                    int k = Builder.BuildSeedRandom.Range(0, transformList.Count, "NO_TAG");
                    LG_PowerGenerator_Core generator = GOUtil.SpawnChildAndGetComp<LG_PowerGenerator_Core>(__instance.m_generatorPrefab, transformList[k]);
                    __instance.m_generators[j] = generator;

                    generator.SpawnNode = __instance.SpawnNode;
                    PowerGeneratorInstanceManager.Current.MarkAsGCGenerator(__instance, generator);
                    generator.Setup();
                    generator.SetCanTakePowerCell(true);
                    //generator.OnSyncStatusChanged += new System.Action<ePowerGeneratorStatus>(status =>
                    //{
                    //    Debug.Log("LG_PowerGeneratorCluster.powerGenerator.OnSyncStatusChanged! status: " + status);

                    //    if (status != ePowerGeneratorStatus.Powered) return;

                    //    uint poweredGenerators = 0u;

                    //    for (int m = 0; m < __instance.m_generators.Length; ++m)
                    //    {
                    //        if (__instance.m_generators[m].m_stateReplicator.State.status == ePowerGeneratorStatus.Powered)
                    //            poweredGenerators++;
                    //    }

                    //    EOSLogger.Log($"Generator Cluster PowerCell inserted ({poweredGenerators} / {__instance.m_generators.Count})");
                    //    var EventsOnInsertCell = def.EventsOnInsertCell;

                    //    int eventsIndex = (int)(poweredGenerators - 1);
                    //    if (eventsIndex >= 0 && eventsIndex < EventsOnInsertCell.Count)
                    //    {
                    //        EOSLogger.Log($"Executing events ({poweredGenerators} / {__instance.m_generators.Count}). Event count: {EventsOnInsertCell[eventsIndex].Count}");
                    //        EventsOnInsertCell[eventsIndex].ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
                    //    }

                    //    if (poweredGenerators == __instance.m_generators.Count && !__instance.m_endSequenceTriggered)
                    //    {
                    //        EOSLogger.Log("All generators powered, executing end sequence");
                    //        __instance.StartCoroutine(__instance.ObjectiveEndSequence());
                    //        __instance.m_endSequenceTriggered = true;
                    //    }
                    //});
                    Debug.Log("Spawning generator at alignIndex: " + k);
                    transformList.RemoveAt(k);
                }
            }
            else
                Debug.LogError("LG_PowerGeneratorCluster does NOT have enough generator aligns to support the warden objective! Has " + transformList.Count + " needs " + numberOfGenerators);
            __instance.ObjectiveItemSolved = true;

            if (def.EndSequenceChainedPuzzle != 0u)
            {
                GeneratorClusterObjectiveManager.Current.RegisterForChainedPuzzleBuild(__instance, def);
            }
        }
    }
}
