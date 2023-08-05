using ExtraObjectiveSetup.BaseClasses;
using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class GeneratorClusterInstanceManager: InstanceManager<LG_PowerGeneratorCluster>
    {
        public static GeneratorClusterInstanceManager Current { get; private set; } = new();

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalZoneIndex(LG_PowerGeneratorCluster instance) => (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);

        static GeneratorClusterInstanceManager()
        {

        }
    }
}
