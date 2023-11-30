using ExtraObjectiveSetup.Utils;
using GTFO.API;
using LevelGeneration;
using UnityEngine;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPLightHandler : EMPHandler
    {
        private static List<EMPLightHandler> handlers = new();

        public static IEnumerable<EMPLightHandler> Handlers => handlers;

        private static void Clear()
        {
            handlers.Clear();
        }

        static EMPLightHandler()
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        private LG_Light _light;
        private float _originalIntensity;
        private Color _originalColor;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            base.Setup(gameObject, controller);

            _light = gameObject.GetComponent<LG_Light>();
            if (_light == null)
            {
                EOSLogger.Warning("No Light!");
            }
            else
            {
                _originalIntensity = _light.GetIntensity();
                _originalColor = _light.m_color;
                State = EMPState.On;
            }
            handlers.Add(this);
        }

        protected override void FlickerDevice()
        {
            if (_light == null)
                return;
            _light.ChangeIntensity(GetRandom01() * _originalIntensity);
        }

        protected override void DeviceOn()
        {
            if (_light == null)
                return;
            _light.ChangeIntensity(_originalIntensity);
            _light.ChangeColor(_originalColor);
        }

        protected override void DeviceOff()
        {
            if (_light == null)
                return;
            _light.ChangeIntensity(0.0f);
            _light.ChangeColor(Color.black);
        }
    }
}
