using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using GTFO.API;
using Player;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public class PlayerpEMPComponent : MonoBehaviour
    {
        private float nextUpdateTime = float.NaN;

        public const float UPDATE_INTERVAL = 1.0f;

        public static PlayerpEMPComponent Current { get; internal set; } = null;
 
        public bool InAnypEMP { get; private set; } = false;

        private void CheckSetup()
        {
            if (EMPManager.Current.LocalPlayerAgent == null) return;

            EMPManager.Current.SetupPlayerFlashLight();
            EMPManager.Current.SetupHUD();
            EMPManager.Current.SetupToolHandler();
        }

        void Update()
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            if (!float.IsNaN(nextUpdateTime) && Clock.Time < nextUpdateTime) return;

            nextUpdateTime = Clock.Time + UPDATE_INTERVAL;

            CheckSetup();

            var player = PlayerManager.GetLocalPlayerAgent();
            if( player == null ) 
            {
                return;
            }

            ItemToDisable itemToDisable = new() {
                BioTracker = false,
                PlayerFlashLight = false,
                PlayerHUD = false,
                EnvLight = false,
                GunSight = false,
                Sentry = false
            };

            InAnypEMP = false;
            foreach (var pEMP in EMPManager.Current.pEMPs)
            {
                if (pEMP.State != ActiveState.ENABLED) continue;

                if (pEMP.InRange(player.m_position))
                {
                    itemToDisable.BioTracker = itemToDisable.BioTracker || pEMP.def.ItemToDisable.BioTracker;
                    itemToDisable.PlayerFlashLight = itemToDisable.PlayerFlashLight || pEMP.def.ItemToDisable.PlayerFlashLight;
                    itemToDisable.PlayerHUD = itemToDisable.PlayerHUD || pEMP.def.ItemToDisable.PlayerHUD;
                    //itemToDisable.EnvLight |= pEMP.def.ItemToDisable.EnvLight;
                    itemToDisable.GunSight = itemToDisable.GunSight || pEMP.def.ItemToDisable.GunSight;
                    itemToDisable.Sentry = itemToDisable.Sentry || pEMP.def.ItemToDisable.Sentry;
                    InAnypEMP = true;
                }
            }

            if (itemToDisable.BioTracker)
            {
                foreach (var handler in EMPBioTrackerHandler.Handlers)
                    handler?.controller?.AddTime(float.PositiveInfinity);
            }
            else
            {
                foreach (var handler in EMPBioTrackerHandler.Handlers)
                    handler?.controller?.ClearTime();
            }

            if (itemToDisable.PlayerFlashLight)
            {
                foreach (var handler in EMPPlayerFlashLightHandler.Handlers)
                    handler?.controller?.AddTime(float.PositiveInfinity);
            }
            else
            {
                foreach (var handler in EMPPlayerFlashLightHandler.Handlers)
                    handler?.controller?.ClearTime();
            }

            if (itemToDisable.PlayerHUD)
            {
                foreach (var handler in EMPPlayerHudHandler.Handlers)
                    handler?.controller?.AddTime(float.PositiveInfinity);
            }
            else
            {
                foreach (var handler in EMPPlayerHudHandler.Handlers)
                    handler?.controller?.ClearTime();
            }

            if (itemToDisable.GunSight)
            {
                foreach(var handler in EMPGunSightHandler.Handlers)
                    handler?.controller?.AddTime(float.PositiveInfinity);                
            }
            else
            {
                foreach (var handler in EMPGunSightHandler.Handlers)
                    handler?.controller?.ClearTime();
            }

            if (itemToDisable.Sentry)
            {
                foreach (var handler in EMPSentryHandler.Handlers)
                    handler?.controller?.AddTime(float.PositiveInfinity);
            }
            else
            {
                foreach (var handler in EMPSentryHandler.Handlers)
                    handler?.controller?.ClearTime();
            }
        }

        public void Reset()
        {
            nextUpdateTime = float.NaN;
        }

        static PlayerpEMPComponent()
        {

        }
    }
}
