using ExtraObjectiveSetup.Utils;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public class ItemToDisable
    {
        public bool BioTracker { set; get; } = true;
        public bool PlayerHUD { set; get; } = true;
        public bool PlayerFlashLight { set; get; } = true;
        public bool EnvLight { set; get; } = true;
        public bool GunSight { set; get; } = true;
        public bool Sentry { set; get; } = true;
    }

    public class pEMPDefinition
    {
        public uint pEMPIndex { get; set; } = 0u;

        public Vec3 Position { set; get; } = new();

        public float Range { set; get; } = 0.0f;

        public ItemToDisable ItemToDisable { get; set; } = new();
    }
}
