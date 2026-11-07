using ExtraObjectiveSetup.Instances;
using HarmonyLib;
using LevelGeneration;
using SNetwork;

namespace ExtraObjectiveSetup.Patches.Expedition
{
    /// <summary>
    /// Routes terminal interaction into <see cref="Tweaks.TerminalTweak.TerminalWrapper"/> so the
    /// expedition terminal definitions can fire events on approach, on password input and on logfile read.
    /// </summary>
    [HarmonyPatch]
    internal static class TerminalWrapper_ExpeditionPatches
    {
        [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.OnProximityEnter))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Post_OnProximityEnter(LG_ComputerTerminal __instance)
        {
            if (!SNet.IsMaster) return;

            TerminalInstanceManager.Current.GetTerminalWrapper(__instance)?.ChangeState();
        }

        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.ReceiveCommand))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        private static void Pre_ReceiveCommand(LG_ComputerTerminalCommandInterpreter __instance, TERM_Command cmd, string param1)
        {
            var wrapper = TerminalInstanceManager.Current.GetTerminalWrapper(__instance.m_terminal);
            wrapper?.ReceiveCommand(cmd, param1.ToUpperInvariant());
        }
    }
}
