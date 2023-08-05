using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.IndividualGenerator
{
    internal sealed class IndividualGeneratorObjectiveManager : InstanceDefinitionManager<IndividualGeneratorDefinition>
    {
        public static IndividualGeneratorObjectiveManager Current { private set; get; } = new();

        protected override string DEFINITION_NAME { get; } = "IndividualGenerator";

        private IndividualGeneratorObjectiveManager() : base() { }

        static IndividualGeneratorObjectiveManager() { }
    }
}
