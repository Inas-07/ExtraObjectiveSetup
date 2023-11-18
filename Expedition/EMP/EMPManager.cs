using UnityEngine;
using System.Collections.Generic;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.Patches.EMP;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public class EMPManager
    {
        private static readonly List<EMPController> _empTargets = new List<EMPController>();
        private static readonly List<ActiveEMPs> _activeEMPs = new List<ActiveEMPs>();

        public static EMPManager Current { get; private set; } = new();

        public void Init() { }

        static EMPManager()
        {
            LevelAPI.OnLevelCleanup += () =>
            {
                _empTargets.Clear();
                EMPHandler.Cleanup();
            };

            LevelAPI.OnBuildDone += Inject_PlayerBackpack.Setup;

            ClassInjector.RegisterTypeInIl2Cpp<EMPController>();
        }

        public static void AddTarget(EMPController target) => _empTargets.Add(target);

        public static void RemoveTarget(EMPController target) => _empTargets.Remove(target);

        public static void Activate(Vector3 position, float range, float duration)
        {
            if (!GameStateManager.IsInExpedition)
            {
                EOSLogger.Error("Tried to activate an EMP when not in level, this shouldn't happen!");
            }
            else
            {
                _activeEMPs.Add(new ActiveEMPs(position, range, duration));
                foreach (EMPController empTarget in _empTargets)
                {
                    if (Vector3.Distance(position, empTarget.Position) < range)
                        empTarget.AddTime(duration);
                }
            }
        }

        public static float DurationFromPosition(Vector3 position)
        {
            _activeEMPs.RemoveAll(e => Mathf.Round(e.RemainingTime) <= 0);
            float totalDurationForPosition = 0;
            foreach (ActiveEMPs active in _activeEMPs)
            {
                if (active.InRange(position))
                {
                    totalDurationForPosition += active.RemainingTime;
                }
            }
            return totalDurationForPosition;
        }

        private struct ActiveEMPs
        {
            private readonly Vector3 _position;
            private readonly float _range;
            private readonly float _duration;

            public float RemainingTime => _duration - Clock.Time;

            public ActiveEMPs(Vector3 position, float range, float duration)
              : this()
            {
                _position = position;
                _range = range;
                _duration = Clock.Time + duration;
            }

            public bool InRange(Vector3 position) => Vector3.Distance(position, _position) < _range;
        }
    }

}
