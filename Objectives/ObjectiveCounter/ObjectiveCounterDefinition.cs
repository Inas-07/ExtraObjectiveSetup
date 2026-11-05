using GameData;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Objectives.ObjectiveCounter
{
    public class OnCounter 
    {
        public int Count { get; set; } = -1;

        public bool ExecuteOnce { get; set; } = false;

        public List<WardenObjectiveEventData> EventsOnReached { get; set; } = new();
    }


    public class ObjectiveCounterDefinition
    {
        public string WorldEventObjectFilter { get; set; } = string.Empty;

        public int StartingCount { get; set; } = 0;

        // The fork replaced IsCapped with MinCount / MaxCount and removed the field; this port keeps
        // the field and still honours it, so definitions written for the old capping keep working.
        // When true the counter is bounded to the min / max of OnReached (see Counter.minBound / maxBound).
        //
        // TODO(review): three edges differ from the pre-fork behaviour, decide whether to keep them:
        //   - JumpCounter (502) is new in the fork and is bounded by the IsCapped range as well; the old
        //     code had no Jump, so there is no legacy behaviour to match
        //   - IsCapped and MinCount / MaxCount may be set together; the tighter range wins, and a range
        //     that does not overlap the OnReached range falls back to the OnReached range (error logged)
        //   - when StartingCount lies outside the OnReached range, the first Increment / Decrement snaps
        //     to the nearest edge of the range, so an OnReached entry at that edge fires at once; the old
        //     code walked the count step by step and only fired what it actually passed
        public bool IsCapped { get; set; } = false;

        public int MinCount { get; set; } = int.MinValue;

        public int MaxCount { get; set; } = int.MaxValue;

        public bool IncrementOnPlayerDowned { get; set; } = false;

        public List<OnCounter> OnReached { get; set; } = new() { new() };
    }
}
