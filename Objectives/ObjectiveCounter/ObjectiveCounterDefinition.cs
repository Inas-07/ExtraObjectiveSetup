using GameData;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Objectives.ObjectiveCounter
{
    public class OnCounter 
    {
        public int Count { get; set; } = -1;

        public List<WardenObjectiveEventData> EventsOnReached { get; set; } = new();
    }


    public class ObjectiveCounterDefinition
    {
        public string WorldEventObjectFilter { get; set; } = string.Empty;

        public int StartingCount { get; set; } = 0;

        public List<OnCounter> OnReached { get; set; } = new() { new() };
    }
}
