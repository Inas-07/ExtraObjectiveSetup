using LevelGeneration;
using GameData;
using System.Collections.Generic;
using System;

namespace ExtraObjectiveSetup.Utils
{
    public static partial class EOSTerminalUtils
    {
        /// <summary>
        /// Find terminals in the specified zone, optionally with a predicate, which found terminals must satisfy.
        /// 
        /// </summary>
        /// <param name="dimensionIndex"></param>
        /// <param name="layerType"></param>
        /// <param name="localIndex"></param>
        /// <param name="predicate"></param>
        /// <returns>
        /// NULL if the specified zone is not found, otherwise a list with terminals satisfying the predicate (if supplied)
        /// </returns>
        public static List<LG_ComputerTerminal> FindTerminal(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex, Predicate<LG_ComputerTerminal> predicate) 
        {
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

            List<LG_ComputerTerminal> result = new();
            foreach (var terminal in zone.TerminalsSpawnedInZone)
            {
                if(predicate != null)
                {
                    if(predicate(terminal))  
                        result.Add(terminal); 
                }
                else
                {
                    result.Add(terminal);
                }
            }

            return result;
        }

        public static TerminalLogFileData GetLocalLog(this LG_ComputerTerminal terminal, string logName)
        {
            var localLogs = terminal.GetLocalLogs();
            logName = logName.ToUpperInvariant();
            return localLogs.ContainsKey(logName) ? localLogs[logName] : null;
        }

        public static void ResetInitialOutput(this LG_ComputerTerminal terminal)
        {
            terminal.m_command.ClearOutputQueueAndScreenBuffer();
            terminal.m_command.AddInitialTerminalOutput();
            if (terminal.IsPasswordProtected)
            {
                terminal.m_command.AddPasswordProtectedOutput();
            }
        }
    }
}
