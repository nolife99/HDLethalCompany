﻿using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using GameNetcodeStuff;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using TMPro;
using HDLethalCompany.Patch;
using HDLethalCompany.Tools;
using System.Reflection;
using System.IO;
using UnityEngine.Rendering;

namespace HDLethalCompany
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Ver)]
    public class HDLethalCompanyInitialization : BaseUnityPlugin
    {
        //Configuration Entrys
        private static ConfigEntry<float> 
            config_ResMult;

        private static ConfigEntry<bool> 
            config_EnablePostProcessing, 
            config_EnableFog, 
            config_EnableAntialiasing,
            config_EnableResolution;

        private static ConfigEntry<int> 
            config_FogQuality, 
            config_TextureQuality, 
            config_LOD, 
            config_ShadowmapQuality;


        private Harmony _harmony;

        private void Awake()
        {
            Logger.LogInfo(PluginInfo.Guid + " loaded");

            ConfigFile();

            _harmony = new Harmony(PluginInfo.Guid);
            _harmony.PatchAll(typeof(GraphicsPatch));
            
        }

        private void ConfigFile()
        {
            //Setup Config File
            #region
            config_ResMult = Config.Bind("RESOLUTION", "Value", 2.233f, "Resolution Scale Multiplier - <EXAMPLES -> | 1.000 = 860x520p | 2.233 =~ 1920x1080p | 2.977 = 2560x1440p | 4.465 = 3840x2060p > - The UI scanned elements have slightly incorrect offsets after 3.000");
            config_EnableResolution = Config.Bind("RESOLUTION", "EnableRes", true, "Toggle Resolution Multiplier - In the case you want to use another solution or apply any widescreen mod");
            config_EnableAntialiasing = Config.Bind("EFFECTS", "EnableAA", false, "Anti-Aliasing (Unity's SMAA)");
            config_EnablePostProcessing = Config.Bind("EFFECTS", "EnablePP", true, "Post-Processing (Color grading)");
            config_TextureQuality = Config.Bind("EFFECTS", "TextureQuality", 3, "Texture Resolution Quality - <PRESETS -> | 0 = VERY LOW (1/8) | 1 = LOW (1/4) | 2 = MEDIUM (1/2) | 3 = HIGH (1/1 VANILLA) >");
            config_FogQuality = Config.Bind("EFFECTS", "FogQuality", 1, "Volumetric Fog Quality - <PRESETS -> | 0 = VERY LOW | 1 = VANILLA FOG | 2 = MEDIUM | 3 = HIGH >");
            config_EnableFog = Config.Bind("EFFECTS", "EnableFOG", true, "Volumetric Fog Toggle - Use this as a last resource in case lowering the fog quality is not enough to get decent performance");
            config_LOD = Config.Bind("EFFECTS", "LOD", 1, "Level Of Detail - <PRESETS -> | 0 = LOW (HALF DISTANCE) | 1 = VANILLA | 2 = HIGH (TWICE THE DISTANCE) >");
            config_ShadowmapQuality = Config.Bind("EFFECTS", "ShadowQuality", 2, "Shadows Resolution - <PRESETS -> 0 = VERY LOW (SHADOWS DISABLED)| 1 = LOW (256) | 2 = MEDIUM (1024) | 3 = VANILLA (2048) > - Shadowmap max resolution");
            #endregion

            //Load Config File
            #region
            GraphicsPatch.m_enableResolutionFix = config_EnableResolution.Value;
            GraphicsPatch.m_setShadowQuality = config_ShadowmapQuality.Value;
            GraphicsPatch.m_setLOD = config_LOD.Value;
            GraphicsPatch.m_setTextureResolution = config_TextureQuality.Value;
            GraphicsPatch.m_setFogQuality = config_FogQuality.Value;
            GraphicsPatch.multiplier = config_ResMult.Value;
            GraphicsPatch.anchorOffsetZ = 0.123f * config_ResMult.Value + 0.877f; //This formula works
            GraphicsPatch.m_widthResolution = 860 * config_ResMult.Value;
            GraphicsPatch.m_heightResolution = 520 * config_ResMult.Value;
            GraphicsPatch.m_enableAntiAliasing = config_EnableAntialiasing.Value;
            GraphicsPatch.m_enableFog = config_EnableFog.Value;
            GraphicsPatch.m_enablePostProcessing = config_EnablePostProcessing.Value;
            #endregion
        }
    }
    public static class PluginInfo
    {
        public const string
            Guid = "HDLethalCompany",
            Name = "HDLethalCompany-Sligili",
            Ver = "1.5.0";
    }
}

namespace HDLethalCompany.Patch
{
    internal class GraphicsPatch
    {
        //Graphics Settings
        public static bool 
            m_enablePostProcessing,
            m_enableFog,
            m_enableAntiAliasing,
            m_enableResolutionFix;
        public static int 
            m_setFogQuality,
            m_setTextureResolution,
            m_setLOD,
            m_setShadowQuality; 

        static HDRenderPipelineAsset myAsset;

        //Resolution Fix
            //Anchor offsets
        public static float 
            anchorOffsetX = 439.48f,
            anchorOffsetY = 244.8f,
            anchorOffsetZ;

            //Resolution multiplier
        public static float multiplier;

            //Resolution
        public static float 
            m_widthResolution,
            m_heightResolution;

        //Others
        static bool SetGlobalQualityFlag = true;
        static bool FogQualityflag=true;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void StartPrefix(PlayerControllerB __instance)
        {
            //Loading asset bundle from plugins folder
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "HDLethalCompany/hdlethalcompany"));

            UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(HDAdditionalCameraData));

            if (SetGlobalQualityFlag)
            {
                Debug.Log("Global quality settings applied");
   
                for (int i = 0; i < array.Length; i++)
                {
                    HDAdditionalCameraData cameraData = array[i] as HDAdditionalCameraData;

                    cameraData.customRenderingSettings = true;

                    SetShadowQuality(assetBundle, cameraData);

                    SetLevelOfDetail(cameraData);

                    SetAntiAliasing(cameraData);

                    ToggleCustomPass(cameraData, m_enablePostProcessing);

                    ToggleVolumetricFog(cameraData, m_enableFog);
                }
            }

            array = null;
            
            SetGlobalQualityFlag = false;

            SetTextureQuality();

            if (m_enableResolutionFix)
            {
                int newWidth = (int)Math.Round(m_widthResolution, 0),
                newHeight = (int)Math.Round(m_heightResolution, 0);

                __instance.gameplayCamera.targetTexture.width = newWidth;
                __instance.gameplayCamera.targetTexture.height = newHeight;
            }

            
        }

        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        private static void UpdateScanNodesPostfix(PlayerControllerB playerScript, HUDManager __instance)
        {
            //For some reason scanned elements won't render above this value so i had to limit it.
            if (anchorOffsetZ > 1.238f) anchorOffsetZ = 1.238f;

            if (FogQualityflag)
            {
                Debug.Log("FOGQUALITYMETHODCALLED-----------------------------");
                SetFogQuality(__instance.playerGraphicsVolume);
                FogQualityflag = false;
            }

            /*
            if (m_setFogQuality != 1)
            {
                Fog fog;
                __instance.playerGraphicsVolume.sharedProfile.TryGet<Fog>(out fog);
                fog.quality.Override(3); //sets preset to custom
                switch (m_setFogQuality)
                {
                    case -1: //VEEERY LOW BackCompat

                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                    case 0: //VEEERY LOW

                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                    case 2: //MEDIUM
                        fog.volumetricFogBudget = 0.333f;
                        fog.resolutionDepthRatio = 0.666f;
                        break;
                    case 3: //HIGH
                        fog.volumetricFogBudget = 0.666f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                }

                m_setFogQuality = 1;

            }
            */

            //I just rerun the entire updateScanNodes method basically, far from ideal but at the time i did this i was kinda braindead apparently and thought this was the only way. 
            //Honestly, this part should be remade to improve compatibility (and performance), but sorry, im lazy af.

            Vector3 vector = Vector3.zero;

            bool flag = false;
            int i = 0;

            while (i < __instance.scanElements.Length)
            {
                ScanNodeProperties scanNodeProperties;
                if ((Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>).Count > 0 && (Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>).TryGetValue(__instance.scanElements[i], out scanNodeProperties) && scanNodeProperties != null)
                {
                    try
                    {                    
                        if((bool)Reflection.CallMethod(__instance, "NodeIsNotVisible", scanNodeProperties, i))
                        {

                            goto IL_1CA;
                        }
                        if (!__instance.scanElements[i].gameObject.activeSelf)
                        {

                            __instance.scanElements[i].gameObject.SetActive(true);
                            __instance.scanElements[i].GetComponent<Animator>().SetInteger("colorNumber", scanNodeProperties.nodeType);

                            if (scanNodeProperties.creatureScanID != -1)
                            {

                                Traverse.Create(__instance).Method("AttemptScanNewCreature", scanNodeProperties.creatureScanID);
                            }
                        }
                    }
                    catch (Exception arg)
                    {
                        UnityEngine.Debug.LogError(string.Format("Error in updatescanNodes A: {0}", arg));
                    }
                    try
                    {
                        Traverse.Create(__instance).Field("scanElementText").SetValue(__instance.scanElements[i].gameObject.GetComponentsInChildren<TextMeshProUGUI>());
                        if ((Traverse.Create(__instance).Field("scanElementText").GetValue() as TextMeshProUGUI[]).Length > 1)
                        {
                            (Traverse.Create(__instance).Field("scanElementText").GetValue() as TextMeshProUGUI[])[0].text = scanNodeProperties.headerText;
                            (Traverse.Create(__instance).Field("scanElementText").GetValue() as TextMeshProUGUI[])[1].text = scanNodeProperties.subText;
                        }
                        if (scanNodeProperties.nodeType == 2)
                        {
                            flag = true;
                        }
                        vector = playerScript.gameplayCamera.WorldToScreenPoint(scanNodeProperties.transform.position);

                        __instance.scanElements[i].position = new Vector3(__instance.scanElements[i].position.x, __instance.scanElements[i].position.y, 12.17f * anchorOffsetZ);

                        __instance.scanElements[i].anchoredPosition = new Vector3(vector.x - anchorOffsetX * multiplier, vector.y - anchorOffsetY * multiplier);
                        if (!(multiplier > 3)) __instance.scanElements[i].localScale = new Vector3(multiplier, multiplier, multiplier);
                        else __instance.scanElements[i].localScale = new Vector3(3, 3, 3);

                        goto IL_1CA;
                    }
                    catch (Exception arg2)
                    {
                        UnityEngine.Debug.LogError(string.Format("Error in updatescannodes B: {0}", arg2));
                        goto IL_1CA;
                    }
                    goto IL_1A3;
                }
                goto IL_1A3;
            IL_1CA:
                i++;
                continue;
            IL_1A3:
                (Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>).Remove(__instance.scanElements[i]);
                __instance.scanElements[i].gameObject.SetActive(false);
                goto IL_1CA;
            }
            try
            {
                if (!flag)
                {
                    __instance.totalScrapScanned = 0;
                    Traverse.Create(__instance).Field("totalScrapScannedDisplayNum").SetValue(0);
                    Traverse.Create(__instance).Field("addToDisplayTotalInterval").SetValue(0.35f);
                }
                __instance.scanInfoAnimator.SetBool("display", (int)Reflection.GetInstanceField(typeof(HUDManager), __instance, "scannedScrapNum") >= 2 && flag);
            }
            catch (Exception arg3)
            {
                UnityEngine.Debug.LogError(string.Format("Error in updatescannodes C: {0}", arg3));
            }
        }


        //QUALITY SETTINGS
        public static void SetShadowQuality(AssetBundle assetBundle, HDAdditionalCameraData cameraData)
        {
            if (assetBundle == null)
            {
                Debug.LogError("HDLETHALCOMPANY: Something is wrong with the Asset Bundle");
                return;
            }

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.ShadowMaps] = true;

            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, m_setShadowQuality==0 ? false:true);

            myAsset = (m_setShadowQuality == 1 ? myAsset = assetBundle.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/VeryLowShadowsAsset.asset")
                : (m_setShadowQuality==2 ? myAsset = assetBundle.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/MediumShadowsAsset.asset") : (HDRenderPipelineAsset)QualitySettings.renderPipeline));

            QualitySettings.renderPipeline = myAsset;
            
        }

        public static void SetLevelOfDetail(HDAdditionalCameraData cameraData)
        {
            if (m_setLOD == 1) return;

            //Setting up custom frame settings
            #region
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBiasMode] = true;
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBias] = true;

            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBiasMode, true);
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBias, true);

            cameraData.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
            #endregion

            cameraData.renderingPathCustomFrameSettings.lodBias = m_setLOD == 0 ? 0.6f : 2.3f; 
            
        }

        public static void SetTextureQuality()
        {
            if (m_setTextureResolution >= 3) return;

            int textureQuality = 3 - m_setTextureResolution;

            QualitySettings.globalTextureMipmapLimit = textureQuality;

        }

        public static void SetAntiAliasing(HDAdditionalCameraData cameraData)
        {
            if (!m_enableAntiAliasing) return;

            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        public static void ToggleCustomPass(HDAdditionalCameraData cameraData, bool enable)
        {
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, enable);
        }

        public static void ToggleVolumetricFog(HDAdditionalCameraData cameraData, bool enable)
        {
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, enable);
        }

        public static void SetFogQuality(Volume volume)
        {
            Fog fog;
            if(volume.sharedProfile.TryGet<Fog>(out fog))
            {
                fog.quality.Override(3); //Custom level
                switch (m_setFogQuality)
                {
                    case -1: //Old config file used to have this value - Back Compat to old Very Low value
                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;

                    case 0: //Very Low
                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;

                    case 2: //Medium
                        fog.volumetricFogBudget = 0.333f;
                        fog.resolutionDepthRatio = 0.666f;
                        break;

                    case 3: //High
                        fog.volumetricFogBudget = 0.666f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                }
            }
            else
            {
                Debug.LogError("HDLETHALCOMPANY: Volume reference is wrong");
            }
            


        }
    }
}

namespace HDLethalCompany.Tools
{
    public class Reflection
    {
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        public static object CallMethod(object instance, string methodName, params object[] args)
        {
            var mi = instance.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(instance, args);
            }
            return null;
        }
    }
    
}
    