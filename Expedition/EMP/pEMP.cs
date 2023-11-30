using System;
using ExtraObjectiveSetup.Utils;


namespace ExtraObjectiveSetup.Expedition.EMP
{
    public enum ActiveState
    {
        DISABLED,
        ENABLED,
    }

    public class pEMP : EMPShock
    {
        public pEMPDefinition def { get; private set; }

        public ActiveState State { get; private set; } = ActiveState.DISABLED;

        public void ChangeState(ActiveState newState)
        {
            EOSLogger.Warning($"pEMP_{def.pEMPIndex} Change state: {State} -> {newState}");
            State = newState;
            switch (State)
            {
                case ActiveState.DISABLED:
                    duration = float.NegativeInfinity; break;
                case ActiveState.ENABLED:
                    duration = float.PositiveInfinity; break;
                default: throw new NotImplementedException();
            }
        }

        public pEMP(pEMPDefinition def)
            : base(def.Position.ToVector3(), def.Range, float.NegativeInfinity)
        {
            this.def = def;
            ChangeState(ActiveState.DISABLED);
        }
    }
}
