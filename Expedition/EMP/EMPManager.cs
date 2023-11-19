using UnityEngine;
using System.Collections.Generic;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Patches.EMP;
using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public class EMPManager
    {
        private readonly List<EMPController> _empTargets = new List<EMPController>();
        private readonly List<EMPShock> _activeEMPShock = new List<EMPShock>();
        private readonly Dictionary<int, pEMP> _pEMPs = new();

        public static EMPManager Current { get; private set; } = new();

        public void AddTarget(EMPController target) => _empTargets.Add(target);

        public void RemoveTarget(EMPController target) => _empTargets.Remove(target);

        public void ActivateEMPShock(Vector3 position, float range, float duration)
        {
            if (!GameStateManager.IsInExpedition)
            {
                EOSLogger.Error("Tried to activate an EMP when not in level, this shouldn't happen!");
            }
            else
            {
                _activeEMPShock.Add(new EMPShock(position, range, duration));
                foreach (EMPController empTarget in _empTargets)
                {
                    if (Vector3.Distance(position, empTarget.Position) < range)
                        empTarget.AddTime(duration);
                }
            }
        }

        public float DurationFromPosition(Vector3 position)
        {
            _activeEMPShock.RemoveAll(e => Mathf.Round(e.RemainingTime) <= 0);
            float totalDurationForPosition = 0;
            foreach (EMPShock active in _activeEMPShock)
            {
                if (active.InRange(position))
                {
                    totalDurationForPosition += active.RemainingTime;
                }
            }
            return totalDurationForPosition;
        }

        public void TogglepEMPState(WardenObjectiveEventData e)
        {
            int pEMPIndex = e.Count;
            bool enabled = e.Enabled;
            if(!_pEMPs.ContainsKey(pEMPIndex))
            {
                EOSLogger.Error($"TogglepEMPState: pEMPIndex {pEMPIndex} not defined");
                return;
            }

            ActiveState newState = enabled ? ActiveState.ENABLED : ActiveState.DISABLED;
            var pEMP = _pEMPs[pEMPIndex];
            pEMP.ChangeState(newState);
            foreach (EMPController empTarget in _empTargets)
            {
                if (empTarget.GetComponent<LG_Light>() == null) continue;

                if(enabled)
                {
                    if (pEMP.InRange(empTarget.Position))
                    {
                        empTarget.AddTime(float.PositiveInfinity);
                    }
                }
                else
                {
                    empTarget.ClearTime();
                }
            }
        }

        private void BuildpEMPs()
        {
            var expDef = ExpeditionDefinitionManager.Current.GetDefinition(ExpeditionDefinitionManager.Current.CurrentMainLevelLayout);
            if (expDef == null || expDef.PersistentEMPs == null || expDef.PersistentEMPs.Count < 1) return;

            foreach(var pEMPDef in expDef.PersistentEMPs)
            {
                if (pEMPDef.pEMPIndex < 0) continue;

                var pEMP = new pEMP(pEMPDef.Position.ToVector3(), pEMPDef.Range, pEMPDef);
                _pEMPs[pEMPDef.pEMPIndex] = pEMP;
            }
        }

        private void Clear()
        {
            _empTargets.Clear();
            _activeEMPShock.Clear();
            _pEMPs.Clear();
            EMPHandler.Cleanup();
        }

        public void Init()
        {
            EMPWardenEvents.Init();

            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;

            LevelAPI.OnBuildDone += BuildpEMPs;
        }

        public IEnumerable<pEMP> pEMPs => _pEMPs.Values;
        public IEnumerable<EMPController> EMPTargets => _empTargets;

        static EMPManager()
        {
            LevelAPI.OnBuildDone += Inject_PlayerBackpack.Setup;
            ClassInjector.RegisterTypeInIl2Cpp<EMPController>();
            ClassInjector.RegisterTypeInIl2Cpp<PlayerpEMPComponent>();
        }

    }
}
