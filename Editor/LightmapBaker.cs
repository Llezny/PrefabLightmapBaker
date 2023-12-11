using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace PrefabLightmapBaker {
    public class LightmapBaker : Editor {


        public const string SO_PATH = "Assets/ScriptableObjects/";

        
        [MenuItem("LuakszTools/Bake Prefab Lightmaps")]
        static void GenerateLightmapInfo()
        {
            if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand) {
                Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                return;
            }
            Lightmapping.BakeAsync();
            Lightmapping.bakeCompleted += OnBaked;
 
        }


        private static void OnBaked() {
            var prefabLightmapData = CreateInstance<PrefabLightmaps>();
            prefabLightmapData.Data = new();
            
            var renderersOnScene = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

            foreach (MeshRenderer renderer in renderersOnScene)
            {
                if (renderer.lightmapIndex != -1)
                {
                    LightMapData.RendererInfo info = new ();
                    info.renderer = renderer;

                    if (renderer.lightmapScaleOffset != Vector4.zero) {
                        //1ibrium's pointed out this issue : https://docs.unity3d.com/ScriptReference/Renderer-lightmapIndex.html
                        if(renderer.lightmapIndex < 0 || renderer.lightmapIndex == 0xFFFE) continue;


                        info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                        Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                        Texture2D lightmapDir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                        Texture2D shadowMask = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;

                        info.lightmapIndex = renderer.lightmapIndex;//prefabLightmapData.Data.m_Lightmaps.IndexOf(lightmap);
                        if (info.lightmapIndex != -1)
                        {
                            info.lightmapIndex = prefabLightmapData.Data.m_Lightmaps.Count;
                            prefabLightmapData.Data.m_Lightmaps.Add(lightmap);
                            prefabLightmapData.Data.m_LightmapsDir.Add(lightmapDir);
                            prefabLightmapData.Data.m_ShadowMasks.Add(shadowMask);
                        }
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(renderer.gameObject);
                        prefabLightmapData.Data.prefabs.Add( prefab );
                        prefabLightmapData.Data.m_RendererInfo.Add( info);
                    }

                }
            }
            
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

            foreach (Light l in lights)
            {
                var lightInfo = new LightMapData.LightInfo();
                lightInfo.light = l;
                lightInfo.lightmapBaketype = (int)l.lightmapBakeType;
    #if UNITY_2020_1_OR_NEWER
                lightInfo.mixedLightingMode = (int)UnityEditor.Lightmapping.lightingSettings.mixedBakeMode;            
    #elif UNITY_2018_1_OR_NEWER
                lightInfo.mixedLightingMode = (int)UnityEditor.LightmapEditorSettings.mixedBakeMode;
    #else
                lightInfo.mixedLightingMode = (int)l.bakingOutput.lightmapBakeType;            
    #endif
                prefabLightmapData.Data.m_LightInfo.Add(lightInfo);

            }


            CreateOrReplaceAsset( prefabLightmapData, SO_PATH + "Configs/LightmapsData.asset" );
            Lightmapping.bakeCompleted -= OnBaked;
        }




        private static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
 
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                asset.name = Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                existingAsset.name = Path.GetFileNameWithoutExtension(path);
            }

            return existingAsset;
        }
    }










    }
