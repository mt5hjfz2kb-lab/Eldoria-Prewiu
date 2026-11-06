#if UNITY_EDITOR
using System;
using System.IO;
using Eldoria.UI.RoyalGold;
using Eldoria.VisualKit;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Safe editor-only A/B preview. Nothing is patched into scenes or builds.
/// Does not start Unity processes, modify gameplay/saves or reserve shared runners.
/// Enabling/disabling during Play removes or applies only the transient skin.
/// </summary>
[InitializeOnLoad]
internal static class ForjaOscuraPreviewEditor
{
    private const string OptInKey = "Eldoria.ForjaOscura.LivePreview.EditorOnly.v02";
    private const string RootMenu = "Eldoria/Forja Oscura/";
    private static double nextPoll;

    static ForjaOscuraPreviewEditor()
    {
        EditorApplication.update -= Poll;
        EditorApplication.update += Poll;
    }

    private static void Poll() => Poll(false);

    [MenuItem(RootMenu + "Activar vista previa reversible")]
    private static void Enable()
    {
        EditorPrefs.SetBool(OptInKey, true);
        Debug.Log("[FORJA] Editor-only preview ENABLED. Enter Play to skin the EXISTING RoyalHUD.");
        Poll(force: true);
    }

    [MenuItem(RootMenu + "Restaurar interfaz original")]
    private static void Disable()
    {
        EditorPrefs.SetBool(OptInKey, false);
        foreach (var skin in UnityEngine.Object.FindObjectsByType<ForjaOscuraLiveHudSkin>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (skin) UnityEngine.Object.Destroy(skin);
        Debug.Log("[FORJA] Preview disabled. Old UI graphics and original handlers restored on the next frame.");
    }

    private static void Poll(bool force = false)
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (!EditorPrefs.GetBool(OptInKey, false)) return;
        if (!force && EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + .35;
        foreach (var hud in UnityEngine.Object.FindObjectsByType<RoyalHUD>(
                     FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!hud || !hud.enabled || !hud.gameObject.activeInHierarchy) continue;
            if (!hud.GetComponent<ForjaOscuraLiveHudSkin>())
            {
                var skin = hud.gameObject.AddComponent<ForjaOscuraLiveHudSkin>();
                Debug.Log("[FORJA] Live HUD found: " + hud.name + ". Visual skin attached in Play only.");
            }
        }
    }

    [MenuItem(RootMenu + "Diagnóstico HUD con datos auténticos")]
    private static void Diagnostics()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("[FORJA] Enter Play to inspect REAL Kingdom and building data.");
            return;
        }
        foreach (var hud in UnityEngine.Object.FindObjectsByType<RoyalHUD>(
                     FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var skin = hud.GetComponent<ForjaOscuraLiveHudSkin>();
            var nav = hud.Navigation;
            int realButtons = 0;
            if (nav && nav.Items != null)
                foreach (var item in nav.Items)
                    if (item != null && item.Button && item.Button.gameObject.activeInHierarchy) realButtons++;
            string wood = hud.Resources && hud.Resources.Wood ? hud.Resources.Wood.text : "unavailable";
            string stone = hud.Resources && hud.Resources.Stone ? hud.Resources.Stone.text : "unavailable";
            string food = hud.Resources && hud.Resources.Food && hud.Resources.Food.gameObject.activeSelf
                ? hud.Resources.Food.text : "locked/hidden";
            string mission = hud.Mission && hud.Mission.Objective ? hud.Mission.Objective.text : "unavailable";
            string building = hud.Building && hud.Building.Title ? hud.Building.Title.text : "not selected";
            string action = hud.Building && hud.Building.Primary && hud.Building.Primary.Label
                ? hud.Building.Primary.Label.text : "unavailable";
            Debug.Log("[FORJA] REAL HUD snapshot -> wood=" + wood + ", stone=" + stone +
                ", food=" + food + ", objective=" + mission + ", building=" + building +
                ", action=" + action + ", visibleNavButtons=" + realButtons +
                ", skin[nav/mission/building]=" + (skin && skin.NavigationSkinned) + "/" +
                (skin && skin.ObjectiveSkinned) + "/" + (skin && skin.BuildingSkinned));
        }
    }

    [MenuItem(RootMenu + "Capturar GameView real (Play)")]
    private static void Screenshot()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("[FORJA] Screenshot requires Play and the existing city scene.");
            return;
        }
        var root = Directory.GetParent(Application.dataPath).FullName;
        var destination = Path.Combine(root, "ForjaPreviewEvidence");
        Directory.CreateDirectory(destination);
        string state = EditorPrefs.GetBool(OptInKey, false) ? "candidate" : "baseline";
        string file = Path.Combine(destination,
            "ELDORIA_FORJA_" + state + "_" + Screen.width + "x" + Screen.height + "_" +
            DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ") + ".png");
        ScreenCapture.CaptureScreenshot(file);
        Debug.Log("[FORJA] Screenshot REQUESTED in current GameView: " + file +
            ". Verify file exists before considering the evidence collected.");
    }
}
#endif
