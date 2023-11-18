using HarmonyLib;
using ExtraObjectiveSetup.Expedition.EMP;
using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using LevelGeneration;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch(typeof(LG_Light))]
    internal static class Inject_LG_Light
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(LG_Light.Start))]
        internal static void Pre_Start(LG_Light __instance) => __instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPLightHandler());
    }
}
