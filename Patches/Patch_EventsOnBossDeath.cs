using HarmonyLib;
using Enemies;
using LevelGeneration;
using GameData;
using AIGraph;
using System.Collections.Generic;
using GTFO.API;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Tweaks.BossEvents;

namespace ExtraObjectiveSetup.Patches
{
    [HarmonyPatch]
    internal class Patch_EventsOnBossDeath
    {
        private static HashSet<ushort> ExecutedForInstances = new(); 

        // called on both host and client side
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemySync), nameof(EnemySync.OnSpawn))]
        private static void Post_SpawnEnemy(EnemySync __instance, pEnemySpawnData spawnData) // 原生怪的mode == hibernate
        {
            AIG_CourseNode node = null;

            if (spawnData.courseNode.TryGet(out node) == false || node == null)
            {
                EOSLogger.Error("Failed to get spawnnode for a boss! Skipped EventsOnBossDeath for it");
                return;
            }

            LG_Zone spawnedZone = node.m_zone;
            var def = BossDeathEventManager.Current.GetDefinition(spawnedZone.DimensionIndex, spawnedZone.Layer.m_type, spawnedZone.LocalIndex);
            if (def == null) return;

            EnemyAgent enemy = __instance.m_agent;

            if (!def.BossIDs.Contains(enemy.EnemyData.persistentID)) return;

            // TODO: test 
            if (!(spawnData.mode == Agents.AgentMode.Hibernate && def.ApplyToHibernate || spawnData.mode == Agents.AgentMode.Agressive && def.ApplyToWave)) return;

            var mode = spawnData.mode == Agents.AgentMode.Hibernate ? BossDeathEventManager.Mode.HIBERNATE : BossDeathEventManager.Mode.WAVE;
            BossDeathEventManager.Current.RegisterInLevelBDEventsExecution(def, mode);

            if(!BossDeathEventManager.Current.TryConsumeBDEventsExecutionTimes(def, mode))
            {
                EOSLogger.Debug($"EventsOnBossDeath: execution times depleted for {def.GlobalZoneIndexTuple()}, {mode}");
                return;
            }

            enemy.add_OnDeadCallback(new System.Action(() =>
            {
                if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

                ushort enemyID = enemy.GlobalID;
                if (ExecutedForInstances.Contains(enemyID))
                {
                    ExecutedForInstances.Remove(enemyID);
                    return;
                }

                def.EventsOnBossDeath.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
                ExecutedForInstances.Add(enemyID);
            }));

            EOSLogger.Debug($"EventsOnBossDeath: added for enemy with id  {enemy.EnemyData.persistentID}, mode: {spawnData.mode}");
        }

        static Patch_EventsOnBossDeath()
        {
            LevelAPI.OnLevelCleanup += ExecutedForInstances.Clear;
        }
    }
}
