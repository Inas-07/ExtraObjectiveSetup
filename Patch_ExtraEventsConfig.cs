﻿using HarmonyLib;
using Enemies;
using LevelGeneration;
using GameData;
using System.Collections;
using LEGACY.Utilities;
using Player;
using BepInEx.Unity.IL2CPP.Utils.Collections; // Bep 3.0
//using BepInEx.IL2CPP.Utils.Collections; // Bep 2.0

using SNetwork;
using Globals;
using AK;
using LEGACY.Patch.ExtraEventsConfig.SpawnEnemyWave_Custom;
namespace LEGACY.Patch.ExtraEventsConfig
{
    enum EventType
    {
        CloseSecurityDoor_Custom = 100,
        KillEnemiesInDimension_Custom = 101,
        SetTimerTitle_Custom = 102,
        ToggleEnableDisableAllTerminalsInZone_Custom = 103,
        ToggleEnableDisableTerminalInZone_Custom = 104,
        KillEnemiesInZone_Custom = 105,
        StopSpecifiedEnemyWave = 106
    }

    enum Extended_TERM_STATE
    {
        Disabled = 100
    }

    [HarmonyPatch]
    class Patch_ExtraEventsConfig
    {
        private static void SetTerminalCommand_Custom(WardenObjectiveEventData eventToTrigger)
        {
            LG_LayerType layer = eventToTrigger.Layer;
            eLocalZoneIndex localIndex = eventToTrigger.LocalIndex;
            eDimensionIndex dimensionIndex = eventToTrigger.DimensionIndex;
            LG_Zone terminalZone = null;
            Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layer, localIndex, out terminalZone);
            if (terminalZone == null)
            {
                Logger.Error("Failed to get terminal in zone {0}, layer {1}, dimension {2}.", localIndex, layer, dimensionIndex);
                return;
            }

            if (terminalZone.TerminalsSpawnedInZone == null)
            {
                Logger.Error("ExtraEventsConfig: terminalZone.TerminalsSpawnedInZone == null");
                return;
            }

            if (terminalZone.TerminalsSpawnedInZone.Count < 1)
            {
                Logger.Error("ExtraEventsConfig: No terminal spawns in the specified zone!");
                return;
            }

            if (eventToTrigger.Count >= terminalZone.TerminalsSpawnedInZone.Count)
            {
                Logger.Error("ExtraEventsConfig: Invalid event.Count: 0 < event.Count < TerminalsSpawnedInZone.Count should suffice.");
                return;
            }

            LG_ComputerTerminal terminal = terminalZone.TerminalsSpawnedInZone[eventToTrigger.Count];
            if (eventToTrigger.Enabled == true)
            {
                terminal.TrySyncSetCommandShow(eventToTrigger.TerminalCommand);
            }
            else
            {
                terminal.TrySyncSetCommandHidden(eventToTrigger.TerminalCommand);
            }
        }

        private static bool CloseSecurityDoor_Custom(WardenObjectiveEventData eventToTrigger)
        {
            LG_Zone zone = null;
            if(Builder.CurrentFloor.TryGetZoneByLocalIndex(eventToTrigger.DimensionIndex, eventToTrigger.Layer, eventToTrigger.LocalIndex, out zone) == false || zone == null)
            {
                Logger.Error("CloseSecurityDoor_Custom: Failed to get zone {0}, layer {1}, dimensionIndex {2}", eventToTrigger.LocalIndex, eventToTrigger.Layer, eventToTrigger.DimensionIndex);
                return false;
            }

            LG_SecurityDoor door = null;
            if(Utils.TryGetZoneEntranceSecDoor(zone, out door) == false || door == null)
            {
                Logger.Error("CloseSecurityDoor_Custom: failed to get LG_SecurityDoor!");
                return false;
            }

            pDoorState currentSyncState1 = door.m_sync.GetCurrentSyncState();
            if (currentSyncState1.status != eDoorStatus.Open && currentSyncState1.status != eDoorStatus.Opening)
                return false;
            Logger.Debug("Door Closed!");
            LG_Door_Sync lgDoorSync = door.m_sync.TryCast<LG_Door_Sync>();
            
            if (lgDoorSync == null) return false;
            
            pDoorState currentSyncState2 = lgDoorSync.GetCurrentSyncState() with
            {
                status = eDoorStatus.Closed,
                hasBeenOpenedDuringGame = false
            };

            lgDoorSync.m_stateReplicator.State = currentSyncState2;
            LG_Gate gate = door.Gate;
            gate.HasBeenOpenedDuringPlay = false;
            gate.IsTraversable = false;

            if (door.ActiveEnemyWaveData.HasActiveEnemyWave)
            {
                door.m_sound.Post(EVENTS.MONSTER_RUCKUS_FROM_BEHIND_SECURITY_DOOR_LOOP_START);
            }

            return true;
        }

        private static void KillEnemiesInZone(LG_Zone zone)
        {
            if (zone == null) return;

            for (int i = 0; i < zone.m_courseNodes.Count; ++i)
            {
                EnemyAgent[] array = zone.m_courseNodes[i].m_enemiesInNode.ToArray();
                for (int j = 0; j < array.Length; ++j)
                {
                    EnemyAgent enemyAgent = array[j];
                    if (enemyAgent != null && enemyAgent.Damage != null)
                    {
                        enemyAgent.Damage.MeleeDamage(float.MaxValue, null, UnityEngine.Vector3.zero, UnityEngine.Vector3.up, 0, 1f, 1f, 1f, 1f, false, DamageNoiseLevel.Normal);
                    }
                }
            }
        }

        private static void KillEnemiesInDimension_Custom(WardenObjectiveEventData eventToTrigger)
        {
            if (!SNet.IsMaster) return;
            Dimension dimension = null;
            Dimension.GetDimension(eventToTrigger.DimensionIndex, out dimension);

            for (int index1 = 0; index1 < dimension.Layers.Count; ++index1)
            {
                LG_Layer layer = dimension.Layers[index1];
                for (int index2 = 0; index2 < layer.m_zones.Count; ++index2)
                {
                    LG_Zone zone2 = layer.m_zones[index2];
                    LG_SecurityDoor door;

                    Utils.TryGetZoneEntranceSecDoor(zone2, out door);

                    // limited kill
                    if (index2 == 0 || (door != null && door.m_sync.GetCurrentSyncState().status == eDoorStatus.Open)) // door opened, kill all
                    {
                        KillEnemiesInZone(zone2);
                        //for (int index3 = 0; index3 < zone2.m_courseNodes.Count; ++index3)
                        //{
                        //    EnemyAgent[] array = zone2.m_courseNodes[index3].m_enemiesInNode.ToArray();
                        //    int num2 = 0;
                        //    for (int index4 = 0; index4 < array.Length; ++index4)
                        //    {
                        //        EnemyAgent enemyAgent = array[index4];
                        //        if (enemyAgent != null && enemyAgent.Damage != null)
                        //        {
                        //            enemyAgent.Damage.MeleeDamage(float.MaxValue, null, UnityEngine.Vector3.zero, UnityEngine.Vector3.up, 0, 1f, 1f, 1f, 1f, false, DamageNoiseLevel.Normal);
                        //            ++num2;
                        //        }
                        //    }
                        //}
                    }
                }
            }
        }

        private static void ToggleEnableDisableTerminal(LG_ComputerTerminal terminal, bool Enabled)
        {
            if (terminal == null) return;

            if (Enabled == false)
            {
                terminal.m_command.ClearOutputQueueAndScreenBuffer();
                //terminal.TrySyncSetCommandHidden(TERM_Command.MAX_COUNT);
            }
            else
            {
                terminal.m_command.AddInitialTerminalOutput();
                //terminal.TrySyncSetCommandShow(TERM_Command.MAX_COUNT);
            }

            terminal.transform.FindChild("Interaction").gameObject.active = Enabled;
            UnityEngine.Transform child = terminal.transform.FindChild("Graphics/kit_ElectronicsTerminalConsole/Display");
            if (child != null)
            {
                child.gameObject.active = Enabled;
            }


            // TODO: Implement state replicator

            //Logger.Error("terminal.TrySyncSetCommandHidden(TERM_Command.MAX_COUNT): {0}", terminal.CommandIsHidden(TERM_Command.MAX_COUNT));

            //pComputerTerminalState e;
            //Logger.Error("terminal.m_stateReplicator.m_currentState.CommandIsRemoved(TERM_Command.MAX_COUNT): {0}", terminal.m_stateReplicator.m_currentState.CommandIsRemoved(TERM_Command.MAX_COUNT));

        }

        private static void ToggleEnableDisableAllTerminalsInZone_Custom(WardenObjectiveEventData eventToTrigger)
        {
            WardenObjectiveEventData e = eventToTrigger;

            LG_Zone zone = null;
            Builder.CurrentFloor.TryGetZoneByLocalIndex(e.DimensionIndex, e.Layer, e.LocalIndex, out zone);
            if(zone == null)
            {
                Logger.Error("ToggleEnableDisableAllTerminalsInZone_Custom - Failed to find LG_Zone.");
                Logger.Error("DimensionIndex: {0}, Layer: {1}, LocalIndex: {2}", eventToTrigger.DimensionIndex, eventToTrigger.Layer, eventToTrigger.LocalIndex);
                return;
            }

            foreach(LG_ComputerTerminal terminalInZone in zone.TerminalsSpawnedInZone)
            {
                ToggleEnableDisableTerminal(terminalInZone, e.Enabled);
            }
        }

        private static void ToggleEnableDisableTerminalInZone_Custom(WardenObjectiveEventData eventToTrigger)
        {
            WardenObjectiveEventData e = eventToTrigger;

            if(e.Count < 0)
            {
                Logger.Error("ToggleEnableDisableTerminalInZone_Custom - Count < 0");
                return;
            }

            LG_Zone zone = null;
            Builder.CurrentFloor.TryGetZoneByLocalIndex(e.DimensionIndex, e.Layer, e.LocalIndex, out zone);
            if (zone == null)
            {
                Logger.Error("ToggleEnableDisableTerminalInZone_Custom - Failed to find LG_Zone.");
                Logger.Error("DimensionIndex: {0}, Layer: {1}, LocalIndex: {2}", eventToTrigger.DimensionIndex, eventToTrigger.Layer, eventToTrigger.LocalIndex);
                return;
            }

            if(e.Count >= zone.TerminalsSpawnedInZone.Count)
            {
                Logger.Error("ToggleEnableDisableTerminalInZone_Custom - Count >= Spawned terminal count");
                return;
            }

            ToggleEnableDisableTerminal(zone.TerminalsSpawnedInZone[e.Count], e.Enabled);
        }

        private static void KillEnemiesInZone_Custom(WardenObjectiveEventData eventToTrigger)
        {
            if (!SNet.IsMaster) return;

            WardenObjectiveEventData e = eventToTrigger;

            LG_Zone zone = null;
            Builder.CurrentFloor.TryGetZoneByLocalIndex(e.DimensionIndex, e.Layer, e.LocalIndex, out zone);
            if (zone == null)
            {
                Logger.Error("KillEnemiesInZone_Custom - Failed to find LG_Zone.");
                Logger.Error("DimensionIndex: {0}, Layer: {1}, LocalIndex: {2}", eventToTrigger.DimensionIndex, eventToTrigger.Layer, eventToTrigger.LocalIndex);
                return;
            }

            KillEnemiesInZone(zone);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WardenObjectiveManager), nameof(WardenObjectiveManager.CheckAndExecuteEventsOnTrigger), new System.Type[] {
            typeof(WardenObjectiveEventData),
            typeof(eWardenObjectiveEventTrigger),
            typeof(bool),
            typeof(float)
        })]
        private static bool Pre_CheckAndExecuteEventsOnTrigger(WardenObjectiveEventData eventToTrigger,
            eWardenObjectiveEventTrigger trigger,
            bool ignoreTrigger,
            float currentDuration)
        {
            if (eventToTrigger == null || !ignoreTrigger && eventToTrigger.Trigger != trigger || currentDuration != 0.0 && eventToTrigger.Delay <= currentDuration)
                return true;

            UnityEngine.Coroutine coroutine = null;

                // custom event
            switch ((int)eventToTrigger.Type)
            {
                case (int)EventType.CloseSecurityDoor_Custom:
                case (int)EventType.KillEnemiesInDimension_Custom:
                case (int)EventType.SetTimerTitle_Custom:
                case (int)EventType.ToggleEnableDisableAllTerminalsInZone_Custom:
                case (int)EventType.ToggleEnableDisableTerminalInZone_Custom:
                case (int)EventType.KillEnemiesInZone_Custom:
                    coroutine = CoroutineManager.StartCoroutine(Handle(eventToTrigger, currentDuration).WrapToIl2Cpp(), null);
                    WardenObjectiveManager.m_wardenObjectiveEventCoroutines.Add(coroutine);
                    return false;
                case (int)EventType.StopSpecifiedEnemyWave:
                    ExtraEventsConfig_SpawnEnemyWave_Custom.StopSpecifiedWave(eventToTrigger, ignoreTrigger, currentDuration);
                    return false;
            }

            // vanilla event modification
            switch (eventToTrigger.Type)
            {
                case eWardenObjectiveEventType.SetTerminalCommand:
                    coroutine = CoroutineManager.StartCoroutine(Handle(eventToTrigger, currentDuration).WrapToIl2Cpp(), null);
                    WardenObjectiveManager.m_wardenObjectiveEventCoroutines.Add(coroutine);
                    return false;
                case eWardenObjectiveEventType.SpawnEnemyWave:
                    bool use_vanilla_impl = ExtraEventsConfig_SpawnEnemyWave_Custom.SpawnWave(eventToTrigger, ignoreTrigger, currentDuration);
                    return use_vanilla_impl;
                case eWardenObjectiveEventType.StopEnemyWaves:
                    ExtraEventsConfig_SpawnEnemyWave_Custom.OnStopAllWave();
                    return true;

                default: return true;
            }
        }

        private static IEnumerator Handle(WardenObjectiveEventData eventToTrigger, float currentDuration)
        {
            WardenObjectiveEventData e = eventToTrigger;

            float delay = UnityEngine.Mathf.Max(e.Delay - currentDuration, 0f);
            if (delay > 0f)
            {
                yield return new UnityEngine.WaitForSeconds(delay);
            }

            WardenObjectiveManager.DisplayWardenIntel(e.Layer, e.WardenIntel);
            if (e.DialogueID > 0u)
            {
                PlayerDialogManager.WantToStartDialog(e.DialogueID, -1, false, false);
            }
            if (e.SoundID > 0u)
            {
                WardenObjectiveManager.Current.m_sound.Post(e.SoundID, true);
                var line = e.SoundSubtitle.ToString();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    GuiManager.PlayerLayer.ShowMultiLineSubtitle(line);
                }
            }

            switch((int)e.Type)
            {
                case (int)EventType.CloseSecurityDoor_Custom:
                    bool close_success = CloseSecurityDoor_Custom(e);
                    if (close_success == false)
                    {
                        break;
                    }

                    LG_Zone zone = null;
                    Builder.CurrentFloor.TryGetZoneByLocalIndex(eventToTrigger.DimensionIndex, eventToTrigger.Layer, eventToTrigger.LocalIndex, out zone);
                    if (zone != null && e.ClearDimension)
                    {
                        yield return new UnityEngine.WaitForSeconds(5.0f);
                        KillEnemiesInZone(zone);
                    }
                    break;
                case (int)EventType.KillEnemiesInDimension_Custom:
                    KillEnemiesInDimension_Custom(e);   break;
                case (int)EventType.ToggleEnableDisableAllTerminalsInZone_Custom:
                    ToggleEnableDisableAllTerminalsInZone_Custom(e);  break;
                case (int)EventType.ToggleEnableDisableTerminalInZone_Custom:
                    ToggleEnableDisableTerminalInZone_Custom(e); break;
                case (int)EventType.KillEnemiesInZone_Custom:
                    KillEnemiesInZone_Custom(e);                break;
                case (int)EventType.SetTimerTitle_Custom: {
                        float duration = e.Duration;

                        // set title
                        if (duration <= 0.0) // no idea why this fked up
                        {
                            // disable title
                            if (e.CustomSubObjectiveHeader.ToString().Length == 0)
                            {
                                GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(false);
                            }
                            // enable title
                            else
                            {
                                GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(true);
                                GuiManager.PlayerLayer.m_objectiveTimer.UpdateTimerTitle(e.CustomSubObjectiveHeader.ToString());
                                GuiManager.PlayerLayer.m_objectiveTimer.SetTimerTextEnabled(false);
                            }

                            break;
                        }

                        // count down
                        else
                        {
                            GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(true, true);
                            GuiManager.PlayerLayer.m_objectiveTimer.UpdateTimerTitle(e.CustomSubObjectiveHeader.ToString());
                            GuiManager.PlayerLayer.m_objectiveTimer.SetTimerTextEnabled(true);

                            UnityEngine.Color color;
                            if (UnityEngine.ColorUtility.TryParseHtmlString(e.CustomSubObjective.ToString(), out color) == false)
                            {
                                color.r = color.g = color.b = 255.0f;
                            }

                            var time = 0.0f;
                            while (time <= duration)
                            {
                                if (GameStateManager.CurrentStateName != eGameStateName.InLevel)
                                {
                                    break;
                                }

                                GuiManager.PlayerLayer.m_objectiveTimer.UpdateTimerText(duration - time, duration, color);
                                time += UnityEngine.Time.deltaTime;
                                yield return null;
                            }

                            GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(false, true);

                            break;
                        }
                }
            }

            switch(e.Type)
            {
                case eWardenObjectiveEventType.SetTerminalCommand:
                    SetTerminalCommand_Custom(e);       break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GS_AfterLevel), nameof(GS_AfterLevel.CleanupAfterExpedition))]
        private static void Post_CleanupAfterExpedition()
        {
            ExtraEventsConfig_SpawnEnemyWave_Custom.OnLevelCleanup();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Global), nameof(Global.OnApplicationQuit))]
        private static void Post_OnApplicationQuit()
        {
            ExtraEventsConfig_SpawnEnemyWave_Custom.OnApplicationQuit();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Mastermind), nameof(Mastermind.OnBuilderDone))]
        private static void Post_MastermindOnBuilderDone(Mastermind __instance)
        {
            ExtraEventsConfig_SpawnEnemyWave_Custom.OnMastermindBuildDone();
        }
    }
}