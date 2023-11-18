using ExtraObjectiveSetup.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPPlayerHudHandler : EMPHandler
    {
        private readonly List<RectTransformComp> _targets = new List<RectTransformComp>();
        private static EMPPlayerHudHandler _instance;

        public static EMPPlayerHudHandler Instance => _instance;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            if (_instance != null)
                return;
            _targets.Add(GuiManager.PlayerLayer.m_compass);
            _targets.Add(GuiManager.PlayerLayer.m_wardenObjective);
            _targets.Add(GuiManager.PlayerLayer.Inventory);
            _targets.Add(GuiManager.PlayerLayer.m_playerStatus);
            _instance = this;
        }

        protected override void DeviceOff()
        {
            foreach (Component target in _targets)
                target.gameObject.SetActive(false);
            GuiManager.NavMarkerLayer.SetVisible(false);
            EOSLogger.Debug("Player HUD off");
        }

        protected override void DeviceOn()
        {
            foreach (Component target in _targets)
                target.gameObject.SetActive(true);
            GuiManager.NavMarkerLayer.SetVisible(true);
            EOSLogger.Debug("Player HUD on");
        }

        protected override void FlickerDevice()
        {
            foreach (RectTransformComp target in _targets)
                target.SetVisible(FlickerUtil());
            GuiManager.NavMarkerLayer.SetVisible(FlickerUtil());
        }
    }

}
