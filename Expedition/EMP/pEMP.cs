using System;
using UnityEngine;
using FloLib.Networks;
using ExtraObjectiveSetup.Utils;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public enum ActiveState
    {
        DISABLED,
        ENABLED,
    }

    public struct pEMPState
    {
        public ActiveState Status = ActiveState.DISABLED;

        public pEMPState() {}
    }


    public class pEMP : EMPShock
    {
        public pEMPDefinition def { get; private set; }

        public ActiveState State => stateReplicator.State.Status;

        public StateReplicator<pEMPState> stateReplicator { get; private set; }

        public void ChangeState(ActiveState status) => stateReplicator.SetState(new pEMPState { Status = status });

        private void OnStateChanged(pEMPState oldState, pEMPState newState, bool isRecall)
        {
            switch (State)
            {
                case ActiveState.DISABLED:
                    duration = float.NegativeInfinity; break;
                case ActiveState.ENABLED:
                    duration = float.PositiveInfinity; break;
                default: throw new NotImplementedException();
            }
        }

        public pEMP(Vector3 position, float range, pEMPDefinition def): base(position, range, float.NegativeInfinity)
        {
            this.def = def;
            stateReplicator = StateReplicator<pEMPState>.Create(1, default, LifeTimeType.Level);
            stateReplicator.OnStateChanged += OnStateChanged;

            EOSLogger.Warning("pEMP inited");
        }
    }
}
