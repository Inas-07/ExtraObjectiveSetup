using ExtraObjectiveSetup.Objectives.TerminalUplink;
using ExtraObjectiveSetup.Instances;
using HarmonyLib;
using LevelGeneration;
using Localization;

namespace ExtraObjectiveSetup.Patches.Uplink
{
    [HarmonyPatch]
    internal static class TerminalUplinkSequenceOutput
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TerminalUplinkSequenceOutputs))]
        private static bool Pre_LG_ComputerTerminalCommandInterpreter_TerminalUplinkSequenceOutputs(LG_ComputerTerminal terminal, bool corrupted)
        {
            if (terminal.m_isWardenObjective) return true; // vanilla uplink

            // `terminal` is either sender or receiver 

            var globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(terminal);
            var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(terminal);
            var uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);
            if (uplinkConfig == null)
            {
                if (terminal.CorruptedUplinkReceiver != null)
                {
                    var receiver = terminal.CorruptedUplinkReceiver;
                    globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(receiver);
                    instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(receiver);
                    uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);

                    if (uplinkConfig == null || uplinkConfig.DisplayUplinkWarning) return true;
                }
                else
                {
                    return true;
                }
            }

            terminal.m_command.AddOutput(TerminalLineType.SpinningWaitNoDone, Text.Get(3418104670), 3f);
            terminal.m_command.AddOutput("");

            if (!corrupted)
            {
                terminal.m_command.AddOutput(string.Format(Text.Get(947485599), terminal.UplinkPuzzle.CurrentRound.CorrectPrefix));
            }

            return false;
        }

    }
}
