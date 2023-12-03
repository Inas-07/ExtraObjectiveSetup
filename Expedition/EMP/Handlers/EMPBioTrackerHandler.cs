using AK;
using Gear;
using ExtraObjectiveSetup.Utils;
using UnityEngine;
using System.Collections.Generic;
using GTFO.API;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPBioTrackerHandler : EMPHandler
    {
        private static List<EMPBioTrackerHandler> handlers = new();

        public static IEnumerable<EMPBioTrackerHandler> Handlers => handlers;

        private static void Clear()
        {
            handlers.Clear();
        }

        static EMPBioTrackerHandler()
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        private EnemyScanner _scanner;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            base.Setup(gameObject, controller);
            _scanner = gameObject.GetComponent<EnemyScanner>();
            if (_scanner == null)
            {
                EOSLogger.Error("Couldn't get bio-tracker component!");
            }

            handlers.Add(this);
        }

        protected override void DeviceOff()
        {
            _scanner.Sound.Post(EVENTS.BIOTRACKER_TOOL_LOOP_STOP);
            _scanner.m_graphics.m_display.enabled = false;
        }

        protected override void DeviceOn() => _scanner.m_graphics.m_display.enabled = true;

        protected override void FlickerDevice() => _scanner.enabled = FlickerUtil();

    }
}
