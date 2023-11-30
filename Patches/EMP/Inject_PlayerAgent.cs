using HarmonyLib;
using ExtraObjectiveSetup.Expedition.EMP;
using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using Player;

namespace ExtraObjectiveSetup.Patches.EMP
{
    [HarmonyPatch(typeof(PlayerAgent))]
    internal static class Inject_PlayerAgent
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(nameof(PlayerAgent.Setup))]
        internal static void Post_Setup(PlayerAgent __instance)
        {
            if (!__instance.IsLocallyOwned)
                return;
            __instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPPlayerFlashLightHandler());
            __instance.gameObject.AddComponent<EMPController>().AssignHandler(new EMPPlayerHudHandler());
            EMPManager.Current.SetLocalPlayerAgent(__instance);
        }
    }
}
