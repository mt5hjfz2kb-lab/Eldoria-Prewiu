#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Eldoria.BreachWatch;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.BastionCardFidelityPilot.Editor
{
    // No production scene, save, button callback, prefab or HUD source is rewritten.
    // Native comparison: same live player, real Bastion, camera and screen dimensions.
    [InitializeOnLoad]
    public static class BastionCardFidelityCapture
    {
        const string Flag = "Eldoria.BastionCardFidelity.Capture";
        const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string Output = "BastionCardFidelityEvidence";
        const string PreviousUiFlag = "Eldoria.BastionCardFidelity.PreviousUiFlag";
        static bool busy;
        static int phase, storyPages;
        static double deadline, namedAt;
        static ValoriaIntegratedWorld host;

        [Serializable] public sealed class Shot
        {
            public string variant, file, sha256, buildingName, actionCaption, surface, action;
            public int width, height;
            public bool realPortrait, liveState, productionVigilia, oldCanvasHidden;
        }
        [Serializable] public sealed class Evidence
        {
            public string status, project, scene, baseline, candidate, modelSource, warning;
            public bool artApproved, realUnityRender, iPhoneTested, saveUnchanged;
            public Shot[] shots;
        }
        static BastionCardFidelityCapture()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        [MenuItem("Eldoria/Arte/Piloto fidelidad ficha Bastion - A B Unity")]
        public static void CaptureBatch()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Start the pilot outside Play mode");
            if (SessionState.GetBool(Flag, false))
                throw new InvalidOperationException("Another fidelity pilot is already running");
            string previous = Environment.GetEnvironmentVariable("ELDORIA_NEW_UI_ONLY");
            SessionState.SetString(PreviousUiFlag, previous ?? "<unset>");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE", "1");
            Environment.SetEnvironmentVariable("ELDORIA_NEW_UI_ONLY", "1");
            EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            SessionState.SetBool(Flag, true);
            phase = storyPages = 0;
            busy = false;
            deadline = 0;
            host = null;
            EditorApplication.EnterPlaymode();
        }
        static void Need(bool condition, string detail)
        {
            if (!condition) throw new InvalidOperationException(detail);
        }
        static void Complete(bool passed, Exception failure = null)
        {
            SessionState.SetBool(Flag, false);
            var previous = SessionState.GetString(PreviousUiFlag, "<unset>");
            Environment.SetEnvironmentVariable("ELDORIA_NEW_UI_ONLY",
                previous == "<unset>" ? null : previous);
            if (failure != null)
            {
                Debug.LogException(failure);
                var folder = Path.GetFullPath(Path.Combine(
                    UnityEngine.Application.dataPath, "../" + Output));
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "error.txt"), failure.ToString());
            }
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(passed ? 0 : 1);
            else EditorApplication.ExitPlaymode();
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Flag, false) || busy || !EditorApplication.isPlaying) return;
            try
            {
                if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 240;
                Need(EditorApplication.timeSinceStartup < deadline,
                    "Disposable native scene did not become ready within the test deadline");
                if (!host) host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if (!host || !host.Ready || !host.Hud || !host.City) return;
                Need(host.City.Probe, "Refusing to touch an owner's real save");
                if (phase == 0)
                {
                    Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.CompleteProbePlayerName(host);
                    namedAt = EditorApplication.timeSinceStartup;
                    phase = 1;
                    return;
                }
                if (host.Hud.NarrativeOpen)
                {
                    Need(storyPages++ < 30, "Canonical onboarding dialogue did not finish");
                    Need(host.Hud.Modal && host.Hud.Modal.Extra,
                        "Cannot advance the genuine gameplay narrative");
                    host.Hud.Modal.Extra.onClick.Invoke();
                    return;
                }
                if (EditorApplication.timeSinceStartup - namedAt < .9) return;
                busy = true;
                CaptureOneRealBastion();
                Debug.Log("BASTION_CARD_FIDELITY_NATIVE_AB_COMPLETE_REVIEW_REQUIRED");
                Complete(true);
            }
            catch (Exception ex)
            {
                busy = true;
                Complete(false, ex);
            }
        }

        static string Sha256(string filename)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(filename))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", "").ToLowerInvariant();
        }

        static Shot Capture(BreachWatchHud ui, string variant, string directory, int w, int h)
        {
            var camera = host.PlayerCamera;
            Need(camera, "Actual Valoria player camera missing");
            var oldTarget = camera.targetTexture;
            var oldAspect = camera.aspect;
            var oldMask = camera.cullingMask;
            var oldActive = RenderTexture.active;
            var surface = ui.BuildingPanel.GetComponent<Image>();
            var action = ui.GetComponentsInChildren<BreachWatchPlate>(true)
                .FirstOrDefault(p => p && p.name ==
                    "One owner-approved real hammered burgundy image");
            Need(surface && surface.sprite && action && action.sprite,
                "The genuine Bastion card surface or action is missing");
            var tex = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(w, h, TextureFormat.RGB24, false);
            string filename = variant + "-real-unity-bastion-" + w + "x" + h + ".png";
            try
            {
                camera.targetTexture = tex;
                camera.aspect = w / (float)h;
                camera.cullingMask |= 1 << 5;
                ui.NativeCanvas.worldCamera = camera;
                ui.Reflow(w, h);
                ui.RenderNow();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = tex;
                pixels.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                pixels.Apply(false);
                string path = Path.Combine(directory, filename);
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                var labels = ui.GetComponentsInChildren<Text>(true);
                var heading = labels.First(t => t && t.name == "Real building name");
                var caption = labels.First(t => t && t.name == "Real action title");
                Need(ui.SelectedBuildingId == "bastion" &&
                     ui.CurrentSnapshot != null &&
                     ui.CurrentSnapshot.Building != null &&
                     ui.CurrentSnapshot.Building.Id == "bastion",
                     "Only live Bastion data may appear in this proof");
                Need(ui.HasLiveBuildingPortrait &&
                     ui.BuildingAction && ui.BuildingAction.targetGraphic,
                     "The real 3D portrait or actual gameplay button was lost");
                Need(ui.LegacyPresentationHidden && ui.ProductionPresentation,
                     "The retired interface appeared or Vigilia is not installed");
                Need(new FileInfo(path).Length > 16000,
                     "An incomplete Unity capture cannot be evidence");
                return new Shot {
                    variant = variant,
                    file = filename,
                    sha256 = Sha256(path),
                    width = w,
                    height = h,
                    buildingName = heading.text,
                    actionCaption = caption.text,
                    surface = AssetDatabase.GetAssetPath(surface.sprite),
                    action = AssetDatabase.GetAssetPath(action.sprite),
                    realPortrait = ui.HasLiveBuildingPortrait,
                    liveState = ui.HasRealSession,
                    productionVigilia = ui.ProductionPresentation,
                    oldCanvasHidden = ui.LegacyPresentationHidden
                };
            }
            finally
            {
                camera.targetTexture = oldTarget;
                camera.aspect = oldAspect;
                camera.cullingMask = oldMask;
                RenderTexture.active = oldActive;
                tex.Release();
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(pixels);
            }
        }

        static void CaptureOneRealBastion()
        {
            string directory = Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath, "../" + Output));
            Directory.CreateDirectory(directory);
            var ui = BreachWatchHud.Install(host);
            Need(ui && ui.IndependentCanvas && ui.ProductionPresentation,
                 "Expected the single real Vigilia presentation");
            ui.SelectBuilding("bastion");
            ui.RenderNow();
            Need(ui.CurrentSnapshot?.Building?.Id == "bastion",
                 "The real Bastion is not selected");
            var initialSnapshot = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            var realPanel = ui.BuildingPanel.GetComponent<Image>();
            var realAction = ui.GetComponentsInChildren<BreachWatchPlate>(true)
                .First(p => p && p.name == "One owner-approved real hammered burgundy image");
            var originalPanel = realPanel.sprite;
            var originalType = realPanel.type;
            var originalAction = realAction.sprite;
            var ownerCard = Resources.Load<Sprite>("VigiliaArt/OwnerReference/carved_context");
            var ownerAction = Resources.Load<Sprite>("VigiliaArt/OwnerControls/upgrade_button");
            Need(ownerCard && ownerAction, "The original user-approved components are absent");
            Need(ownerCard.border.x > 0 && ownerCard.border.y > 0 &&
                 ownerCard.border.z > 0 && ownerCard.border.w > 0,
                 "Owner card needs real original 9-slice import borders");
            var sizes = new [] { (900, 1600), (390, 844), (1600, 900), (844, 390) };
            var shots = new System.Collections.Generic.List<Shot>();
            try
            {
                foreach (var v in sizes)
                    shots.Add(Capture(ui, "A-current", directory, v.Item1, v.Item2));

                // This is the only experiment. No duplicate Canvas, button, portrait
                // or static/fake gameplay data; swap source art IN PLACE.
                realPanel.sprite = ownerCard;
                realPanel.type = Image.Type.Sliced;
                realPanel.preserveAspect = false;
                realPanel.color = Color.white;
                realAction.ApplyIndependentArt(ownerAction);
                realAction.type = Image.Type.Simple;
                realAction.preserveAspect = true;
                realAction.raycastTarget = false;
                foreach (var v in sizes)
                    shots.Add(Capture(ui, "B-owner-original", directory, v.Item1, v.Item2));
            }
            finally
            {
                // Leave the disposable Play session exactly as it was before
                // this isolated visual test. Nothing is written to production.
                realPanel.sprite = originalPanel;
                realPanel.type = originalType;
                realAction.ApplyIndependentArt(originalAction);
                ui.RenderNow();
            }
            bool unchanged = initialSnapshot ==
                JsonUtility.ToJson(host.City.Gateway.Snapshot());
            Need(unchanged, "Gameplay state changed during the visual-only A/B");
            var report = new Evidence {
                status = "UNITY_CAPTURED_REVIEW_REQUIRED",
                project = "Unity 6000.3.23f1 / URP, editor-only disposable probe",
                scene = Scene,
                baseline = AssetDatabase.GetAssetPath(originalPanel) + " / " +
                           AssetDatabase.GetAssetPath(originalAction),
                candidate = AssetDatabase.GetAssetPath(ownerCard) + " / " +
                            AssetDatabase.GetAssetPath(ownerAction),
                modelSource = "Live game Bastion RenderTexture, not a pasted reference photograph",
                warning = "Pixel differences and a green build are not art approval. " +
                          "Compare complete genuine A/B, then validate physical iPhone separately.",
                artApproved = false,
                realUnityRender = true,
                iPhoneTested = false,
                saveUnchanged = unchanged,
                shots = shots.ToArray()
            };
            File.WriteAllText(Path.Combine(directory, "evidence.json"),
                JsonUtility.ToJson(report, true));
            Need(shots.Count == 8, "Exactly eight real A/B captures required");
        }
    }
}
#endif
