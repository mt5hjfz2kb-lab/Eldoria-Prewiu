#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using Eldoria.VisualKit;
using Eldoria.UI.RoyalGold;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.VisualKit.Editor
{
    /// <summary>
    /// Same-scene, same-save native Unity A/B evidence. The actual screenshot
    /// function is the certified ValoriaIntegrationBuild.Capture camera/canvas
    /// pipeline, located by reflection so no game Editor assembly is patched.
    ///
    /// Captures only a disposable VALORIA_GAMEPLAY_PROBE session in batchmode.
    /// Never launches a second Unity process or changes user saves or scenes.
    /// Requires the live Unity Editor, same project's native scene and genuine
    /// RoyalHUD, not a rendering mockup. Fail closed if any of these are absent.
    /// </summary>
    [InitializeOnLoad]
    public static class ForjaOscuraNativeEvidence
    {
        const string Flag="ForjaOscura.Evidence.Active";
        const string BatchFlag="ForjaOscura.Evidence.Batch";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string SkinOptIn="Eldoria.ForjaOscura.LivePreview.EditorOnly.v02";
        static double deadline;
        static bool running;

        static ForjaOscuraNativeEvidence()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        [MenuItem("Eldoria/Forja Oscura/Capturar A-B nativo sin alterar escena")]
        public static void CaptureInPlay()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play in the canonical scene first.");
            RunCapture(false);
        }

        // Example: Unity -batchmode -force-d3d11 -projectPath <Unity> -executeMethod
        // Eldoria.VisualKit.Editor.ForjaOscuraNativeEvidence.CaptureBatch -logFile <log>
        public static void CaptureBatch()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Batch capture needs an initial Editor state.");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE", "1");
            EditorPrefs.SetBool(SkinOptIn, false);
            EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            SessionState.SetBool(Flag, true);
            SessionState.SetBool(BatchFlag, true);
            deadline = EditorApplication.timeSinceStartup + 240;
            EditorApplication.EnterPlaymode();
        }

        static void Poll()
        {
            if (!SessionState.GetBool(Flag, false) || running || !EditorApplication.isPlaying) return;
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 240;
            if (EditorApplication.timeSinceStartup > deadline) { Fail("Timed out waiting for the genuine gameplay scene and HUD"); return; }
            var host = FindHost();
            if (!host) return;
            bool ready = BoolProperty(host,"Ready");
            if (!ready) return;
            var hud = HostHud(host);
            if (!hud) return;
            running = true;
            SessionState.SetBool(Flag, false);
            try { RunCapture(true); }
            catch (Exception e) { Debug.LogException(e); Fail(e.ToString()); }
            finally { running = false; }
        }

        static MonoBehaviour FindHost() => Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Exclude,FindObjectsSortMode.None)
            .FirstOrDefault(m=>m && m.GetType().FullName == "Eldoria.ReferenceClean.ValoriaIntegratedWorld");

        static bool BoolProperty(object obj,string name)
        {
            var prop=obj.GetType().GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
            return prop!=null && prop.PropertyType==typeof(bool) && (bool)prop.GetValue(obj);
        }
        static RoyalHUD HostHud(object host)
        {
            return host?.GetType().GetProperty("Hud",BindingFlags.Public|BindingFlags.Instance)?.GetValue(host) as RoyalHUD;
        }
        static Type BuildType()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a=>a.GetType("Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild",false))
                .FirstOrDefault(t=>t!=null);
        }
        static void Capture(Type captureType, object host,string basename,int width,int height,string outputDir,
            Action synchronizeVisuals = null, Action validateRenderedView = null)
        {
            // The public five-argument routine is the canonically established
            // native capture. For the candidate we must additionally refresh
            // nine-slice/nav geometry AFTER the offscreen Canvas gets its
            // new resolution. The canonical six-argument internal sibling
            // exposes precisely that pre-render callback. If it disappears in
            // a future project revision, FAIL instead of generating misleading
            // after screenshots with a stale layout.
            var all=captureType.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            MethodInfo m;
            object[] args;
            if (synchronizeVisuals != null || validateRenderedView != null)
            {
                m=all.FirstOrDefault(x=>x.Name=="CapturePrepared"&&x.GetParameters().Length==6);
                if(m==null)throw new InvalidOperationException(
                    "Original native CapturePrepared pre-render callback missing; cannot prove responsive Forja UI");
                args=new object[]{host,basename,width,height,validateRenderedView,synchronizeVisuals};
            }
            else
            {
                m=all.FirstOrDefault(x=>x.Name=="Capture"&&x.GetParameters().Length==5);
                if(m==null)throw new InvalidOperationException(
                    "Original native 5-argument Capture missing; refuse simulated evidence");
                args=new object[]{host,basename,width,height,null};
            }
            var original=Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../docs/evidence/valoria-reference-clean",basename));
            Directory.CreateDirectory(Path.GetDirectoryName(original));
            try{m.Invoke(null,args);}catch(TargetInvocationException e){throw e.InnerException??e;}
            if(!File.Exists(original)||new FileInfo(original).Length<4096)
                throw new IOException("Native Unity camera capture missing or tiny: "+original);
            File.Copy(original,Path.Combine(outputDir,basename),true);
        }
        [Serializable] class UiTouchMetric
        {
            public string resolution;
            public string smallestDestination;
            public float minWidthPx;
            public float minHeightPx;
            public float minGraphicWidthPx;
            public float minGraphicHeightPx;
            public bool hasFiveVisibleActions;
        }

        private static UiTouchMetric MeasureNavigationPixels(RoyalHUD hud,
            Camera nativeCamera, int width, int height)
        {
            var nav = hud.Navigation;
            Need(nav && nav.Items != null, "Native pixel hit test requires the genuine navigation");
            float minW = float.PositiveInfinity;
            float minH = float.PositiveInfinity;
            float minGraphicW = float.PositiveInfinity;
            float minGraphicH = float.PositiveInfinity;
            string worst = "";
            int seen = 0;
            foreach (var item in nav.Items)
            {
                if (item == null || !item.Button || !item.Button.gameObject.activeInHierarchy) continue;
                var rect = item.Button.transform as RectTransform;
                if (!rect) continue;
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(nativeCamera, corners[0]);
                Vector2 topLeft = RectTransformUtility.WorldToScreenPoint(nativeCamera, corners[1]);
                Vector2 bottomRight = RectTransformUtility.WorldToScreenPoint(nativeCamera, corners[3]);
                float pixelW = Vector2.Distance(bottomLeft, bottomRight);
                float pixelH = Vector2.Distance(bottomLeft, topLeft);
                if (pixelW < minW || pixelH < minH) worst = item.Section.ToString();
                minW = Mathf.Min(minW, pixelW);
                minH = Mathf.Min(minH, pixelH);
                var target = item.Button.targetGraphic;
                Need(target && target.isActiveAndEnabled && target.raycastTarget,
                    "Native navigation graphic is not an active raycast target: " + item.Section);
                var graphicCorners = new Vector3[4];
                (target.transform as RectTransform).GetWorldCorners(graphicCorners);
                var graphicLeft = RectTransformUtility.WorldToScreenPoint(nativeCamera, graphicCorners[0]);
                var graphicTop = RectTransformUtility.WorldToScreenPoint(nativeCamera, graphicCorners[1]);
                var graphicRight = RectTransformUtility.WorldToScreenPoint(nativeCamera, graphicCorners[3]);
                minGraphicW = Mathf.Min(minGraphicW, Vector2.Distance(graphicLeft, graphicRight));
                minGraphicH = Mathf.Min(minGraphicH, Vector2.Distance(graphicLeft, graphicTop));
                // Run after the canonical camera render, not in the earlier
                // prepareView callback. Camera/canvas matrices may still be
                // stale before their first offscreen render.
                Need(EventSystem.current != null, "Native UI EventSystem missing at " + width + "x" + height);
                var center = (graphicCorners[0] + graphicCorners[2]) * .5f;
                var pointer = new PointerEventData(EventSystem.current) {
                    pointerId = -71,
                    button = PointerEventData.InputButton.Left,
                    position = RectTransformUtility.WorldToScreenPoint(nativeCamera, center)
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                Need(hits.Count > 0 &&
                    ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == item.Button.gameObject,
                    "Native rendered navigation touch is intercepted: " + item.Section +
                    " at " + width + "x" + height);
                seen++;
            }
            Need(seen == 5, "Native mobile pixel hit test could not find exactly five controls");
            return new UiTouchMetric {
                resolution = width + "x" + height,
                smallestDestination = worst,
                minWidthPx = minW,
                minHeightPx = minH,
                minGraphicWidthPx = minGraphicW,
                minGraphicHeightPx = minGraphicH,
                hasFiveVisibleActions = seen == 5
            };
        }

        // Native readability must be measured in final capture pixels;
        // a larger logical font can still become tiny on a 390px Canvas.
        [Serializable] class UiReadabilityMetric
        {
            public string resolution;
            public float minNavigationGlyphPx;
            public float minResourceGlyphPx;
            public bool navigationLabelsFit;
            public bool resourceValuesFit;
            public string smallestNavigationLabel;
            public string smallestResourceLabel;
        }

        private static UiReadabilityMetric MeasureReadability(RoyalHUD hud, int width, int height)
        {
            var canvas = hud.GetComponent<Canvas>();
            Need(canvas != null, "Native text QA requires the actual HUD Canvas");
            var m = new UiReadabilityMetric {
                resolution = width + "x" + height,
                minNavigationGlyphPx = float.PositiveInfinity,
                minResourceGlyphPx = float.PositiveInfinity,
                navigationLabelsFit = true,
                resourceValuesFit = true
            };
            float scale = Mathf.Max(.01f, canvas.scaleFactor);
            int n = 0;
            foreach (var item in hud.Navigation.Items)
            {
                if (item == null || !item.Button || !item.Button.gameObject.activeInHierarchy ||
                    !item.Label || !item.Label.gameObject.activeInHierarchy) continue;
                var label = item.Label;
                float px = label.fontSize * scale;
                if (px < m.minNavigationGlyphPx) {
                    m.minNavigationGlyphPx = px;
                    m.smallestNavigationLabel = item.Section.ToString();
                }
                var rect = label.rectTransform.rect;
                m.navigationLabelsFit &= label.preferredWidth <= rect.width + 1f &&
                    label.preferredHeight <= rect.height + 1f;
                n++;
            }
            Need(n == 5, "Native readability QA must inspect five live navigation labels");
            int resources = 0;
            foreach (var label in new[]{hud.Resources.Wood, hud.Resources.Stone, hud.Resources.Food})
            {
                if (!label || !label.gameObject.activeInHierarchy || !label.isActiveAndEnabled)
                    continue;
                float px = label.fontSize * scale;
                if (px < m.minResourceGlyphPx) {
                    m.minResourceGlyphPx = px;
                    m.smallestResourceLabel = label == hud.Resources.Wood ? "wood" :
                        label == hud.Resources.Stone ? "stone" : "food";
                }
                var rect = label.rectTransform.rect;
                m.resourceValuesFit &= label.preferredWidth <= rect.width + 1f &&
                    label.preferredHeight <= rect.height + 1f;
                resources++;
            }
            Need(resources >= 2, "Native readability QA missing actual resource values");
            return m;
        }

        [Serializable] class UiPixelDifference
        {
            public string resolution;
            public int navigationChanged;
            public int headerChanged;
            public int bodyChanged;
            public int navigationArea;
            public int headerArea;
        }

        private static int CountChangedInRect(Color32[] original, Color32[] candidate,
            int width, int height, float xmin, float xmax, float ymin, float ymax)
        {
            int left = Mathf.FloorToInt(width * xmin);
            int right = Mathf.CeilToInt(width * xmax);
            int bottom = Mathf.FloorToInt(height * ymin);
            int top = Mathf.CeilToInt(height * ymax);
            int result = 0;
            for (int y = bottom; y < top; y++)
                for (int x = left; x < right; x++)
                {
                    int i = y * width + x;
                    var a = original[i]; var b = candidate[i];
                    if (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) +
                        Math.Abs(a.b - b.b) >= 50) result++;
                }
            return result;
        }

        private static UiPixelDifference CompareNativeUiRegions(string before, string after,
            int width, int height)
        {
            var a = new Texture2D(2, 2, TextureFormat.RGB24, false);
            var b = new Texture2D(2, 2, TextureFormat.RGB24, false);
            try
            {
                Need(a.LoadImage(File.ReadAllBytes(before), false), "Cannot decode real baseline PNG");
                Need(b.LoadImage(File.ReadAllBytes(after), false), "Cannot decode real candidate PNG");
                Need(a.width == width && a.height == height && b.width == width &&
                     b.height == height, "Native image dimensions differ from requested GameView");
                var p = a.GetPixels32();
                var q = b.GetPixels32();
                return new UiPixelDifference {
                    resolution = width + "x" + height,
                    navigationChanged = CountChangedInRect(p,q,width,height,0f,1f,0f,.20f),
                    headerChanged = CountChangedInRect(p,q,width,height,0f,1f,.80f,1f),
                    bodyChanged = CountChangedInRect(p,q,width,height,.27f,.73f,.31f,.68f),
                    navigationArea = Mathf.CeilToInt(width * height * .20f),
                    headerArea = Mathf.CeilToInt(width * height * .20f)
                };
            }
            finally
            {
                Object.DestroyImmediate(a);
                Object.DestroyImmediate(b);
            }
        }

        static string Hash(string file)
        {
            using(var sha=SHA256.Create())using(var fs=File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-","").ToLowerInvariant();
        }
        static string Snapshot(RoyalHUD hud)
        {
            return (hud.Realm&&hud.Realm.Realm?hud.Realm.Realm.text:"") + "|" +
                   (hud.Resources&&hud.Resources.Wood?hud.Resources.Wood.text:"") + "|" +
                   (hud.Resources&&hud.Resources.Stone?hud.Resources.Stone.text:"") + "|" +
                   (hud.Resources&&hud.Resources.Food&&hud.Resources.Food.gameObject.activeSelf?hud.Resources.Food.text:"locked") + "|" +
                   (hud.Mission&&hud.Mission.Objective?hud.Mission.Objective.text:"") + "|" + hud.SelectedBuildingId;
        }
        static void Need(bool yes,string message){if(!yes)throw new InvalidOperationException(message);}
        static void ValidateNavigation(RoyalHUD hud)
        {
            var nav=hud.Navigation;
            Need(nav&&nav.Items!=null,"Existing navigation missing");
            int count=nav.Items.Count(i=>i!=null&&i.Button&&i.Button.gameObject.activeInHierarchy);
            Need(count==5,"Real nav must show exactly five canonical destinations, actual "+count);
            // Safe geometry and actual pointer dispatch, not synthetic onClick invocation.
            Canvas.ForceUpdateCanvases();
            if (!EventSystem.current) throw new InvalidOperationException("Existing EventSystem missing");
            foreach(var item in nav.Items.Where(i=>i!=null&&i.Button&&i.Button.gameObject.activeInHierarchy))
            {
                var button=item.Button;
                var rect=(RectTransform)button.transform;
                Need(rect.rect.width>=44&&rect.rect.height>=44,"Mobile destination hit target smaller than 44: "+item.Section);
                Need(button.GetComponentsInChildren<Graphic>(true).Any(g=>g.isActiveAndEnabled&&g.raycastTarget),
                    "Real destination has no enabled raycast graphic: "+item.Section);
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                var center=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);
                // The native game handles real pointer events on the SAME
                // RoyalButton, regardless of whether the visible Image is a
                // child. Reject an overlay that steals the center hit.
                var pointer=new PointerEventData(EventSystem.current) {
                    pointerId=-71,button=PointerEventData.InputButton.Left,position=center
                };
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Need(hits.Count>0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,
                    "An overlay intercepts the real navigation button: "+item.Section);

            }
        }
        [Serializable] class NavigationRouteCheck
        {
            public string destination;
            public string status;
            public bool originalEventDelivered;
            public bool destinationVisible;
        }

        /// <summary>
        /// Exercise ACTUAL uGUI pointer down/up/click on the five existing
        /// navigation buttons, only after A/B captures and only in a disposable
        /// gameplay probe. Respect game-progression locks: locked actions are
        /// reported as not exercised, never counted as five successful routes.
        /// No synthetic RoyalHUD.Open or Button.onClick.Invoke shortcuts.
        /// </summary>
        static NavigationRouteCheck[] ExerciseNavigationInProbe(MonoBehaviour host,
            RoyalHUD hud, Type integrationBuild, ForjaOscuraLiveHudSkin skin)
        {
            var city = host.GetType().GetProperty("City")?.GetValue(host);
            Need(city != null && BoolProperty(city, "Probe"),
                "Refuse navigation dispatch without a disposable game probe");
            var click = integrationBuild.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == "Click" && m.GetParameters().Length == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(Button));
            Need(click != null, "Canonical real pointer-click helper missing");
            var original = Snapshot(hud);
            bool originalWorld = BoolProperty(host, "InWorld");
            string selectedBefore = hud.SelectedBuildingId;
            var result = new List<NavigationRouteCheck>();
            int eventCount = 0;
            RoyalSection? observed = null;
            Action<RoyalSection> observer = value => { eventCount++; observed = value; };
            hud.Navigation.Requested += observer;
            try
            {
                // Check every live destination without inventing any unlock.
                // World is last: its real action may switch the camera.
                var sections = new[] {
                    RoyalSection.Bastion, RoyalSection.Inventory,
                    RoyalSection.Missions, RoyalSection.Army, RoyalSection.World
                };
                foreach (var section in sections)
                {
                    hud.Modal.Hide();
                    hud.Refresh();
                    var v2 = hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV2Hud>();
                    if (v2) v2.SynchronizeNow();
                    skin.ApplyNow(true);
                    Canvas.ForceUpdateCanvases();
                    var item = hud.Navigation.Items.FirstOrDefault(i => i != null && i.Section == section);
                    Need(item != null && item.Button && item.Button.gameObject.activeInHierarchy,
                        "Real destination not visible: " + section);
                    var check = new NavigationRouteCheck { destination = section.ToString() };
                    if (!item.Button.interactable)
                    {
                        // Locked remains locked by the authoritative host.
                        // Clicking a disabled destination would not be a valid
                        // gameplay test; explicitly preserve the distinction.
                        check.status = "LOCKED_NOT_EXERCISED";
                        check.originalEventDelivered = false;
                        check.destinationVisible = true;
                        result.Add(check);
                        continue;
                    }
                    int beforeCount = eventCount;
                    try { click.Invoke(null, new object[] { item.Button }); }
                    catch (TargetInvocationException e) { throw e.InnerException ?? e; }
                    check.originalEventDelivered = eventCount == beforeCount + 1 &&
                        observed.HasValue && observed.Value == section;
                    Need(check.originalEventDelivered,
                        "Live navigation event did not reach the original handler: " + section);
                    if (section == RoyalSection.Bastion)
                        check.destinationVisible = hud.SelectedBuildingId == "bastion";
                    else if (section == RoyalSection.Inventory)
                    {
                        // This native probe starts from an unadvanced, named
                        // player. The existing RoyalHUD.Open(Inventory) route
                        // must select the SAWMILL via its original live logic.
                        // Previously "non-empty building" was a false-positive:
                        // the preceding Bastion click already selected Bastion,
                        // so a dead Construir route could pass without moving.
                        // Do not fake a build, unlock anything or invoke Open.
                        check.destinationVisible = hud.SelectedBuildingId == "sawmill";
                    }
                    else if (section == RoyalSection.Missions || section == RoyalSection.Army)
                        check.destinationVisible = hud.Modal && hud.Modal.gameObject.activeSelf;
                    else if (section == RoyalSection.World)
                        check.destinationVisible = BoolProperty(host, "InWorld") != originalWorld;
                    Need(check.destinationVisible,
                        "Original handler executed but requested destination was not shown: " + section);
                    check.status = "NATIVE_POINTER_ROUTE_PASS";
                    result.Add(check);
                }
            }
            finally
            {
                hud.Navigation.Requested -= observer;
                var changeWorld = host.GetType().GetMethod("SetWorld",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(bool) }, null);
                Need(changeWorld != null, "Canonical city/world return route missing");
                if (BoolProperty(host, "InWorld") != originalWorld)
                    changeWorld.Invoke(host, new object[] { originalWorld });
                hud.Modal.Hide();
                var select = host.GetType().GetMethod("Select",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(string) }, null);
                Need(select != null && (bool)select.Invoke(host, new object[] { selectedBefore }),
                    "Could not restore genuine building selection after navigation QA");
                hud.Refresh();
                var context = hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV3ContextHud>();
                if (context) context.SynchronizeNow();
                skin.ApplyNow(true);
                Canvas.ForceUpdateCanvases();
            }
            Need(result.Count == 5, "Five original navigation destinations not accounted for");
            Need(original == Snapshot(hud),
                "Disposable navigation smoke changed the initial HUD values after restore");
            return result.ToArray();
        }

        static void RunCapture(bool batch)
        {
            var host=FindHost();Need(host&&BoolProperty(host,"Ready"),"Canonical gameplay host must be ready");
            var hud=HostHud(host);Need(hud,"Single RoyalHUD from canonical game host missing");
            var t=BuildType();Need(t!=null,"Original native Unity capture class missing");
            var probe=host.GetType().GetProperty("City")?.GetValue(host);
            Need(!batch || (probe!=null&&BoolProperty(probe,"Probe")),
                "Batch evidence REQUIRES disposable city Probe; refuse any real player save");
            if (batch)
            {
                var nickname=t.GetMethods(BindingFlags.Public|BindingFlags.Static)
                    .FirstOrDefault(x=>x.Name=="CompleteProbePlayerName" && x.GetParameters().Length==1);
                Need(nickname!=null,"Fixture name helper missing");
                try{nickname.Invoke(null,new[]{(object)host});}catch(TargetInvocationException e){throw e.InnerException??e;}
                // The first-entry flow can legitimately open a narrated
                // tutorial after the name has been saved. Dismiss it ONLY
                // via the existing visible RoyalButton and the canonical
                // native raycast helper, never by changing a save flag or
                // hiding an overlay for the sake of a screenshot.
                var click=t.GetMethods(BindingFlags.NonPublic|BindingFlags.Static)
                    .FirstOrDefault(x=>x.Name=="Click" && x.GetParameters().Length==1 &&
                        x.GetParameters()[0].ParameterType==typeof(Button));
                if (hud.NarrativeOpen) Need(click!=null,"Canonical real pointer click helper missing");
                for (int page=0;page<24 && hud.NarrativeOpen;page++)
                {
                    try{click.Invoke(null,new object[]{hud.Modal.Extra});}
                    catch(TargetInvocationException e){throw e.InnerException??e;}
                }
                Need(!hud.NarrativeOpen,"Genuine story must finish via real UI before Forja city A/B");
                // Select a REAL resource-building parcel through the existing
                // gameplay host, to make the contextual offer visible in BOTH
                // native A and native B. Selection never executes the offer.
                var select=host.GetType().GetMethod("Select",BindingFlags.Public|BindingFlags.Instance,
                    null,new[]{typeof(string)},null);
                Need(select!=null && (bool)select.Invoke(host,new object[]{"sawmill"}),
                    "Existing city cannot select its genuine sawmill/buildable parcel");
                hud.Refresh();
                Need(hud.SelectedBuildingId=="sawmill",
                    "Capture must reflect a genuine selected building");
            }
            Need(!hud.NarrativeOpen,"Narrative overlay obscures city; dismiss it through real UI before comparing");
            // The canonical contextual card is reprojected from the actual
            // camera and selection by its public synchronizer. Merely calling
            // RoyalHUD.Refresh does not update this independent V3 overlay.
            var context = hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV3ContextHud>();
            if (context) context.SynchronizeNow();
            EditorPrefs.SetBool(SkinOptIn, false);
            var prior=hud.GetComponent<ForjaOscuraLiveHudSkin>();
            if(prior)Object.DestroyImmediate(prior);
            var original=Snapshot(hud);
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"../ForjaNativeEvidence"));
            Directory.CreateDirectory(root);
            const string before="native-before",after="native-after";
            foreach(var wh in new[]{(1600,900),(900,1600),(844,390),(390,844)})
                Capture(t,host,before+"-"+wh.Item1+"x"+wh.Item2+".png",wh.Item1,wh.Item2,root,
                    () => { if (context) context.SynchronizeNow(); Canvas.ForceUpdateCanvases(); });
            ValidateNavigation(hud);
            var skin=hud.gameObject.AddComponent<ForjaOscuraLiveHudSkin>();
            Need(skin!=null,"Skin creation failed");
            skin.ApplyNow();Canvas.ForceUpdateCanvases();skin.ApplyNow();
            Need(skin.NavigationSkinned && skin.ResourceSkinned && skin.HeaderSkinned &&
                 skin.ObjectiveSkinned && skin.BuildingSkinned,
                 "Real nav, illustrated resource icons, header, mission or building did not receive Forja sprites");
            if (context) {
                context.SynchronizeNow();
                skin.ApplyNow();
                Need(!context.ContextVisible || skin.ContextSkinned,
                    "Genuine V3 contextual building visible without the new Forja skin");
            }
            Need(hud.GetComponents<Canvas>().Length==1, "New standalone Canvas introduced unexpectedly");
            ValidateNavigation(hud);
            // ValoriaIntegratedWorld is located reflectively to avoid a
            // second compile-time dependency on the canonical scene assembly.
            var playerCamera = host.GetType()
                .GetProperty("PlayerCamera", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(host) as Camera;
            Need(playerCamera != null, "Canonical ValoriaIntegratedWorld.PlayerCamera unavailable");
            var touchMetrics = new List<UiTouchMetric>();
            var readabilityMetrics = new List<UiReadabilityMetric>();
            foreach(var wh in new[]{(1600,900),(900,1600),(844,390),(390,844)})
                Capture(t,host,after+"-"+wh.Item1+"x"+wh.Item2+".png",wh.Item1,wh.Item2,root,
                    () => {
                        // Refresh the same real contextual projection as the baseline.
                        // Otherwise a rotated camera could masquerade as a UI improvement.
                        if (context) context.SynchronizeNow();
                        skin.ApplyNow(true);
                        Canvas.ForceUpdateCanvases();
                        // Measure screen pixels AFTER Unity sets the actual
                        // offscreen camera resolution; RectTransform units
                        // alone are not an accessibility/hit-area guarantee.
                    },
                    () => {
                        // The source capture invokes this only AFTER c.Render(),
                        // Canvas matrices and all UI raycasts are final.
                        touchMetrics.Add(MeasureNavigationPixels(hud,playerCamera,
                            wh.Item1,wh.Item2));
                        readabilityMetrics.Add(MeasureReadability(hud, wh.Item1, wh.Item2));
                    });
            Need(original==Snapshot(hud),"HUD gameplay data changed while applying presentation-only skin");
            // These are additional live clicks, NOT inferred from graphics or
            // button raycasts. A/B frames are already captured and immutable.
            // Never dispatch them in a manual Editor session using real saves.
            var navigationActions = batch
                ? ExerciseNavigationInProbe(host, hud, t, skin)
                : new NavigationRouteCheck[0];
            Object.DestroyImmediate(skin);Canvas.ForceUpdateCanvases();
            ValidateNavigation(hud);
            var originalAfter=Snapshot(hud);
            Need(original==originalAfter,"Restoring skin changed existing HUD data");
            var files=new[]{(1600,900),(900,1600),(844,390),(390,844)}
                .SelectMany(wh=>new[]{before,after}.Select(prefix=>Path.Combine(root,
                    prefix+"-"+wh.Item1+"x"+wh.Item2+".png"))).ToArray();
            Need(files.Length==8&&files.All(f=>File.Exists(f)&&new FileInfo(f).Length>4096),
                 "Missing or invalid native A/B PNGs; expected eight exact output names");
            int differences=0;
            var uiDifferences = new List<UiPixelDifference>();
            foreach(var wh in new[]{(1600,900),(900,1600),(844,390),(390,844)})
            {
                var pathA=Path.Combine(root,before+"-"+wh.Item1+"x"+wh.Item2+".png");
                var pathB=Path.Combine(root,after+"-"+wh.Item1+"x"+wh.Item2+".png");
                if(Hash(pathA)!=Hash(pathB))differences++;
                var proof=CompareNativeUiRegions(pathA,pathB,wh.Item1,wh.Item2);
                uiDifferences.Add(proof);
                // Hash differences can result from a running world animation.
                // Demand visible change specifically in bottom navigation AND
                // top resource/identity UI for every target orientation.
                Need(proof.navigationChanged >= Mathf.Max(40,proof.navigationArea/2500),
                    "Native Forja navigation does not visibly change at "+proof.resolution);
                Need(proof.headerChanged >= Mathf.Max(40,proof.headerArea/2500),
                    "Native Forja header does not visibly change at "+proof.resolution);
            }
            Need(differences==4,"Native before/after frames have no detectable change in some orientations");
            Need(touchMetrics.Count==4,
                "Native click-area pixel metrics must exist at every orientation");
            foreach (var measurement in touchMetrics)
                Need(measurement.minWidthPx >= 44f && measurement.minHeightPx >= 44f &&
                     measurement.minGraphicWidthPx >= 44f && measurement.minGraphicHeightPx >= 44f,
                    "Forja nav touch target below 44px at " + measurement.resolution +
                    " (smallest " + measurement.smallestDestination + ": " +
                    measurement.minWidthPx + "x" + measurement.minHeightPx + "px; graphic " +
                    measurement.minGraphicWidthPx + "x" + measurement.minGraphicHeightPx + "px)");
            Need(readabilityMetrics.Count == 4,
                "Missing native text-readability evidence for a requested resolution");
            foreach (var reading in readabilityMetrics)
            {
                Need(reading.minNavigationGlyphPx >= 12f && reading.navigationLabelsFit,
                    "Nav text is too small or clipped at " + reading.resolution +
                    ": " + reading.smallestNavigationLabel + " (" +
                    reading.minNavigationGlyphPx + "px)");
                Need(reading.minResourceGlyphPx >= 13f && reading.resourceValuesFit,
                    "Resource values are too small or clipped at " + reading.resolution +
                    ": " + reading.smallestResourceLabel + " (" +
                    reading.minResourceGlyphPx + "px)");
            }
            var report=new EvidenceReport{
                status="UNITY_NATIVE_CAPTURED_VISUAL_REVIEW_PENDING",sourceScene=Scene,
                gameplayHost=host.GetType().FullName,dataBefore=original,dataAfter=originalAfter,
                screenshotCount=files.Length,orientationsWithDifference=differences,
                uiRegionDifferences=uiDifferences.ToArray(),
                nativeTouchTargets=touchMetrics.ToArray(),
                nativeReadability=readabilityMetrics.ToArray(),
                navigationActions=navigationActions,
                captureMethod="ValoriaIntegrationBuild.Capture: Unity RenderTexture/Canvas",
                artifacts=files.Select(f=>new Artifact {name=Path.GetFileName(f),sha256=Hash(f)}).ToArray(),
                gameplayAndSaveRulesChanged=false,physicalMobileTested=false,artApproved=false
            };
            File.WriteAllText(Path.Combine(root,"ForjaNativeEvidence.json"),JsonUtility.ToJson(report,true));
            Debug.Log("[FORJA] REAL UNITY A/B PASS: "+root+" / 8 native images, 4 changed orientations, original HUD values unchanged");
            if(batch)EditorApplication.Exit(0);
        }
        static void Fail(string reason)
        {
            SessionState.SetBool(Flag,false);Debug.LogError("[FORJA] NATIVE_CAPTURE_FAILED: "+reason);
            if(SessionState.GetBool(BatchFlag,false))EditorApplication.Exit(1);
        }
        [Serializable] class Artifact{public string name,sha256;}
        [Serializable] class EvidenceReport
        {
            public string status,sourceScene,gameplayHost,dataBefore,dataAfter,captureMethod;
            public int screenshotCount,orientationsWithDifference;
            public UiPixelDifference[] uiRegionDifferences;
            public UiTouchMetric[] nativeTouchTargets;
            public UiReadabilityMetric[] nativeReadability;
            public NavigationRouteCheck[] navigationActions;
            public bool gameplayAndSaveRulesChanged,physicalMobileTested,artApproved;
            public Artifact[] artifacts;
        }
    }
}
#endif
