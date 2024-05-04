using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraObjectiveSetup.Tweaks.TerminalTweak
{
    public struct TerminalState
    {
        public bool Enabled = true;

        public TerminalState() { }

        public TerminalState(bool Enabled) { this.Enabled = Enabled; }

        public TerminalState(TerminalState o) { Enabled = o.Enabled; }
    }
}
