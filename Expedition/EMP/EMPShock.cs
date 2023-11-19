using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public class EMPShock
    {
        public Vector3 position { get; internal set; }
        public float range { get; internal set; }
        public float duration { get; internal set; }

        public float RemainingTime => duration - Clock.Time;

        public EMPShock(Vector3 position, float range, float duration)
        {
            this.position = position;
            this.range = range;
            this.duration = Clock.Time + duration;
        }

        public bool InRange(Vector3 position) => Vector3.Distance(position, this.position) < range;
    }
}
