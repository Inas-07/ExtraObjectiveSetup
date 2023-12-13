using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.BaseClasses
{
    public class BaseInstanceDefinition: GlobalZoneIndex
    {
        [JsonPropertyOrder(-8)]
        public uint InstanceIndex { get; set; } = uint.MaxValue;

        public override string ToString() => $"{GlobalZoneIndexTuple()}, Instance_{InstanceIndex}";
    }

    public class InstanceDefinitionsForLevel<T> where T : BaseInstanceDefinition, new()
    {
        public uint MainLevelLayout { set; get; } = 0;

        public List<T> Definitions { set; get; } = new() { new() };
    }
}
