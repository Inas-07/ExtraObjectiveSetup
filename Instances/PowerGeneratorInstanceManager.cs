﻿using GameData;
using GTFO.API;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class PowerGeneratorInstanceManager: InstanceManager<LG_PowerGenerator_Core>
    {
        public static PowerGeneratorInstanceManager Current { get; private set; } = new();

        //private HashSet<IntPtr> gcGenerators = new();

        private Dictionary<IntPtr, LG_PowerGeneratorCluster> gcGenerators = new();

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalZoneIndex(LG_PowerGenerator_Core instance) => (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);

        public override uint Register((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalZoneIndex, LG_PowerGenerator_Core instance) 
        { 
            if(gcGenerators.ContainsKey(instance.Pointer))
            {
                EOSLogger.Error("PowerGeneratorInstanceManager: Trying to register a GC Generator, which is an invalid operation");
                return INVALID_INSTANCE_INDEX;
            }

            return base.Register(globalZoneIndex, instance);
        }

        /// <summary>
        /// Mark the yet setup-ed LG_PowerGenerator_Core as a generator spawned with a generator cluster.
        /// These generator instance will not be registered 
        /// </summary>
        /// <param name="child">The unsetuped LG_PowerGenerator_Core instance.</param>
        public void MarkAsGCGenerator(LG_PowerGeneratorCluster parent, LG_PowerGenerator_Core child)
        {
            if(IsRegistered(child))
            {
                EOSLogger.Error("PowerGeneratorInstanceManager: Trying to mark a registered Generator as GC Generator, which is an invalid operation");
                return;
            }

            gcGenerators[child.Pointer] = parent;
        }

        public bool IsGCGenerator(LG_PowerGenerator_Core instance) => gcGenerators.ContainsKey(instance.Pointer);

        public LG_PowerGeneratorCluster GetParentGeneratorCluster(LG_PowerGenerator_Core instance) => gcGenerators.TryGetValue(instance.Pointer, out var gc) ? gc : null;

        private void OutputLevelInstanceInfo()
        {
            StringBuilder s = new();
            s.AppendLine();

            foreach (var globalZoneIndex in RegisteredZones())
            {
                s.AppendLine($"{globalZoneIndex.Item3}, {globalZoneIndex.Item2}, Dim {globalZoneIndex.Item1}");

                List<LG_PowerGenerator_Core> PGInstanceInZone = GetInstancesInZone(globalZoneIndex);
                for (int instanceIndex = 0; instanceIndex < PGInstanceInZone.Count; instanceIndex++)
                {
                    var PGInstance = PGInstanceInZone[instanceIndex];
                    s.AppendLine($"GENERATOR_{PGInstance.m_serialNumber}. Instance index: {instanceIndex}");
                }

                s.AppendLine();
            }

            string msg = s.ToString();

            if (!string.IsNullOrWhiteSpace(msg))
                EOSLogger.Debug(s.ToString());
        }

        private void Clear()
        {
            gcGenerators.Clear();
        }

        private PowerGeneratorInstanceManager() 
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
            LevelAPI.OnEnterLevel += OutputLevelInstanceInfo;
        }

        static PowerGeneratorInstanceManager()
        {

        }
    }
}
