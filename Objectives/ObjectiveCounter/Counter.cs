using AmorLib.Networking.StateReplicators;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtraObjectiveSetup.Objectives.ObjectiveCounter
{
    public unsafe struct CounterStatus
    {
        public int count;

        public fixed ulong executeOnce[16];

        public CounterStatus(int count, ulong[] executeOnce)
        {
            this.count = count;
            fixed (ulong* dst = this.executeOnce)
            {
                executeOnce.AsSpan(0, 16).CopyTo(new Span<ulong>(dst, 16));
            }
        }
    }

    public class Counter
    {
        public ObjectiveCounterDefinition Def { get; private set; }
        public int CurrentCount { get; private set; } = 0;
        public ulong[] ExecuteOnce { get; private set; } = new ulong[16];
        public List<int> ExecuteOnceOverflow { get; private set; } = new();
        public StateReplicator<CounterStatus> Replicator { get; private set; }

        private readonly int minBound;
        private readonly int maxBound;

        private int Clamp(long count) => (int)Math.Clamp(count, minBound, maxBound);

        public Counter(ObjectiveCounterDefinition def)
        {
            if (def.MinCount > def.MaxCount)
            {
                EOSLogger.Error($"Counter: '{def.WorldEventObjectFilter}' MinCount > MaxCount!");
                (def.MinCount, def.MaxCount) = (def.MaxCount, def.MinCount);
            }

            Def = def;

            minBound = def.MinCount;
            maxBound = def.MaxCount;

            // IsCapped: bound the counter to the range covered by OnReached, as before MinCount / MaxCount
            if (def.IsCapped && def.OnReached.Count > 0)
            {
                int cappedMin = def.OnReached.Min(x => x.Count);
                int cappedMax = def.OnReached.Max(x => x.Count);

                minBound = Math.Max(minBound, cappedMin);
                maxBound = Math.Min(maxBound, cappedMax);

                if (minBound > maxBound)
                {
                    EOSLogger.Error($"Counter: '{def.WorldEventObjectFilter}' IsCapped range [{cappedMin}, {cappedMax}] does not overlap MinCount / MaxCount, using the OnReached range");
                    minBound = cappedMin;
                    maxBound = cappedMax;
                }
            }

            // only the explicit MinCount / MaxCount bound the starting count: with IsCapped the starting
            // count may lie outside the OnReached range (the legacy code did not bound it either)
            CurrentCount = (int)Math.Clamp(def.StartingCount, def.MinCount, def.MaxCount);
            EOSLogger.Debug($"Counter: '{def.WorldEventObjectFilter}' starting count: {def.StartingCount}, as clamped: {CurrentCount}, bounds: [{minBound}, {maxBound}]");

            uint allottedID = EOSNetworking.AllotReplicatorID();
            if (allottedID == EOSNetworking.INVALID_ID)
            {
                EOSLogger.Error("Counter: replicator IDs depleted, cannot setup StateReplicator");
                return;
            }
            Replicator = StateReplicator<CounterStatus>.Create(allottedID, new() { count = CurrentCount }, LifeTimeType.Session);
            Replicator!.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(CounterStatus _, CounterStatus state, bool isRecall)
        {
            EOSLogger.Debug($"Counter: '{Def.WorldEventObjectFilter}' OnStateChanged new count: {state.count}, isRecall: {isRecall}");
            CurrentCount = state.count;
            unsafe
            {
                ulong* src = state.executeOnce;
                for (int i = 0; i < ExecuteOnce.Length; i++)
                {
                    ExecuteOnce[i] = src[i];
                }
            }
        }

        private void ReachTo(int count)
        {
            EOSLogger.Debug($"Counter '{Def.WorldEventObjectFilter}' reached {count}");
            for (int idx = 0; idx < Def.OnReached.Count; idx++)
            {
                var counter = Def.OnReached[idx];
                if (counter.Count != count || (counter.ExecuteOnce && HasExecuted(idx)))
                    continue;
                EOSWardenEventManager.ExecuteWardenEvents(counter.EventsOnReached);
            }
        }

        private void ReachTo(long low, long high, bool flag)
        {
            EOSLogger.Debug($"Counter '{Def.WorldEventObjectFilter}' reached [{(flag ? low : high)}, {(flag ? high : low)}]");
            var range = Enumerable.Range(0, Def.OnReached.Count).Where(i => Def.OnReached[i].Count >= low && Def.OnReached[i].Count <= high);
            var onReached = flag ? range.OrderBy(i => Def.OnReached[i].Count) : range.OrderByDescending(i => Def.OnReached[i].Count);
            foreach (int idx in onReached)
            {
                var counter = Def.OnReached[idx];
                if (counter.ExecuteOnce && HasExecuted(idx))
                    continue;
                EOSWardenEventManager.ExecuteWardenEvents(counter.EventsOnReached);
            }
        }

        private unsafe bool HasExecuted(int index)
        {
            if (index >= 1024)
            {
                if (!ExecuteOnceOverflow.Contains(index))
                {
                    ExecuteOnceOverflow.Add(index);
                    EOSLogger.Warning($"Counter '{Def.WorldEventObjectFilter}' ExecuteOnce index at {index} > 1024, will not sync on recall");
                    return false;
                }
                return true;
            }

            int word = index >> 6;
            int bit = index & 63;
            bool result = false;

            fixed (ulong* mask = ExecuteOnce)
            {
                result = (mask[word] & (1UL << bit)) != 0;
                if (!result) mask[word] |= 1UL << bit;
            }
            return result;
        }

        public void Increment(long by)
        {
            long prev = CurrentCount;
            CurrentCount = Clamp(prev + by);
            long num = CurrentCount != prev ? prev + 1 : CurrentCount;
            ReachTo(num, CurrentCount, true);
            Replicator?.SetStateUnsynced(new(CurrentCount, ExecuteOnce));
        }

        public void Decrement(long by)
        {
            long prev = CurrentCount;
            CurrentCount = Clamp(prev - by);
            long num = CurrentCount != prev ? prev - 1 : CurrentCount;
            ReachTo(CurrentCount, num, false);
            Replicator?.SetStateUnsynced(new(CurrentCount, ExecuteOnce));
        }

        public void Set(long num)
        {
            CurrentCount = Clamp(num);
            ReachTo(CurrentCount);
            Replicator?.SetStateUnsynced(new(CurrentCount, ExecuteOnce));
        }

        public void Jump(long num)
        {
            long prev = CurrentCount;
            CurrentCount = Clamp(prev + num);
            ReachTo(CurrentCount);
            Replicator?.SetStateUnsynced(new(CurrentCount, ExecuteOnce));
        }
    }
}
