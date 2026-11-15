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
            public string variant, file, sha256, buildingName, actionCaption, surface, action, border;
            public int width, height;
            public bool realPortrait, liveState, productionVigilia, oldCanvasHidden, exactApprovedBorder;
            public float shaderErrorMagentaFraction;
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
            var originalBorder = Resources.Load<Sprite>("VigiliaArt/ApprovedContour/owner_gold_border");
            var borderImage = ui.BuildingPanel.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(i => i && i.name == "Pilot / single genuine owner gold contour");
            if (variant.StartsWith("C-"))
                Need(originalBorder && borderImage && borderImage.sprite == originalBorder &&
                    borderImage.type == Image.Type.Sliced && !borderImage.raycastTarget &&
                    ui.BuildingPanel.GetComponentsInChildren<Image>(true)
                      .Count(i => i && i.name == "Pilot / single genuine owner gold contour") == 1,
                    "Exact owner border is missing, duplicated or cannot stretch properly");
            else
                Need(borderImage == null,
                    "The baseline must not contain the experiment's gold border");
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
                // A green Editor or PlayMode test is NOT an art pass if
                // the real model is rendered with Unity's magenta error shader.
                // Compare actual framebuffer pixels (not Shader.isSupported):
                // archived genuine native 43623869301 had 3.79%-5.82% magenta
                // even though material shaderSupported=true appeared in inventory.
                var actualPixels = pixels.GetPixels32();
                int errorMagentaPixels = 0;
                foreach (var pixel in actualPixels)
                    if(pixel.r >= 230 && pixel.b >= 230 && pixel.g <= 50)
                        errorMagentaPixels++;
                float magentaFraction = errorMagentaPixels/(float)actualPixels.Length;
                string path = Path.Combine(directory, filename);
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                Need(magentaFraction < .006f,
                    "Genuine Unity scene contains " +
                    (magentaFraction*100f).ToString("F2") +
                    "% suspected error-shader magenta; UI quality comparison INVALID: " +
                    filename);
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
                    border = borderImage ? AssetDatabase.GetAssetPath(borderImage.sprite) : "",
                    exactApprovedBorder = borderImage && borderImage.sprite == originalBorder,
                    shaderErrorMagentaFraction = magentaFraction,
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
            var ownerFill = Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            var ownerBorder = Resources.Load<Sprite>("VigiliaArt/ApprovedContour/owner_gold_border");
            Need(ownerCard && ownerAction && ownerFill && ownerBorder,
                 "The preserved owner-reference artwork for the three variants is absent");
            Need(ownerCard.border.x > 0 && ownerCard.border.y > 0 &&
                 ownerCard.border.z > 0 && ownerCard.border.w > 0,
                 "Owner card needs real original 9-slice import borders");
            var sizes = new [] { (900, 1600), (390, 844), (1600, 900), (844, 390) };
            Need(ownerBorder.border.x > 0 && ownerBorder.border.y > 0 &&
                 ownerBorder.border.z > 0 && ownerBorder.border.w > 0,
                 "The exact owner gold border is missing its four-sided 9-slice");
            var shots = new System.Collections.Generic.List<Shot>();
            GameObject ownerGoldObject = null;
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
                    shots.Add(Capture(ui, "B-earlier-context", directory, v.Item1, v.Item2));

                // Third approach: the exact visual-language contour cropped
                // from the approved 864x1536 reference, over its own blank
                // stone fill. One frame, no second background frame and no
                // screenshot/static gameplay data.
                realPanel.sprite = ownerFill;
                realPanel.type = Image.Type.Simple;
                realPanel.preserveAspect = false;
                ownerGoldObject = new GameObject("Pilot / single genuine owner gold contour",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                ownerGoldObject.layer = ui.BuildingPanel.gameObject.layer;
                var borderRect = ownerGoldObject.GetComponent<RectTransform>();
                borderRect.SetParent(ui.BuildingPanel, false);
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = borderRect.offsetMax = Vector2.zero;
                borderRect.SetAsLastSibling();
                var border = ownerGoldObject.GetComponent<Image>();
                border.sprite = ownerBorder;
                border.type = Image.Type.Sliced;
                border.fillCenter = true; // the source interior itself is transparent
                border.color = Color.white;
                border.raycastTarget = false;
                foreach (var v in sizes)
                    shots.Add(Capture(ui, "C-exact-approved-contour", directory, v.Item1, v.Item2));
            }
            finally
            {
                // Leave the disposable Play session exactly as it was before
                // this isolated visual test. Nothing is written to production.
                if (ownerGoldObject) Object.DestroyImmediate(ownerGoldObject);
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
                candidate = "B: " + AssetDatabase.GetAssetPath(ownerCard) + " / " +
                            AssetDatabase.GetAssetPath(ownerAction) +
                            "; C: " + AssetDatabase.GetAssetPath(ownerFill) + " + " +
                            AssetDatabase.GetAssetPath(ownerBorder) + " / " +
                            AssetDatabase.GetAssetPath(ownerAction),
                modelSource = "Live game Bastion RenderTexture, not a pasted reference photograph",
                warning = "B context comes from a DIFFERENT earlier art source; C border is from approved 864x1536 reference. Pixel differences and a green build are not art approval. " +
                          "Compare complete genuine A/B, then validate physical iPhone separately.",
                artApproved = false,
                realUnityRender = true,
                iPhoneTested = false,
                saveUnchanged = unchanged,
                shots = shots.ToArray()
            };
            File.WriteAllText(Path.Combine(directory, "evidence.json"),
                JsonUtility.ToJson(report, true));
            Need(shots.Count == 12, "Exactly twelve real three-way captures required");
        }
    }
}
#endif
