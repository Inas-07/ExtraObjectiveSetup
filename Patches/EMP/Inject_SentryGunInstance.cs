using HarmonyLib;
using ExtraObjectiveSetup.Expedition.EMP;
using ExtraObjectiveSetup.Expedition.EMP.Handlers;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch(typeof(SentryGunInstance))]
    internal static class Inject_SentryGunInstance
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(SentryGunInstance.Setup))]
        internal static void Post_Setup(SentryGunInstance __instance)
        {
            __instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPSentryHandler());
        }
    }
}
