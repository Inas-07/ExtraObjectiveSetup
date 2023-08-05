using GameData;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition
{
    public class TerminalDefinition
    {
        public List<TerminalLogFileData> LocalLogFiles { set; get; } = new();

        public List<CustomCommand> UniqueCommands { set; get; } = new() { new() };

        public TerminalPasswordData PasswordData { set; get; } = new();
    }
}
