using System.Collections.Generic;
using UnityEngine;

namespace PrefabLightmapBaker {

    [System.Serializable]
    public class LightMapDataSO : ScriptableObject {
        public LightMapData Data = new();
    }
}
