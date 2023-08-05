using GameData;
using System;

namespace ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition
{
    public class CustomTerminalZoneSelectionData : GlobalZoneIndex
    {
        public eSeedType SeedType { set; get; } = eSeedType.SessionSeed;

        public int TerminalIndex { set; get; } = 0;

        public int StaticSeed { set; get; } = 0;

        public CustomTerminalZoneSelectionData()
        {
            TerminalIndex = Math.Max(0, TerminalIndex);
        }
    }
}
