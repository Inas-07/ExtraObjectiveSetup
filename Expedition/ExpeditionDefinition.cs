using System.Collections.Generic;
using ExtraObjectiveSetup.Expedition.Gears;
using ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup;

namespace ExtraObjectiveSetup.Expedition
{
    public class ExpeditionDefinition
    {
        public uint MainLevelLayout { set; get; } = 0;

        // Add expedition definition as needed
        public ExpeditionGearsDefinition ExpeditionGears { set; get; } = new();

        public List<ExpeditionIGGroup> GeneratorGroups { set; get; } = new() { new() };
    }
}
