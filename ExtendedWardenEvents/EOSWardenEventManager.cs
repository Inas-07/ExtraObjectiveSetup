using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtraObjectiveSetup.Utils;
using GameData;
using Player;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ExtraObjectiveSetup.ExtendedWardenEvents
{
    public class EOSWardenEventManager
    {
        public static EOSWardenEventManager Current { get; private set; } = new();

        private Dictionary<string, uint> eventNames2ID = new();
        private Dictionary<uint, string> eventID2Name = new();

        private Dictionary<uint, Action<WardenObjectiveEventData>> eventDefinition = new();

        private readonly ImmutableHashSet<eWardenObjectiveEventType> vanillaEventIDs = Enum.GetValues<eWardenObjectiveEventType>().ToImmutableHashSet();

        private const uint AWOEventIdsStart = 10000;

        public bool IsVanillaEventID(uint eventID) => vanillaEventIDs.Contains((eWardenObjectiveEventType)eventID);

        private bool IsAWOEventID(uint eventID) => eventID >= AWOEventIdsStart;

        public bool AddEventDefinition(string eventName, uint eventID, Action<WardenObjectiveEventData> definition)
        {
            if(IsAWOEventID(eventID))
            {
                EOSLogger.Error($"EventID {eventID} is already used by AWO");
                return false;
            }

            if(IsVanillaEventID(eventID))
            {
                EOSLogger.Warning($"EventID {eventID}: overriding vanilla event!");
            }

            var eNameLowerCase = eventName.ToLowerInvariant();
            if (eventNames2ID.ContainsKey(eNameLowerCase) || eventID2Name.ContainsKey(eventID))
            {
                EOSLogger.Error($"AddEventDefinition: duplicate event name '{eventName}' or id '{eventID}'");
                return false;
            }

            eventNames2ID[eNameLowerCase] = eventID;
            eventID2Name[eventID] = eNameLowerCase;
            eventDefinition[eventID] = definition;

            EOSLogger.Debug($"EOSWardenEventManager: added event with name '{eventName}', id '{eventID}'");
            return true;
        }

        internal void ExecuteEvent(WardenObjectiveEventData e, float currentDuration)
        {
            uint eventID = (uint)e.Type;
            if (!eventDefinition.ContainsKey(eventID))
            {
                EOSLogger.Error($"ExecuteEvent: event ID {eventID} doesn't have a definition");
                return;
            }

            var coroutine = CoroutineManager.StartCoroutine(Handle(e, currentDuration).WrapToIl2Cpp());
            WorldEventManager.m_worldEventEventCoroutines.Add(coroutine);
        }

        public bool HasEventDefinition(string eventName) => eventNames2ID.ContainsKey(eventName.ToLowerInvariant());

        public bool HasEventDefinition(uint eventID) => eventDefinition.ContainsKey(eventID);

        private System.Collections.IEnumerator Handle(WardenObjectiveEventData e, float currentDuration)
        {
            uint eventID = (uint)e.Type;
            if (!eventDefinition.ContainsKey(eventID)) yield break;

            float delay = UnityEngine.Mathf.Max(e.Delay - currentDuration, 0f);
            if (delay > 0f)
            {
                yield return new UnityEngine.WaitForSeconds(delay);
            }

            if (WorldEventManager.GetCondition(e.Condition.ConditionIndex) != e.Condition.IsTrue)
            {
                yield break;
            }

            WardenObjectiveManager.DisplayWardenIntel(e.Layer, e.WardenIntel);
            if (e.DialogueID > 0u)
            {
                PlayerDialogManager.WantToStartDialog(e.DialogueID, -1, false, false);
            }

            if (e.SoundID > 0u)
            {
                WardenObjectiveManager.Current.m_sound.Post(e.SoundID, true);
                var line = e.SoundSubtitle.ToString();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    GuiManager.PlayerLayer.ShowMultiLineSubtitle(line);
                }
            }

            eventDefinition[eventID]?.Invoke(e);
        }

        private EOSWardenEventManager() 
        { 

        }

        static EOSWardenEventManager() { }
    }
}
