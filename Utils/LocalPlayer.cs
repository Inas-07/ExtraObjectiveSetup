using System.Collections;
using GTFO.API;
using GTFO.API.Utilities;
using Player;
using UnityEngine;

namespace ExtraObjectiveSetup.Utils
{
    /// <summary>
    /// Cached local player info. Behaviour mirrors FloLib.Infos.LocalPlayer.
    /// </summary>
    public static class LocalPlayer
    {
        public static bool HasAgent { get; private set; }
        public static PlayerAgent Agent { get; private set; }
        public static LocalPlayerAgent LocalAgent { get; private set; }

        private static int? _InstanceID;
        private static bool _started;

        public static bool TryGetAgent(out PlayerAgent agent)
        {
            agent = Agent;
            return HasAgent;
        }

        public static bool TryGetLocalAgent(out LocalPlayerAgent agent)
        {
            agent = LocalAgent;
            return HasAgent;
        }

        public static Vector3 GetPosition() => HasAgent ? Agent.Position : Vector3.zero;

        public static Vector3 GetPosition(Vector3 fallback) => HasAgent ? Agent.Position : fallback;

        public static Vector3 GetEyePosition() => HasAgent ? Agent.EyePosition : Vector3.zero;

        public static Vector3 GetEyePosition(Vector3 fallback) => HasAgent ? Agent.EyePosition : fallback;

        internal static void Init()
        {
            AssetAPI.OnStartupAssetsLoaded += StartPolling;
            if (AssetAPI.Status != null && AssetAPI.Status.Ready)
            {
                StartPolling();
            }
        }

        private static void StartPolling()
        {
            if (_started) return;
            _started = true;
            CoroutineDispatcher.StartCoroutine(UpdatePlayerInfo());
        }

        private static IEnumerator UpdatePlayerInfo()
        {
            while (true)
            {
                if (PlayerManager.HasLocalPlayerAgent())
                {
                    var agent = PlayerManager.GetLocalPlayerAgent();
                    var instanceID = agent.GetInstanceID();
                    if (!_InstanceID.HasValue || _InstanceID.Value != instanceID)
                    {
                        Agent = agent;
                        LocalAgent = agent.Cast<LocalPlayerAgent>();
                        _InstanceID = instanceID;
                        HasAgent = true;
                    }
                }
                else
                {
                    Agent = null;
                    LocalAgent = null;
                    _InstanceID = null;
                    HasAgent = false;
                }

                yield return null;
            }
        }
    }
}
