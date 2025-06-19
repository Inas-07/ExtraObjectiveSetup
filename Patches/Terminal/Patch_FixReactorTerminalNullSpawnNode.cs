using AIGraph;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraObjectiveSetup.Patches.Terminal
{
    [HarmonyPatch]
    internal static class Patch_FixReactorTerminalNullSpawnNode
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.SpawnNode), MethodType.Getter)]
        private static bool Pre(LG_ComputerTerminal __instance, ref AIG_CourseNode __result)
        {
            //EOSLogger.Error($"LG_ComputerTerminal.SpawnNode: reactor == null {__instance.ConnectedReactor == null}, spawn node == null {__instance.m_terminalItem.SpawnNode == null}");
            if (__instance.ConnectedReactor != null && __instance.m_terminalItem.SpawnNode == null)
            {
                __result = __instance.ConnectedReactor.SpawnNode;
                return false;
            }

            return true;
        }
    }
}
