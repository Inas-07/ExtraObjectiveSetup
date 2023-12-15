using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraObjectiveSetup.Objectives.TerminalUplink
{
    public enum UplinkStatus
    {
        Unfinished,
        InProgress,
        Finished,
    }

    public struct UplinkState
    {
        public UplinkStatus Status { get; set; } = UplinkStatus.Unfinished;

        public int CurrentRoundIndex { get; set; } = 0;

        public UplinkState() { }    

        public UplinkState(UplinkState o) 
        {
            CurrentRoundIndex = o.CurrentRoundIndex;
            Status = o.Status;
        }
    }
}
