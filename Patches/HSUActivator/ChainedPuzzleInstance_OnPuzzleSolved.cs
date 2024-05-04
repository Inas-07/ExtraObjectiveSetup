//using ChainedPuzzles;
//using ExtraObjectiveSetup.Instances;
//using ExtraObjectiveSetup.Objectives.ActivateSmallHSU;
//using ExtraObjectiveSetup.Utils;
//using GTFO.API.Extensions;
//using HarmonyLib;
//using SNetwork;


//namespace ExtraObjectiveSetup.Patches.HSUActivator
//{
//    [HarmonyPatch]

//    internal static class ChainedPuzzleInstance_OnPuzzleSolved
//    {
//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.OnStateChange))]
//        private static void Post_ChainedPuzzleOnActivationInstance_OnStateChange(ChainedPuzzleInstance __instance, 
//            pChainedPuzzleState oldState, pChainedPuzzleState newState, bool isRecall)
//        {
//            if (isRecall || newState.status != eChainedPuzzleStatus.Solved) return;

//            var def = HSUActivatorObjectiveManager.Current.GetHSUActivatorDefinition(__instance); 
            
//            if (def == null) return;

//            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(def.EventsOnActivationScanSolved.ToIl2Cpp(), GameData.eWardenObjectiveEventTrigger.None, true);
//            var HSUActivator = HSUActivatorInstanceManager.Current.GetInstance(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex);
//            if(HSUActivator == null)
//            {
//                EOSLogger.Error("HSUActivatorInstanceManager cannot find a registered instance but we can get a built ChainedPuzzle! What the heck>???");
//                return;
//            }

//            if(def.TakeOutItemAfterActivation)
//            {
//                HSUActivator.m_triggerExtractSequenceRoutine = HSUActivator.StartCoroutine(HSUActivator.TriggerRemoveSequence());
//            }
//        }
//    }
//}
