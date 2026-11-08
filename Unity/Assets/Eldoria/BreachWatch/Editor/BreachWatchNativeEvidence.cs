#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.BreachWatch;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.BreachWatch.Editor
{
    // Genuine disposable-scene capture. Installs a new Canvas, not a skin.
    // No scene, production prefab, gameplay, clock or player save is modified.
    [InitializeOnLoad]
    public static class BreachWatchNativeEvidence
    {
        const string Flag = "BreachWatch.IndependentNative.Run";
        const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string ArtifactDir = "BreachWatchNativeEvidence";
        static double deadline, afterNaming;
        static int phase, storyPages;
        static bool busy;
        static ValoriaIntegratedWorld host;

        [Serializable] public sealed class Shot
        {
            public string file, sha256, selected, realBuilding, actualObjective;
            public int width, height, buttons;
            public bool oldCanvasOff, newCanvasIndependent, liveSession, fiveInOneRow, liveBuildingPortrait;
            public float portraitPixels;
        }
        [Serializable] public sealed class Report
        {
            public string status, source, scene, note;
            public int screenshotCount, functioningNewNavigationClicks, oldCanvasVisible;
            public bool saveUnchanged, oldHudGraphicsReused, artworkApproved, androidTested;
            public Shot[] shots;
        }

        static BreachWatchNativeEvidence()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }
        static void Need(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException(message);
        }
        [MenuItem("Eldoria/Vigilia de la Brecha/Captura Unity nativa independiente")]
        public static void CaptureBatch()
        {
            Need(!EditorApplication.isPlaying, "Must enter Play from fresh Editor session");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE", "1");
            EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            SessionState.SetBool(Flag, true);
            deadline = EditorApplication.timeSinceStartup + 240;
            phase = 0; storyPages = 0; busy = false;
            EditorApplication.EnterPlaymode();
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Flag, false) || busy || !EditorApplication.isPlaying) return;
            try
            {
                if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 240;
                Need(EditorApplication.timeSinceStartup < deadline, "Valoria disposable scene initialization timeout");
                if (!host) host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if (!host || !host.Ready || !host.Hud || !host.City) return;
                Need(host.City.Probe, "Refuse to capture or change an actual player save");
                if (phase == 0)
                {
                    Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.CompleteProbePlayerName(host);
                    afterNaming = EditorApplication.timeSinceStartup;
                    phase = 1;
                    return;
                }
                if (phase == 1)
                {
                    // Honor existing narrative and advance only through its real command.
                    if (host.Hud.NarrativeOpen)
                    {
                        Need(storyPages++ < 30, "Cannot complete authentic first-entry narrative");
                        host.Hud.Modal.Extra.onClick.Invoke();
                        return;
                    }
                    if (EditorApplication.timeSinceStartup - afterNaming < .9) return;
                    phase = 2;
                }
                if (phase == 2)
                {
                    busy = true;
                    SessionState.SetBool(Flag, false);
                    CaptureRealCity();
                    Debug.Log("BREACH_WATCH_NATIVE_CAPTURE_PASS");
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception e)
            {
                SessionState.SetBool(Flag, false);
                Debug.LogException(e);
                var dir = Path.Combine(UnityEngine.Application.dataPath, "../" + ArtifactDir);
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "error.txt"), e.ToString());
                EditorApplication.Exit(1);
            }
        }
        static string HexHash(string path)
        {
            using(var sha = SHA256.Create())
            using(var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        static void CaptureRealCity()
        {
            var dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../" + ArtifactDir));
            Directory.CreateDirectory(dir);
            Need(!host.Hud.NarrativeOpen, "Existing narrative still active at capture start");
            var before = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            var ui = BreachWatchHud.Install(host);
            Need(ui != null && ui.IndependentCanvas && ui.LegacyPresentationHidden,
                "This must be a new Canvas; the entire old HUD presentation must be OFF");
            Need(ui.HasRealSession, "New Canvas cannot access existing real game session");
            Need(ui.HasTrueResourceIllustrations,
                "All three resource icons must be real, recognizable licensed independent sprites");
            Need(!ui.transform.IsChildOf(host.Hud.transform), "New Canvas must not be attached to legacy HUD");
            Need(ui.UsesOriginalPaintedMedallions && ui.UsesApprovedReferenceMedallions &&
                 ui.UsesOriginalForgedPortraitFrame && ui.UsesOriginalBurgundyStandard &&
                 ui.UsesOwnerApprovedCarvedContext,
                "Vigilia must render 5 distinct medallions extracted from OWNER'S approved concept, along with the independent forged panel and banner");


            var shots = new List<Shot>();
            foreach (var selection in new [] {"", "bastion"})
            {
                ui.SelectBuilding(selection);
                Need(ui.SelectedBuildingId == selection && host.Hud.SelectedBuildingId == selection,
                    "New selection did not reach canonical game host");
                foreach (var size in new [] {(1600,900),(900,1600),(844,390),(390,844)})
                    shots.Add(RenderNative(ui, dir, selection, size.Item1, size.Item2));
            }
            // The canonical city selection must reach the independent interface
            // through the genuine event, not only through our own navigation.
            Need(host.Select("sawmill"),"Existing city cannot select real Sawmill");
            Need(ui.SelectedBuildingId=="sawmill" &&
                 ui.CurrentSnapshot?.Building?.Id=="sawmill",
                 "Real city selection did not reach the independent Vigilia panel");
            // Exercise the REAL uGUI GraphicRaycaster and pointer sequence, not Button.onClick.Invoke.
            ui.SelectBuilding("");
            int pointerClicks = 0;
            var target = ui.NavButtons[0];
            var camera = host.PlayerCamera;
            var temp = new RenderTexture(1600,900,24);
            var prev = camera.targetTexture;
            var prevAspect = camera.aspect;
            int mask = camera.cullingMask;
            try
            {
                camera.targetTexture = temp;
                camera.aspect = 1600f/900f;
                camera.cullingMask |= 1 << 5;
                ui.Reflow(1600,900);
                Canvas.ForceUpdateCanvases();
                camera.Render(); camera.Render();
                var eventSystem = EventSystem.current;
                Need(eventSystem, "Real Unity EventSystem missing");
                Vector3[] corners = new Vector3[4];
                ((RectTransform)target.transform).GetWorldCorners(corners);
                var pos = RectTransformUtility.WorldToScreenPoint(camera,(corners[0]+corners[2])*.5f);
                var e = new PointerEventData(eventSystem) { position = pos,
                    pointerId = -1, button = PointerEventData.InputButton.Left };
                var hits = new List<RaycastResult>();
                ui.NativeCanvas.GetComponent<GraphicRaycaster>().Raycast(e,hits);
                Need(hits.Count>0 && hits[0].gameObject.transform.IsChildOf(target.transform),
                    "New native pointer hit-test failed: first raycast target is not REINO");
                Need(host.City.Presenter.ExternalPointerBlocked!=null &&
                     host.City.Presenter.ExternalPointerBlocked(pos),
                     "New HUD pointer penetrates to the city camera/world behind a real button");
                ExecuteEvents.Execute(target.gameObject,e,ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(target.gameObject,e,ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(target.gameObject,e,ExecuteEvents.pointerClickHandler);
                Need(ui.SelectedBuildingId == "bastion", "REAL UI pointer could not select Bastion");
                pointerClicks++;
            }
            finally
            {
                camera.targetTexture = prev;
                camera.aspect = prevAspect;
                camera.cullingMask = mask;
                temp.Release(); Object.DestroyImmediate(temp);
            }
            var after = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            Need(before == after, "New HUD preview modified gameplay/save data");
            Need(shots.Count == 8 && shots.All(x=>x.liveSession && x.oldCanvasOff &&
                x.newCanvasIndependent && x.fiveInOneRow && x.buttons == 5 &&
                (x.selected != "bastion" || x.liveBuildingPortrait)),
                "An orientation failed native, independent HUD or five-destination proof");
            var report = new Report {
                status = "UNITY_NATIVE_INDEPENDENT_CAPTURED_ART_REVIEW_PENDING",
                source = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local",
                scene = Scene, note = "Genuine Unity play mode, authentic disposable Valoria session; only one new Canvas; old Canvas disabled; real Bastion selection. No Android/WebGL certification.",
                screenshotCount = shots.Count, functioningNewNavigationClicks = pointerClicks,
                oldCanvasVisible = 0, saveUnchanged = before == after,
                oldHudGraphicsReused = false, artworkApproved = false, androidTested = false,
                shots = shots.ToArray()
            };
            File.WriteAllText(Path.Combine(dir,"BreachWatchNativeEvidence.json"),JsonUtility.ToJson(report,true));
        }
        static Shot RenderNative(BreachWatchHud ui,string root,string selection,int w,int h)
        {
            Camera camera = host.PlayerCamera;
            Need(camera, "Canonical Valoria camera missing");
            var lastRt = camera.targetTexture;
            var lastMask = camera.cullingMask;
            var lastAspect = camera.aspect;
            var lastActive = RenderTexture.active;
            var rt = new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var image = new Texture2D(w,h,TextureFormat.RGB24,false);
            string name = "real-unity-" + (selection.Length==0?"city":"bastion") + "-" + w + "x" + h + ".png";
            string file = Path.Combine(root,name);
            try
            {
                camera.targetTexture = rt;
                camera.aspect = w/(float)h;
                camera.cullingMask |= 1 << 5;
                ui.NativeCanvas.worldCamera = camera;
                ui.Reflow(w,h);
                ui.RenderNow();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = rt;
                image.ReadPixels(new Rect(0,0,w,h),0,0);
                image.Apply();
                File.WriteAllBytes(file,image.EncodeToPNG());
                Need(new FileInfo(file).Length > 16000, "Unity native PNG incomplete: "+name);
                Need(ui.IndependentCanvas && ui.LegacyPresentationHidden && ui.HasRealSession,
                    "Old presentation visible or disconnected UI in "+name);
                var nav = ui.NavButtons;
                Need(nav.Count==5 && nav.All(x=>x), "Exactly 5 original independent navigation buttons required");
                float previousRight = -1;
                bool oneRow = true;
                float baselineY = float.NaN;
                foreach (var button in nav)
                {
                    var rect = (RectTransform)button.transform;
                    Vector3[] corners = new Vector3[4];
                    rect.GetWorldCorners(corners);
                    var a = RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
                    var b = RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
                    Need(b.x-a.x>=43.5 && b.y-a.y>=43.5,
                        "Navigation touch-target smaller than 44px: "+name+"/"+button.name);
                    Need(a.x>=-2 && b.x<=w+2 && a.y>=-2 && b.y<=h+2,
                        "Off-screen independent nav button: "+name+"/"+button.name);
                    if (!float.IsNaN(baselineY) && Mathf.Abs(a.y-baselineY)>2) oneRow=false;
                    baselineY = a.y;
                    if (previousRight>=0 && a.x<previousRight+2) oneRow=false;
                    previousRight=b.x;
                }
                Need(oneRow,"Five independent navigation symbols overlap or stack in "+name);
                foreach(var label in ui.GetComponentsInChildren<Text>(true))
                {
                    if(!label || !label.gameObject.activeInHierarchy ||
                       string.IsNullOrWhiteSpace(label.text))continue;
                    float actual=label.fontSize*ui.NativeCanvas.scaleFactor;
                    Need(actual>=11.75f,
                        "Text too small in physical capture "+name+"/"+label.name+
                        ": "+actual.ToString("F2")+"px");
                    Need(label.preferredHeight<=label.rectTransform.rect.height+4.5f,
                        "Text is vertically clipped in "+name+"/"+label.name+
                        " wanted="+label.preferredHeight.ToString("F1")+
                        " available="+label.rectTransform.rect.height.ToString("F1"));
                }
                if (selection=="bastion")
                {
                    Need(ui.BuildingPanel.gameObject.activeInHierarchy &&
                         ui.CurrentSnapshot?.Building != null &&
                         ui.CurrentSnapshot.Building.Id=="bastion",
                         "Real Bastion selection is missing contextual native card in "+name);
                    Need(ui.HasLiveBuildingPortrait && ui.BuildingPortraitExclusive,
                         "Real isolated Bastion mesh portrait did not render in "+name);
                    Need(ui.BuildingPortraitDrawnFraction>.0125f,
                        "Genuine isolated Bastion camera produced a mostly empty image in "+name+
                        " pixels="+ui.BuildingPortraitDrawnFraction);
                    // Each panel child shares the same top-left anchored
                    // coordinate space; verify costs/status/action are ordered
                    // without occupying the same pixels on narrow phones.
                    RectTransform Part(string childName)
                    {
                        var text=ui.BuildingPanel.GetComponentsInChildren<Text>(true)
                            .FirstOrDefault(t=>t && t.name==childName);
                        Need(text!=null,"Missing native live text "+childName);
                        return text.rectTransform;
                    }
                    var costs=Part("Real costs");
                    var state=Part("Real state and timer");
                    var action=(RectTransform)ui.BuildingAction.transform;
                    float Bottom(RectTransform r) => -r.anchoredPosition.y+r.rect.height;
                    float Top(RectTransform r) => -r.anchoredPosition.y;
                    Need(Bottom(costs)<=Top(state)+.5f,
                        "Building costs overlap live state: "+name);
                    Need(Bottom(state)<=Top(action)+.5f,
                        "Live status overlaps main action: "+name);
                    Need(Bottom(action)<=ui.BuildingPanel.rect.height+.5f,
                        "Main building action clipped by native card: "+name);
                }
                return new Shot {
                    file=name,sha256=HexHash(file),width=w,height=h,
                    selected=selection,realBuilding=ui.CurrentSnapshot?.Building?.Name ?? "",
                    actualObjective=ui.CurrentSnapshot?.Objective ?? "",buttons=nav.Count,
                    oldCanvasOff=ui.LegacyPresentationHidden,
                    newCanvasIndependent=ui.IndependentCanvas,
                    liveSession=ui.HasRealSession,fiveInOneRow=oneRow,
                    liveBuildingPortrait=ui.HasLiveBuildingPortrait,
                    portraitPixels=ui.BuildingPortraitDrawnFraction
                };
            }
            finally
            {
                camera.targetTexture = lastRt;
                camera.cullingMask = lastMask;
                camera.aspect = lastAspect;
                RenderTexture.active = lastActive;
                rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(image);
            }
        }
    }
}
#endif
