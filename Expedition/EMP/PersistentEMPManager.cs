using GTFO.API;
using LevelGeneration;
using System.Collections.Generic;
using System;
using System.Linq;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Instances;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using AK;
using ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    internal class PersistentEMPManager
    {
        public static PersistentEMPManager Current { get; private set; } = new();

        private List<(HashSet<IntPtr> group, ExpeditionIGGroup groupDef)> generatorGroups = new();

        public void Init() { }

        private void Clear()
        {
            foreach (var generatorGroup in generatorGroups)
            {
                generatorGroup.groupDef.GeneratorInstances.Clear();
            }

            generatorGroups.Clear();
        }

        private void BuildPersistentEMPs()
        {

        }

        private PersistentEMPManager()
        {
            LevelAPI.OnBuildDone += BuildPersistentEMPs;
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        static PersistentEMPManager() { }
    }
}
