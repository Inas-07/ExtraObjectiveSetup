using GTFO.API;
using LevelGeneration;
using System.Collections.Generic;
using System;
using System.Linq;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Instances;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using AK;

namespace ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup
{
    internal class ExpeditionIGGroupManager
    {
        public static ExpeditionIGGroupManager Current { get; private set; } = new();

        private List<(HashSet<IntPtr> group, ExpeditionIGGroup groupDef)> generatorGroups = new();

        private List<LG_PowerGenerator_Core> GatherIGs(ExpeditionIGGroup IGGroup)
        {
            List<LG_PowerGenerator_Core> result = new();
            IGGroup.Generators.ForEach(index => {
                var instance = PowerGeneratorInstanceManager.Current.GetInstance(index.GlobalZoneIndexTuple(), index.InstanceIndex);
                if(instance == null)
                {
                    EOSLogger.Error($"generator instance not found! Instance index: {index}");
                }
                else
                {
                    result.Add(instance);
                }
            });

            IGGroup.GeneratorInstances = result;
            return result;
        }

        public ExpeditionIGGroup FindGroupDefOf(LG_PowerGenerator_Core core)
        {
            foreach(var generatorGroup in generatorGroups)
            {
                if(generatorGroup.group.Contains(core.Pointer))
                {
                    return generatorGroup.groupDef;
                }
            }
            return null;
        }

        internal static System.Collections.IEnumerator PlayGroupEndSequence(ExpeditionIGGroup IGGroup)
        {
            yield return new UnityEngine.WaitForSeconds(4f);

            CellSound.Post(EVENTS.DISTANT_EXPLOSION_SEQUENCE);
            yield return new UnityEngine.WaitForSeconds(2f);
            EnvironmentStateManager.AttemptSetExpeditionLightMode(false);
            CellSound.Post(EVENTS.LIGHTS_OFF_GLOBAL);
            yield return new UnityEngine.WaitForSeconds(3f);

            for (int g = 0; g < IGGroup.GeneratorInstances.Count; ++g)
            {
                IGGroup.GeneratorInstances[g].TriggerPowerFailureSequence();
                yield return new UnityEngine.WaitForSeconds(UnityEngine.Random.Range(0.3f, 1f));
            }

            yield return new UnityEngine.WaitForSeconds(4f);
            EnvironmentStateManager.AttemptSetExpeditionLightMode(true);

            int eventIndex = IGGroup.GeneratorInstances.Count - 1;
            if(eventIndex >= 0 && eventIndex < IGGroup.EventsOnInsertCell.Count)
            {
                IGGroup.EventsOnInsertCell[eventIndex].ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, GameData.eWardenObjectiveEventTrigger.None, true));
            }
        }

        private void Configure(LG_PowerGenerator_Core core)
        {
            // NOTE: moved to SyncStatusChanged patch to fix checkpoint restore issue.

            //core.OnSyncStatusChanged += new Action<ePowerGeneratorStatus>((status) => {
            //    if (status != ePowerGeneratorStatus.Powered) return;
                
            //    var groupDef = FindGroupDefOf(core);
            //    if(groupDef == null)
            //    {
            //        EOSLogger.Error("LG_PowerGenerator_Core.OnSyncStatusChanged: generator does not belong to any IGGroup, but has been configured! WTF?");
            //        EOSLogger.Error($"pointer: {core.Pointer}");
            //        return;
            //    }

            //    int poweredGeneratorCount = 0;
            //    foreach(var g in groupDef.GeneratorInstances)
            //    {
            //        if (g.m_stateReplicator.State.status == ePowerGeneratorStatus.Powered) 
            //        {
            //            poweredGeneratorCount += 1;
            //        }
            //    }

            //    if (poweredGeneratorCount == groupDef.GeneratorInstances.Count && groupDef.PlayEndSequenceOnGroupComplete)
            //    {
            //        var coroutine = CoroutineManager.StartCoroutine(PlayGroupEndSequence(groupDef).WrapToIl2Cpp());
            //        WorldEventManager.m_worldEventEventCoroutines.Add(coroutine);
            //    }

            //    else 
            //    {
            //        int eventIndex = poweredGeneratorCount - 1;
            //        if(eventIndex >= 0 && eventIndex < groupDef.EventsOnInsertCell.Count)
            //        {
            //            groupDef.EventsOnInsertCell[eventIndex].ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, GameData.eWardenObjectiveEventTrigger.None, true));
            //        }
            //    }
            //});
        }

        internal void BuildIGGroupsLogic()
        {
            var expDef = ExpeditionDefinitionManager.Current.GetDefinition(ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.GeneratorGroups == null || expDef.GeneratorGroups.Count < 1) return;

            foreach(var generatorGroup in expDef.GeneratorGroups)
            {
                var generators = GatherIGs(generatorGroup);
                generatorGroups.Add((generators.ConvertAll(new Converter<LG_PowerGenerator_Core, IntPtr>(core => core.Pointer)).ToHashSet(), generatorGroup));

                foreach (var generator in generators)
                {
                    Configure(generator);
                }
            }
        }

        public void Init() { }

        private void Clear()
        {
            foreach (var generatorGroup in generatorGroups)
            {
                generatorGroup.groupDef.GeneratorInstances.Clear();
            }

            generatorGroups.Clear();
        }

        private ExpeditionIGGroupManager() 
        {
            LevelAPI.OnBuildDone += BuildIGGroupsLogic;
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        static ExpeditionIGGroupManager() { }
    }
}
