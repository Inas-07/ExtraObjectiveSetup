using System.Collections.Generic;
using GTFO.API;
using GameData;
using ChainedPuzzles;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.GeneratorCluster
{
    internal sealed class GeneratorClusterObjectiveManager : InstanceDefinitionManager<GeneratorClusterDefinition>
    {
        public static readonly GeneratorClusterObjectiveManager Current = new();

        protected override string DEFINITION_NAME { get; } = "GeneratorCluster";

        private List<(LG_PowerGeneratorCluster, GeneratorClusterDefinition)> chainedPuzzleToBuild = new();

        protected override void AddDefinitions(InstanceDefinitionsForLevel<GeneratorClusterDefinition> definitions)
        {
            // because we have chained puzzles, sorting is necessary to preserve chained puzzle instance order.
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        internal void RegisterForChainedPuzzleBuild(LG_PowerGeneratorCluster __instance, GeneratorClusterDefinition GeneratorClusterConfig) => chainedPuzzleToBuild.Add((__instance, GeneratorClusterConfig));

        private void BuildChainedPuzzleMidObjective()
        {
            foreach (var tuple in chainedPuzzleToBuild)
            {
                LG_PowerGeneratorCluster __instance = tuple.Item1;
                var config = tuple.Item2;
                uint persistentId = config.EndSequenceChainedPuzzle;

                var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(persistentId);

                if (block != null)
                {
                    EOSLogger.Debug($"GeneratorCluster: Building EndSequenceChainedPuzzle for LG_PowerGeneratorCluster in {__instance.SpawnNode.m_zone.LocalIndex}, {__instance.SpawnNode.LayerType}, {__instance.SpawnNode.m_dimension.DimensionIndex}");

                    __instance.m_chainedPuzzleMidObjective = ChainedPuzzleManager.CreatePuzzleInstance(
                        block,
                        __instance.SpawnNode.m_area,
                        __instance.m_chainedPuzzleAlignMidObjective.position,
                        __instance.m_chainedPuzzleAlignMidObjective);

                    __instance.m_chainedPuzzleMidObjective.OnPuzzleSolved += new System.Action(() => config.EventsOnEndSequenceChainedPuzzleComplete?.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true)));
                }
            }
        }

        private void OnBuildDone()
        {
            BuildChainedPuzzleMidObjective();
        }

        private GeneratorClusterObjectiveManager() : base()
        {
            //LevelAPI.OnBuildDone += OnBuildDone;
            BatchBuildManager.Current.Add_OnBeforeFactoryDone(OnBuildDone);
        }

        static GeneratorClusterObjectiveManager()
        {

        }
    }
}
