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

        // Kept for JSON compatibility with pre-fork definitions. The fork removed this field and
        // uses MinCount / MaxCount instead. The two capping mechanisms are mutually exclusive:
        //   - IsCapped == true: the counter is bounded to the min / max of OnReached, matching
        //     pre-fork EOS core (Increment: upper only, Decrement: lower only, Set / Jump: both,
        //     StartingCount: unbounded). MinCount / MaxCount are ignored
        //   - otherwise: MinCount / MaxCount are applied unconditionally (fork behaviour)
        //
        // TODO(review): the IsCapped branch has no equivalent in the fork. It keeps the fork's
        // Clamp / ReachTo / ExecuteOnce architecture, so event firing and Jump follow the fork.
        public bool IsCapped { get; set; } = false;

        public int MinCount { get; set; } = int.MinValue;

        public int MaxCount { get; set; } = int.MaxValue;

        public bool IncrementOnPlayerDowned { get; set; } = false;

        public List<OnCounter> OnReached { get; set; } = new() { new() };
    }
}
