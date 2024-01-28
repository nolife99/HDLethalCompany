using System;
using GameNetcodeStuff;
using HarmonyLib;
using HDLethalCompany.Tools;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace HDLethalCompany.Patching;

internal static class HDPatches
{
    [HarmonyPatch(typeof(PlayerControllerB), "Start")]
    static void Prefix(Camera ___gameplayCamera)
    {
        var local = Resources.FindObjectsOfTypeAll(typeof(HDAdditionalCameraData));
        ref var cameras = ref UnsafeUtility.As<UnityEngine.Object[], HDAdditionalCameraData[]>(ref local);

        foreach (var cameraData in cameras)
        {
            if (cameraData.gameObject.name == "MapCamera") continue;
            cameraData.customRenderingSettings = true;

            GraphicsPatch.ToggleCustomPass(cameraData);
            GraphicsPatch.SetLevelOfDetail(cameraData);
            GraphicsPatch.ToggleVolumetricFog(cameraData);

            if (!GraphicsPatch.Foliage) cameraData.GetComponent<Camera>().cullingMask &= ~(1 << 10);

            GraphicsPatch.SetShadowQuality(cameraData);
            if (cameraData.gameObject.name is "SecurityCamera" or "ShipCamera") continue;
            GraphicsPatch.SetAntiAliasing(cameraData);
        }

        GraphicsPatch.SetTextureQuality();
        GraphicsPatch.SetFogQuality();

        if (GraphicsPatch.ResFix && GraphicsPatch.Multiplier != 1)
        {
            var texture = ___gameplayCamera.targetTexture;
            texture.width = (int)MathF.Round(GraphicsPatch.WidthRes);
            texture.height = (int)MathF.Round(GraphicsPatch.HeightRes);
        }
    }

    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    static void Prefix()
    {
        GraphicsPatch.SetFogQuality();
        if (GraphicsPatch.LevelOfDetail == 0) GraphicsPatch.RemoveLodFromGameObject("CatwalkStairs");
    }

    static readonly Rect playerScreen = GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen").GetComponent<RectTransform>().rect;

    [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
    static void Postfix(PlayerControllerB playerScript, HUDManager __instance)
    {
        if (!GraphicsPatch.ResFix || GraphicsPatch.Multiplier == 1) return;

        var nodes = Reflection.get_scanNodes(__instance);
        foreach (var element in __instance.scanElements) if (nodes.TryGetValue(element, out var node))
        {
            var vector = playerScript.gameplayCamera.WorldToViewportPoint(node.transform.position);
            element.anchoredPosition = new(playerScreen.x + playerScreen.width * vector.x, playerScreen.y + playerScreen.height * vector.y);
        }
    }
}