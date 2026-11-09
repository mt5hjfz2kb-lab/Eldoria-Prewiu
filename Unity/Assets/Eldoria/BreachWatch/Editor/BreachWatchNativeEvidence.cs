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
            public float portraitPixels,uiFootprintPercent;
            public float cityCameraOrtho,castleViewportWidth,castleViewportX,castleViewportY;
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
            Need(ui.HasNativePremiumTypography,
                "Vigilia must use its own legally bundled Garamond text on all devices");
            var expectedDestinations=new []{"REINO","RECURSOS","EJÉRCITO","EXPLORAR","HÉROES"};
            Need(ui.NavButtons.Count==5 &&
                 ui.NavButtons.Select(x=>x.name).SequenceEqual(expectedDestinations),
                 "The five sculpted medallions have misleading destination labels");
            Need(!ui.NavButtons[2].interactable && !ui.NavButtons[4].interactable,
                 "Army and Heroes must remain visibly locked until native screens exist");
            Need(!ui.transform.IsChildOf(host.Hud.transform), "New Canvas must not be attached to legacy HUD");
            Need(ui.UsesOriginalPaintedMedallions && ui.UsesApprovedReferenceMedallions &&
                 ui.UsesPremiumSculptedMedallions && ui.UsesPremiumForgedPortraitFrame &&
                 ui.UsesCompactRealmCrest && ui.UsesSinglePremiumContext &&
                 ui.UsesExactOwnerApprovedGoldBorder && ui.UsesPremiumBurgundyAction,
                "Vigilia needs original 5 sculpted medallions, true 9-slice basalt, deep burgundy gameplay button and premium brass 3D portrait frame");


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
            // Second real destination uses the provisions medallion. It must
            // select a resource building, not the barracks or an old HUD panel.
            ui.NavButtons[1].onClick.Invoke();
            Need(ui.SelectedBuildingId=="sawmill" &&
                 ui.CurrentSnapshot?.Building?.Id=="sawmill",
                 "Provisions/RECURSOS did not select the live sawmill");
            // Exercise the REAL uGUI GraphicRaycaster and pointer sequence.
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
                // Test actual Unity raycast, pointer down/up/click and world
                // gesture interception for each supported gameplay destination.
                // Button.onClick.Invoke alone cannot prove touch functionality.
                void ClickAsPlayer(Button button,string label,bool allowDisabled=false)
                {
                    Need(button && button.gameObject.activeInHierarchy &&
                         (allowDisabled||button.interactable),
                         "Missing real native destination "+label);
                    Canvas.ForceUpdateCanvases();
                    var corners=new Vector3[4];
                    ((RectTransform)button.transform).GetWorldCorners(corners);
                    var pos=RectTransformUtility.WorldToScreenPoint(camera,
                        (corners[0]+corners[2])*.5f);
                    var e=new PointerEventData(eventSystem) {
                        position=pos,pointerId=-1,
                        button=PointerEventData.InputButton.Left
                    };
                    var hits=new List<RaycastResult>();
                    ui.NativeCanvas.GetComponent<GraphicRaycaster>().Raycast(e,hits);
                    Need(hits.Count>0 &&
                         hits[0].gameObject.transform.IsChildOf(button.transform),
                         "Player cannot touch native "+label+" at its visible center");
                    Need(host.City.Presenter.ExternalPointerBlocked!=null &&
                         host.City.Presenter.ExternalPointerBlocked(pos),
                         "Native "+label+" taps penetrate the city behind the UI");
                    ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerClickHandler);
                    pointerClicks++;
                }

                ClickAsPlayer(target,"REINO");
                Need(ui.SelectedBuildingId=="bastion",
                    "REAL pointer to Reino could not open Bastion");
                // A visually disabled B1 upgrade still explains the real
                // canonical reason on touch; it must NEVER consume resources.
                if(!ui.BuildingAction.interactable)
                {
                    string canonical=ui.CurrentSnapshot?.Building?.Action?.Reason??"";
                    ClickAsPlayer(ui.BuildingAction,"MEJORA BLOQUEADA",true);
                    string feedback=ui.LastFeedback??"";
                    Need(!string.IsNullOrWhiteSpace(feedback) &&
                         (canonical.Length==0 ||
                          feedback==canonical.Replace("\r"," ").Replace("\n"," ")),
                         "Tapping a blocked action did not explain the real game requirement");
                    Need(ui.HasVisibleFeedbackPlate,
                         "Blocked upgrade reason is not protected by a readable compact slate toast");
                    Need(ui.SelectedBuildingId=="bastion",
                         "A disabled upgrade tap accidentally changed building selection");
                }
                ClickAsPlayer(ui.BuildingClose,"CERRAR BASTIÓN");
                Need(ui.SelectedBuildingId=="" &&
                     !ui.BuildingPanel.gameObject.activeInHierarchy,
                     "Real native close touch did not dismiss the live Bastion card");
                ClickAsPlayer(ui.NavButtons[1],"RECURSOS");
                Need(ui.SelectedBuildingId=="sawmill" &&
                     ui.CurrentSnapshot?.Building?.Id=="sawmill",
                     "RECURSOS medallion failed to select the live sawmill");
                ui.SelectBuilding("");
                var objectiveButton=ui.GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(q=>q && q.name=="Chapter side tablet");
                Need(objectiveButton,"Native objective tablet is not a real Button");
                var objectiveRect=(RectTransform)objectiveButton.transform;
                float collapsedHeight=objectiveRect.rect.height;
                ClickAsPlayer(objectiveButton,"OBJETIVO");
                Need(objectiveRect.rect.height>collapsedHeight+10f,
                    "Player could not expand the real chapter objective");
                ClickAsPlayer(objectiveButton,"CERRAR OBJETIVO");
                Need(objectiveRect.rect.height<collapsedHeight+2f,
                    "Expanded chapter objective did not return to compact HUD");
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
        // Conservative union of visible actual native plate bounds. This is
        // an upper estimate (rounded silhouettes have transparent corners),
        // not a fabricated per-pixel screenshot segmentation result.
        // The old art mock covered about 30%; the approved 1672x941 concept
        // occupies about 14-17% by the same hand-traced mask methodology.
        static float ConservativeUIFootprint(BreachWatchHud ui,Camera camera,int w,int h)
        {
            const int columns=240,rows=135;
            var pixels=new bool[columns*rows];
            int marked=0;
            var corners=new Vector3[4];
            foreach(var surface in ui.GetComponentsInChildren<BreachWatchPlate>(true))
            {
                if(!surface || !surface.gameObject.activeInHierarchy ||
                   surface.color.a<.08f)continue;
                surface.rectTransform.GetWorldCorners(corners);
                float minX=float.PositiveInfinity,minY=float.PositiveInfinity;
                float maxX=float.NegativeInfinity,maxY=float.NegativeInfinity;
                foreach(var corner in corners)
                {
                    var point=RectTransformUtility.WorldToScreenPoint(camera,corner);
                    minX=Mathf.Min(minX,point.x);maxX=Mathf.Max(maxX,point.x);
                    minY=Mathf.Min(minY,point.y);maxY=Mathf.Max(maxY,point.y);
                }
                int x0=Mathf.Clamp(Mathf.FloorToInt(minX/w*columns),0,columns);
                int x1=Mathf.Clamp(Mathf.CeilToInt(maxX/w*columns),0,columns);
                int y0=Mathf.Clamp(Mathf.FloorToInt(minY/h*rows),0,rows);
                int y1=Mathf.Clamp(Mathf.CeilToInt(maxY/h*rows),0,rows);
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
                {
                    int i=y*columns+x;
                    if(!pixels[i]){pixels[i]=true;marked++;}
                }
            }
            return marked*100f/pixels.Length;
        }
        // Compare two actual Unity GPU renders in the SAME native frame,
        // differing only by whether the selected Text component is enabled.
        // Old "bright pixel" tests could pass on sunlit grass or golden trim
        // even if the actual Spanish caption was invisible.
        static int RealTextContribution(Color32[] withGlyph,Color32[] withoutGlyph,
            int width,int height,Camera camera,Text label)
        {
            if(label==null || withGlyph==null || withoutGlyph==null)return 0;
            var corners=new Vector3[4];
            label.rectTransform.GetWorldCorners(corners);
            var bottom=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
            var top=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            int x0=Mathf.Clamp(Mathf.FloorToInt(bottom.x),0,width);
            int x1=Mathf.Clamp(Mathf.CeilToInt(top.x),0,width);
            int y0=Mathf.Clamp(Mathf.FloorToInt(bottom.y),0,height);
            int y1=Mathf.Clamp(Mathf.CeilToInt(top.y),0,height);
            int meaningful=0;
            for(int y=y0;y<y1;y++)
            for(int x=x0;x<x1;x++)
            {
                int i=y*width+x;
                var a=withGlyph[i];var b=withoutGlyph[i];
                int delta=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+
                    Mathf.Abs(a.b-b.b);
                if(delta>45)meaningful++;
            }
            return meaningful;
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
                var actualPixels=image.GetPixels32();
                var allText=ui.GetComponentsInChildren<Text>(true);
                string[] important=selection=="bastion"
                    ?new[]{"Real building name","Real effects",
                        "Compact engraved cost heading","Real action title",
                        "Objective progress"}
                    :new[]{"Actual chapter","Objective progress"};
                var scratch=new Texture2D(w,h,TextureFormat.RGB24,false);
                var checkedLabels=new List<Text>();
                try
                {
                    foreach(var labelName in important)
                    {
                        var label=allText.FirstOrDefault(t=>t && t.name==labelName);
                        Need(label && label.enabled && label.gameObject.activeInHierarchy,
                            "Missing real visible UI text component "+name+"/"+labelName);
                        checkedLabels.Add(label);
                        label.enabled=false;
                        Canvas.ForceUpdateCanvases();
                        camera.Render();
                        RenderTexture.active=rt;
                        scratch.ReadPixels(new Rect(0,0,w,h),0,0);
                        scratch.Apply(false);
                        var without=scratch.GetPixels32();
                        label.enabled=true;
                        Canvas.ForceUpdateCanvases();
                        int contribution=RealTextContribution(actualPixels,without,w,h,camera,label);
                        Need(contribution>=6,
                            "The authentic Spanish caption contributes no readable "+
                            "pixels to the ACTUAL Unity GPU image: "+
                            name+"/"+labelName+" changed="+contribution);
                    }
                }
                finally
                {
                    foreach(var label in checkedLabels)
                        if(label)label.enabled=true;
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    RenderTexture.active=rt;
                    Object.DestroyImmediate(scratch);
                }
                File.WriteAllBytes(file,image.EncodeToPNG());
                Need(new FileInfo(file).Length > 16000, "Unity native PNG incomplete: "+name);
                Need(ui.IndependentCanvas && ui.LegacyPresentationHidden && ui.HasRealSession,
                    "Old presentation visible or disconnected UI in "+name);
                var atmosphericVeil=ui.GetComponentInChildren<BreachWatchAtmosphere>(true);
                Need(atmosphericVeil && !atmosphericVeil.raycastTarget &&
                     atmosphericVeil.gameObject.activeInHierarchy,
                     "Independent unobtrusive atmospheric layer missing or stealing input: "+name);
                var rift=ui.GetComponentInChildren<BreachWatchRiftSeparator>(true);
                Need(rift && rift.gameObject.activeInHierarchy && !rift.raycastTarget &&
                     !rift.GetComponent<BreachWatchPlate>(),
                     "Floating Breach emblem must be transparent open-air artwork, not an opaque gameplay button: "+name);
                Need(ui.HasSinglePremiumIdentity,
                    "Superseded player plate or second bevel stacked over the one definitive premium profile");
                Need(!ui.GetComponentsInChildren<Transform>(true).Any(t=>
                         t && t.name=="Independent forged copper surface trim"),
                    "New premium Vigilia must DELETE obsolete stacked bevel surfaces, not overlay them");
                Need(ui.HasPremiumCarvedQuest,
                    "Chapter objective must be the new angled carved nine-slice tablet, not an old rectangular overlay");
                if(selection=="bastion" && (w>h || h>=w))
                {
                    var section=ui.GetComponentInChildren<BreachWatchSectionDivider>(true);
                    Need(section && !section.raycastTarget &&
                         (w<h || section.gameObject.activeInHierarchy || h<500),
                         "Selected Bastion narrative needs the original non-blocking metallic divider");
                }
                var depthLayers=ui.GetComponentsInChildren<Shadow>(true)
                    .Where(q=>q && (q.name=="Identity / basalt crest" ||
                        q.name=="Chapter side tablet" ||
                        q.name=="Independent selected-building cartouche"))
                    .ToArray();
                Need(depthLayers.Length==3 &&
                     depthLayers.All(q=>q.effectColor.a>.33f),
                     "Owner-approved premium metalwork must retain real contact depth "+
                     "without a large flat HUD footer: "+name);
                Need(ui.HasPremiumResourceSurfaces,
                    "All three live resources must use real independent clean slate nine-slice capsules, not overlapping procedural frames");
                Need(ui.HasOwnerApprovedUtilityArtwork,
                    "The three compact utility seals must match the separately cropped "+
                    "gold medallions in the owner's approved UI, not generic hexagons");
                Need(ui.BuildingPanel.GetComponentsInChildren<BreachWatchPanelInk>(true).Length==1 &&
                     !ui.BuildingPanel.GetComponent<BreachWatchPlate>() &&
                     ui.BuildingPanel.GetComponent<BreachWatchPanelInk>().raycastTarget,
                     "Superseded stacked card or pass-through background remains beneath the final gold frame: "+name);
                Need(ui.UsesCompactRealmCrest,
                    "Compact circular Valoria crest missing, or discarded red header returned: "+name);
                Need(!ui.GetComponentsInChildren<Text>(true).Any(t=>
                    t && t.gameObject.activeInHierarchy && t.name=="Vigilia de la Brecha"),
                    "Large redundant central title must not cover the real city: "+name);
                var nav = ui.NavButtons;
                Need(nav.Count==5 && nav.All(x=>x), "Exactly 5 original independent navigation buttons required");
                // An approved medallion image is now ONE authority:
                // no secondary button background, glyph or extra ring.
                // A transparent hit-only Image on the Button is permitted,
                // because it does not paint a duplicate frame or symbol.
                Need(!ui.GetComponentsInChildren<Graphic>(true).Any(g=>g &&
                         g.name=="Selected medallion / warm engraved relief"),
                     "Old duplicated selection halo remains in native scene: "+name);
                var expectedMedallions=new[]{"castle","provisions","swords","raven","helm"};
                for(int index=0;index<nav.Count;index++)
                {
                    var button=nav[index];
                    var rootImage=button.GetComponent<Image>();
                    Need(rootImage && rootImage.raycastTarget &&
                         rootImage.color.a<=.001f,
                         "Native navigation must use an invisible, touch-only hit image: "+name);
                    var painted=button.GetComponentsInChildren<BreachWatchPlate>(true);
                    Need(painted.Length==1 && painted[0] &&
                         painted[0].sprite &&
                         painted[0].sprite.name==expectedMedallions[index] &&
                         !painted[0].raycastTarget &&
                         painted[0].type==Image.Type.Simple,
                         "Incorrect or duplicated approved icon art in "+
                         name+"/"+button.name);
                    Need(button.GetComponentsInChildren<BreachWatchGlyph>(true).Length==0,
                         "Old vector icons still layered under a final medallion in "+
                         name+"/"+button.name);
                    var visiblePaint=button.GetComponentsInChildren<Graphic>(true)
                        .Count(g=>g && g.enabled &&
                            g.gameObject.activeInHierarchy && g.color.a>.01f);
                    Need(visiblePaint==1,
                         "More than one painted icon or backing button is visible in "+
                         name+"/"+button.name);
                }
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
                float castleViewportWidth=0f,castleViewportX=0f,castleViewportY=0f;
                if(w<h)
                {
                    Need(ui.PortraitCityFramed,
                         "Approved panoramic portrait city view not applied: "+name);
                    var cityBuilding=host.City.BastionOne;
                    Need(cityBuilding && cityBuilding.activeInHierarchy,
                         "Real Bastion I model missing from playable city");
                    bool found=false;
                    Bounds bounds=default;
                    foreach(var renderer in cityBuilding.GetComponentsInChildren<Renderer>(true))
                    {
                        if(!renderer || !renderer.enabled ||
                           !renderer.gameObject.activeInHierarchy ||
                           renderer.bounds.size.sqrMagnitude<.001f)continue;
                        if(!found){bounds=renderer.bounds;found=true;}
                        else bounds.Encapsulate(renderer.bounds);
                    }
                    Need(found,"No actual first Bastion mesh to measure camera coverage");
                    float minX=float.PositiveInfinity,maxX=float.NegativeInfinity;
                    var a=bounds.min;var z=bounds.max;
                    for(int corner=0;corner<8;corner++)
                    {
                        var world=new Vector3((corner&1)!=0?z.x:a.x,
                            (corner&2)!=0?z.y:a.y,(corner&4)!=0?z.z:a.z);
                        var viewport=camera.WorldToViewportPoint(world);
                        minX=Mathf.Min(minX,viewport.x);
                        maxX=Mathf.Max(maxX,viewport.x);
                    }
                    castleViewportWidth=maxX-minX;
                    var center=camera.WorldToViewportPoint(bounds.center);
                    castleViewportX=center.x;castleViewportY=center.y;
                    // The approved 864x1536 portrait places the genuine
                    // Bastion at approx one third of physical phone width
                    // with the gate/bridge/river visible below. The old
                    // real 390x844 screen was a giant 70% castle close-up.
                    // The canonical city terrain bounds limit maximum
                    // orthographic zoom-out. A 390x844 phone is ~18% narrower
                    // than the approved 864x1536 reference: the same honest
                    // world naturally uses more horizontal screen width.
                    // 900x1600 keeps a strict reference-like .49 maximum;
                    // narrow phones may reach .56 but must still preserve
                    // the complete Bastion, bridge and river in the screenshot.
                    float maxCastleWidth=w/(float)h<.50f?.56f:.49f;
                    Debug.Log("BREACHWATCH_REAL_CASTLE_RATIO "+name+
                        " width="+castleViewportWidth.ToString("F3")+
                        " maximum="+maxCastleWidth.ToString("F3")+
                        " ortho="+camera.orthographicSize.ToString("F2"));
                    Need(castleViewportWidth>.22f &&
                         castleViewportWidth<maxCastleWidth,
                         "Owner reference lost: Bastion too large or invisible "+
                         name+" castle width="+castleViewportWidth.ToString("F3")+
                         " maximum="+maxCastleWidth.ToString("F3"));
                    Need(castleViewportX>.47f && castleViewportX<.69f &&
                         castleViewportY>.60f && castleViewportY<.84f,
                         "Real city portrait camera no longer frames the castle "+
                         name+" center="+center);
                    Debug.Log("BREACHWATCH_APPROVED_CITY_COMPOSITION "+name+
                        " zoom="+camera.orthographicSize.ToString("F2")+
                        " width="+castleViewportWidth.ToString("F3")+
                        " center="+center.x.ToString("F3")+","+
                        center.y.ToString("F3"));
                }
                float footprint=ConservativeUIFootprint(ui,camera,w,h);
                float maxFootprint=w<h?34f:h<500?32f:22f;
                Need(footprint<=maxFootprint,
                     "Owner-approved compact UI footprint exceeded in "+name+
                     ": "+footprint.ToString("F2")+"% / maximum "+
                     maxFootprint.ToString("F1")+"%");
                Debug.Log("BREACHWATCH_COMPACT_UI_FOOTPRINT "+
                    name+" "+footprint.ToString("F2")+"%");
                foreach(var label in ui.GetComponentsInChildren<Text>(true))
                {
                    if(!label || !label.gameObject.activeInHierarchy ||
                       string.IsNullOrWhiteSpace(label.text))continue;
                    float actual=label.fontSize*ui.NativeCanvas.scaleFactor;
                    Need(actual>=11.75f,
                        "Text too small in physical capture "+name+"/"+label.name+
                        ": "+actual.ToString("F2")+"px");
                    // Unity's Text.preferredHeight/preferredWidth describe
                    // the NOMINAL font size. With best-fit enabled they can
                    // report two lines even though the GPU actually draws a
                    // perfectly readable single line (as observed at 900x1600).
                    // Use the rendered TextGenerator plus on/off GPU pixel
                    // contribution for those captions instead of a false fail.
                    if(label.resizeTextForBestFit)
                    {
                        Need(label.cachedTextGenerator.lineCount==1 &&
                             label.cachedTextGenerator.vertexCount>0,
                             "Premium best-fit caption actually wraps or has no glyphs: "+
                             name+"/"+label.name);
                    }
                    else
                    {
                        Need(label.preferredHeight<=label.rectTransform.rect.height+4.5f,
                             "Live text genuinely clips vertically: "+name+"/"+label.name+
                             " wanted="+label.preferredHeight.ToString("F1")+
                             " available="+label.rectTransform.rect.height.ToString("F1"));
                        if(label.name=="Real building name")
                            Need(label.preferredWidth<=label.rectTransform.rect.width+5f,
                                 "Real Bastion title exceeds its premium heading: "+name);
                    }
                }
                // Only Chapter I gets the owner's Breach subtitle; the
                // gameplay chapter number and objective stay canonical.
                var chapterLabel=ui.GetComponentsInChildren<Text>(true)
                    .FirstOrDefault(q=>q && q.name=="Actual chapter");
                if(host.City.Gateway.Snapshot().BastionLevel==1)
                    Need(chapterLabel && chapterLabel.gameObject.activeInHierarchy &&
                         (host.English
                            ?chapterLabel.text.Contains("THE BREACH")
                            :chapterLabel.text.Contains("LA BRECHA")),
                         "Owner-approved Chapter I / La Brecha identity not readable: "+name);
                // Verify that the authentic chapter progress is visible
                // inside its carved tablet, not hidden behind the lower edge.
                var realProgress=ui.GetComponentsInChildren<Text>(true)
                    .FirstOrDefault(x=>x && x.name=="Objective progress");
                Need(realProgress && realProgress.gameObject.activeInHierarchy &&
                     realProgress.text==ui.CurrentSnapshot?.ObjectiveProgress &&
                     !string.IsNullOrWhiteSpace(realProgress.text),
                     "Real chapter completion fraction hidden or not bound: "+name);
                var tablet=(RectTransform)realProgress.transform.parent;
                var tabletCorners=new Vector3[4];
                var progressCorners=new Vector3[4];
                tablet.GetWorldCorners(tabletCorners);
                realProgress.rectTransform.GetWorldCorners(progressCorners);
                var tabletBottom=RectTransformUtility.WorldToScreenPoint(camera,tabletCorners[0]);
                var progressBottom=RectTransformUtility.WorldToScreenPoint(camera,progressCorners[0]);
                Need(progressBottom.y>=tabletBottom.y+3,
                     "Real objective fraction is hidden by carved quest lower bevel: "+name);
                if (selection=="bastion")
                {
                    Need(ui.BuildingPanel.gameObject.activeInHierarchy &&
                         ui.CurrentSnapshot?.Building != null &&
                         ui.CurrentSnapshot.Building.Id=="bastion",
                         "Real Bastion selection is missing contextual native card in "+name);
                    // Real inactive areas of this carved card must not
                    // accidentally select a building behind the menu.
                    var panelCorners=new Vector3[4];
                    ui.BuildingPanel.GetWorldCorners(panelCorners);
                    var panelPoint=RectTransformUtility.WorldToScreenPoint(camera,
                        (panelCorners[0]+panelCorners[2])*.5f);
                    Need(host.City.Presenter.ExternalPointerBlocked!=null &&
                         host.City.Presenter.ExternalPointerBlocked(panelPoint),
                         "Premium Bastion panel allows city clicks through a blank slate area: "+name);
                    Need(ui.HasLiveBuildingPortrait && ui.BuildingPortraitExclusive,
                         "Real isolated Bastion mesh portrait did not render in "+name);
                    Need(!ui.BuildingPortraitEdgeClipped,
                         "Real Bastion geometry is cropped at the miniature frame boundary: "+name);
                    // A 3D mesh can be inside the render texture yet cropped
                    // by the responsive RawImage's display UVs. Check BOTH,
                    // using silhouettes measured from actual GPU pixels.
                    var castleUv=ui.BuildingPortraitMeshUvBounds;
                    var photoUv=ui.BuildingPortraitUvRect;
                    Need(castleUv.width>.20f && castleUv.height>.35f,
                         "Real Bastion miniature has implausible GPU silhouette: "+name);
                    const float uvTolerance=.007f;
                    Need(photoUv.xMin<=castleUv.xMin+uvTolerance &&
                         photoUv.xMax>=castleUv.xMax-uvTolerance &&
                         photoUv.yMin<=castleUv.yMin+uvTolerance &&
                         photoUv.yMax>=castleUv.yMax-uvTolerance,
                         "Gold picture frame CROPS real turrets, flag or staircase: "+
                         name+" imageUV="+photoUv+" modelUV="+castleUv);
                    Need(ui.BuildingPortraitDrawnFraction>.23f,
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
                    var description=Part("Real effects");
                    var descText=description.GetComponent<Text>();
                    Need(descText && descText.gameObject.activeInHierarchy &&
                         descText.preferredHeight<=description.rect.height+1.0f,
                         "The actual two-line Bastion description is truncated: "+name+
                         " preferred="+(descText?descText.preferredHeight.ToString("F2"):"null")+
                         " allotted="+description.rect.height.ToString("F2"));
                    var header=Part("Compact engraved cost heading");
                    var headingLabel=header.GetComponent<Text>();
                    var stateLabel=state.GetComponent<Text>();
                    Need(headingLabel && header.gameObject.activeInHierarchy &&
                         header.rect.height>=headingLabel.fontSize*1.15f,
                         "Actual premium cost heading is invisible or vertically cropped in "+name);
                    Need(stateLabel && state.gameObject.activeInHierarchy &&
                         state.rect.height>=stateLabel.fontSize*1.15f,
                         "Actual operation status must render in "+name);
                    var action=(RectTransform)ui.BuildingAction.transform;
                    float Bottom(RectTransform r) => -r.anchoredPosition.y+r.rect.height;
                    float Top(RectTransform r) => -r.anchoredPosition.y;
                    Need(Bottom(costs)<=Top(state)+.5f,
                        "Building costs overlap live state: "+name);
                    Need(Bottom(state)<=Top(action)+.5f,
                        "Live status overlaps main action: "+name);
                    Need(Bottom(action)<=ui.BuildingPanel.rect.height+.5f,
                        "Main building action clipped by native card: "+name);
                    var hammers=ui.BuildingAction.GetComponentInChildren<BreachWatchCrossedHammers>(true);
                    Need(hammers && hammers.gameObject.activeInHierarchy &&
                         !hammers.raycastTarget &&
                         hammers.transform.IsChildOf(ui.BuildingAction.transform),
                         "Original forged upgrade hammers must remain separate undistorted "+
                         "geometry, not nine-sliced bars or a tappable overlay: "+name);
                    var actionText=ui.BuildingAction.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(q=>q && q.name=="Real action title");
                    Need(actionText && actionText.gameObject.activeInHierarchy &&
                         actionText.rectTransform.anchoredPosition.x>=
                            hammers.rectTransform.anchoredPosition.x+
                            hammers.rectTransform.rect.width+1f,
                         "Real premium upgrade text overlaps engraved crossed hammers: "+name);
                    Need(actionText.rectTransform.anchoredPosition.x+
                         actionText.rectTransform.rect.width<=action.rect.width-2f,
                         "Real upgrade caption escapes the right copper frame: "+name);
                    foreach(var control in new []{ui.BuildingAction,ui.BuildingClose})
                    {
                        Need(control && control.gameObject.activeInHierarchy,
                            "Native building control unavailable: "+name);
                        Vector3[] touchCorners = new Vector3[4];
                        ((RectTransform)control.transform).GetWorldCorners(touchCorners);
                        var lower=RectTransformUtility.WorldToScreenPoint(camera,touchCorners[0]);
                        var upper=RectTransformUtility.WorldToScreenPoint(camera,touchCorners[2]);
                        Need(upper.x-lower.x>=43.5f && upper.y-lower.y>=43.5f,
                            "Owner-approved phone action must have >=44px physical touch target: "+
                            name+"/"+control.name);
                    }
                }
                return new Shot {
                    file=name,sha256=HexHash(file),width=w,height=h,
                    selected=selection,realBuilding=ui.CurrentSnapshot?.Building?.Name ?? "",
                    actualObjective=ui.CurrentSnapshot?.Objective ?? "",buttons=nav.Count,
                    oldCanvasOff=ui.LegacyPresentationHidden,
                    newCanvasIndependent=ui.IndependentCanvas,
                    liveSession=ui.HasRealSession,fiveInOneRow=oneRow,
                    liveBuildingPortrait=ui.HasLiveBuildingPortrait,
                    portraitPixels=ui.BuildingPortraitDrawnFraction,
                    uiFootprintPercent=footprint,
                    cityCameraOrtho=camera.orthographicSize,
                    castleViewportWidth=castleViewportWidth,
                    castleViewportX=castleViewportX,
                    castleViewportY=castleViewportY
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
