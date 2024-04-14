using ExtraObjectiveSetup.Utils;
using FloLib.Networks.Replications;
using GTFO.API.Extensions;
using SNetwork;

namespace ExtraObjectiveSetup.Objectives.ObjectiveCounter
{
    public struct CounterStatus
    {
        public int Count = 0;

        public CounterStatus() { }
    }

    public class Counter
    {
        public ObjectiveCounterDefinition def { get; private set; }

        public string WorldEventObjectFilter => def.WorldEventObjectFilter;

        public int CurrentCount => StateReplicator?.State.Count ?? 0;

        public StateReplicator<CounterStatus> StateReplicator { get; private set; }

        private void OnStateChanged(CounterStatus oldStatus, CounterStatus newStatus, bool isRecall)
        {
            // currently have nothing to do here
        }

        private void ReachTo(int count)
        {
            def.OnReached
                .FindAll(Counter => Counter.Count == count)
                .ForEach(Counter =>
                WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(Counter.EventsOnReached.ToIl2Cpp(), GameData.eWardenObjectiveEventTrigger.None, true));

            EOSLogger.Debug($"Counter '{WorldEventObjectFilter}' reached to {count}");
        }

        public void Increment(int by)
        {
            if (by <= 0) return;

            int target = CurrentCount + by;
            for(int count = CurrentCount + 1; count <= target; count++)
            {
                ReachTo(count);
            }

            if(SNet.IsMaster)
            {
                StateReplicator.SetState(new() { Count = target });
            }
        }

        public void Decrement(int by)
        {
            if (by <= 0) return;

            int target = CurrentCount - by;
            for (int count = CurrentCount - 1; count >= target; count--)
            {
                ReachTo(count);
            }

            if (SNet.IsMaster)
            {
                StateReplicator.SetState(new() { Count = target });
            }
        }

        internal bool TrySetupReplicator()
        {
            uint id = EOSNetworking.AllotReplicatorID();
            if(id == EOSNetworking.INVALID_ID) return false;

            StateReplicator = StateReplicator<CounterStatus>.Create(id, new() { Count = def.StartingCount }, LifeTimeType.Level);
            StateReplicator.OnStateChanged += OnStateChanged;
            return true;
        }

        public Counter(ObjectiveCounterDefinition def) 
        {
            this.def = def;
        }
    }
}
