using ChainedPuzzles;
using LevelGeneration;
using ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition;
using GameData;
using GTFO.API.Extensions;
using Localization;
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
                        ChainedPuzzleInstance puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(block, terminal.SpawnNode.m_area, terminal.m_wardenObjectiveSecurityScanAlign.position, terminal.m_wardenObjectiveSecurityScanAlign, e.UseStaticBioscanPoints);
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
