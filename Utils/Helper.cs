using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Utils
{
    public static class Helper
    {
        public static LG_ComputerTerminal SelectTerminal(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex, 
            eSeedType seedType, int staticSeed = 1,
            Predicate<LG_ComputerTerminal> predicate = null)
        {
            if (seedType == eSeedType.None)
            {
                EOSLogger.Error($"SelectTerminal: unsupported seed type {seedType}");
                return null;
            }

            if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layerType, localIndex, out var zone) || zone == null)
            {
                EOSLogger.Error($"SelectTerminal: Could NOT find zone {(dimensionIndex, layerType, localIndex)}");
                return null;
            }

            if (zone.TerminalsSpawnedInZone.Count <= 0)
            {
                EOSLogger.Error($"SelectTerminal: Could not find any terminals in zone {(dimensionIndex, layerType, localIndex)}");
                return null;
            }

            List<LG_ComputerTerminal> candidateTerminals = new();
            foreach (var terminal in zone.TerminalsSpawnedInZone)
            {
                if (predicate != null)
                {
                    if(predicate(terminal)) candidateTerminals.Add(terminal);
                }
                else
                {
                    candidateTerminals.Add(terminal);
                }
            }

            if (candidateTerminals.Count <= 0)
            {
                EOSLogger.Error($"SelectTerminal: Could not find any terminals without a password part in zone {(dimensionIndex, layerType, localIndex)}, putting the password on random (session) already used terminal.");
                return zone.TerminalsSpawnedInZone[Builder.SessionSeedRandom.Range(0, zone.TerminalsSpawnedInZone.Count, "NO_TAG")];
            }

            switch (seedType)
            {
                case eSeedType.SessionSeed:
                    return candidateTerminals[Builder.SessionSeedRandom.Range(0, candidateTerminals.Count, "NO_TAG")];
                case eSeedType.BuildSeed:
                    return candidateTerminals[Builder.BuildSeedRandom.Range(0, candidateTerminals.Count, "NO_TAG")];
                case eSeedType.StaticSeed:
                    UnityEngine.Random.InitState(staticSeed);
                    return candidateTerminals[UnityEngine.Random.Range(0, candidateTerminals.Count)];
                default:
                    EOSLogger.Error("SelectTerminal: did not have a valid SeedType!!");
                    return null;
            }
        }
    }
}
