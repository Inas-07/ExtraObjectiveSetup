using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Expedition.Gears;
using ExtraObjectiveSetup.Expedition.IndividualGeneratorGroup;
using ExtraObjectiveSetup.Tweaks.TerminalTweak;
using GameData;

namespace ExtraObjectiveSetup.Expedition
{
    public class ExpeditionDefinition
    {
        public uint MainLevelLayout { set; get; } = 0;

        // Add expedition definition as needed
        public ExpeditionGearsDefinition ExpeditionGears { set; get; } = new();

        public List<ExpeditionIGGroup> GeneratorGroups { set; get; } = new() { new() };

        public List<ExpeditionTerminalsDefinition> Terminals { set; get; } = new() { new() };
    }

    public class ExpeditionTerminalsDefinition : BaseInstanceDefinition
    {
        public List<WardenObjectiveEventData> EventsOnApproach { set; get; } = new();

        public SerialGeneratorManager.CodeWordLength PasswordWordLength { set; get; } = SerialGeneratorManager.CodeWordLength.Four;

        public List<WardenObjectiveEventData> EventsOnPasswordInputSuccess { set; get; } = new();

        public List<WardenObjectiveEventData> EventsOnPasswordInputFailure { set; get; } = new();

        public List<TerminalLogFileEvents> LogFiles { set; get; } = new();
    }

    public class TerminalLogFileEvents
    {
        public string FileName { set; get; } = string.Empty;

        public List<WardenObjectiveEventData> EventsOnFileRead { set; get; } = new();
    }
}
