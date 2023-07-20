//using HarmonyLib;
//using LevelGeneration;
//using ExtraObjectiveSetup.Utils;

//namespace ExtraObjectiveSetup.Patches.LGFactory
//{
//    [HarmonyPatch]
//    internal class Patch_LG_Factory_Setup
//    {
//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.Setup))]
//        private static void Post_LG_Factory_Setup(LG_Factory __instance)
//        {
//            BatchBuildManager.Current.Init();
//            EOSLogger.Debug($"BatchBuildManager setup completed");
//        }
//    }
//}
