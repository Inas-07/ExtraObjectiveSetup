﻿//using HarmonyLib;
//using Enemies;
//using LevelGeneration;
//using GameData;
//using AIGraph;
//using System.Collections.Generic;
//using GTFO.API;
//using ExtraObjectiveSetup.Utils;
//namespace ExtraObjectiveSetup.Patches
//{
//    [HarmonyPatch]
//    internal class Patch_EventsOnBossDeath
//    {
//        private static Dictionary<ushort, Il2CppSystem.Collections.Generic.List<WardenObjectiveEventData>> bossesWithDeathEvents = null;

//        private static void checkInit()
//        {
//            if (bossesWithDeathEvents != null) return;

//            bossesWithDeathEvents = new Dictionary<ushort, Il2CppSystem.Collections.Generic.List<WardenObjectiveEventData>>();
//        }


//        private static Il2CppSystem.Collections.Generic.List<WardenObjectiveEventData> getEventsOnBossDeathForZone(LG_Zone zone)
//        {
//            if (zone == null)
//            {
//                EOSLogger.Error("WTF why are you passing in a null LG_Zone?");
//                return null;
//            }

//            return zone.m_settings.m_zoneData.EventsOnBossDeath;
//        }


//        // called on both host and client side
//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(EnemySync), nameof(EnemySync.OnSpawn))]
//        private static void Post_SpawnEnemy(EnemySync __instance, pEnemySpawnData spawnData) // 原生怪的mode == hibernate
//        {
//            AIG_CourseNode node = null;

//            if (spawnData.courseNode.TryGet(out node) == false || node == null)
//            {
//                EOSLogger.Error("Failed to get spawnnode for a boss! Skipped EventsOnBossDeath for it");
//                return;
//            }

//            if (!isBoss(__instance.m_agent.EnemyData.persistentID)) return;

//            EnemyAgent boss = __instance.m_agent;

//            //may take into good use: spawnData.mode
//            //if (__instance.m_enemyStateData.agentMode != Agents.AgentMode.Hibernate && __instance.m_enemyStateData.agentMode != Agents.AgentMode.Off) return;
//            //Logger.Log("spawnData.mode == {0}", spawnData.mode);
//            if (spawnData.mode != Agents.AgentMode.Hibernate) return;

//            LG_Zone spawnedZone = node.m_zone;

//            Il2CppSystem.Collections.Generic.List<WardenObjectiveEventData> eventsOnBossDeath = getEventsOnBossDeathForZone(spawnedZone);

//            if (eventsOnBossDeath == null || eventsOnBossDeath.Count == 0) return; // No events on boss death. 

//            checkInit();

//            //boss.add_OnDeadCallback(new System.Action(() => 
//            //{
//            //    WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(eventsOnBossDeath, eWardenObjectiveEventTrigger.None, true);
//            //}));

//            bossesWithDeathEvents.Add(boss.GlobalID, eventsOnBossDeath);
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(EnemyAgent), nameof(EnemyAgent.OnDead))]
//        private static void Post_EnemyOnDeadExecuteDeathEvent(EnemyAgent __instance)
//        {
//            if (!hasDeathEvent(__instance)) return;

//            if (GameStateManager.IsInExpedition == false) return;

//            EnemyAgent boss = __instance;

//            Il2CppSystem.Collections.Generic.List<WardenObjectiveEventData> eventsOnBossDeath = null;

//            if (bossesWithDeathEvents.TryGetValue(boss.GlobalID, out eventsOnBossDeath) == false || eventsOnBossDeath == null) return;

//            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(eventsOnBossDeath, eWardenObjectiveEventTrigger.None, true, 0.0f);
//            bossesWithDeathEvents.Remove(boss.GlobalID);
//        }

//        private static void CleanupAfterExpedition()
//        {
//            bossesWithDeathEvents?.Clear();
//            bossesWithDeathEvents = null;
//        }

//        static Patch_EventsOnBossDeath()
//        {
//            LevelAPI.OnLevelCleanup += CleanupAfterExpedition;
//        }
//    }
//}
