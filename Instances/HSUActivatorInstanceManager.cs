using ExtraObjectiveSetup.BaseClasses;
using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class HSUActivatorInstanceManager: InstanceManager<LG_HSUActivator_Core>
    {
        public static HSUActivatorInstanceManager Current { get; private set; } = new();

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalZoneIndex(LG_HSUActivator_Core instance) => (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);
        
        private HSUActivatorInstanceManager() {}
    
        static HSUActivatorInstanceManager() { }
    }
}
