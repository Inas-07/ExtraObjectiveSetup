using LevelGeneration;
using GameData;
using System.Collections.Generic;
using System;
using ChainedPuzzles;
using Il2cppTerminalList = Il2CppSystem.Collections.Generic.List<LevelGeneration.LG_ComputerTerminal>;

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

        /// <summary>
        /// Obtain list of command events for a specific command. \n
        /// This is a workaround method of terminal.m_command.TryGetCommandEvents:\n 
        /// The method does not work because in the method signature `events` are modified with `out`, and it messes up with il2cppList.\n
        /// This method extracts command events from level layout data instead.
        /// TODO: add support for reactor terminnal custom command.
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="command"></param>
        /// <returns>Aforemention event list.\nReturn an empty list if command is not found, or `terminal` is actually reactor terminal</returns>        
        public static List<WardenObjectiveEventData> GetUniqueCommandEvents(this LG_ComputerTerminal terminal, string command)
        {
            var EMPTY = new List<WardenObjectiveEventData>();

            if (terminal.ConnectedReactor != null) return EMPTY; // reactor terminal

            uint layoutID = 0u;
            if (terminal.SpawnNode.m_dimension.IsMainDimension)
            {
                switch (terminal.SpawnNode.LayerType)
                {
                    case LG_LayerType.MainLayer: layoutID = RundownManager.ActiveExpedition.LevelLayoutData; break;
                    case LG_LayerType.SecondaryLayer: layoutID = RundownManager.ActiveExpedition.SecondaryLayout; break;
                    case LG_LayerType.ThirdLayer: layoutID = RundownManager.ActiveExpedition.ThirdLayout; break;
                    default: EOSLogger.Error($"GetCommandEvents: Unimplemented layer type {terminal.SpawnNode.LayerType}"); return EMPTY;
                }
            }
            else
            {
                layoutID = terminal.SpawnNode.m_dimension.DimensionData.LevelLayoutData;
            }

            // __instance.m_commandEventMap.TryGetValue is unusable. Get around this by getting it from gamedatablock.
            LevelLayoutDataBlock levellayoutData = GameDataBlockBase<LevelLayoutDataBlock>.GetBlock(layoutID);
            if(levellayoutData == null) 
            {
                EOSLogger.Error($"GetCommandEvents: {terminal.ItemKey} is in {terminal.SpawnNode.LayerType}, {terminal.SpawnNode.m_dimension.DimensionIndex} but its LevelLayoutData is not found!");
                return EMPTY;
            }

            // CRITICAL: The order of spawning terminals is the same to that of specifying terminalplacementdatas in the datablock!
            Il2cppTerminalList TerminalsInZone = terminal.SpawnNode.m_zone.TerminalsSpawnedInZone;
            int TerminalDataIndex = TerminalsInZone.IndexOf(terminal);

            ExpeditionZoneData TargetZoneData = null;
            foreach (ExpeditionZoneData zonedata in levellayoutData.Zones)
            {
                if (zonedata.LocalIndex == terminal.SpawnNode.m_zone.LocalIndex)
                {
                    TargetZoneData = zonedata;
                    break;
                }
            }

            if (TargetZoneData == null)
            {
                EOSLogger.Error("GetCommandEvents: Cannot find target zone data.");
                return EMPTY;
            }

            if (TerminalDataIndex >= TargetZoneData.TerminalPlacements.Count)
            {
                EOSLogger.Debug("GetCommandEvents: TerminalDataIndex >= TargetZoneData.TerminalPlacements.Count: found a custom terminal, skipped");
                return EMPTY;
            }

            var UniqueCommands = TargetZoneData.TerminalPlacements[TerminalDataIndex].UniqueCommands;
            foreach(var cmd in UniqueCommands)
            {
                if (cmd.Command.ToLower().Equals(command.ToLower()))
                {
                    return cmd.CommandEvents.ToManaged();
                }
            }

            EOSLogger.Error($"GetCommandEvents: command '{command}' not found on {terminal.ItemKey}");
            return EMPTY;
        }
    }
}
