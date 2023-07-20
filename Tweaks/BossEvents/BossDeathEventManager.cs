using ExtraObjectiveSetup.BaseClasses;
using GameData;
using System.Collections.Concurrent;
using LevelGeneration;
using GTFO.API;
using ExtraObjectiveSetup.Utils;


namespace ExtraObjectiveSetup.Tweaks.BossEvents
{
    internal class BossDeathEventManager : ZoneDefinitionManager<EventsOnZoneBossDeath>
    {
        public static BossDeathEventManager Current = new();

        public enum Mode { HIBERNATE, WAVE }

        // maintained on both host and client side.
        private ConcurrentDictionary<(eDimensionIndex, LG_LayerType, eLocalZoneIndex, Mode), int> InLevelBDEventsExecution = new();

        protected override string DEFINITION_NAME => "EventsOnBossDeath";

        public void RegisterInLevelBDEventsExecution(EventsOnZoneBossDeath def, Mode mode)
        {
            var zoneBossType = (def.DimensionIndex, def.LayerType, def.LocalIndex, mode);
            if (InLevelBDEventsExecution.ContainsKey(zoneBossType)) return;
            switch(mode)
            {
                case Mode.HIBERNATE: InLevelBDEventsExecution[zoneBossType] = def.ApplyToHibernateCount; break;
                case Mode.WAVE: InLevelBDEventsExecution[zoneBossType] = def.ApplyToWaveCount; break;

                default: EOSLogger.Error($"BossDeathEventManager.RegisterInLevelBDEventsExecution: unimplemented mode: {mode}"); break;
            }
        }

        public bool TryConsumeBDEventsExecutionTimes(EventsOnZoneBossDeath def, Mode mode) => TryConsumeBDEventsExecutionTimes(def.DimensionIndex, def.LayerType, def.LocalIndex, mode);

        public bool TryConsumeBDEventsExecutionTimes(eDimensionIndex dimensionIndex, LG_LayerType layer, eLocalZoneIndex localIndex, Mode mode)
        {
            if(!InLevelBDEventsExecution.ContainsKey((dimensionIndex, layer, localIndex, mode)))
            {
                EOSLogger.Error($"BossDeathEventManager: got an unregistered entry: {(dimensionIndex, layer, localIndex, mode)}");
                return false;
            }

            int remain = InLevelBDEventsExecution[(dimensionIndex, layer, localIndex, mode)];
            if (remain == int.MaxValue) return true;

            if (remain > 0)
            {
                InLevelBDEventsExecution[(dimensionIndex, layer, localIndex, mode)] = remain - 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Clear()
        {
            InLevelBDEventsExecution.Clear();
        }

        private BossDeathEventManager() 
        {
            LevelAPI.OnLevelCleanup += Clear;
        }

        static BossDeathEventManager()
        {

        }
    }
}
