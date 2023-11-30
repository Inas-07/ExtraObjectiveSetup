using GTFO.API;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPPlayerHudHandler : EMPHandler
    {

        private static List<EMPPlayerHudHandler> handlers = new();

        public static IEnumerable<EMPPlayerHudHandler> Handlers => handlers;

        private static void Clear()
        {
            handlers.Clear();
        }

        static EMPPlayerHudHandler()
        {
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;
        }

        private readonly List<RectTransformComp> _targets = new List<RectTransformComp>();

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            _targets.Clear();
            base.Setup(gameObject, controller);
            
            _targets.Add(GuiManager.PlayerLayer.m_compass);
            _targets.Add(GuiManager.PlayerLayer.m_wardenObjective);
            _targets.Add(GuiManager.PlayerLayer.Inventory);
            _targets.Add(GuiManager.PlayerLayer.m_playerStatus);
            handlers.Add(this);
        }

        protected override void DeviceOff()
        {
            foreach (Component target in _targets)
                target.gameObject.SetActive(false);
            GuiManager.NavMarkerLayer.SetVisible(false);
            //EOSLogger.Debug("Player HUD off");
        }

        protected override void DeviceOn()
        {
            foreach (Component target in _targets)
                target.gameObject.SetActive(true);
            GuiManager.NavMarkerLayer.SetVisible(true);
            //EOSLogger.Debug("Player HUD on");
        }

        protected override void FlickerDevice()
        {
            foreach (RectTransformComp target in _targets)
                target.SetVisible(FlickerUtil());
            GuiManager.NavMarkerLayer.SetVisible(FlickerUtil());
        }
    }

}
