using System.Collections.Generic;
using UnityEngine;

namespace PrefabLightmapBaker {

[System.Serializable]
    public class LightMapData {

        [System.Serializable]
        public struct RendererInfo {
            public Renderer renderer;
            public int lightmapIndex;
            public Vector4 lightmapOffsetScale;
        }

        [System.Serializable]
        public struct LightInfo {
            public Light light;
            public int lightmapBaketype;
            public int mixedLightingMode;
        }

        public List<GameObject> prefabs = new();
        public List<RendererInfo> m_RendererInfo = new();
        public List<Texture2D> m_Lightmaps = new();
        public List<Texture2D> m_LightmapsDir = new();
        public List<Texture2D> m_ShadowMasks = new();
        public List<LightInfo> m_LightInfo = new();
    }
}
