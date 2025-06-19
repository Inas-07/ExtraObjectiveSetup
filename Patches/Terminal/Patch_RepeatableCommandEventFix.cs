using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Tweaks.TerminalTweak;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;


namespace ExtraObjectiveSetup.Patches.Terminal
{
    [HarmonyPatch]
    internal static class Patch_RepeatableCommandEventFix
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.SetupCommandEvents))]
        private static void Post_ResetRepeatableUniqueCommandChainedPuzzle(LG_ComputerTerminalCommandInterpreter __instance)
        {
            var terminal = __instance.m_terminal;

            foreach(var CMD in TerminalInstanceManager.UNIQUE_CMDS)
            {
                if(!__instance.m_commandsPerEnum.ContainsKey(CMD)) 
                {
                    // vanilla use UniqueCommands only in order,
                    // but anyway we iterate through instead of just return
                    continue;
                }

                var cmdName = __instance.m_commandsPerEnum[CMD];

                if (__instance.m_terminal.GetCommandRule(CMD) != TERM_CommandRule.Normal) continue;

                var cmdEvents = __instance.m_terminal.GetUniqueCommandEvents(cmdName);
                for(int i = 0; i < cmdEvents.Count; i++)
                {
                    if (cmdEvents[i].ChainPuzzle == 0) continue;

                    if (__instance.m_terminal.TryGetChainPuzzleForCommand(CMD, i, out var cpInstance) && cpInstance != null)
                    {
                        cpInstance.OnPuzzleSolved += new System.Action(cpInstance.ResetProgress);
                    }

                    EOSLogger.Debug($"TerminalTweak: {terminal.ItemKey}, command {cmdName} set to be repeatable!");
                }
            }
        }
    }
}
