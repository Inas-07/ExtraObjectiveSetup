using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Tweaks.TerminalPosition;
using ExtraObjectiveSetup.Utils;

namespace ExtraObjectiveSetup.Patches.Terminal
{
    [HarmonyPatch]
    internal static class Patch_LG_ComputerTerminal_Setup
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.Setup))]
        private static void Post_LG_ComputerTerminal_Setup(LG_ComputerTerminal __instance)
        {
            uint instanceIndex = TerminalInstanceManager.Current.Register(__instance);
            TerminalInstanceManager.Current.SetupTerminalWrapper(__instance);

            // modify terminal position
            if (__instance.SpawnNode == null) return; // disallow changing position of reactor terminal

            var globalZoneIndex = TerminalInstanceManager.Current.GetGlobalZoneIndex(__instance);
            var _override = TerminalPositionOverrideManager.Current.GetDefinition(globalZoneIndex, instanceIndex);

            if (_override == null) return;

            if (_override.Position.ToVector3() != UnityEngine.Vector3.zeroVector)
            {
                __instance.transform.position = _override.Position.ToVector3();
                __instance.transform.rotation = _override.Rotation.ToQuaternion();
            }

            EOSLogger.Debug($"TerminalPositionOverride: {_override.LocalIndex}, {_override.LayerType}, {_override.DimensionIndex}, TerminalIndex {_override.InstanceIndex}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.SetupAsWardenObjectiveTerminalUplink))]
        [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.SetupAsWardenObjectiveCorruptedTerminalUplink))]
        private static void Post_LG_ComputerTerminal_UplinkSetup(LG_ComputerTerminal __instance)
        {
            TerminalInstanceManager.Current.RegisterWardenUplink(__instance);
        }
    }
}
