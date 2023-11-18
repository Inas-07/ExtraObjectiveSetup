using AK;
using Gear;
using ExtraObjectiveSetup.Utils;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPBioTrackerHandler : EMPHandler
    {
        private EnemyScanner _scanner;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            if (gameObject.gameObject.TryGetComponent(out _scanner))
                return;
            EOSLogger.Error("Couldn't get bio-tracker component!");
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
