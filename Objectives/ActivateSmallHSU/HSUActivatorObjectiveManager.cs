using GTFO.API;
using ExtraObjectiveSetup.Utils;
using ChainedPuzzles;
using GameData;
using UnityEngine;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.BaseClasses;
using System.Collections.Generic;
using System;

namespace ExtraObjectiveSetup.Objectives.ActivateSmallHSU
{
    internal sealed class HSUActivatorObjectiveManager : InstanceDefinitionManager<HSUActivatorDefinition>
    {
        public static HSUActivatorObjectiveManager Current { get; private set; } = new();

        protected override string DEFINITION_NAME { get; } = "ActivateSmallHSU";

        protected override void AddDefinitions(InstanceDefinitionsForLevel<HSUActivatorDefinition> definitions)
        {
            // because we have chained puzzles, sorting is necessary to preserve chained puzzle instance order.
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        //private List<HSUActivatorDefinition> builtHSUActivatorPuzzles = new();

        // key: ChainedPuzzleInstance.Pointer
        private Dictionary<IntPtr, HSUActivatorDefinition> m_HSUActivatorPuzzles = new();

        private void BuildHSUActivatorChainedPuzzle(HSUActivatorDefinition def)
        {
            var instance = HSUActivatorInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
            if (instance == null)
            {
                EOSLogger.Error($"Found unused HSUActivator config: {(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex)}");
                return;
            }

            if (def.RequireItemAfterActivationInExitScan == true)
            {
                instance.m_sequencerExtractionDone.OnSequenceDone += new System.Action(() => {
                    WardenObjectiveManager.AddObjectiveItemAsRequiredForExitScan(true, new iWardenObjectiveItem[1] { new iWardenObjectiveItem(instance.m_linkedItemComingOut.Pointer) });
                    EOSLogger.Debug($"HSUActivator: {(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex)} - added required item for extraction scan");
                });
            }

            if (def.TakeOutItemAfterActivation)
            {
                instance.m_sequencerExtractionDone.OnSequenceDone += new System.Action(() => {
                    instance.LinkedItemComingOut.m_navMarkerPlacer.SetMarkerVisible(true);
                });
            }

            if (def.ChainedPuzzleOnActivation != 0)
            {
                var block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(def.ChainedPuzzleOnActivation);
                if (block == null)
                {
                    EOSLogger.Error($"HSUActivator: ChainedPuzzleOnActivation is specified but ChainedPuzzleDatablock definition is not found, won't build");
                }
                else
                {
                    Vector3 startPosition = def.ChainedPuzzleStartPosition.ToVector3();

                    if (startPosition == Vector3.zeroVector)
                    {
                        startPosition = instance.m_itemGoingInAlign.position;
                    }

                    var puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(
                        block,
                        instance.SpawnNode.m_area,
                        startPosition,
                        instance.SpawnNode.m_area.transform);

                    def.ChainedPuzzleOnActivationInstance = puzzleInstance;

                    m_HSUActivatorPuzzles[puzzleInstance.Pointer] = def;

                    // PuzzleInstance will be activated in SyncStateChanged
                    // EventsOnActivationScanSolved and HSU removeSequence will be executed in 
                    // ChainedPuzzleInstance.OnStateChanged(patch ChainedPuzzleInstance_OnPuzzleSolved)

                    EOSLogger.Debug($"HSUActivator: ChainedPuzzleOnActivation ID: {def.ChainedPuzzleOnActivation} specified and created");
                }
            }
            else
            {
                if (def.TakeOutItemAfterActivation)
                {
                    instance.m_triggerExtractSequenceRoutine = instance.StartCoroutine(instance.TriggerRemoveSequence());
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
            //if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            //definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(def => { def.ChainedPuzzleOnActivationInstance = null; });
            foreach(var h in m_HSUActivatorPuzzles.Values)
            {
                h.ChainedPuzzleOnActivationInstance = null;
            }

            m_HSUActivatorPuzzles.Clear();
        }

        internal HSUActivatorDefinition GetHSUActivatorDefinition(ChainedPuzzleInstance chainedPuzzle) => m_HSUActivatorPuzzles.TryGetValue(chainedPuzzle.Pointer, out var def) ? def : null;

        static HSUActivatorObjectiveManager()
        {
        }

        private HSUActivatorObjectiveManager() : base()
        {
            LevelAPI.OnBuildDone += OnBuildDone;
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
            LevelAPI.OnBuildStart += OnLevelCleanup;
        }
    }
}