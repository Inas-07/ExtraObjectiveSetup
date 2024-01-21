using ChainedPuzzles;
using ExtraObjectiveSetup.Objectives.TerminalUplink;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using SNetwork;
using Localization;

namespace ExtraObjectiveSetup.Patches.Uplink
{
    [HarmonyPatch]
    internal class CorruptedUplinkConfirm
    {
        // rewrite the method to do more things
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.TerminalCorruptedUplinkConfirm))]
        private static bool Pre_LG_ComputerTerminalCommandInterpreter_TerminalCorruptedUplinkConfirm(LG_ComputerTerminalCommandInterpreter __instance, string param1, string param2, ref bool __result)
        {
            // invoked on receiver side
            // sender, receiver references are 'flipped'
            var receiver = __instance.m_terminal;
            var sender = __instance.m_terminal.CorruptedUplinkReceiver;

            if (sender == null)
            {
                EOSLogger.Error("TerminalCorruptedUplinkConfirm() critical failure because terminal does not have a CorruptedUplinkReceiver (sender).");
                __result = false;
                return false;
            }

            if (sender.m_isWardenObjective) return true; // vanilla uplink

            // TODO: config is unused here, prolly add more stuff
            var globalIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(sender);
            var instanceIndex = TerminalInstanceManager.Current.GetZoneInstanceIndex(sender);
            var uplinkConfig = UplinkObjectiveManager.Current.GetDefinition(globalIndex, instanceIndex);

            receiver.m_command.AddOutput(TerminalLineType.Normal, string.Format(Text.Get(2816126705), sender.PublicName));
            // vanilla code in this part is totally brain-dead
            if (sender.ChainedPuzzleForWardenObjective != null)
            {
                sender.ChainedPuzzleForWardenObjective.OnPuzzleSolved += new System.Action(() => {
                    receiver.m_command.StartTerminalUplinkSequence(string.Empty, true);
                    UplinkObjectiveManager.Current.ChangeState(sender, new() { Status = UplinkStatus.InProgress, CurrentRoundIndex = 0 });
                });
                sender.m_command.AddOutput("");
                sender.m_command.AddOutput(Text.Get(3268596368));
                sender.m_command.AddOutput(Text.Get(2277987284));

                receiver.m_command.AddOutput("");
                receiver.m_command.AddOutput(Text.Get(3268596368));
                receiver.m_command.AddOutput(Text.Get(2277987284));

                if (SNet.IsMaster)
                {
                    sender.ChainedPuzzleForWardenObjective.AttemptInteract(eChainedPuzzleInteraction.Activate);
                }
            }
            else
            {
                receiver.m_command.StartTerminalUplinkSequence(string.Empty, true);
                UplinkObjectiveManager.Current.ChangeState(sender, new() { Status = UplinkStatus.InProgress, CurrentRoundIndex = 0 });
            }

            __result = true;
            return false;
        }


    }
}
