using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ExtraObjectiveSetup.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace ExtraObjectiveSetup.Expedition.EMP.Handlers
{
    public class EMPGunSightHandler : EMPHandler
    {
        public GameObject[] _sightPictures;

        public override void Setup(GameObject gameObject, EMPController controller)
        {
            Il2CppArrayBase<Renderer> componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(true);
            if (componentsInChildren != null)
                _sightPictures = componentsInChildren.Where(x => x.sharedMaterial != null && x.sharedMaterial.shader != null).Where(
                            x => x.sharedMaterial.shader.name.Contains("HolographicSight")).Select(
                        x => x.gameObject)
.ToArray(
                    );
            if (_sightPictures != null && _sightPictures.Length >= 1)
                return;
            EOSLogger.Warning($"Unable to find sight on {gameObject.name}!");
        }

        protected override void DeviceOff() => ForEachSights(x => x.SetActive(false));

        protected override void DeviceOn() => ForEachSights(x => x.SetActive(true));

        protected override void FlickerDevice() => ForEachSights(x => x.SetActive(FlickerUtil()));

        private void ForEachSights(Action<GameObject> action)
        {
            if (_sightPictures == null)
                return;
            foreach (GameObject sightPicture in _sightPictures)
            {
                if (sightPicture != null && action != null)
                    action(sightPicture);
            }
        }
    }

}
