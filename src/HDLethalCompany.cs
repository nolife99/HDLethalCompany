using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HDLethalCompany.Patching;
using HDLethalCompany.Tools;
using UnityEngine;

namespace HDLethalCompany;

[BepInPlugin("HDLethalCompany", "HDLethalCompany-Sligili", "1.5.6")]
internal class HDLethalCompany : BaseUnityPlugin
{
    static ConfigEntry<float> config_ResMult;
    static ConfigEntry<bool> config_EnablePostProcessing, config_EnableFog, config_EnableAntialiasing, config_EnableResolution, config_EnableFoliage;
    static ConfigEntry<int> config_FogQuality, config_TextureQuality, config_LOD, config_ShadowmapQuality;
    static Harmony patcher;

    void Awake()
    {
        ConfigFile();
        Reflection.CompileTypedExpression();

        var type = GetType();
        using (var resource = type.Assembly.GetManifestResourceStream(type, "Resources.hdlethalcompany")) GraphicsPatch.Assets = AssetBundle.LoadFromStream(resource);

        new PatchClassProcessor(patcher = new("HDLethalCompany"), typeof(HDPatches), true).Patch();
        Logger.LogInfo("HDLethalCompany initialised");
    }
    void OnDestroy() => Harmony.UnpatchID(patcher.Id);

    void ConfigFile()
    {
        config_ResMult = Config.Bind("RESOLUTION", "Scale Multiplier", 2.232558f, "<EXAMPLES -> | 1 = 860x520p | 2.233 =~ 1920x1080p | 2.977 = 2560x1440p | 4.465 = 3840x2060p >");
        config_EnableResolution = Config.Bind("RESOLUTION", "Enable Resolution Fix", true, "If you wanna use another resolution mod or apply any widescreen mod while keeping the graphics settings");
        config_EnableAntialiasing = Config.Bind("EFFECTS", "Enable Anti-Aliasing", false, "Anti-Aliasing (FXAA)");
        config_EnablePostProcessing = Config.Bind("EFFECTS", "Enable Post-Processing Shader", true);
        config_TextureQuality = Config.Bind("EFFECTS", "Texture Quality", 3, "<PRESETS -> | 0 = VERY LOW (1/8) | 1 = LOW (1/4) | 2 = MEDIUM (1/2) | 3 = HIGH (1/1 VANILLA) >");
        config_FogQuality = Config.Bind("EFFECTS", "Volumetric Fog Quality", 1, "<PRESETS -> | 0 = VERY LOW | 1 = VANILLA FOG | 2 = MEDIUM | 3 = HIGH >");
        config_EnableFog = Config.Bind("EFFECTS", "Enable Volumetric Fog", true, "Use as a last resort if lowering the fog quality is not enough to get decent performance");
        config_LOD = Config.Bind("EFFECTS", "Level Of Detail", 1, "<PRESETS -> | 0 = LOW (HALF DISTANCE) | 1 = VANILLA | 2 = HIGH (TWICE THE DISTANCE) >");
        config_ShadowmapQuality = Config.Bind("EFFECTS", "Shadow Quality", 2, "<PRESETS -> 0 = VERY LOW (SHADOWS DISABLED) | 1 = LOW (256) | 2 = MEDIUM (1024) | 3 = VANILLA (2048) > - Shadowmap max resolution");
        config_EnableFoliage = Config.Bind("EFFECTS", "Enable Foliage", true, "If the game camera should or not render bushes/grass (trees won't be affected)");

        GraphicsPatch.Foliage = config_EnableFoliage.Value;
        GraphicsPatch.ResFix = config_EnableResolution.Value;
        GraphicsPatch.ShadowQuality = config_ShadowmapQuality.Value;
        GraphicsPatch.LevelOfDetail = config_LOD.Value;
        GraphicsPatch.TextureRes = config_TextureQuality.Value;
        GraphicsPatch.FogQuality = config_FogQuality.Value;
        GraphicsPatch.Multiplier = config_ResMult.Value;
        GraphicsPatch.WidthRes = 860 * config_ResMult.Value;
        GraphicsPatch.HeightRes = 520 * config_ResMult.Value;
        GraphicsPatch.AntiAlias = config_EnableAntialiasing.Value;
        GraphicsPatch.EnableFog = config_EnableFog.Value;
        GraphicsPatch.PostProcess = config_EnablePostProcessing.Value;
    }
}