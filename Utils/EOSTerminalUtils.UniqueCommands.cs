using ChainedPuzzles;
using LevelGeneration;
using ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition;
using GameData;
using GTFO.API.Extensions;
using Localization;
using UnityEngine;

namespace ExtraObjectiveSetup.Utils
{
    public static partial class EOSTerminalUtils
    {
        public static void AddUniqueCommand(LG_ComputerTerminal terminal, CustomCommand cmd)
        {
            if (terminal.m_command.m_commandsPerString.ContainsKey(cmd.Command))
            {
                EOSLogger.Error($"Duplicate command name: '{cmd.Command}', cannot add command");
                return;
            }

            if (!terminal.m_command.TryGetUniqueCommandSlot(out var uniqueCmdSlot))
            {
                EOSLogger.Error($"Cannot get more unique command slot, max: 5");
                return;
            }

            terminal.m_command.AddCommand(uniqueCmdSlot, cmd.Command, cmd.CommandDesc, cmd.SpecialCommandRule, cmd.CommandEvents.ToIl2Cpp(), cmd.PostCommandOutputs.ToIl2Cpp());
            for (int i = 0; i < cmd.CommandEvents.Count; i++)
            {
                var e = cmd.CommandEvents[i];
                if (e.ChainPuzzle != 0U)
                {
                    ChainedPuzzleDataBlock block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(e.ChainPuzzle);
                    if (block != null)
                    {
                        LG_Area sourceArea;
                        Transform transform;
                        if (terminal.SpawnNode != null)
                        {
                            sourceArea = terminal.SpawnNode.m_area;
                            transform = terminal.m_wardenObjectiveSecurityScanAlign;
                        }
                        else
                        {
                            sourceArea = terminal.ConnectedReactor?.SpawnNode?.m_area ?? null;
                            transform = terminal.ConnectedReactor?.m_chainedPuzzleAlign ?? null;
                        }

                        if (sourceArea == null)
                        {
                            EOSLogger.Error($"Terminal Source Area is not found! Cannot create chained puzzle for command {cmd.Command}!");
                            continue;
                        }

                        ChainedPuzzleInstance puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(block, sourceArea, transform.position, transform, e.UseStaticBioscanPoints);

                        var events = cmd.CommandEvents.GetRange(i, cmd.CommandEvents.Count - i).ToIl2Cpp(); 
                        puzzleInstance.OnPuzzleSolved += new System.Action(() => {
                            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(events, eWardenObjectiveEventTrigger.None, true);
                        });

                        terminal.SetChainPuzzleForCommand(uniqueCmdSlot, i, puzzleInstance);
                    }
                }
            }
        }
    }
}
