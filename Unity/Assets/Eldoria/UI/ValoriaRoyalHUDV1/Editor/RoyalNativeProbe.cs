using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold.Preview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Eldoria.UI.RoyalGold.Editor
{
    /// <summary>Explicit isolated native gate. Never starts itself, changes Valoria, writes saves or publishes a build.</summary>
    [InitializeOnLoad]
    public static class RoyalNativeProbe
    {
        private const string Root = "Assets/Eldoria/UI/ValoriaRoyalHUDV1";
        private const string Key = "Eldoria.RoyalGold.NativeProbe";
        private static readonly List<string> Checks = new List<string>();
        private static readonly List<string> Captures = new List<string>();
        private static bool started;
        private static double waitUntil;
        private static int stage;
        private static string Output => Path.GetFullPath("../docs/ui-royal-gold-v1/native");

        static RoyalNativeProbe() { EditorApplication.update += Tick; }
        [MenuItem("Eldoria/Royal Gold/Run isolated native validation")]
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit PlayMode before the isolated gate.");
            Directory.CreateDirectory(Output);
            Checks.Clear(); Captures.Clear(); stage = 0; started = false;
            SessionState.SetBool(Key, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/RoyalGoldHUD.prefab");
            if (!prefab || !prefab.GetComponent<RoyalHUD>()) throw new InvalidOperationException("Royal HUD prefab or script not imported.");
            if (prefab.GetComponentsInChildren<Camera>(true).Length != 0 || prefab.GetComponentsInChildren<EventSystem>(true).Length != 0 ||
                prefab.GetComponent<RoyalPreviewDriver>()) throw new InvalidOperationException("Production prefab must not contain a camera, EventSystem or TEST driver.");
            EditorSceneManager.OpenScene(Root + "/Preview/RoyalGold_UI_TEST.unity", OpenSceneMode.Single);
            SessionState.SetString(Key + ".source", Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "manual");
            SessionState.SetFloat(Key + ".deadline", (float)EditorApplication.timeSinceStartup + 150f);
            SessionState.SetBool(Key, true);
            EditorApplication.EnterPlaymode();
        }
        private static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("ROYAL_NATIVE_FAIL: " + message);
            Checks.Add(message); Debug.Log("ROYAL_NATIVE_PASS " + message);
        }
        private static Vector2 Center(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var canvas = rect.GetComponentInParent<Canvas>();
            var camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.WorldToScreenPoint(camera, (corners[0] + corners[2]) * .5f);
        }
        private static void Click(RoyalButton button, bool dragged = false)
        {
            Check(button && button.gameObject.activeInHierarchy, "target active: " + (button ? button.name : "missing"));
            var point = Center((RectTransform)button.transform);
            var data = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left, position = point };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits);
            var target = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
            Check(target == button.gameObject, "real uGUI raycast resolves " + button.name);
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerDownHandler);
            if (dragged)
            {
                ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.beginDragHandler);
                data.position += new Vector2(30, 0);
                ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.dragHandler);
                ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.endDragHandler);
            }
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
        }
        private static void Tick()
        {
            if (!SessionState.GetBool(Key, false)) return;
            try
            {
                if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Key + ".deadline", 0)) throw new TimeoutException("Isolated native probe timed out.");
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
                if (!started) { started = true; waitUntil = EditorApplication.timeSinceStartup + 1.5; return; }
                if (EditorApplication.timeSinceStartup < waitUntil) return;
                var hud = Object.FindFirstObjectByType<RoyalHUD>();
                var driver = Object.FindFirstObjectByType<RoyalPreviewDriver>();
                Check(hud && driver, "isolated HUD and driver available");
                Canvas.ForceUpdateCanvases();
                if (stage == 0)
                {
                    Check(Object.FindObjectsByType<RoyalHUD>(FindObjectsSortMode.None).Length == 1, "one HUD");
                    Check(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.enabled) == 1, "one preview camera");
                    Check(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "one EventSystem");
                    Check(hud.TestMark.gameObject.activeSelf, "TEST watermark visible");
                    Check(hud.GetComponentsInChildren<Transform>(true).All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "no missing scripts");
                    driver.SetNormal(); hud.Refresh();
                    Check(hud.Resources.Wood.text == "900", "actual fixture wallet shown");
                    Check(hud.Building.Primary.Button.interactable, "normal action enabled");
                    Check(RoyalInputGate.Blocks(Center((RectTransform)hud.Building.Primary.Button.transform)), "UI intercepts its own pointer");
                    Capture(hud, "normal-16x9", 1600, 900);
                    Capture(hud, "normal-20x9", 2000, 900);
                    Capture(hud, "normal-tablet", 1200, 900);
                    Capture(hud, "normal-notch-simulated", 2000, 900, .04f);
                    driver.SetLocked(); hud.Refresh();
                    Check(!hud.Building.Primary.Button.interactable && hud.Resources.Wood.text == "0", "insufficient resources remain disabled");
                    Capture(hud, "locked", 1600, 900);
                    driver.SetError(); hud.Refresh(); Click(hud.Building.Primary.Button); hud.Refresh();
                    Check(hud.Feedback.text.Contains("Error simulado"), "simulated transport failure shown");
                    Check(hud.Resources.Wood.text == "900", "failed request leaves wallet unchanged");
                    Capture(hud, "error", 1600, 900);
                    stage = 1; waitUntil = EditorApplication.timeSinceStartup + .5; return;
                }
                if (stage == 1)
                {
                    driver.SetNormal(); hud.Refresh();
                    Click(hud.Building.Primary.Button, true); hud.Refresh();
                    Check(hud.Resources.Wood.text == "900", "drag does not submit command");
                    Click(hud.Building.Primary.Button); hud.Refresh();
                    Check(hud.Resources.Wood.text == RoyalFormatting.Amount(900 - SliceRules.SawmillWoodCost), "real gateway construction charged once");
                    Check(!hud.Building.Primary.Button.interactable, "action disabled during construction");
                    Capture(hud, "construction", 1600, 900);
                    stage = 2; waitUntil = EditorApplication.timeSinceStartup + SliceRules.SawmillBuildSeconds + .6; return;
                }
                if (stage == 2)
                {
                    hud.Refresh();
                    Check(hud.Building.Level.text.Contains("1"), "live gateway completion updates open panel");
                    Check(!hud.Building.Primary.gameObject.activeSelf, "no fictional sawmill upgrade button");
                    Capture(hud, "completed", 1600, 900);
                    Click(hud.Realm.Info);
                    Check(hud.Modal.gameObject.activeSelf, "realm modal opens");
                    Check(RoyalInputGate.Blocks(new Vector2(2, 2)), "modal blocks background gestures");
                    Capture(hud, "realm-modal", 1600, 900);
                    Click(hud.Modal.Close);
                    Check(!hud.Modal.gameObject.activeSelf, "modal closes");
                    float height = ((RectTransform)hud.Mission.transform).rect.height;
                    Click(hud.Mission.Toggle); Check(!hud.Mission.Details.gameObject.activeSelf, "mission collapse");
                    Click(hud.Mission.Toggle); Check(hud.Mission.Details.gameObject.activeSelf && Mathf.Abs(((RectTransform)hud.Mission.transform).rect.height - height) < .1f, "mission expands to original height");
                    hud.SetEnglish(true); hud.SelectBuilding("barracks"); hud.Refresh();
                    Check(hud.Building.Title.text == "Barracks", "English view projection");
                    Capture(hud, "english", 1600, 900);
                    Check(hud.Navigation.Items.Where(i => i.Section == RoyalSection.World || i.Section == RoyalSection.Inventory).All(i => !i.Button.gameObject.activeSelf), "unconnected destinations hidden");
                    Finish(true, "");
                }
            }
            catch (Exception error) { Debug.LogException(error); Finish(false, error.ToString()); }
        }
        private static void Capture(RoyalHUD hud, string name, int width, int height, float safeInset = 0)
        {
            var canvas = hud.GetComponent<Canvas>(); var camera = Camera.main;
            var scaler = hud.GetComponent<RoyalScaler>(); var areas = hud.GetComponentsInChildren<RoyalSafeArea>(true);
            var rects = areas.Select(a => (RectTransform)a.transform).ToArray();
            var min = rects.Select(r => r.anchorMin).ToArray(); var max = rects.Select(r => r.anchorMax).ToArray();
            var enabled = areas.Select(a => a.enabled).ToArray(); bool scalerEnabled = scaler.enabled;
            var mode = canvas.renderMode; var uiCamera = canvas.worldCamera; float scale = canvas.scaleFactor;
            float plane = canvas.planeDistance; int mask = camera.cullingMask; var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                scaler.enabled = false; for (int i = 0; i < areas.Length; i++) areas[i].enabled = false;
                camera.cullingMask = 1 << 5; camera.targetTexture = rt;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                canvas.scaleFactor = height / 900f;
                for (int i = 0; i < rects.Length; i++) { rects[i].anchorMin = new Vector2(safeInset, 0); rects[i].anchorMax = new Vector2(1 - safeInset, 1); }
                foreach (var layout in hud.GetComponentsInChildren<RoyalHudLayout>(true)) layout.Apply(width * (1 - 2 * safeInset) / canvas.scaleFactor);
                foreach (var text in hud.GetComponentsInChildren<RoyalText>(true)) if (!text.font) text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                Canvas.ForceUpdateCanvases(); camera.Render(); camera.Render();
                RenderTexture.active = rt; image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                string path = Path.Combine(Output, name + ".png"); File.WriteAllBytes(path, image.EncodeToPNG()); Captures.Add(Path.GetFileName(path));
                using (var hash = SHA256.Create()) File.WriteAllText(path + ".sha256", BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant());
            }
            finally
            {
                camera.targetTexture = oldTarget; camera.cullingMask = mask; RenderTexture.active = oldActive;
                canvas.renderMode = mode; canvas.worldCamera = uiCamera; canvas.planeDistance = plane; canvas.scaleFactor = scale;
                for (int i = 0; i < areas.Length; i++) { rects[i].anchorMin = min[i]; rects[i].anchorMax = max[i]; areas[i].enabled = enabled[i]; }
                scaler.enabled = scalerEnabled;
                foreach (var layout in hud.GetComponentsInChildren<RoyalHudLayout>(true)) layout.Apply(((RectTransform)layout.transform).rect.width);
                if (image) Object.DestroyImmediate(image); Object.DestroyImmediate(rt); Canvas.ForceUpdateCanvases();
            }
        }
        [Serializable] private sealed class Report
        {
            public string source_sha, unity_version, run_id, error;
            public bool technical_pass, native_unity = true, offscreen_ui_capture = true, integrated_in_valoria = false, physical_mobile_test = false, art_approved = false;
            public string[] checks, captures;
        }
        private static void Finish(bool pass, string error)
        {
            SessionState.SetBool(Key, false);
            Directory.CreateDirectory(Output);
            var report = new Report { source_sha = SessionState.GetString(Key + ".source", "unknown"), unity_version = UnityEngine.Application.unityVersion,
                run_id = Environment.GetEnvironmentVariable("GITHUB_RUN_ID"), technical_pass = pass, error = error, checks = Checks.ToArray(), captures = Captures.ToArray() };
            File.WriteAllText(Path.Combine(Output, "native-result.json"), JsonUtility.ToJson(report, true));
            Debug.Log(pass ? "ROYAL_GOLD_NATIVE_COMPLETE" : "ROYAL_GOLD_NATIVE_FAILED");
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(pass ? 0 : 1); else EditorApplication.ExitPlaymode();
        }
    }
}
