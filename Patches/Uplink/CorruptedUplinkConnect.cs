using ExtraObjectiveSetup.Objectives.TerminalUplink;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using Localization;

namespace ExtraObjectiveSetup.Patches.Uplink
{
    [HarmonyPatch]
    internal static class CorruptedUplinkConnect
    {
        // rewrite the method to do more things
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TerminalCorruptedUplinkConnect))]
        private static bool Pre_LG_ComputerTerminalCommandInterpreter_TerminalCorruptedUplinkConnect(LG_ComputerTerminalCommandInterpreter __instance, string param1, string param2, ref bool __result)
        {
            var sender = __instance.m_terminal;
            if (sender.m_isWardenObjective) return true; // vanilla uplink

            __result = false; // this method always return false

            var receiver = sender.CorruptedUplinkReceiver;
            if (receiver == null)
            {
                EOSLogger.Error("TerminalCorruptedUplinkConnect() critical failure because terminal does not have a CorruptedUplinkReceiver.");
                return false;
            }

            if (LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId != 0U && LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId != sender.SyncID)
            {
                __instance.AddOngoingUplinkOutput();
                __result = false;
                return false;
            }

            LG_ComputerTerminalManager.OngoingUplinkConnectionTerminalId = sender.SyncID;

            var globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(sender);
            var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(sender);
            var uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);

            if (uplinkConfig.UseUplinkAddress)
            {
                param1 = param1.ToUpper();
                EOSLogger.Debug($"TerminalCorruptedUplinkConnect, param1: {param1}, TerminalUplink: {sender.UplinkPuzzle.ToString()}");
            }
            else
            {
                param1 = sender.UplinkPuzzle.TerminalUplinkIP.ToUpper();
                EOSLogger.Debug($"TerminalCorruptedUplinkConnect, not using uplink address, TerminalUplink: {sender.UplinkPuzzle.ToString()}");
            }

            if (!uplinkConfig.UseUplinkAddress || param1 == sender.UplinkPuzzle.TerminalUplinkIP)
            {
                if (receiver.m_command.HasRegisteredCommand(TERM_Command.TerminalUplinkConfirm))
                {
                    sender.m_command.AddUplinkCorruptedOutput();
                }
                else
                {
                    sender.m_command.AddUplinkCorruptedOutput();
                    sender.m_command.AddOutput("");
                    sender.m_command.AddOutput(TerminalLineType.ProgressWait, string.Format(Text.Get(3492863045), receiver.PublicName), 3f);
                    sender.m_command.AddOutput(TerminalLineType.Normal, Text.Get(2761366063), 0.6f);
                    sender.m_command.AddOutput("");
                    sender.m_command.AddOutput(TerminalLineType.Normal, Text.Get(3435969025), 0.8f);
                    receiver.m_command.AddCommand(
                        TERM_Command.TerminalUplinkConfirm,
                        "UPLINK_CONFIRM",
                        new LocalizedText() { UntranslatedText = Text.Get(112719254), Id = 0 },
                        TERM_CommandRule.OnlyOnceDelete);
                    receiver.m_command.AddOutput(TerminalLineType.Normal, string.Format(Text.Get(1173595354), sender.PublicName));
                }
            }
            else
            {
                sender.m_command.AddUplinkWrongAddressError(param1);
            }

            return false;
        }
    }
}
