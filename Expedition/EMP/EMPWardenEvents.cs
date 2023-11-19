using ExtraObjectiveSetup.ExtendedWardenEvents;
using GameData;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    internal static class EMPWardenEvents
    {
        public enum EMPEvents {
            Instant_Shock = 300,
            Toggle_PEMP_State = 301
        }

        internal static void InstantShock(WardenObjectiveEventData e)
        {
            var position = e.Position;
            float range = e.FogTransitionDuration;
            float duration = e.Duration;
            
            EMPManager.Current.ActivateEMPShock(position, range, duration);
        }

        internal static void TogglepEMPState(WardenObjectiveEventData e)
        {
            EMPManager.Current.TogglepEMPState(e);
        }

        internal static void Init()
        {
            EOSWardenEventManager.Current.AddEventDefinition(EMPEvents.Instant_Shock.ToString(), (uint)EMPEvents.Instant_Shock, InstantShock);
            EOSWardenEventManager.Current.AddEventDefinition(EMPEvents.Toggle_PEMP_State.ToString(), (uint)EMPEvents.Toggle_PEMP_State, TogglepEMPState);
        }
    }
}
