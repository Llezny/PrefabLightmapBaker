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

            var prefabLightmapData = CreateInstance<PrefabLightmaps>();
            prefabLightmapData.Data = new();
            
            Lightmapping.Bake();

      
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
                        if (info.lightmapIndex == -1)
                        {
                            info.lightmapIndex = prefabLightmapData.Data.m_Lightmaps.Count;
                            prefabLightmapData.Data.m_Lightmaps.Add(lightmap);
                            prefabLightmapData.Data.m_LightmapsDir.Add(lightmapDir);
                            prefabLightmapData.Data.m_ShadowMasks.Add(shadowMask);
                        }
                        prefabLightmapData.Data.prefabs.Add( PrefabUtility.GetCorrespondingObjectFromOriginalSource(renderer.gameObject) );
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

    //         foreach (var instance in prefabs)
    //         {
    //             var gameObject = instance.gameObject;
    //             var rendererInfos = new List<RendererInfo>();
    //             var lightmaps = new List<Texture2D>();
    //             var lightmapsDir = new List<Texture2D>();
    //             var shadowMasks = new List<Texture2D>();
    //             var lightsInfos = new List<LightInfo>();

    //             GenerateLightmapInfo(gameObject, rendererInfos, lightmaps, lightmapsDir, shadowMasks, lightsInfos);

    //             instance.m_RendererInfo = rendererInfos.ToArray();
    //             instance.m_Lightmaps = lightmaps.ToArray();
    //             instance.m_LightmapsDir = lightmapsDir.ToArray();
    //             instance.m_LightInfo = lightsInfos.ToArray();
    //             instance.m_ShadowMasks = shadowMasks.ToArray();
    // #if UNITY_2018_3_OR_NEWER
    //             var targetPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject) as GameObject;
    //             if (targetPrefab != null)
    //             {
    //                 GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(instance.gameObject);// 根结点
    //                 //如果当前预制体是是某个嵌套预制体的一部分（IsPartOfPrefabInstance）
    //                 if (root != null)
    //                 {
    //                     GameObject rootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
    //                     string rootPath = AssetDatabase.GetAssetPath(rootPrefab);
    //                     //打开根部预制体
    //                     PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);
    //                     try
    //                     {
    //                         //Apply各个子预制体的改变
    //                         PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
    //                     }
    //                     catch { }
    //                     finally
    //                     {
    //                         //重新更新根预制体
    //                         PrefabUtility.SaveAsPrefabAssetAndConnect(root, rootPath, InteractionMode.AutomatedAction);
    //                     }
    //                 }
    //                 else
    //                 {
    //                     PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
    //                 }
    //             }
    // #else
    //             var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
    //             if (targetPrefab != null)
    //             {
    //                 //UnityEditor.Prefab
    //                 UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
    //             }
    // #endif
    //         }

            CreateOrReplaceAsset( prefabLightmapData, SO_PATH + "Configs/LightmapsData.asset" );
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
