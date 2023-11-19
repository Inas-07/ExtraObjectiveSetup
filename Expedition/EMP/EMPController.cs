using Il2CppInterop.Runtime.Attributes;
using ExtraObjectiveSetup.Utils;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public sealed class EMPController : MonoBehaviour
    {
        private EMPHandler _handler;
        private bool _hasHandler;
        private bool _setup;

        public float duration { get; private set; }

        [HideFromIl2Cpp]
        private bool IsEMPActive => duration > Clock.Time;

        [HideFromIl2Cpp]
        public Vector3 Position => transform.position;

        private void Awake() => EMPManager.Current.AddTarget(this);

        private void OnEnable()
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel || !_setup)
                return;
            if(!float.IsPositiveInfinity(duration)) // handle pEMP 
            {
                duration = Clock.Time + EMPManager.Current.DurationFromPosition(transform.position);
            }

            if (duration > Clock.Time)
            {
                _handler.ForceState(EMPState.Off);
            }
            else
            {
                _handler.ForceState(EMPState.On);
            }
        }

        private void Update()
        {
            if (!_hasHandler)
                return;
            _handler.Tick(IsEMPActive);
        }

        [HideFromIl2Cpp]
        public void AddTime(float time) => duration = Clock.Time + time;

        [HideFromIl2Cpp]
        public void ClearTime() => duration = Clock.Time - 1f;

        [HideFromIl2Cpp]
        public void AssignHandler(EMPHandler handler)
        {
            if (_handler != null)
            {
                EOSLogger.Warning("Tried to assign a handler to a controller that already had one!");
            }
            else
            {
                _handler = handler;
                _handler.Setup(gameObject, this);
                _hasHandler = true;
                _setup = true;
            }
        }

        [HideFromIl2Cpp]
        public void ForceState(EMPState state)
        {
            if (_handler == null)
                return;
            _handler.ForceState(state);
        }

        private void OnDestroy()
        {
            EMPManager.Current.RemoveTarget(this);
            _handler.OnDespawn();
            _handler = null;
        }
    }

}
