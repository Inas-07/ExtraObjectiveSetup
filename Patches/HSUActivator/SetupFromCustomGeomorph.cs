using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using GameData;
using SNetwork;
using Player;
using ExtraObjectiveSetup.Objectives.ActivateSmallHSU;
using ExtraObjectiveSetup.Instances;
using System;

namespace ExtraObjectiveSetup.Patches.HSUActivator
{
    [HarmonyPatch]
    internal class SetupFromCustomGeomorph
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_HSUActivator_Core), nameof(LG_HSUActivator_Core.SetupFromCustomGeomorph))]
        private static void Post_LG_HSUActivator_Core_SetupFromCustomGeomorph(LG_HSUActivator_Core __instance)
        {
            uint instanceIndex = HSUActivatorInstanceManager.Current.Register(__instance);

            HSUActivatorDefinition def = HSUActivatorObjectiveManager.Current.GetDefinition(__instance.SpawnNode.m_dimension.DimensionIndex, __instance.SpawnNode.LayerType, __instance.SpawnNode.m_zone.LocalIndex, instanceIndex);

            if (def == null) return;
            if (__instance.m_isWardenObjective)
            {
                EOSLogger.Error($"BuildCustomHSUActivator: the HSUActivator has been set up by vanilla! Aborting custom setup...");
                EOSLogger.Error($"HSUActivator in {__instance.SpawnNode.m_zone.LocalIndex}, {__instance.SpawnNode.LayerType}, {__instance.SpawnNode.m_dimension.DimensionIndex}");
                return;
            }

            // LG_HSUActivator_Core.Setup
            __instance.m_linkedItemGoingIn = __instance.SpawnPickupItemOnAlign(def.ItemFromStart, __instance.m_itemGoingInAlign, false, -1);
            __instance.m_linkedItemComingOut = __instance.SpawnPickupItemOnAlign(def.ItemAfterActivation, __instance.m_itemComingOutAlign, false, -1);

            LG_LevelInteractionManager.DeregisterTerminalItem(__instance.m_linkedItemGoingIn.GetComponentInChildren<iTerminalItem>());
            LG_LevelInteractionManager.DeregisterTerminalItem(__instance.m_linkedItemComingOut.GetComponentInChildren<iTerminalItem>());
            __instance.m_linkedItemGoingIn.SetPickupInteractionEnabled(false);
            __instance.m_linkedItemComingOut.SetPickupInteractionEnabled(false);

            // reset, do nothing
            // do not interfere with warden objective
            __instance.m_insertHSUInteraction.OnInteractionSelected = new Action<PlayerAgent>((p) => { });

            __instance.m_sequencerInsertItem.OnSequenceDone = new Action(() =>
            {
                pHSUActivatorState state = __instance.m_stateReplicator.State;
                if (!state.isSequenceIncomplete)
                    EOSLogger.Log(">>>>>> HSUInsertSequenceDone! Sequence was already complete");
                state.isSequenceIncomplete = false;
                __instance.m_stateReplicator.SetStateUnsynced(state);
                EOSLogger.Log(">>>>>> HSUInsertSequenceDone!");
                if (__instance.m_triggerExtractSequenceRoutine != null)
                {
                    __instance.StopCoroutine(__instance.m_triggerExtractSequenceRoutine);
                }
            });

            __instance.m_sequencerExtractItem.OnSequenceDone = new Action(() =>
            {
                __instance.m_stateReplicator.SetStateUnsynced(__instance.m_stateReplicator.State with
                {
                    isSequenceIncomplete = true
                });
                if (SNet.IsMaster)
                {
                    __instance.AttemptInteract(new pHSUActivatorInteraction()
                    {
                        type = eHSUActivatorInteractionType.SetExtractDone
                    });
                }
            });

            EOSLogger.Debug($"HSUActivator: {(def.DimensionIndex, def.LayerType, def.LocalIndex, def.InstanceIndex)}, custom setup complete");
        }
    }
}
