using ExtraObjectiveSetup.BaseClasses;
using GameData;
using System.Collections.Concurrent;
using LevelGeneration;
using GTFO.API;
using ExtraObjectiveSetup.Utils;
using SNetwork;

namespace ExtraObjectiveSetup.Tweaks.BossEvents
{
    internal class BossDeathEventManager : ZoneDefinitionManager<EventsOnZoneBossDeath>
    {
        public static BossDeathEventManager Current = new();

        public enum Mode { HIBERNATE, WAVE }

        public const int UNLIMITED_COUNT = int.MaxValue;

        // maintained on both host and client side.
        private ConcurrentDictionary<(eDimensionIndex, LG_LayerType, eLocalZoneIndex), EventsOnZoneBossDeath> LevelBDEs { get; } = new();

        protected override string DEFINITION_NAME => "EventsOnBossDeath";

        //public void RegisterInLevelBDEventsExecution(EventsOnZoneBossDeath def, Mode mode)
        //{
        //    var zoneBossType = (def.DimensionIndex, def.LayerType, def.LocalIndex, mode);
        //    if (LevelBDEs.ContainsKey(zoneBossType)) return;
        //    switch(mode)
        //    {
        //        case Mode.HIBERNATE: LevelBDEs[zoneBossType] = def.ApplyToHibernateCount; break;
        //        case Mode.WAVE: LevelBDEs[zoneBossType] = def.ApplyToWaveCount; break;

        //        default: EOSLogger.Error($"BossDeathEventManager.RegisterInLevelBDEventsExecution: unimplemented mode: {mode}"); break;
        //    }
        //}

        public bool TryConsumeBDEventsExecutionTimes(EventsOnZoneBossDeath def, Mode mode) => TryConsumeBDEventsExecutionTimes(def.DimensionIndex, def.LayerType, def.LocalIndex, mode);

        public bool TryConsumeBDEventsExecutionTimes(eDimensionIndex dimensionIndex, LG_LayerType layer, eLocalZoneIndex localIndex, Mode mode)
        {
            if(!LevelBDEs.ContainsKey((dimensionIndex, layer, localIndex)))
            {
                EOSLogger.Error($"BossDeathEventManager: got an unregistered entry: {(dimensionIndex, layer, localIndex, mode)}");
                return false;
            }

            var bde = LevelBDEs[(dimensionIndex, layer, localIndex)];
            int remain = mode == Mode.HIBERNATE ? bde.RemainingHibernateBDE : bde.RemainingWaveBDE;
            
            if (remain == UNLIMITED_COUNT)
            {
                return true;
            }
            else if (remain > 0)
            {
                var oldState = bde.FiniteBDEStateReplicator.State;
                 
                if(SNet.IsMaster)
                {
                    bde.FiniteBDEStateReplicator.SetState(new()
                    {
                        ApplyToHibernateCount = mode == Mode.HIBERNATE ? remain - 1 : oldState.ApplyToHibernateCount,
                        ApplyToWaveCount = mode == Mode.WAVE ? remain - 1 : oldState.ApplyToWaveCount,
                    });
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Clear()
        {
            foreach(var bde in LevelBDEs.Values)
            {
                bde.Destroy();
            }

            LevelBDEs.Clear();
        }

        private void SetupForCurrentExpedition()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;

            foreach(var zoneBDE in definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions)
            {
                if(LevelBDEs.ContainsKey(zoneBDE.GlobalZoneIndexTuple()))
                {
                    EOSLogger.Error($"BossDeathEvent: found duplicate setup for zone {zoneBDE.GlobalZoneIndexTuple()}, will overwrite!");
                }

                if(zoneBDE.ApplyToHibernateCount != UNLIMITED_COUNT || zoneBDE.ApplyToWaveCount != UNLIMITED_COUNT)
                {
                    uint alloted_id = EOSNetworking.AllotReplicatorID();
                    if(alloted_id != EOSNetworking.INVALID_ID)
                    {
                        zoneBDE.SetupReplicator(alloted_id);
                    }
                    else
                    {
                        EOSLogger.Error($"BossDeathEvent: replicator ID depleted, cannot setup replicator!");
                    }
                }

                LevelBDEs[zoneBDE.GlobalZoneIndexTuple()] = zoneBDE;
            }
        }

        private BossDeathEventManager() 
        {
            LevelAPI.OnBuildStart += () => { Clear(); SetupForCurrentExpedition(); };
            LevelAPI.OnLevelCleanup += Clear;
        }

        static BossDeathEventManager()
        {

        }
    }
}
