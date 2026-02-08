using ExtraObjectiveSetup.Objectives.TerminalUplink;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using Localization;
using GameData;
namespace ExtraObjectiveSetup.Patches.Uplink
{
    [HarmonyPatch]
    internal static class StartTerminalUplinkSequence
    {
        // rewrite is indispensable
        // both uplink and corruplink call this method
        // uplink calls on uplink terminal
        // corruplink calls on receiver side
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.StartTerminalUplinkSequence))]
        private static bool Pre_LG_ComputerTerminalCommandInterpreter_StartTerminalUplinkSequence(LG_ComputerTerminalCommandInterpreter __instance, string uplinkIp, bool corrupted)
        {
            // normal uplink
            if (!corrupted)
            {
                var uplinkTerminal = __instance.m_terminal;
                if (uplinkTerminal.m_isWardenObjective) return true;

                var globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(uplinkTerminal);
                var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(uplinkTerminal);
                var uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);

                uplinkTerminal.m_command.AddOutput(TerminalLineType.ProgressWait, string.Format(Text.Get(2583360288), uplinkIp), 3f);
                __instance.TerminalUplinkSequenceOutputs(uplinkTerminal, false);

                uplinkTerminal.m_command.OnEndOfQueue = new System.Action(() =>
                {
                    EOSLogger.Debug("UPLINK CONNECTION DONE!");
                    uplinkTerminal.UplinkPuzzle.Connected = true;
                    uplinkTerminal.UplinkPuzzle.CurrentRound.ShowGui = true;
                    uplinkTerminal.UplinkPuzzle.OnStartSequence();
                    uplinkConfig.EventsOnCommence.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));

                    int i = uplinkConfig.RoundOverrides.FindIndex(o => o.RoundIndex == 0);
                    UplinkRound firstRoundOverride = i != -1 ? uplinkConfig.RoundOverrides[i] : null;
                    firstRoundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnStart, false));
                });
            }

            // corruplink
            else
            {
                // corruplink
                var receiver = __instance.m_terminal;
                var sender = __instance.m_terminal.CorruptedUplinkReceiver;

                if (sender.m_isWardenObjective) return true; // vanilla uplink

                var globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(sender);
                var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(sender);
                var uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);

                sender.m_command.AddOutput(TerminalLineType.ProgressWait, string.Format(Text.Get(2056072887), sender.PublicName), 3f);
                sender.m_command.AddOutput("");
                receiver.m_command.AddOutput(TerminalLineType.ProgressWait, string.Format(Text.Get(2056072887), sender.PublicName), 3f);
                receiver.m_command.AddOutput("");

                receiver.m_command.TerminalUplinkSequenceOutputs(sender, false);
                receiver.m_command.TerminalUplinkSequenceOutputs(receiver, true);

                receiver.m_command.OnEndOfQueue = new System.Action(() =>
                {
                    EOSLogger.Debug("UPLINK CONNECTION DONE!");
                    sender.UplinkPuzzle.Connected = true;
                    sender.UplinkPuzzle.CurrentRound.ShowGui = true;
                    sender.UplinkPuzzle.OnStartSequence();
                    uplinkConfig.EventsOnCommence.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));

                    int i = uplinkConfig.RoundOverrides.FindIndex(o => o.RoundIndex == 0);
                    UplinkRound firstRoundOverride = i != -1 ? uplinkConfig.RoundOverrides[i] : null;
                    firstRoundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnStart, false));
                });
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.StartTerminalUplinkSequence))]
        private static void Post_LG_ComputerTerminalCommandInterpreter_StartTerminalUplinkSequence(LG_ComputerTerminalCommandInterpreter __instance, bool corrupted)
        {
            // normal uplink
            if (!corrupted)
            {
                var uplinkTerminal = __instance.m_terminal;
                if (!uplinkTerminal.m_isWardenObjective) return;

                var uplinkConfig = UplinkObjectiveManager.Current.GetWardenDefinition(__instance.m_terminal);
                if (uplinkConfig == null) return;

                uplinkTerminal.m_command.OnEndOfQueue += new System.Action(() =>
                {
                    uplinkConfig.EventsOnCommence.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));

                    int i = uplinkConfig.RoundOverrides.FindIndex(o => o.RoundIndex == 0);
                    UplinkRound firstRoundOverride = i != -1 ? uplinkConfig.RoundOverrides[i] : null;
                    firstRoundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnStart, false));
                });
            }
            // corruplink
            else
            {
                // corruplink
                var receiver = __instance.m_terminal;
                var sender = __instance.m_terminal.CorruptedUplinkReceiver;

                if (!sender.m_isWardenObjective) return; // vanilla uplink

                var uplinkConfig = UplinkObjectiveManager.Current.GetWardenDefinition(sender);
                if (uplinkConfig == null) return;

                receiver.m_command.OnEndOfQueue = new System.Action(() =>
                {
                    uplinkConfig.EventsOnCommence.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));

                    int i = uplinkConfig.RoundOverrides.FindIndex(o => o.RoundIndex == 0);
                    UplinkRound firstRoundOverride = i != -1 ? uplinkConfig.RoundOverrides[i] : null;
                    firstRoundOverride?.EventsOnRound.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.OnStart, false));
                });
            }
        }
    }
}
