using GTFO.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ExtraObjectiveSetup.Utils
{
    public static class VanillaTMPPros
    {
        public const string VANILLA_CP_PREFAB_PATH = "Assets/AssetPrefabs/Complex/Generic/ChainedPuzzles/CP_Bioscan_sustained_RequireAll.prefab";
    
        public static GameObject Instantiate(GameObject parent = null)
        {
            var vanillaCP = AssetAPI.GetLoadedAsset<GameObject>(VANILLA_CP_PREFAB_PATH);
            if(vanillaCP == null)
            {
                EOSLogger.Error("VanillaTMPPros.Instantiate: Cannot find TMPPro from vanilla CP!");
                return null;
            }

            var templateGO = vanillaCP.transform.GetChild(0).GetChild(1).GetChild(0).gameObject;
            
            return parent != null ? GameObject.Instantiate(templateGO.gameObject, parent.transform) : GameObject.Instantiate(templateGO.gameObject);
        }
    }
}
