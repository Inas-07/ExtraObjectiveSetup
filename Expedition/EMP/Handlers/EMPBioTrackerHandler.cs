using AK;
using Gear;
using ExtraObjectiveSetup.Utils;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPBioTrackerHandler : EMPHandler
    {
        private EnemyScanner _scanner;

        public static EMPBioTrackerHandler Instance { get; private set; }

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            base.Setup(gameObject, controller);
            _scanner = gameObject.GetComponent<EnemyScanner>();
            if (_scanner == null)
            {
                EOSLogger.Error("Couldn't get bio-tracker component!");
            }

            Instance = this;
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
