using GameData;
using LevelGeneration;
using Localization;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition
{
    public class CustomCommand
    {
        public string Command { set; get; } = string.Empty;

        public LocalizedText CommandDesc { set; get; } = null;

        public List<TerminalOutput> PostCommandOutputs { set; get; } = new();

        public List<WardenObjectiveEventData> CommandEvents { set; get; } = new();

        public TERM_CommandRule SpecialCommandRule { set; get; } = TERM_CommandRule.Normal;
    }
}
