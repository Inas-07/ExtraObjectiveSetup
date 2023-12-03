using GameData;
using Gear;
using ExtraObjectiveSetup.Utils;
using Player;
using UnityEngine;
using System.Collections.Generic;
using GTFO.API;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPPlayerFlashLightHandler : EMPHandler
    {

        private static List<EMPPlayerFlashLightHandler> handlers = new();

        public static IEnumerable<EMPPlayerFlashLightHandler> Handlers => handlers;

        private static void Clear()
        {
            handlers.Clear();
        }

        static EMPPlayerFlashLightHandler()
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        private PlayerInventoryBase _inventory;
        private float _originalIntensity;
        private bool _originalFlashlightState;

        protected override bool IsDeviceOnPlayer => true;

        private bool FlashlightEnabled => _inventory.FlashlightEnabled;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            base.Setup(gameObject, controller);

            _inventory = gameObject.GetComponent<PlayerAgent>().Inventory;
            if (_inventory == null)
            {
                EOSLogger.Warning("Player inventory was null!");
            }
            else
            {
                State = EMPState.On;
                Events.FlashLightWielded += InventoryEvents_ItemWielded;
            }

            handlers.Add(this);
        }

        private void InventoryEvents_ItemWielded(GearPartFlashlight flashlight) => _originalIntensity = GameDataBlockBase<FlashlightSettingsDataBlock>.GetBlock(flashlight.m_settingsID).intensity;

        protected override void DeviceOff()
        {
            _originalFlashlightState = FlashlightEnabled;
            if (!_originalFlashlightState)
                return;
            _inventory.Owner.Sync.WantsToSetFlashlightEnabled(false);
        }

        protected override void DeviceOn()
        {
            if (_originalFlashlightState != FlashlightEnabled)
                _inventory.Owner.Sync.WantsToSetFlashlightEnabled(_originalFlashlightState);
            _inventory.m_flashlight.intensity = _originalIntensity;
        }

        protected override void FlickerDevice()
        {
            if (!FlashlightEnabled)
                return;
            _inventory.m_flashlight.intensity = GetRandom01() * _originalIntensity;
        }

    }

}
