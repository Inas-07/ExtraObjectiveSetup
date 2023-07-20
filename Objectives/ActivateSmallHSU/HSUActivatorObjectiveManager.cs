using GTFO.API;
using ExtraObjectiveSetup.Utils;
using ChainedPuzzles;
using GameData;
using UnityEngine;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.ActivateSmallHSU
{
    internal sealed class HSUActivatorObjectiveManager : InstanceDefinitionManager<HSUActivatorDefinition>
    {
        public static readonly HSUActivatorObjectiveManager Current = new();

        protected override string DEFINITION_NAME { get; } = "ActivateSmallHSU";

        protected override void AddDefinitions(InstanceDefinitionsForLevel<HSUActivatorDefinition> definitions)
        {
            // because we have chained puzzles, sorting is necessary to preserve chained puzzle instance order.
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        private void BuildHSUActivatorChainedPuzzle(HSUActivatorDefinition config)
        {
            var instance = HSUActivatorInstanceManager.Current.GetInstance(config.DimensionIndex, config.LayerType, config.LocalIndex, config.InstanceIndex);
            if (instance == null)
            {
                EOSLogger.Error($"Found unused HSUActivator config: {(config.DimensionIndex, config.LayerType, config.LocalIndex, config.InstanceIndex)}");
                return;
            }

            if (config.RequireItemAfterActivationInExitScan == true)
            {
                instance.m_sequencerExtractionDone.OnSequenceDone += new System.Action(() => {
                    WardenObjectiveManager.AddObjectiveItemAsRequiredForExitScan(true, new iWardenObjectiveItem[1] { new iWardenObjectiveItem(instance.m_linkedItemComingOut.Pointer) });
                    EOSLogger.Debug($"HSUActivator: {(config.DimensionIndex, config.LayerType, config.LocalIndex, config.InstanceIndex)} - added required item for extraction scan");
                });
            }

            if (config.TakeOutItemAfterActivation)
            {
                instance.m_sequencerExtractionDone.OnSequenceDone += new System.Action(() => {
                    instance.LinkedItemComingOut.m_navMarkerPlacer.SetMarkerVisible(true);
                });
            }

            if (config.ChainedPuzzleOnActivation != 0)
            {
                var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(config.ChainedPuzzleOnActivation);
                if (block == null)
                {
                    EOSLogger.Error($"HSUActivator: ChainedPuzzleOnActivation is specified but ChainedPuzzleDatablock definition is not found, won't build");
                }
                else
                {
                    Vector3 startPosition = config.ChainedPuzzleStartPosition.ToVector3();

                    if (startPosition == Vector3.zeroVector)
                    {
                        startPosition = instance.m_itemGoingInAlign.position;
                    }

                    var puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(
                        block,
                        instance.SpawnNode.m_area,
                        startPosition,
                        instance.SpawnNode.m_area.transform);

                    config.ChainedPuzzleOnActivationInstance = puzzleInstance;
                    EOSLogger.Debug($"HSUActivator: ChainedPuzzleOnActivation ID: {config.ChainedPuzzleOnActivation} specified and created");
                }
            }
        }

        private void OnBuildDone()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(BuildHSUActivatorChainedPuzzle);
        }

        private void OnLevelCleanup()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(def => def.ChainedPuzzleOnActivationInstance = null);
        }

        static HSUActivatorObjectiveManager()
        {
        }

        private HSUActivatorObjectiveManager() : base()
        {
            //LevelAPI.OnBuildDone += OnBuildDone;
            BatchBuildManager.Current.Add_OnBeforeFactoryDone(OnBuildDone);
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
        }
    }
}