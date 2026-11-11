using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtraObjectiveSetup.Expedition;
using ExtraObjectiveSetup.Utils;
using AmorLib.Networking.StateReplicators;
using GameData;
using GTFO.API.Extensions;
using LevelGeneration;
using Player;
using SNetwork;
using System.Collections;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Tweaks.TerminalTweak
{
    public class TerminalWrapper
    {
        public LG_ComputerTerminal lgTerminal { get; private set; }

        public StateReplicator<TerminalState> stateReplicator { get; private set; }

        private ExpeditionTerminalsDefinition expeditionTerminalDefinition { get; set; } = null;

        private readonly Dictionary<string, List<WardenObjectiveEventData>> logFileEvents = new();

        private readonly List<string> logFileNames = new();

        private void ChangeStateUnsynced(bool enabled)
        {
            EOSLogger.Debug($"{lgTerminal.ItemKey} state, enabled: {enabled}");

            lgTerminal.OnProximityExit();
            Interact_ComputerTerminal interact = lgTerminal.GetComponentInChildren<Interact_ComputerTerminal>(true);
            bool active = enabled;

            if (interact != null)
            {
                interact.enabled = active;
                interact.SetActive(active);
            }

            lgTerminal.m_interfaceScreen.SetActive(active);
            lgTerminal.m_loginScreen.SetActive(active);

            if (lgTerminal.m_text != null)
            {
                lgTerminal.m_text.enabled = active;
            }

            if (!active)
            {
                PlayerAgent interactionSource = lgTerminal.m_localInteractionSource;
                if (interactionSource != null && interactionSource.FPItemHolder.InTerminalTrigger)
                {
                    lgTerminal.ExitFPSView();
                }
            }
        }

        public void ChangeState(bool enabled)
        {
            ChangeStateUnsynced(enabled);
            if (SNet.IsMaster)
            {
                stateReplicator.SetState(stateReplicator.State with { Enabled = enabled });
            }
        }

        /// <summary>
        /// Marks the terminal as approached, which is what fires EventsOnApproach.
        /// </summary>
        public void ChangeState()
        {
            if (!SNet.IsMaster || stateReplicator.State.Approached) return;

            stateReplicator.SetState(stateReplicator.State with { Approached = true });
        }

        public void ReceiveCommand(TERM_Command cmd, string param)
        {
            if (expeditionTerminalDefinition == null)
                return;

            if (SNet.IsMaster && cmd == TERM_Command.ReadLog)
                ChangeState(param);

            if (cmd == TERM_Command.TryUnlockingTerminal)
            {
                var (events, delay) = lgTerminal.EvaluatePassword(param)
                    ? (expeditionTerminalDefinition.EventsOnPasswordInputSuccess, 2.8f)
                    : (expeditionTerminalDefinition.EventsOnPasswordInputFailure, 2.6f);
                ExecuteEvents(events, delay);
            }
        }

        public void ChangeState(string fileName)
        {
            int index = logFileNames.IndexOf(fileName);
            if (expeditionTerminalDefinition == null || stateReplicator == null || index == -1)
                return;

            bool[] currentBits = stateReplicator.State.Value;
            bool[] bits = new bool[logFileNames.Count];
            for (int i = 0; i < currentBits.Length && i < bits.Length; i++)
            {
                bits[i] = currentBits[i];
            }

            if (bits[index]) return; // log was already read
            bits[index] = true;
            stateReplicator.SetState(stateReplicator.State with { Value = bits });
        }

        private void OnStateChanged(TerminalState oldState, TerminalState newState, bool isRecall)
        {
            if (oldState.Enabled != newState.Enabled)
            {
                ChangeStateUnsynced(newState.Enabled);
            }

            if (expeditionTerminalDefinition == null || isRecall) return;

            if (!oldState.Approached && newState.Approached)
            {
                ExecuteEvents(expeditionTerminalDefinition.EventsOnApproach, 0f);
            }

            bool[] oldBits = oldState.Value;
            bool[] bits = newState.Value;
            for (int i = 0; i < logFileNames.Count; i++)
            {
                bool wasRead = i < oldBits.Length && oldBits[i];
                bool isRead = i < bits.Length && bits[i];
                if (wasRead || !isRead) continue;

                if (logFileEvents.TryGetValue(logFileNames[i], out var events))
                {
                    ExecuteEvents(events, 3f); // wait for the logfile output to finish
                }
            }
        }

        private void ExecuteEvents(List<WardenObjectiveEventData> events, float delay)
        {
            if (events == null || events.Count == 0) return;

            if (delay <= 0f)
            {
                WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(events.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true);
                return;
            }

            CoroutineManager.StartCoroutine(DelayedEvents(events, delay).WrapToIl2Cpp());
        }

        private static IEnumerator DelayedEvents(List<WardenObjectiveEventData> events, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(events.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true);
        }

        private void SetupExpeditionDefinition()
        {
            if (!ExpeditionDefinitionManager.Current.TryGetTerminalDefinitionFromInstance(lgTerminal, out var def)) return;

            expeditionTerminalDefinition = def;

            foreach (var logFile in def.LogFiles)
            {
                if (string.IsNullOrWhiteSpace(logFile.FileName)) continue;

                string fileName = logFile.FileName.ToUpperInvariant();
                logFileEvents[fileName] = logFile.EventsOnFileRead;
                logFileNames.Add(fileName);
            }
        }

        public static TerminalWrapper Instantiate(LG_ComputerTerminal lgTerminal, uint replicatorID)
        {
            if (lgTerminal == null || replicatorID == EOSNetworking.INVALID_ID) return null;
            var t = new TerminalWrapper();

            t.lgTerminal = lgTerminal;
            t.stateReplicator = StateReplicator<TerminalState>.Create(replicatorID, new() { Enabled = true }, LifeTimeType.Session);
            t.stateReplicator.OnStateChanged += t.OnStateChanged;
            t.SetupExpeditionDefinition();

            return t;
        }

        private TerminalWrapper()
        {
        }
    }
}
