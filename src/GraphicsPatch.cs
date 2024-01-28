using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace HDLethalCompany.Patching;

internal static class GraphicsPatch
{
    public static bool PostProcess, EnableFog, AntiAlias, ResFix, Foliage;
    public static int FogQuality, TextureRes, LevelOfDetail, ShadowQuality;
    public static float AnchorOffsetZ, Multiplier, WidthRes, HeightRes;
    public static AssetBundle Assets;

    public static void SetShadowQuality(HDAdditionalCameraData cameraData)
    {
        if (!Assets)
        {
            Debug.LogError("HDLETHALCOMPANY: Something is wrong with the Asset Bundle - Null");
            return;
        }

        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.ShadowMaps] = true;
        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, ShadowQuality != 0);

        QualitySettings.renderPipeline = 
            ShadowQuality == 1 ? Assets.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/VeryLowShadowsAsset.asset") : 
            (ShadowQuality == 2 ? Assets.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/MediumShadowsAsset.asset") : 
            (HDRenderPipelineAsset)QualitySettings.renderPipeline);
    }
    public static void SetLevelOfDetail(HDAdditionalCameraData cameraData)
    {
        if (LevelOfDetail == 1) return;
        var mask = cameraData.renderingPathCustomFrameSettingsOverrideMask;
        var settings = cameraData.renderingPathCustomFrameSettings;

        mask.mask[(int)FrameSettingsField.LODBiasMode] = true;
        mask.mask[(int)FrameSettingsField.LODBias] = true;

        settings.SetEnabled(FrameSettingsField.LODBiasMode, true);
        settings.SetEnabled(FrameSettingsField.LODBias, true);

        settings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
        settings.lodBias = LevelOfDetail == 0 ? .6f : 2.3f;

        var camera = cameraData.GetComponent<Camera>();
        if (LevelOfDetail == 0 && camera.farClipPlane > 180) camera.farClipPlane = 170;
    }
    public static void SetTextureQuality()
    {
        if (TextureRes < 3) QualitySettings.globalTextureMipmapLimit = 3 - TextureRes;
    }
    public static void SetAntiAliasing(HDAdditionalCameraData cameraData)
    {
        if (AntiAlias) cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
    }
    public static void ToggleCustomPass(HDAdditionalCameraData cameraData)
    {
        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, PostProcess);
    }
    public static void ToggleVolumetricFog(HDAdditionalCameraData cameraData)
    {
        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, EnableFog);
    }
    public static void SetFogQuality()
    {
        var local = Resources.FindObjectsOfTypeAll(typeof(Volume));
        if (local.Length == 0)
        {
            Debug.LogError("No volumes found");
            return;
        }

        ref var volumes = ref UnsafeUtility.As<Object[], Volume[]>(ref local);
        for (var i = 0; i < volumes.Length; ++i)
        {
            if (!volumes[i].sharedProfile.TryGet(out Fog fog)) continue;

            fog.quality.Override(3);
            switch (FogQuality)
            {
                case 0: // Very Low
                    fog.volumetricFogBudget = .1f;
                    fog.resolutionDepthRatio = .5f;
                    break;

                case 2: // Medium
                    fog.volumetricFogBudget = .333f;
                    fog.resolutionDepthRatio = .5f;
                    break;

                case 3: // High
                    fog.volumetricFogBudget = .666f;
                    fog.resolutionDepthRatio = .5f;
                    break;
            }
        }
    }
    public static void RemoveLodFromGameObject(string name)
    {
        var local = Resources.FindObjectsOfTypeAll(typeof(LODGroup));
        ref var lods = ref UnsafeUtility.As<Object[], LODGroup[]>(ref local);

        foreach (var lod in lods) if (lod.gameObject.name == name) lod.enabled = false;
    }
}