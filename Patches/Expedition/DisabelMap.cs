using CellMenu;
using ExtraObjectiveSetup.Expedition;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using Player;


namespace ExtraObjectiveSetup.Patches.Expedition
{
    [HarmonyPatch]
    [HarmonyWrapSafe]
    internal class DisabelMap
    {
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CM_PageMap), nameof(CM_PageMap.UpdatePlayerData))]
        //private static void Pre_CM_PageMap_OnEnable()
        //{
        //    var map = MainMenuGuiLayer.Current.PageMap;
        //    if (map == null || RundownManager.ActiveExpedition == null) return;
        //    var def = ExpeditionDefinitionManager.Current.GetDefinition(RundownManager.ActiveExpedition.LevelLayoutData);
        //    if (def != null && def.DisableMap)
        //    {
        //        map.SetMapVisualsIsActive(false);
        //        map.SetMapDisconnetedTextIsActive(true);
        //        EOSLogger.Debug("ExpeditionDefinitionManager: Map disabled");
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.TryWarpTo), new System.Type[] { 
        //    typeof(eDimensionIndex),
        //    typeof(UnityEngine.Vector3),
        //    typeof(UnityEngine.Vector3),
        //    typeof(bool),
        //})]
        //private static void Post_TryWarpTo(PlayerAgent __instance, bool __result)
        //{
        //    if (__result == false) return;
        //    if (__instance.DimensionIndex != eDimensionIndex.Reality) return;

        //    var def = ExpeditionDefinitionManager.Current.GetDefinition(RundownManager.ActiveExpedition.LevelLayoutData);
        //    if (def == null) return;

        //    if(def.DisableMap)
        //    {
        //        MainMenuGuiLayer.Current.PageMap.SetMapDisconnetedTextIsActive(true);
        //    }
        //}
    }
}
