using ExtraObjectiveSetup.Expedition.EMP.Handlers;
using Player;
using UnityEngine;
using GTFO.API;

namespace ExtraObjectiveSetup.Expedition.EMP
{
    public class PlayerpEMPComponent : MonoBehaviour
    {
        private float nextUpdateTime = float.NaN;

        public const float UPDATE_INTERVAL = 1.0f;

        public static PlayerpEMPComponent Current { get; private set; } = null;

        public bool InAnypEMP { get; private set; } = false;

        void Update()
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            if (!float.IsNaN(nextUpdateTime) && Clock.Time < nextUpdateTime) return;

            nextUpdateTime = Clock.Time + UPDATE_INTERVAL;

            var player = PlayerManager.GetLocalPlayerAgent();
            if( player == null ) 
            {
                return;
            }

            ItemToDisable itemToDisable = new() {
                BioTracker = false,
                PlayerFlashLight = false,
                PlayerHUD = false,
                //EnvLight = false,
                GunSight = false,
                Sentry = false
            };

            InAnypEMP = false;
            foreach (var pEMP in EMPManager.Current.pEMPs)
            {
                if (pEMP.State != ActiveState.ENABLED) continue;

                if (pEMP.InRange(player.m_position))
                {
                    itemToDisable.BioTracker |= pEMP.def.ItemToDisable.BioTracker;
                    itemToDisable.PlayerFlashLight |= pEMP.def.ItemToDisable.PlayerFlashLight;
                    itemToDisable.PlayerHUD |= pEMP.def.ItemToDisable.PlayerHUD;
                    //itemToDisable.EnvLight |= pEMP.def.ItemToDisable.EnvLight;
                    itemToDisable.GunSight |= pEMP.def.ItemToDisable.GunSight;
                    itemToDisable.Sentry |= pEMP.def.ItemToDisable.Sentry;
                    InAnypEMP = true;
                }
            }

            if(itemToDisable.BioTracker) 
                EMPBioTrackerHandler.Instance?.controller.AddTime(float.PositiveInfinity);
            else
                EMPBioTrackerHandler.Instance?.controller.ClearTime();

            if(itemToDisable.PlayerFlashLight)
                EMPPlayerFlashLightHandler.Instance?.controller.AddTime(float.PositiveInfinity);
            else
                EMPPlayerFlashLightHandler.Instance?.controller.ClearTime();

            if (itemToDisable.PlayerHUD)
                EMPPlayerHudHandler.Instance?.controller.AddTime(float.PositiveInfinity);
            else
                EMPPlayerHudHandler.Instance?.controller.ClearTime();

            if (itemToDisable.GunSight)
            {
                EMPGunSightHandler.StandardHandler?.controller.AddTime(float.PositiveInfinity);
                EMPGunSightHandler.SpecialHandler?.controller.AddTime(float.PositiveInfinity);
            }
            else
            {
                EMPGunSightHandler.StandardHandler?.controller.ClearTime();
                EMPGunSightHandler.SpecialHandler?.controller.ClearTime();
            }

            if (itemToDisable.Sentry)
                EMPSentryHandler.Instance?.controller.AddTime(float.PositiveInfinity);
            else
                EMPSentryHandler.Instance?.controller.ClearTime();
        }

        public void Reset()
        {
            nextUpdateTime = float.NaN;
        }

        static PlayerpEMPComponent()
        {
            LevelAPI.OnBuildStart += () =>
            {
                if (Current == null)
                {
                    Current = PlayerManager.Current.m_localPlayerAgentInLevel.gameObject.AddComponent<PlayerpEMPComponent>();

                    LevelAPI.OnBuildStart += Current.Reset;
                    LevelAPI.OnLevelCleanup += Current.Reset;
                }
            };
        }
    }
}
