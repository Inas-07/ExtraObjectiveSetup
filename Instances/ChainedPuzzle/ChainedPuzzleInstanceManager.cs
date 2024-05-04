using ChainedPuzzles;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using LevelGeneration;
using System;
using System.Collections.Generic;
using pCPState = ChainedPuzzles.pChainedPuzzleState;

namespace ExtraObjectiveSetup.Instances.ChainedPuzzle
{
    public class ChainedPuzzleInstanceManager : InstanceManager<ChainedPuzzleInstance>
    {
        public static ChainedPuzzleInstanceManager Current { get; } = new();

        private Dictionary<IntPtr, Action<pCPState, pCPState, bool>> Puzzles_OnStateChange { get; } = new();

        public override (eDimensionIndex dim, LG_LayerType layer, eLocalZoneIndex zone) GetGlobalZoneIndex(ChainedPuzzleInstance instance)
        {
            var node = instance.m_sourceArea.m_courseNode;
            return (node.m_dimension.DimensionIndex, node.LayerType, node.m_zone.LocalIndex);
        }

        public override uint Register((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalZoneIndex, ChainedPuzzleInstance instance)
        {
            uint instanceIndex = base.Register(globalZoneIndex, instance);
            if (instanceIndex != INVALID_INSTANCE_INDEX)
            {
                Puzzles_OnStateChange[instance.Pointer] = null;
            }

            return instanceIndex;
        }

        public void Add_OnStateChange(ChainedPuzzleInstance instance, Action<pCPState, pCPState, bool> action) => Add_OnStateChange(instance.Pointer, action);

        public void Add_OnStateChange(IntPtr pointer, Action<pCPState, pCPState, bool> action)
        {
            if (Puzzles_OnStateChange.ContainsKey(pointer))
            {
                Puzzles_OnStateChange[pointer] += action;
            }
            else
            {
                EOSLogger.Error($"ChainedPuzzleInstanceManager: passed in pointer is an unregistered ChainedPuzzleInstance, or is not a ChainedPuzzle");
                return;
            }
        }

        public void Remove_OnStateChange(ChainedPuzzleInstance instance, Action<pCPState, pCPState, bool> action) => Remove_OnStateChange(instance.Pointer, action);

        public void Remove_OnStateChange(IntPtr pointer, Action<pCPState, pCPState, bool> action)
        {
            if (Puzzles_OnStateChange.ContainsKey(pointer))
            {
                Puzzles_OnStateChange[pointer] -= action;
            }
            else
            {
                EOSLogger.Error($"ChainedPuzzleInstanceManager: passed in pointer is an unregistered ChainedPuzzleInstance, or is not a ChainedPuzzle");
                return;
            }
        }

        public Action<pCPState, pCPState, bool> Get_OnStateChange(ChainedPuzzleInstance instance) => Get_OnStateChange(instance.Pointer);

        public Action<pCPState, pCPState, bool> Get_OnStateChange(IntPtr pointer) => Puzzles_OnStateChange.TryGetValue(pointer, out var actions) ? actions : null;

        private void Clear()
        {
            Puzzles_OnStateChange.Clear();
        }

        private ChainedPuzzleInstanceManager()
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }
    }
}
