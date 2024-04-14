using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using System.Collections.Generic;


namespace ExtraObjectiveSetup.Objectives.ObjectiveCounter
{
    public class ObjectiveCounterManager : GenericExpeditionDefinitionManager<ObjectiveCounterDefinition>
    {
        public static ObjectiveCounterManager Current { get; } = new();

        private Dictionary<string, Counter> Counters { get; } = new();

        protected override string DEFINITION_NAME => "ObjectiveCounter";

        private void Build(ObjectiveCounterDefinition def)
        {
            if(Counters.ContainsKey(def.WorldEventObjectFilter))
            {
                EOSLogger.Error($"Build Counter: counter '{def.WorldEventObjectFilter}' already exists...");
                return;
            }

            var counter = new Counter(def);
            if(!counter.TrySetupReplicator())
            {
                EOSLogger.Error($"Build Counter: counter '{def.WorldEventObjectFilter}' failed to setup state replicator! What's going wrong?");
                return;
            }

            Counters[def.WorldEventObjectFilter] = counter;
            EOSLogger.Debug($"Build Counter: counter '{def.WorldEventObjectFilter}' setup completed");
        }

        public void ChangeCounter(string worldEventObjectFilter, int by)
        {
            if(!Counters.ContainsKey(worldEventObjectFilter))
            {
                EOSLogger.Error($"ChangeCounter: {worldEventObjectFilter} is not defined");
                return;
            }

            if (by == 0) return;

            var counter = Counters[worldEventObjectFilter];
            if(by > 0)
            {
                counter.Increment(by);
            }
            else
            {
                counter.Decrement(-by);
            }
        }

        private void ChangeCounter(WardenObjectiveEventData e)
        {
            string worldEventObjectFilter = e.WorldEventObjectFilter;
            int by = e.Count;
            ChangeCounter(worldEventObjectFilter, by);
        }

        private void BuildCounters()
        {
            if (!definitions.ContainsKey(CurrentMainLevelLayout)) return;
            definitions[CurrentMainLevelLayout].Definitions.ForEach(Build);
        }

        private void Clear()
        {
            Counters.Clear();
        }

        private ObjectiveCounterManager() 
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
            LevelAPI.OnBuildDone += BuildCounters;
            EOSWardenEventManager.Current.AddEventDefinition(CounterWardenEvent.ChangeCounter.ToString(), (uint)CounterWardenEvent.ChangeCounter, ChangeCounter);
                        
        }

        static ObjectiveCounterManager() { }
    }
}
