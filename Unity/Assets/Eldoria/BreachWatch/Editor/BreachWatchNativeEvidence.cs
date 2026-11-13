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
            public int width, height, buttons, activeForeignScreenSpaceCanvases;
            public string[] foreignScreenSpaceCanvasPaths;
            public bool oldCanvasOff, newCanvasIndependent, liveSession, fiveInOneRow, liveBuildingPortrait;
            public float portraitPixels,uiFootprintPercent;
            public float cityCameraOrtho,castleViewportWidth,castleViewportX,castleViewportY;
        }
        [Serializable] public sealed class CanvasDiagnostic
        {
            public string name, renderMode, path;
            public int sortOrder, livePaintedGraphics;
            public bool active, legacy, independentVigilia;
        }
        [Serializable] public sealed class Report
        {
            public string status, source, scene, note;
            public CanvasDiagnostic[] canvasDiagnostics;
            public int activeForeignScreenSpaceCanvases;
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
        static CanvasDiagnostic[] AuditActualCanvases(BreachWatchHud vigilia)
        {
            // Screen-space overlays created by unrelated systems would
            // remain visible underneath an independent transparent HUD even
            // when RoyalHUD itself is disabled. Do not assume one disabled
            // legacy Canvas proves no double UI in the actual scene.
            // Diagnostic only: never disable gameplay UI or world labels.
            return Resources.FindObjectsOfTypeAll<Canvas>()
                .Where(c=>c && c.gameObject.scene.IsValid() &&
                    c.gameObject.scene.isLoaded)
                .Select(c=>new CanvasDiagnostic {
                    name=c.name,
                    path=HierarchyPath(c.transform),
                    renderMode=c.renderMode.ToString(),
                    sortOrder=c.sortingOrder,
                    active=c.isActiveAndEnabled && c.gameObject.activeInHierarchy,
                    legacy=host && host.Hud &&
                        c==host.Hud.GetComponent<Canvas>(),
                    independentVigilia=vigilia &&
                        (c==vigilia.NativeCanvas ||
                         c.transform.IsChildOf(vigilia.transform)),
                    livePaintedGraphics=c.GetComponentsInChildren<Graphic>(true)
                        .Count(g=>g && g.isActiveAndEnabled &&
                             g.gameObject.activeInHierarchy &&
                             g.color.a>.02f),
                }).OrderBy(x=>x.path).ToArray();
        }
        static string HierarchyPath(Transform node)
        {
            var segments=new List<string>();
            while(node){segments.Add(node.name);node=node.parent;}
            segments.Reverse();
            return string.Join("/",segments);
        }
        [MenuItem("Eldoria/Vigilia de la Brecha/Captura Unity nativa independiente")]
        static void AuditExclusivePresentation()
        {
            if(!BreachWatchProductionPresentation.Required)return;
            var ui=Object.FindFirstObjectByType<BreachWatchHud>();
            Need(ui && ui.ProductionPresentation,"Production new UI was not installed");
            var isolation=ui.GetComponent<BreachWatchProductionPresentation>();
            Need(isolation,"Production art isolation boundary missing");
            ui.GetComponent<BreachWatchTypography>()?.SynchronizeNow();
            isolation.Enforce();
            Need(isolation.RetiredCanvasDisabled,"Retired UI Canvas reactivated");
            string old=isolation.FirstVisibleRetiredPainting();
            Need(string.IsNullOrEmpty(old),"Legacy UI artwork still visible: "+old);
            Debug.Log("VIGILIA_EXCLUSIVE_PRESENTATION_NATIVE_PASS");
        }

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
                    if(BreachWatchProductionPresentation.Required)VerifyProductionFirstEntry();
                    else Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.CompleteProbePlayerName(host);
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
                        if(BreachWatchProductionPresentation.Required)TouchVisibleProductionNarrative();
                        else host.Hud.Modal.Extra.onClick.Invoke();
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
        // Run on ACTUAL Unity uGUI after Reflow: a source-only division by
        // sprite.pixelsPerUnit is not the physical 9-slice size. Unity's
        // Image.pixelsPerUnit includes Canvas.referencePixelsPerUnit and
        // GetAdjustedBorders compresses both sides if the center is negative.
        // Check the genuinely instantiated, visible image geometry at each
        // real landscape/portrait screenshot resolution before certification.
        static void CheckLiveSlicedGeometry(BreachWatchHud ui,string shot)
        {
            var surfaces=ui.GetComponentsInChildren<BreachWatchPlate>(true)
                .Where(x=>x && x.gameObject.activeInHierarchy && x.sprite &&
                    x.type==Image.Type.Sliced &&
                    (x.sprite.name=="profile" || x.sprite.name=="mission" ||
                     x.sprite.name=="resource")).ToArray();
            foreach(string category in new[]{"profile","mission","resource"})
                Need(surfaces.Any(x=>x.sprite.name==category),
                     "Missing active original 2K painted nine-slice "+category+" / "+shot);
            foreach(var paint in surfaces)
            {
                float ratio=paint.pixelsPerUnit;
                float reference=paint.canvas ? paint.canvas.referencePixelsPerUnit : 0f;
                Need(reference>0f && ratio>0f &&
                    Mathf.Abs(ratio-paint.sprite.pixelsPerUnit/reference)<.001f,
                    "Unexpected Unity canvas sprite PPU "+paint.name+" / "+shot);
                var border=paint.sprite.border;
                var rect=paint.rectTransform.rect;
                float freeX=rect.width-(border.x+border.z)/ratio;
                float freeY=rect.height-(border.y+border.w)/ratio;
                Need(freeX>=8f && freeY>=7f,
                    "Unity 2K nine-slice borders collapse painted center "+
                    paint.sprite.name+" / "+shot+" width="+freeX+" height="+freeY+
                    " spritePPU="+paint.sprite.pixelsPerUnit+" refPPU="+reference);
            }
        }
        // Diagnostic only: log the *actual* live terrain/shore materials from
        // the disposable city BEFORE attempting any palette change. The
        // approved image has a muted ochre/olive ground, whereas the native
        // screenshot has brighter lime-green terrain. Pixel masks alone do
        // not identify a source Renderer; tinting without this evidence risks
        // changing masonry, water or vegetation, so this probe is read-only.
        static void CaptureActualGroundMaterialInventory(string directory)
        {
            var entries=new List<string> {
                "VIGILIA_GROUND_MATERIAL_INVENTORY_ONLY: no scene or shared material changed"
            };
            foreach(var terrain in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                if(!terrain || !terrain.gameObject.scene.isLoaded)continue;
                var data=terrain.terrainData;
                entries.Add("TERRAIN path="+HierarchyPath(terrain.transform)+
                    " size="+(data?data.size.ToString("F2"):"missing")+
                    " material="+(terrain.materialTemplate?terrain.materialTemplate.name:"default")+
                    " shader="+(terrain.materialTemplate && terrain.materialTemplate.shader
                        ?terrain.materialTemplate.shader.name:"default"));
                if(data)
                {
                    var layers=data.terrainLayers;
                    var alphaLayers=data.alphamapLayers;
                    entries.Add("TERRAIN_ALPHAMAP width="+data.alphamapWidth+
                        " height="+data.alphamapHeight+
                        " channelCount="+alphaLayers+
                        " sourceLayers="+(layers==null?0:layers.Length));
                    if(layers!=null && layers.Length>0 &&
                       layers.Length==alphaLayers &&
                       data.alphamapWidth>0 && data.alphamapHeight>0)
                    {
                        var sum=new double[layers.Length];
                        var max=new float[layers.Length];
                        int observations=0;
                        int strideX=Mathf.Max(1,data.alphamapWidth/14);
                        int strideY=Mathf.Max(1,data.alphamapHeight/14);
                        for(int y=strideY/2;y<data.alphamapHeight;y+=strideY)
                        for(int x=strideX/2;x<data.alphamapWidth;x+=strideX)
                        {
                            var weights=data.GetAlphamaps(x,y,1,1);
                            for(int layer=0;layer<layers.Length;layer++)
                            {
                                float weight=weights[0,0,layer];
                                sum[layer]+=weight;
                                max[layer]=Mathf.Max(max[layer],weight);
                            }
                            observations++;
                        }
                        for(int i=0;i<layers.Length;i++)
                        {
                            var layer=layers[i];
                            entries.Add("LAYER index="+i+
                                " name="+(layer?layer.name:"missing")+
                                " remapMax="+(layer?layer.diffuseRemapMax.ToString("F3"):"missing")+
                                " meanAlphamap="+(sum[i]/Mathf.Max(1,observations)).ToString("F5")+
                                " maxAlphamap="+max[i].ToString("F5")+
                                " samples="+observations);
                        }
                    }
                }
            }
            var large=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .Where(r=>r && r.gameObject.scene.isLoaded && r.enabled &&
                    r.gameObject.activeInHierarchy &&
                    r.bounds.size.x>=10f && r.bounds.size.z>=10f)
                .OrderByDescending(r=>r.bounds.size.x*r.bounds.size.z)
                .Take(48).ToArray();
            foreach(var renderer in large)
            {
                var area=renderer.bounds.size.x*renderer.bounds.size.z;
                var materials=renderer.sharedMaterials;
                var descriptions=new List<string>();
                for(int i=0;i<materials.Length && i<24;i++)
                {
                    var material=materials[i];
                    if(!material)continue;
                    var color=material.HasProperty("_BaseColor")
                        ?material.GetColor("_BaseColor").ToString("F3")
                        :material.HasProperty("_Color")
                            ?material.GetColor("_Color").ToString("F3"):"none";
                    descriptions.Add(i+":"+material.name+
                        " color="+color+
                        " shader="+(material.shader?material.shader.name:"none"));
                }
                entries.Add("RENDERER areaXZ="+area.ToString("F2")+
                    " bounds="+renderer.bounds.size.ToString("F2")+
                    " path="+HierarchyPath(renderer.transform)+
                    " materialSlots=["+string.Join(";",descriptions)+"]");
            }
            File.WriteAllLines(Path.Combine(directory,"VigiliaGroundMaterialInventory.txt"),entries);
            Debug.Log("VIGILIA_GROUND_INVENTORY_READ_ONLY "+
                "terrain="+Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Length+
                " largeRenderers="+large.Length);
        }

        static void VerifyProductionFirstEntry()
        {
            var before=host.City.Gateway.Snapshot();
            Need(string.IsNullOrEmpty(before.PlayerName),"Fresh entry fixture must start unnamed");
            var ui=BreachWatchHud.Install(host);
            var coordinator=host.Hud.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>();
            coordinator.Refresh();
            var prompt=ui.GetComponentInChildren<Eldoria.PlayerClarity.RoyalPlayerNamePrompt>(true);
            Need(prompt && prompt.IsOpen && coordinator.IsBlockingNarrative,
                "Canonical nickname prompt stranded under retired Canvas");
            var input=prompt.GetComponentInChildren<InputField>(true);
            var confirm=prompt.GetComponentInChildren<Button>(true);
            Need(input && confirm,"Production nickname input or confirmation missing");
            var identityArt=prompt.GetComponent<BreachWatchIdentityPresentation>();
            Need(identityArt && identityArt.Installed &&
                prompt.GetComponentsInChildren<InputField>(true).Length==1 &&
                prompt.GetComponentsInChildren<Button>(true).Length==1,
                "Nickname must have the engraved Vigilia presentation and one real input/confirmation");
            var camera=host.PlayerCamera;var previous=camera.targetTexture;
            float aspect=camera.aspect;int mask=camera.cullingMask;
            var active=RenderTexture.active;
            var target=new RenderTexture(390,844,24);
            var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+ArtifactDir+"/FirstEntry"));
            Directory.CreateDirectory(folder);
            try
            {
                camera.targetTexture=target;camera.aspect=390f/844f;camera.cullingMask|=1<<5;
                ui.Reflow(390,844);ui.RenderNow();
                ui.GetComponent<BreachWatchTypography>().SynchronizeNow();
                Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                void Click()
                {
                    Canvas.ForceUpdateCanvases();camera.Render();
                    var rect=(RectTransform)confirm.transform;
                    var point=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));
                    var pointer=new PointerEventData(EventSystem.current){position=point,pointerId=-1,button=PointerEventData.InputButton.Left};
                    var hits=new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer,hits);
                    Need(hits.Count>0 && hits[0].gameObject.GetComponentInParent<Button>()==confirm,
                        "Nickname confirmation is not the actual first touch target");
                    ExecuteEvents.Execute(confirm.gameObject,pointer,ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(confirm.gameObject,pointer,ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(confirm.gameObject,pointer,ExecuteEvents.pointerClickHandler);
                }
                input.text="";Click();
                Need(prompt.IsOpen && string.IsNullOrEmpty(host.City.Gateway.Snapshot().PlayerName),
                    "Invalid nickname committed or dismissed first entry");
                input.text="Prueba Valoria";
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
                File.WriteAllBytes(Path.Combine(folder,"nickname-390x844.png"),pixels.EncodeToPNG());
                Click();coordinator.Refresh();
                var after=host.City.Gateway.Snapshot();
                Need(after.PlayerName=="Prueba Valoria" && !coordinator.IsBlockingNarrative,
                    "New Canvas did not commit and reread canonical nickname");
                Need(after.PlayerId==before.PlayerId && JsonUtility.ToJson(after.Resources)==JsonUtility.ToJson(before.Resources),
                    "First entry altered identity or resources");
                File.WriteAllText(Path.Combine(folder,"source.txt"),Environment.GetEnvironmentVariable("GITHUB_SHA"));
                File.WriteAllText(Path.Combine(folder,"complete.txt"),"PRODUCTION_FIRST_ENTRY_TWO_REAL_TOUCHES_PASS");
                Debug.Log("PRODUCTION_FIRST_ENTRY_TWO_REAL_TOUCHES_PASS");
            }
            finally
            {
                camera.targetTexture=previous;camera.aspect=aspect;camera.cullingMask=mask;RenderTexture.active=active;
                target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
            }
        }

        // A callback invocation can pass with a dialog outside the viewport.
        // This regression requires the real visible button to be the first
        // GraphicRaycaster target on a 390x844 production Canvas after naming.
        static void TouchVisibleProductionNarrative()
        {
            var ui=Object.FindFirstObjectByType<BreachWatchHud>();
            Need(ui && ui.ProductionPresentation,"Production narrative fixture missing");
            var camera=host.PlayerCamera;var previous=camera.targetTexture;
            float aspect=camera.aspect;int mask=camera.cullingMask;
            var active=RenderTexture.active;
            var target=new RenderTexture(390,844,24);
            var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+ArtifactDir+"/FirstEntry"));
            Directory.CreateDirectory(folder);
            try
            {
                camera.targetTexture=target;camera.aspect=390f/844f;camera.cullingMask|=1<<5;
                ui.Reflow(390,844);ui.RenderNow();
                host.Hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV3CinematicFinish>()?.SynchronizeNow();
                Canvas.ForceUpdateCanvases();camera.Render();
                // A late color property correction can pass while the browser
                // still draws a mesh built with legacy brown. Require the
                // color invariant immediately when the actual painter runs,
                // before another canvas update can conceal that ordering bug.
                host.Hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV3CinematicFinish>()?.SynchronizeNow();
                host.Hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV4DialogueArt>()?.SynchronizeNow();
                Need(host.Hud.Modal.Body && host.Hud.Modal.Body.color.grayscale>.7f,
                    "Legacy narrative painter can dirty a brown mesh before Vigilia renders");
                Canvas.ForceUpdateCanvases();camera.Render();
                Need(host.Hud.Modal.Body && host.Hud.Modal.Body.color.grayscale>.7f &&
                     host.Hud.Modal.Body.color.a>.9f,
                    "Canonical parchment ink made the Vigilia story unreadable");
                var button=host.Hud.Modal.Extra;
                Need(button && button.IsInteractable(),"Narrative Continue is not interactable");
                var rect=(RectTransform)button.transform;
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                foreach(var corner in corners)
                {
                    var screen=RectTransformUtility.WorldToScreenPoint(camera,corner);
                    Need(screen.x>=0 && screen.x<=390 && screen.y>=0 && screen.y<=844,
                        "Production Continue is offscreen after nickname: "+screen);
                }
                RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
                File.WriteAllBytes(Path.Combine(folder,"story-visible-"+storyPages+".png"),pixels.EncodeToPNG());
                var point=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));
                var pointer=new PointerEventData(EventSystem.current){position=point,pointerId=-1,button=PointerEventData.InputButton.Left};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Need(hits.Count>0 && hits[0].gameObject.GetComponentInParent<Button>()==button,
                    "Production Continue is not first touch target after nickname");
                ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
                File.AppendAllText(Path.Combine(folder,"visible-narrative-touches.txt"),
                    "PRODUCTION_VISIBLE_NARRATIVE_TOUCH "+storyPages+" at "+point+"\n");
                Debug.Log("PRODUCTION_VISIBLE_NARRATIVE_TOUCH "+storyPages);
            }
            finally
            {
                camera.targetTexture=previous;camera.aspect=aspect;camera.cullingMask=mask;
                RenderTexture.active=active;target.Release();
                Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
            }
        }

        static void CaptureRealCity()
        {
            var dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../" + ArtifactDir));
            Directory.CreateDirectory(dir);
            Need(!host.Hud.NarrativeOpen, "Existing narrative still active at capture start");
            var before = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            var ui = BreachWatchHud.Install(host);
            CaptureActualGroundMaterialInventory(dir);
            Need(ui != null && ui.IndependentCanvas && ui.LegacyPresentationHidden,
                "This must be a new Canvas; the entire old HUD presentation must be OFF");
            // A disabled legacy Canvas must NOT hide canonical story or
            // confirmation dialogs. Verify the ONE existing ModalWindow is
            // actually moved to Vigilia, its old frame switched off, and
            // its real Show/Hide lifecycle still works with gameplay intact.
            Need(ui.CanonicalModalVisibleOnIndependentCanvas &&
                 host.Hud.Modal.transform.IsChildOf(ui.transform),
                 "Canonical narrative/confirmation modal is stranded under hidden Royal HUD");
            var genuineModal=host.Hud.Modal;
            var modalFrame=genuineModal.transform.Find("SafeArea/Window");
            // Transform.Find treats '/' as hierarchy path delimiters, even
            // when the literal child name contains '/'. Inspect the actual
            // rendered Image components instead of querying that path.
            var oldModalPaint=modalFrame?modalFrame.GetComponent<Graphic>():null;
            var newModalStone=modalFrame?
                modalFrame.GetComponentsInChildren<Image>(true).FirstOrDefault(x=>
                    x.name=="Vigilia / one forged stone modal"):null;
            Need(oldModalPaint && !oldModalPaint.enabled && newModalStone &&
                 newModalStone.enabled && newModalStone.sprite,
                 "Old modal painting remains or new modal stone is missing; "+
                 "old="+(oldModalPaint?oldModalPaint.enabled.ToString():"null")+
                 " stone="+(newModalStone?newModalStone.enabled.ToString():"null"));
            var preservedModalGame=JsonUtility.ToJson(host.City.Gateway.Snapshot());
            var currentChapter=host.Read("bastion",host.English);
            genuineModal.Show(currentChapter.Chapter,currentChapter.Objective);
            Canvas.ForceUpdateCanvases();
            Need(genuineModal.gameObject.activeInHierarchy &&
                 genuineModal.Close &&
                 genuineModal.Close.targetGraphic &&
                 genuineModal.Close.targetGraphic.gameObject.activeInHierarchy,
                 "Real modal open/close became invisible after old HUD disabled");
            genuineModal.Hide();
            Need(!genuineModal.gameObject.activeInHierarchy &&
                 preservedModalGame==JsonUtility.ToJson(host.City.Gateway.Snapshot()),
                 "Native modal lifecycle modified actual gameplay/save state");
            Need(ui.HasRealSession, "New Canvas cannot access existing real game session");
            Need(ui.HasTrueResourceIllustrations,
                "All three resource icons must be real, recognizable licensed independent sprites");
            Need(ui.VisibleResourceCount == (ui.CurrentSnapshot.ShowFood?3:2),
                 "Unintroduced food capsule is visible or unlocked food is missing");
            Need(ui.HasNativePremiumTypography,
                "Vigilia must use its own legally bundled Garamond text on all devices");
            var expectedDestinations=new []{"REINO","RECURSOS","EJÉRCITO","EXPLORAR","HÉROES"};
            Need(ui.NavButtons.Count==5 &&
                 ui.NavButtons.Select(x=>x.name).SequenceEqual(expectedDestinations),
                 "The five sculpted medallions have misleading destination labels");
            Need(!ui.NavButtons[2].gameObject.activeInHierarchy &&
                 !ui.NavButtons[4].gameObject.activeInHierarchy &&
                 !ui.NavButtons[2].interactable && !ui.NavButtons[4].interactable,
                 "Army and Heroes must not appear before their native screens exist");
            Need(ui.NavButtons[3].gameObject.activeInHierarchy ==
                 host.CanOpen(RoyalSection.World),
                 "Explore medallion appeared before canonical World unlock");
            Need(!ui.GetComponentsInChildren<Button>(true).Any(b=>
                 (b.name=="Notices"||b.name=="Settings"||b.name=="Fissure") &&
                 b.gameObject.activeInHierarchy),
                 "Future utility buttons must not appear before their gameplay exists");
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
            // Inspect both real destinations through repeated orientations.
            // The Bastion-only screenshots cannot detect a clipped Aserradero
            // title or BestFit bounds inherited from the preceding layout.
            foreach(var size in new[]{(900,1600),(390,844),(844,390),(1600,900),(900,1600)})
            {
                ui.Reflow(size.Item1,size.Item2);
                foreach(var id in new[]{"sawmill","bastion"})
                {
                    ui.SelectBuilding(id);
                    ui.RenderNow();
                    Canvas.ForceUpdateCanvases();
                    var title=ui.GetComponentsInChildren<Text>(true)
                        .First(t=>t && t.name=="Real building name");
                    Need(title.text==ui.CurrentSnapshot.Building.Name &&
                         title.preferredWidth<=title.rectTransform.rect.width+.1f,
                         "Real building title escapes its painted well: "+id+"/"+size);
                    Need(title.fontSize*ui.NativeCanvas.scaleFactor>=12f,
                         "Dynamic building title was shrunk below readability: "+id+"/"+size);
                    var cost=ui.GetComponentsInChildren<Text>(true)
                        .First(t=>t && t.name=="Compact engraved cost heading");
                    int expectedCostFont=size.Item2>size.Item1
                        ?(size.Item1<600?13:10):14;
                    Need(!cost.resizeTextForBestFit && cost.fontSize==expectedCostFont,
                         "Cost heading inherited a different orientation's font: "+size);
                }
            }
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
                AuditExclusivePresentation();camera.Render(); AuditExclusivePresentation();camera.Render();
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
                    // Synchronous editor probes do not advance a player frame.
                    // Render newly re-enabled camera-space graphics before
                    // querying their depth/culling in the GraphicRaycaster.
                    AuditExclusivePresentation();camera.Render();
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
                         "Player cannot touch native "+label+" at its visible center "+pos+
                         " hits="+string.Join(" | ",hits.Select(h=>HierarchyPath(h.gameObject.transform))));
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
            var canvasDiagnostics=AuditActualCanvases(ui);
            // Check every screenshot, not only the final city state: a
            // separate modal HUD could be created only while Bastion is
            // selected and disappear before final navigation checks.
            int foreignCanvasCount=Math.Max(shots.Max(x=>x.activeForeignScreenSpaceCanvases),
                canvasDiagnostics.Count(c=>c.active &&
                    !c.legacy && !c.independentVigilia &&
                    (c.renderMode=="ScreenSpaceOverlay" ||
                     c.renderMode=="ScreenSpaceCamera") &&
                    c.livePaintedGraphics>0));
            Debug.Log("VIGILIA_SCREEN_SPACE_CANVAS_AUDIT "+
                JsonUtility.ToJson(new Report {
                    canvasDiagnostics=canvasDiagnostics,
                    activeForeignScreenSpaceCanvases=foreignCanvasCount
                }));
            var report = new Report {
                canvasDiagnostics=canvasDiagnostics,
                activeForeignScreenSpaceCanvases=foreignCanvasCount,
                status = "UNITY_NATIVE_INDEPENDENT_CAPTURED_ART_REVIEW_PENDING",
                source = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local",
                scene = Scene, note = "Genuine Unity play mode and disposable Valoria session; original RoyalHUD Canvas disabled; complete other-Canvas census attached. No ART_APPROVED or Android/WebGL certification.",
                screenshotCount = shots.Count, functioningNewNavigationClicks = pointerClicks,
                oldCanvasVisible = canvasDiagnostics.Count(c=>c.legacy && c.active),
                saveUnchanged = before == after,
                oldHudGraphicsReused = canvasDiagnostics.Any(c=>c.legacy && c.active &&
                    c.livePaintedGraphics>0),
                artworkApproved = false, androidTested = false,
                shots = shots.ToArray()
            };
            File.WriteAllText(Path.Combine(dir,"BreachWatchNativeEvidence.json"),JsonUtility.ToJson(report,true));
            // Six additional native images at 3x, with the exact same
            // logical camera composition and UI scale as the six phone 1x
            // captures. This is evidence, NOT an art approval or a change
            // to the physical iPhone WebGL render resolution.
            BreachWatchRetinaEvidence.Capture(ui,host,dir);
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
                // Direct camera captures must execute the same responsive
                // treasury layout that its normal frame update applies.
                var treasure=ui.GetComponentInChildren<Eldoria.DailyTreasures.DailyTreasurePanel>(true);
                if(treasure)treasure.ProbeOrientationLayoutForNativeEvidence();
                AuditExclusivePresentation();camera.Render();
                Canvas.ForceUpdateCanvases();
                AuditExclusivePresentation();camera.Render();
                RenderTexture.active = rt;
                image.ReadPixels(new Rect(0,0,w,h),0,0);
                image.Apply();
                var actualPixels=image.GetPixels32();
                // Always archive the exact real screenshot even when a following
                // visual/text gate fails. Previous FAILs hid the actual cause.
                File.WriteAllBytes(file,image.EncodeToPNG());
                foreach(var projected in ui.NativeCanvas.GetComponentsInChildren<BuildingLabel>(false))
                {
                    if(!projected.Select || !projected.Select.gameObject.activeInHierarchy)continue;
                    var corners=new Vector3[4];
                    ((RectTransform)projected.transform).GetWorldCorners(corners);
                    var bounds=projected.Layer.rect;
                    foreach(var corner in corners)
                    {
                        var p=projected.Layer.InverseTransformPoint(corner);
                        Need(p.x>=bounds.xMin-1f && p.x<=bounds.xMax+1f &&
                             p.y>=bounds.yMin-1f && p.y<=bounds.yMax+1f,
                            "Projected guidance clipped at viewport edge: "+projected.name+" / "+name);
                    }
                }
                CheckLiveSlicedGeometry(ui,name);
                var allText=ui.GetComponentsInChildren<Text>(true);
                // Do not accept a visually blank upper-left player plate
                // just because other headings passed. The original cf73
                // screen had zero visible name and power despite valid
                // gateway data; prove actual GPU glyph contribution for both.
                string[] important=selection=="bastion"
                    ?new[]{"Player name","Realm power",
                        "Real building name","Real building level","Real effects",
                        "Real state and timer","Compact engraved cost heading","Real action title",
                        "Objective progress"}
                    :new[]{"Player name","Realm power",
                        "Actual chapter","Objective progress"};
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
                        AuditExclusivePresentation();camera.Render();
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
                    AuditExclusivePresentation();camera.Render();
                    RenderTexture.active=rt;
                    Object.DestroyImmediate(scratch);
                }
                Need(new FileInfo(file).Length > 16000, "Unity native PNG incomplete: "+name);
                Need(ui.IndependentCanvas && ui.LegacyPresentationHidden && ui.HasRealSession,
                    "Old presentation visible or disconnected UI in "+name);
                var atmosphericVeil=ui.GetComponentInChildren<BreachWatchAtmosphere>(true);
                Need(!atmosphericVeil,
                     "Fresh HUD rebuild must NOT overlay a previous fullscreen atmospheric layer: "+name);
                var rift=ui.GetComponentsInChildren<Image>(true)
                    .FirstOrDefault(x=>x && x.sprite &&
                        x.sprite.name=="breach_signal");
                Need(rift && rift.gameObject.activeInHierarchy &&
                     rift.type==Image.Type.Simple && rift.preserveAspect &&
                     !rift.raycastTarget &&
                     ui.GetComponentsInChildren<Image>(true).Count(x=>
                         x && x.sprite && x.sprite.name=="breach_signal")==1,
                     "Owner's approved glowing fissure must be ONE native transparent "+
                     "painted sprite, never the generic procedural X: "+name);
                Need(ui.GetComponentsInChildren<BreachWatchFrameEngraving>(true).All(trim=>
                         trim.GetComponentInParent<BreachWatchIdentityPresentation>(true) &&
                         !trim.isActiveAndEnabled),
                     "City contains visible fallback trim or trim outside the closed identity modal: "+name);
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
                    var authoredCard=ui.BuildingPanel.GetComponent<Image>();
                    Need(section && !section.raycastTarget &&
                         !section.gameObject.activeInHierarchy &&
                         authoredCard && authoredCard.sprite &&
                         authoredCard.sprite.name=="panel",
                         "The authored card owns its single painted rule; a procedural divider must not duplicate it");
                }
                var depthLayers=ui.GetComponentsInChildren<Shadow>(true)
                    .Where(q=>q && (q.name=="Identity / basalt crest" ||
                        q.name=="Chapter side tablet" ||
                        q.name=="Independent selected-building cartouche"))
                    .ToArray();
                // The source PNGs have their own sculpted contours and
                // shadows. A second uGUI shadow detached every plaque from
                // the world (even a 1.4u shadow reintroduces the doubled rim).
                Need(depthLayers.Length==0,
                     "Extra uGUI contact shadows double the original "+
                     "painted relief and leave the HUD floating: "+name);
                var profilePlate=ui.GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(x=>x && x.name=="Identity / basalt crest");
                var duplicatedLion=ui.GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(x=>x && x.name=="Independent circular realm crest");
                // The independently regenerated gold lion is part of the one
                // finished profile illustration; any second crest is a bug.
                Need(profilePlate && !duplicatedLion &&
                     profilePlate.GetComponentsInChildren<Graphic>(true).Length==3,
                     "Duplicate lion badge or second identity surface in "+name);
                // Strict cleanroom invariant: the entire panel must have
                // precisely one independent high-resolution sprite.
                // No old owner_gold_border overlay, separate brass portrait
                // plate or procedural fallback can survive underneath it.
                var oldGold=ui.BuildingPanel.GetComponentsInChildren<Image>(true)
                    .Count(i=>i && i.sprite && i.sprite.name=="owner_gold_border");
                var oldBrass=ui.BuildingPanel.GetComponentsInChildren<BreachWatchPlate>(true)
                    .Count(i=>i && i.sprite && i.sprite.name=="brass_portrait");
                var cleanPanel=ui.BuildingPanel.GetComponent<Image>();
                Need(oldGold==0 && oldBrass==0 &&
                     cleanPanel && cleanPanel.sprite &&
                     cleanPanel.sprite==Resources.Load<Sprite>("VigiliaArt/CleanRoom/panel") &&
                     cleanPanel.type==Image.Type.Simple && cleanPanel.raycastTarget &&
                     ui.UsesPremiumForgedPortraitFrame &&
                     ui.UsesSinglePremiumContext,
                     "Old panel artwork, detached rim or duplicate portrait frame "+
                     "is still layered under the new 2K painted panel: "+name);
                var palette=ui.GetComponentsInChildren<Image>(true)
                    .Where(x=>x && x.sprite).ToArray();
                Need(palette.Count(x=>x.sprite.name=="panel" &&
                         x.color.r>=.95f && x.color.g>=.95f)==1 &&
                     palette.Count(x=>x.sprite.name=="profile" &&
                         x.color.r>=.95f && x.color.g>=.95f)==1 &&
                     palette.Count(x=>x.sprite.name=="resource" &&
                         x.color.r>=.95f && x.color.g>=.95f)>=2,
                     "Player, resources and cartouche no longer share "+
                     "the same warm material treatment: "+name);
                Need(ui.HasPremiumResourceSurfaces,
                    "All three live resources must use original complete 2K cleanroom capsules, not old procedural frames");
                Need(ui.HasOwnerApprovedUtilityArtwork,
                    "The three compact utility seals must match the separately cropped "+
                    "gold medallions in the owner's approved UI, not generic hexagons");
                var mineral=ui.BuildingPanel.GetComponent<Image>();
                Need(mineral && mineral.sprite &&
                     mineral.sprite.name=="panel" &&
                     mineral.type==Image.Type.Simple && mineral.raycastTarget &&
                     ui.BuildingPanel.GetComponentsInChildren<BreachWatchPanelInk>(true).Length==0 &&
                     !ui.BuildingPanel.GetComponent<BreachWatchPlate>(),
                     "Old procedural card mesh or duplicate border still exists in "+
                     "place of the real owner-reference mineral material: "+name);
                Need(ui.UsesCompactRealmCrest,
                    "Compact circular Valoria crest missing, or discarded red header returned: "+name);
                Need(!ui.GetComponentsInChildren<Text>(true).Any(t=>
                    t && t.gameObject.activeInHierarchy && t.name=="Vigilia de la Brecha"),
                    "Large redundant central title must not cover the real city: "+name);
                Need(ui.VisibleResourceCount == (ui.CurrentSnapshot.ShowFood?3:2),
                     "The native screenshot shows the wrong number of live resource chips: "+name);
                var nav = ui.NavButtons;
                Need(nav.Count==5 && nav.All(x=>x), "Exactly 5 original independent navigation buttons required");
                // An approved medallion image is now ONE authority:
                // no secondary button background, glyph or extra ring.
                // A transparent hit-only Image on the Button is permitted,
                // because it does not paint a duplicate frame or symbol.
                Need(!ui.GetComponentsInChildren<Graphic>(true).Any(g=>g &&
                         g.name=="Selected medallion / warm engraved relief"),
                     "Old duplicated selection halo remains in native scene: "+name);
                var expectedMedallions=new[]{"nav_castle","nav_provisions","nav_swords","nav_raven","nav_helm"};
                Need(nav.Count(x=>x.gameObject.activeInHierarchy)==
                     (host.CanOpen(RoyalSection.World)?3:2),
                     "Incorrect count of unlocked destination icons: "+name);
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
                    Need(button.transition==Selectable.Transition.ColorTint &&
                         button.targetGraphic==painted[0] &&
                         button.colors.disabledColor==Color.white,
                         "Premium nav button has no genuine painted pressed state, "+
                         "or a second hit illustration is being tinted: "+name+"/"+button.name);
                    Need(button.GetComponentsInChildren<BreachWatchGlyph>(true).Length==0,
                         "Old vector icons still layered under a final medallion in "+
                         name+"/"+button.name);
                    var visiblePaint=button.GetComponentsInChildren<Graphic>(true)
                        .Count(g=>g && g.enabled &&
                            g.gameObject.activeInHierarchy && g.color.a>.01f);
                    Need(visiblePaint==(button.gameObject.activeInHierarchy?1:0),
                         "Unimplemented or duplicated painted navigation appears in "+
                         name+"/"+button.name);
                }
                float previousRight = -1;
                bool oneRow = true;
                float baselineY = float.NaN;
                foreach (var button in nav.Where(x=>x.gameObject.activeInHierarchy))
                {
                    var rect = (RectTransform)button.transform;
                    Vector3[] corners = new Vector3[4];
                    rect.GetWorldCorners(corners);
                    var a = RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
                    var b = RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
                    Need(b.x-a.x>=43.5 && b.y-a.y>=43.5,
                        "Navigation touch-target smaller than 44px: "+name+"/"+button.name);
                    // A 44px invisible tap surface is NOT enough: the earlier
                    // native 390x844 / 844x390 screenshots painted medallions
                    // only ~31px wide. Check the real source Image rect and
                    // its mesh-space VisualScale at all four resolutions.
                    var medal=button.GetComponentInChildren<BreachWatchPlate>(true);
                    Need(medal && medal.sprite && medal.VisualScale>=.88f,
                        "Approved painted navigation medallion missing: "+name);
                    Vector3[] paintedCorners=new Vector3[4];
                    medal.rectTransform.GetWorldCorners(paintedCorners);
                    var pa=RectTransformUtility.WorldToScreenPoint(camera,paintedCorners[0]);
                    var pb=RectTransformUtility.WorldToScreenPoint(camera,paintedCorners[2]);
                    Need(medal.sprite.texture.mipmapCount>1 &&
                         medal.sprite.texture.filterMode==FilterMode.Trilinear &&
                         medal.sprite.texture.width<=512,
                         "Medallion must use bounded, filtered minification: "+name);
                    float visibleInkWidth=(pb.x-pa.x)*medal.VisualScale;
                    float visibleInkHeight=(pb.y-pa.y)*medal.VisualScale;
                    Need(visibleInkWidth>=38f && visibleInkHeight>=38f,
                        "Native navigation art under 38px despite legal hit target: "+
                        name+"/"+button.name+" ["+visibleInkWidth+","+visibleInkHeight+"]");
                    Need(a.x>=-2 && b.x<=w+2 && a.y>=-2 && b.y<=h+2,
                        "Off-screen independent nav button: "+name+"/"+button.name);
                    if (!float.IsNaN(baselineY) && Mathf.Abs(a.y-baselineY)>2) oneRow=false;
                    baselineY = a.y;
                    if (previousRight>=0 && a.x<previousRight+2) oneRow=false;
                    previousRight=b.x;
                }
                Need(oneRow,"Unlocked navigation symbols overlap or stack in "+name);
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
                // Native layout validation must compare REAL independent
                // text and icon boxes. The previous proof only checked
                // that cost/state/CTA touched in a legal order while the
                // player identity and resource pictograms could overlap.
                var playerTitle=ui.GetComponentsInChildren<Text>(true)
                    .FirstOrDefault(x=>x && x.name=="Player name");
                var playerPower=ui.GetComponentsInChildren<Text>(true)
                    .FirstOrDefault(x=>x && x.name=="Realm power");
                Need(playerTitle && playerPower &&
                     -playerTitle.rectTransform.anchoredPosition.y+
                         playerTitle.rectTransform.rect.height<=
                     -playerPower.rectTransform.anchoredPosition.y+.5f,
                     "Player title overlaps the real power value: "+name);
                foreach(var stone in ui.GetComponentsInChildren<RectTransform>(true)
                    .Where(x=>x && x.gameObject.activeInHierarchy &&
                        x.name.StartsWith("Resource ",StringComparison.Ordinal)))
                {
                    var icon=stone.GetComponentsInChildren<Image>(true)
                        .FirstOrDefault(x=>x && x.name=="True illustrated resource");
                    var amount=stone.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(x=>x && x.name=="Actual quantity");
                    Need(icon && amount &&
                         icon.rectTransform.anchoredPosition.x+
                             icon.rectTransform.rect.width+1.5f<=
                         amount.rectTransform.anchoredPosition.x,
                         "Resource symbol covers the true amount: "+name+"/"+stone.name);
                    float iconCentre=icon.rectTransform.anchoredPosition.x+
                        icon.rectTransform.rect.width*.5f;
                    float iconY=-icon.rectTransform.anchoredPosition.y+
                        icon.rectTransform.rect.height*.5f;
                    Need(Mathf.Abs(iconCentre-423f/22f)<.25f &&
                         Mathf.Abs(iconY-stone.rect.height*.49f)<.25f,
                         "Resource artwork is not centred on its actual capsule badge: "+name);
                    Need(icon.sprite.texture.mipmapCount>1 &&
                         icon.sprite.texture.filterMode==FilterMode.Trilinear,
                         "Resource icon missing small-size filtering: "+name);

                }
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
                        // The real Chapter I / La Brecha is deliberately
                        // TWO legible engraved lines in the original slate,
                        // not an accidental overflow. All other best-fit
                        // captions must remain exactly one line.
                        int expectedLines=label.name=="Actual chapter" &&
                            label.text.Contains("\n") ? 2 : 1;
                        Need(label.cachedTextGenerator.lineCount==expectedLines &&
                             label.cachedTextGenerator.vertexCount>0,
                             "Premium best-fit caption has unexpected lines "+
                             name+"/"+label.name+
                             " expected="+expectedLines+
                             " actual="+label.cachedTextGenerator.lineCount);
                        // Unity 6000.3 TextGenerator.fontSizeUsedForBestFit
                        // reports actual painted glyph size, not nominal.
                        if(label.name=="Actual chapter" &&
                           ((w==390&&h==844)||(w==844&&h==390)))
                        {
                            var realPt=label.cachedTextGenerator.fontSizeUsedForBestFit;
                            Need(realPt*ui.NativeCanvas.scaleFactor>=11.75f,
                                "Actual rendered phone chapter is too small "+
                                name+" actual="+realPt+
                                " scale="+ui.NativeCanvas.scaleFactor.ToString("F3"));
                        }
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
                var chapterSigil=ui.GetComponentsInChildren<BreachWatchGlyph>(true)
                    .FirstOrDefault(q=>q && q.name=="Chapter stone rune");
                Need(chapterLabel && chapterLabel.gameObject.activeInHierarchy &&
                     chapterSigil==null &&
                     chapterLabel.rectTransform.anchoredPosition.x>=16f,
                     "A vector chapter crest still floats over the ORIGINAL "+
                     "painted quest or the actual title clips its margin: "+name);
                if(host.City.Gateway.Snapshot().BastionLevel==1)
                    Need(chapterLabel && chapterLabel.gameObject.activeInHierarchy &&
                         (host.English
                            ?chapterLabel.text.Contains("THE BREACH")
                            :chapterLabel.text.Contains("LA BRECHA")) &&
                         chapterLabel.text.Contains(" / ") &&
                         !chapterLabel.text.Contains("\n") &&
                         // The new shallow owner-reference plaque uses a
                         // tighter Cormorant box. The full GPU ink-diff probe
                         // above is authoritative, not this nominal box height.
                         chapterLabel.rectTransform.rect.height>=15f &&
                         chapterLabel.fontSize*ui.NativeCanvas.scaleFactor>=11.75f &&
                         chapterLabel.preferredWidth <=
                            chapterLabel.rectTransform.rect.width+5f,
                         "Owner-approved Chapter I / La Brecha identity not readable: "+name+
                         " value="+(chapterLabel?chapterLabel.text:"<missing>")+
                         " size="+(chapterLabel?chapterLabel.rectTransform.rect.height.ToString("F1"):"0")+
                         " preferredWidth="+(chapterLabel?chapterLabel.preferredWidth.ToString("F1"):"0")+
                         " availableWidth="+(chapterLabel?chapterLabel.rectTransform.rect.width.ToString("F1"):"0")+
                         " actualFontPx="+(chapterLabel?(chapterLabel.fontSize*ui.NativeCanvas.scaleFactor).ToString("F1"):"0"));
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
                     "Real objective fraction is hidden by carved quest lower bevel: "+name+
                     " progressBottom="+progressBottom.y.ToString("F1")+
                     " tabletBottom="+tabletBottom.y.ToString("F1"));
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
                    var engravedBastion=ui.BuildingPanel.GetComponentsInChildren<BreachWatchGlyph>(true)
                        .FirstOrDefault(g=>g && g.name=="Building crest");
                    Need(engravedBastion==null,
                         "An extra vector tower is pasted over the owner "+
                         "stone while the actual Bastion title already "+
                         "identifies the building: "+name);
                    // The old right card placed the live Bastion header
                    // and level RIGHT against the invisible close-hit rect,
                    // creating a visible title/button collision on phones.
                    var heading=ui.BuildingPanel.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(t=>t && t.name=="Real building name");
                    var level=ui.BuildingPanel.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(t=>t && t.name=="Real building level");
                    var closeRect=(RectTransform)ui.BuildingClose.transform;
                    Need(heading && level &&
                         -heading.rectTransform.anchoredPosition.y+
                             heading.rectTransform.rect.height+.7f<=
                         -level.rectTransform.anchoredPosition.y,
                         "Bastion heading and actual level visually overlap: "+name);
                    Need(heading && level &&
                         heading.rectTransform.anchoredPosition.x+
                           heading.rectTransform.rect.width<=
                             closeRect.anchoredPosition.x-3.5f &&
                         level.rectTransform.anchoredPosition.x+
                           level.rectTransform.rect.width<=
                             closeRect.anchoredPosition.x-3.5f,
                         "Bastion title or level touches the top-right close badge: "+name);
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
                    // Transparent URP clear is the only honest way to avoid
                    // the black rectangular photo glued on top of the owner's
                    // mineral cartouche. Verify ACTUAL GPU alpha rather than
                    // trusting source settings or a mockup. Opaque masonry
                    // must survive; the underlying game and model are real.
                    Need(ui.BuildingPortraitTransparentBackground>.12f &&
                         ui.BuildingPortraitOpaqueMasonry>.18f,
                         "The URP real Bastion photo still has an opaque black "+
                         "rectangle or the genuine meshes lost alpha: "+name+
                         " clear="+ui.BuildingPortraitTransparentBackground+
                         " stone="+ui.BuildingPortraitOpaqueMasonry);
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
                    // This new native CTA is one approved owner sprite,
                    // with its forged hammers INSIDE the painted source,
                    // not another procedural child stacked on the button.
                    var actionArt=ui.BuildingAction.GetComponentInChildren<BreachWatchPlate>(true);
                    var actionHit=ui.BuildingAction.GetComponent<Image>();
                    Need(actionArt && actionArt.sprite &&
                         actionArt.sprite.name=="upgrade" &&
                         actionArt.type==Image.Type.Simple &&
                         actionArt.preserveAspect && !actionArt.raycastTarget &&
                         actionHit && actionHit.color.a<=.001f &&
                         actionHit.raycastTarget,
                         "Owner's hammered button must render as one undistorted original image plus only an invisible native hit area: "+name);
                    Need(ui.BuildingAction.transition==Selectable.Transition.ColorTint &&
                         ui.BuildingAction.targetGraphic==actionArt &&
                         ui.BuildingAction.colors.disabledColor==Color.white,
                         "Real red button has no pressed feedback or its invisible "+
                         "raycast image is mistakenly being tinted: "+name);
                    var artImages=ui.BuildingAction.GetComponentsInChildren<Image>(true);
                    Need(artImages.Length==2 && artImages.Count(x=>x && x.color.a>.01f)==1 &&
                         ui.BuildingAction.GetComponentsInChildren<BreachWatchGlyph>(true).Length==0,
                         "Duplicated upgrade art or old procedural glyph remains layered over the CTA: "+name);
                    var visualRect=(RectTransform)actionArt.transform;
                    float artAspect=actionArt.sprite.rect.width/
                        Mathf.Max(1f,actionArt.sprite.rect.height);
                    Need(visualRect.rect.height>22f &&
                         visualRect.rect.height<=action.rect.height-3.5f &&
                         Mathf.Abs(visualRect.rect.height*artAspect-
                                   visualRect.rect.width)<.85f,
                         "Original painted button is stretched or does not fill the authored width: "+name);
                    float artTopInCard=Top(action)+Top(visualRect);
                    float artBottomInCard=artTopInCard+visualRect.rect.height;
                    Need(artTopInCard>=Bottom(state)-.65f &&
                         artBottomInCard<=ui.BuildingPanel.rect.height+.5f,
                         "Actual painted burgundy art overlaps gameplay status or escapes card: "+name);
                    var actionText=ui.BuildingAction.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(q=>q && q.name=="Real action title");
                    // Compare against VISIBLE sprite geometry (246x60)
                    // instead of the wider invisible touch target. A caption
                    // can be inside the Button but outside the red painting.
                    float artDrawnWidth=Mathf.Min(visualRect.rect.width,
                        visualRect.rect.height*artAspect);
                    float artDrawnLeft=(action.rect.width-artDrawnWidth)*.5f;
                    Need(artDrawnWidth>=action.rect.width*.70f,
                         "Real 2K burgundy action is too small relative to its touch target: "+name);
                    Need(actionText && actionText.gameObject.activeInHierarchy &&
                         actionText.rectTransform.anchoredPosition.x>=
                           artDrawnLeft+artDrawnWidth*.28f,
                         "Real upgrade text intersects the painted source hammers: "+name);
                    Need(actionText.rectTransform.anchoredPosition.x+
                         actionText.rectTransform.rect.width<=
                           artDrawnLeft+artDrawnWidth-3f,
                         "Real upgrade caption escapes the PAINTED burgundy sprite: "+name);
                    // Full destination title fits on roomy portrait; landscape shows
                    // the same real Bastion II action in one clear line.
                    bool fullCaption=h>w && w>=800;
                    string expectedCaption=host.English
                        ?(fullCaption?"Upgrade to Bastion II":"Upgrade II")
                        :(fullCaption?"Mejorar a Bastión II":"Mejorar a II");
                    Need(actionText.text==expectedCaption,
                         "Native button caption is not appropriate for language/orientation: "+name);
                    // True visible-art glyph fit, not only the caption's
                    // rectangular anchor: no overflown "II" outside the gold.
                    // Keep this check even when BestFit is disabled to avoid
                    // passing the exact screenshot defect observed by owner.
                    Need(actionText.cachedTextGenerator.lineCount==1 &&
                         actionText.cachedTextGenerator.vertexCount>=4 &&
                         actionText.preferredWidth<=
                             actionText.rectTransform.rect.width-1f,
                         "Real one-line upgrade caption escapes original burgundy art: "+
                         name+" font="+actionText.fontSize+
                         " glyphWidth="+actionText.preferredWidth.ToString("F1")+
                         " available="+actionText.rectTransform.rect.width.ToString("F1"));
                    // The approved reference's circular gold X is one
                    // painted image, never a generic octagonal Backing +
                    // painted vector X. Hidden root image owns the hit only.
                    var closeRoot=ui.BuildingClose;
                    var realClose=closeRoot.GetComponentsInChildren<Image>(true);
                    var sourceX=realClose.FirstOrDefault(x=>x && x.sprite &&
                        x.sprite.name=="close_medallion");
                    var closeHit=closeRoot.GetComponent<Image>();
                    Need(closeRoot.transition==Selectable.Transition.ColorTint &&
                         closeRoot.targetGraphic==sourceX &&
                         closeRoot.colors.disabledColor==Color.white,
                         "Real golden close glyph has no painted pressed state: "+name);
                    Need(realClose.Length==2 && sourceX &&
                         realClose.Count(x=>x && x.color.a>.01f)==1 &&
                         sourceX.preserveAspect && !sourceX.raycastTarget &&
                         closeHit && closeHit.raycastTarget &&
                         closeHit.color.a<=.001f &&
                         closeRoot.GetComponentsInChildren<BreachWatchGlyph>(true).Length==0,
                         "Owner X must be ONE small golden disk plus an invisible 44px tap zone: "+name);

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
                var foreignScreenSpace=AuditActualCanvases(ui)
                    .Where(c=>c.active && !c.legacy && !c.independentVigilia &&
                        c.livePaintedGraphics>0 &&
                        (c.renderMode=="ScreenSpaceOverlay" ||
                         c.renderMode=="ScreenSpaceCamera"))
                    .ToArray();
                BreachWatchTypographyEvidence.Capture(ui.NativeCanvas,camera,rt,file,w,h);
                return new Shot {
                    activeForeignScreenSpaceCanvases=foreignScreenSpace.Length,
                    foreignScreenSpaceCanvasPaths=foreignScreenSpace.Select(c=>c.path).ToArray(),
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
