using AK;
using ExtraObjectiveSetup.Utils;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPSentryHandler : EMPHandler
    {
        public static EMPSentryHandler Instance { get; private set; }

        private static Color _offColor = new Color()
        {
            r = 0.0f,
            g = 0.0f,
            b = 0.0f,
            a = 0.0f
        };
        private SentryGunInstance _sentry;
        private SentryGunInstance_ScannerVisuals_Plane _visuals;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            base.Setup(gameObject, controller);

            _sentry = gameObject.GetComponent<SentryGunInstance>();
            _visuals = gameObject.GetComponent<SentryGunInstance_ScannerVisuals_Plane>();
            if (_sentry == null || _visuals == null)
            {
                EOSLogger.Error($"Missing components on Sentry! Has Sentry?: {_sentry == null}, Has Visuals?: {_visuals == null}");
            }
            Instance = this;
        }

        protected override void DeviceOff()
        {
            _visuals.m_scannerPlane.SetColor(_offColor);
            _visuals.UpdateLightProps(_offColor, false);
            _sentry.m_isSetup = false;
            _sentry.m_isScanning = false;
            _sentry.m_isFiring = false;
            _sentry.Sound.Post(EVENTS.SENTRYGUN_STOP_ALL_LOOPS);
        }

        protected override void DeviceOn()
        {
            _sentry.m_isSetup = true;
            _sentry.m_visuals.SetVisualStatus(eSentryGunStatus.BootUp);
            _sentry.m_isScanning = false;
            _sentry.m_startScanTimer = Clock.Time + _sentry.m_initialScanDelay;
            _sentry.Sound.Post(EVENTS.SENTRYGUN_LOW_AMMO_WARNING);
        }

        protected override void FlickerDevice()
        {
            int randomRange = GetRandomRange(0, 3);
            _sentry.StopFiring();
            switch (randomRange)
            {
                case 0:
                    _visuals.SetVisualStatus(eSentryGunStatus.OutOfAmmo, true);
                    break;
                case 1:
                    _visuals.SetVisualStatus(eSentryGunStatus.Scanning, true);
                    break;
                case 2:
                    _visuals.SetVisualStatus(eSentryGunStatus.HasTarget, true);
                    break;
            }
        }
    }
}
